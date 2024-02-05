using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Channels;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SatelliteRpc.Protocol.Protocol;
using SatelliteRpc.Server.Configuration;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Server.Transport;

/// <summary>
/// Handles RPC connections, including receiving requests, processing them and sending responses.
/// </summary>
public class RpcConnectionHandler : ConnectionHandler
{
    private readonly ILogger<RpcConnectionHandler> _logger;
    private readonly SatelliteRpcServerOptions _satelliteRpcServerOptions;
    private readonly ApplicationDelegate<RpcRawContext> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RpcConnectionHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger used for logging.</param>
    /// <param name="rpcOptionsAccessor">The options accessor for the RPC server.</param>
    /// <param name="handlerBuilder">The builder for the application handler.</param>
    public RpcConnectionHandler(
        ILogger<RpcConnectionHandler> logger,
        IOptions<SatelliteRpcServerOptions>? rpcOptionsAccessor,
        IRpcConnectionApplicationHandlerBuilder handlerBuilder)
    {
        _logger = logger;
        _satelliteRpcServerOptions = rpcOptionsAccessor?.Value ?? new SatelliteRpcServerOptions();
        _handler = handlerBuilder.Build();
    }

    /// <summary>
    /// This method is called when a new connection is established. It reads requests from the input,
    /// deserializes them, and handles them asynchronously. If an error occurs, it logs the error and aborts the connection.
    /// </summary>
    /// <param name="context">The context of the connection, which includes information such as the connection ID and the input and output.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task OnConnectedAsync(ConnectionContext context)
    {
        try
        {

            _logger.LogInformation("[{ConnectionId}]Connection established", context.ConnectionId);

            // Create a channel for sending responses and start it.
            var responseChannel = CreateAndRunResponseChannel(context);

            _logger.LogInformation("[{ConnectionId}]Start reading", context.ConnectionId);

            var input = context.Transport.Input;
            // Continuously read from the input as long as the connection is not closed.
            while (context.ConnectionClosed.IsCancellationRequested == false)
            {
                // Read from the input asynchronously, waiting for more data if necessary.
                var result = await input.ReadAsync(context.ConnectionClosed);
                if (result.IsCanceled)
                {
                    // If the read operation was cancelled, break out of the loop.
                    break;
                }

                // Try to deserialize the received data into a request.
                var (success, request) = AppRequest.TryDeserialize(result.Buffer, out var consumed);
                if (success == false)
                {
                    // If the deserialization failed, continue with the next iteration of the loop.
                    continue;
                }

                // Create a context for the RPC, including the request and a new response with the same ID as the request.
                var rpcContext = new RpcRawContext(request!, new AppResponse { Id = request!.Id },
                    context.ConnectionClosed);
                // Handle the request asynchronously, sending the response through the response channel.
                AsyncRunRequestHandler(responseChannel, rpcContext);
                // Advance the input to the position after the consumed data.
                input.AdvanceTo(consumed);

                if (result.IsCompleted)
                {
                    // If all data has been read, break out of the loop.
                    break;
                }
            }
            
            _logger.LogInformation("[{ConnectionId}]Connection closed", context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{ConnectionId}]Connection error", context.ConnectionId);
            context.Abort();
        }
    }


    /// <summary>
    /// Creates a response channel and runs the response handler.
    /// </summary>
    /// <param name="context">The connection context.</param>
    /// <returns>The response channel.</returns>
    private Channel<RpcRawContext> CreateAndRunResponseChannel(ConnectionContext context)
    {
        var responseChannel = Channel.CreateBounded<RpcRawContext>(
            new BoundedChannelOptions(_satelliteRpcServerOptions.WriteChannelMaxCount)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
        _ = RunResponseHandler(responseChannel.Reader, context.Transport.Output, context.ConnectionClosed);
        return responseChannel;
    }

    /// <summary>
    /// Runs the request handler asynchronously.
    /// </summary>
    /// <param name="writer">The writer of the channel.</param>
    /// <param name="context">The RPC context.</param>
    private void AsyncRunRequestHandler(
        Channel<RpcRawContext, RpcRawContext> writer,
        RpcRawContext context)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _handler(context);
            }
            catch (Exception ex)
            {
                // Logs the error if an exception occurs
                _logger.LogError(ex, "[{Id}]Async run request handler error", context.Request.Id);
                context.Response.Status = ResponseStatus.InternalError;
                context.Response.PayloadWriter = new PayloadWriter
                {
                    GetPayloadSize = () => "System Exception".Length,
                    PayloadWriteTo = (bw) => bw.Write("System Exception"u8)
                };
            }

            await writer.Writer.WriteAsync(context, context.Cancel);
        });
    }

    /// <summary>
    /// Runs the response handler.
    /// </summary>
    /// <param name="reader">The reader of the channel.</param>
    /// <param name="pipeWriter">The pipe writer for writing the response.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async ValueTask RunResponseHandler(
        ChannelReader<RpcRawContext> reader,
        PipeWriter pipeWriter,
        CancellationToken cancellationToken)
    {
        try
        {
            while (await reader.WaitToReadAsync(cancellationToken))
            {
                while (reader.TryRead(out var context))
                {
                    try
                    {
                        context.Response.Serialize(pipeWriter);
                        await pipeWriter.FlushAsync(cancellationToken);
                        // Disposes the context after the request is handled
                        context.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[{Id}]Write response error", context.Request.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Run response handler error");
        }
    }
}