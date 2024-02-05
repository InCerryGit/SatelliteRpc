namespace SatelliteRpc.Client.Transport;

public interface IRpcConnection
{
    /// <summary>
    /// Send request to server middleware
    /// </summary>
    /// <param name="callContext"></param>
    /// <returns></returns>
    ValueTask RequestMiddlewareAsync(CallContext callContext);
}