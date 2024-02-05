using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SatelliteRpc.Client.Configuration;
using SatelliteRpc.Protocol.Protocol;

namespace SatelliteRpc.Client.Transport;

/// <summary>
/// Represents a connection for executing remote procedure calls (RPCs).
/// This class implements both <see cref="IRpcConnection"/> and <see cref="IDisposable"/>.
/// </summary>
public class RpcConnection : IRpcConnection, IDisposable
{
    private readonly ILogger<RpcConnection> _logger;
    private readonly PipeReader _pipeReader;
    private readonly PipeWriter _pipeWriter;

    // Channel for sending requests.
    private readonly Channel<AppRequest> _requestChannel;

    // Table for tracking response tasks.
    private readonly ConcurrentDictionary<ulong, TaskCompletionSource<AppResponse>> _responseTable = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RpcConnection"/> class.
    /// </summary>
    public RpcConnection(
        ILogger<RpcConnection> logger,
        IOptions<SatelliteRpcClientOptions> optionsAccessor,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        var options = optionsAccessor.Value;

        _requestChannel = Channel.CreateBounded<AppRequest>(
            new BoundedChannelOptions(options.RequestChannelMaxCount)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait,
            });

        var tcpClient = new TcpClient();
        tcpClient.Connect(new IPEndPoint(options.ServerAddress, options.ServerPort));
        var stream = tcpClient.GetStream();
        _pipeReader = PipeReader.Create(stream);
        _pipeWriter = PipeWriter.Create(stream);

        // Start write and read tasks.
        _ = WriteAsync(lifetime.ApplicationStopping);
        _ = ReadAsync(lifetime.ApplicationStopping);

        // Register to dispose when application is stopping.
        lifetime.ApplicationStopping.Register(Dispose);
    }

    /// <summary>
    /// Middleware for handling RPC requests.
    /// </summary>
    public async ValueTask RequestMiddlewareAsync(CallContext callContext)
    {
        var request = callContext.Request;
        var cancellationToken = callContext.Cancel;
        var tcs = new TaskCompletionSource<AppResponse>();
        _responseTable.TryAdd(request.Id, tcs);
        try
        {
            await _requestChannel.Writer.WriteAsync(request, cancellationToken);

            // When cancel, remove TaskCompletionSource from table, and throw exception.
            cancellationToken.Register(() =>
            {
                if (_responseTable.TryRemove(request.Id, out var resTcs))
                {
                    resTcs.TrySetCanceled(cancellationToken);
                }
            });

            callContext.Response = await tcs.Task;
        }
        finally
        {
            _responseTable.TryRemove(request.Id, out _);
        }
    }

    /// <summary>
    /// This class is responsible for writing RPC requests asynchronously to a pipe writer.
    /// </summary>
    private async Task WriteAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Continuously write to the pipe writer while there are requests to be read
            while (await _requestChannel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (_requestChannel.Reader.TryRead(out var request))
                {
                    try
                    {
                        // Serialize the request and write it to the pipe writer
                        request.Serialize(_pipeWriter);
                        await _pipeWriter.FlushAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[{Id}]Write request error", request.Id);

                        // If an error occurs, set the response status to InternalError
                        SetResponse(new AppResponse
                        {
                            Id = request.Id,
                            Status = ResponseStatus.InternalError
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WriteAsync error");
        }
    }

    /// <summary>
    /// This class is responsible for reading RPC responses asynchronously from a pipe reader.
    /// </summary>
    private async Task ReadAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Continuously read from the pipe reader until a cancellation is requested
            while (cancellationToken.IsCancellationRequested == false)
            {
                var result = await _pipeReader.ReadAsync(cancellationToken);
                if (result.IsCanceled)
                {
                    break;
                }

                // Attempt to deserialize the read buffer into an AppResponse
                var (success, response) = AppResponse.TryDeserialize(result.Buffer, out var consumed);

                // If the deserialization failed, continue to the next iteration
                if (success == false)
                {
                    continue;
                }

                // If the deserialization succeeded, set the response and advance the pipe reader
                SetResponse(response!);
                _pipeReader.AdvanceTo(consumed);

                // If the read operation completed, break the loop
                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReadAsync error");
        }
    }


    /// <summary>
    /// Sets the response for a request.
    /// </summary>
    private void SetResponse(AppResponse response)
    {
        if (_responseTable.TryGetValue(response.Id, out var tcs))
        {
            tcs.SetResult(response);
        }
    }

    /// <summary>
    /// Disposes resources used by the connection.
    /// </summary>
    public void Dispose()
    {
        _pipeWriter.CancelPendingFlush();
        _pipeWriter.Complete();

        _pipeReader.CancelPendingRead();
        _pipeReader.Complete();
    }
}