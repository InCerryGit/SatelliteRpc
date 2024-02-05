namespace SatelliteRpc.Server.Exceptions;

/// <summary>
/// Not found
/// it means the request  is not found service or method
/// </summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(string? message) : base(message)
    {
    }
}