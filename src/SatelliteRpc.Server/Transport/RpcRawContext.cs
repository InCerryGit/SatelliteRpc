using SatelliteRpc.Protocol.Protocol;

namespace SatelliteRpc.Server.Transport;

/// <summary>
/// Represents the context for a raw RPC (Remote Procedure Call) operation. 
/// This class provides access to the request, response, and cancellation token associated with the RPC operation.
/// Implements the IDisposable interface to properly dispose of the request and response when the context is no longer needed.
/// </summary>
public class RpcRawContext : IDisposable
{
    /// <summary>
    /// Gets the cancellation token for the RPC operation.
    /// This token can be used to signal a cancellation request to the operation.
    /// </summary>
    public CancellationToken Cancel { get; }
    
    /// <summary>
    /// Gets the request associated with the RPC operation.
    /// This request contains the data sent by the client to the server.
    /// </summary>
    public AppRequest Request { get; }
    
    /// <summary>
    /// Gets the response associated with the RPC operation.
    /// This response will contain the data to be sent from the server to the client after the operation is completed.
    /// </summary>
    public AppResponse Response { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RpcRawContext"/> class with the specified request, response, and cancellation token.
    /// </summary>
    /// <param name="request">The request associated with the RPC operation.</param>
    /// <param name="response">The response associated with the RPC operation.</param>
    /// <param name="cancel">The cancellation token for the RPC operation.</param>
    public RpcRawContext(AppRequest request, AppResponse response, CancellationToken cancel)
    {
        Request = request;
        Response = response;
        Cancel = cancel;
    }

    /// <summary>
    /// Disposes of the resources used by the <see cref="RpcRawContext"/> instance.
    /// This includes disposing of the request and response associated with the RPC operation.
    /// </summary>
    public void Dispose()
    {
        Request.Dispose();
        Response.Dispose();
    }
}

