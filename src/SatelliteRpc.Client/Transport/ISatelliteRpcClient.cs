namespace SatelliteRpc.Client.Transport;

public interface ISatelliteRpcClient
{
    /// <summary>
    ///  Invoke the remote method
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InvokeAsync(string path, CancellationToken? cancellationToken = null);

    /// <summary>
    ///  Invoke the remote method
    /// </summary>
    /// <param name="path"></param>
    /// <param name="rawRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    Task<TResponse?> InvokeAsync<TRequest, TResponse>(
        string path,
        TRequest rawRequest,
        CancellationToken? cancellationToken = null);
}