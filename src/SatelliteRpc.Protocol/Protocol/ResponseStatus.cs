namespace SatelliteRpc.Protocol.Protocol;

/// <summary>
///  Response status
/// </summary>
public enum ResponseStatus
{
    /// <summary>
    /// Success
    /// </summary>
    Success = 0,
    
    /// <summary>
    /// Not found
    /// it means the request is not found service or method
    /// </summary>
    NotFound = 1,
    
    /// <summary>
    ///  Bad request
    ///  it means the request is not valid
    /// </summary>
    BadRequest = 2,
    
    /// <summary>
    ///  Internal error
    ///  it means the request is valid, but the server has an internal error
    /// </summary>
    InternalError = 3
}