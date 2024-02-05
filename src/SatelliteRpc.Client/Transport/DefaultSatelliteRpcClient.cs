using System.Text;
using Microsoft.Extensions.Options;
using SatelliteRpc.Client.Configuration;
using SatelliteRpc.Client.Middleware;
using SatelliteRpc.Protocol.PayloadConverters;
using SatelliteRpc.Protocol.Protocol;
using SatelliteRpc.Shared.Application;

namespace SatelliteRpc.Client.Transport;

/// <summary>
/// Default implementation of the ISatelliteRpcClient interface.
/// This class is responsible for invoking remote methods.
/// </summary>
public class DefaultSatelliteRpcClient : ISatelliteRpcClient
{
    private ulong _id;

    private readonly IPayloadConverter _payloadConverter;
    private readonly ApplicationDelegate<CallContext> _clientMiddleware;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultSatelliteRpcClient"/> class.
    /// </summary>
    /// <param name="optionsAccessor">Provides access to the SatelliteRpcClientOptions.</param>
    /// <param name="connection">The RPC connection to use.</param>
    /// <param name="converterSource">The source for payload converters.</param>
    /// <param name="builder">The middleware builder.</param>
    public DefaultSatelliteRpcClient(
        IOptions<SatelliteRpcClientOptions> optionsAccessor,
        IRpcConnection connection,
        PayloadConverterSource converterSource,
        IRpcClientMiddlewareBuilder builder)
    {
        var options = optionsAccessor.Value;

        _payloadConverter = converterSource.GetConverter(options.PayloadType);

        // The last middleware, used to send network requests
        builder.Builder.Use(async (_, context) => { await connection.RequestMiddlewareAsync(context); });
        _clientMiddleware = builder.Build();
    }

    /// <summary>
    /// Invoke a remote method without a payload.
    /// </summary>
    /// <param name="path">The remote method path.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    public async Task InvokeAsync(string path, CancellationToken? cancellationToken = null)
    {
        // because the request payload is empty, so we use the Empty type
        await InvokeAsync<Empty>(path, PayloadWriter.Empty, cancellationToken);
    }

    /// <summary>
    /// Invoke a remote method with a specified request and response type.
    /// </summary>
    /// <param name="path">The remote method path.</param>
    /// <param name="rawRequest">The request payload.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <typeparam name="TRequest">The type of the request payload.</typeparam>
    /// <typeparam name="TResponse">The type of the response payload.</typeparam>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response payload.</returns>
    public async Task<TResponse?> InvokeAsync<TRequest, TResponse>(
        string path,
        TRequest rawRequest,
        CancellationToken? cancellationToken = null)
    {
        var payload = _payloadConverter.CreatePayloadWriter(rawRequest);
        return await InvokeAsync<TResponse>(path, payload, cancellationToken);
    }

    /// <summary>
    /// Private helper method to invoke a remote method with a specified response type.
    /// </summary>
    /// <param name="path">The remote method path.</param>
    /// <param name="payload">The request payload writer.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <typeparam name="TResponse">The type of the response payload.</typeparam>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response payload.</returns>
    private async ValueTask<TResponse?> InvokeAsync<TResponse>(
        string path,
        PayloadWriter payload,
        CancellationToken? cancellationToken = null)
    {
        var request = new AppRequest
        {
            Id = Interlocked.Increment(ref _id),
            Path = path,
            PayloadWriter = payload
        };

        // Create a new call context
        // The call context will be disposed after the request is completed
        using var callContext = new CallContext(request, cancellationToken ?? CancellationToken.None);
        await _clientMiddleware(callContext);
        
        // if the response status is not success, the payload is the error message
        // this is a convention
        if (callContext.Response!.Status != ResponseStatus.Success)
        {
            throw new Exception(Encoding.UTF8.GetString(callContext.Response.Payload.Span));
        }

        return typeof(TResponse) == typeof(Empty)
            ? default
            : (TResponse?)_payloadConverter.Convert(callContext.Response.Payload, typeof(TResponse));
    }
}
