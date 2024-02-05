namespace SatelliteRpc.Server.Exceptions;

/// <summary>
/// Parameters bind exception
/// it means the request parameters bind error, maybe the request parameters is not valid
/// </summary>
public class ParametersBindException : ApplicationException
{
    public ParametersBindException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public ParametersBindException(string? message) : base(message)
    {
    }
}