using SatelliteRpc.Protocol.Protocol;

namespace SatelliteRpc.Client.Transport;

/// <summary>
/// The call context of Rpc Client
/// Use this class to pass data between middleware
/// </summary>
public class CallContext : IDisposable
{
    /// <summary>
    /// Request cancellation token
    /// Pass client call cancellation token to middleware
    /// </summary>
    public CancellationToken Cancel { get; set; }

    /// <summary>
    ///  Rpc request
    /// </summary>
    public AppRequest Request { get; set; }

    /// <summary>
    ///  Rpc response
    /// </summary>
    public AppResponse? Response { get; set; }

    /// <summary>
    ///  Constructor
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancel"></param>
    public CallContext(AppRequest request, CancellationToken cancel)
    {
        Request = request;
        Cancel = cancel;
    }

    /// <summary>
    ///  When the context is disposed, the request and response will be disposed
    /// </summary>
    public void Dispose()
    {
        Request.Dispose();
        Response?.Dispose();
    }
}