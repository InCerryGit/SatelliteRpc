using SatelliteRpc.Protocol.PayloadConverters;
using SatelliteRpc.Protocol.Protocol;
using SatelliteRpc.Server.Exceptions;
using SatelliteRpc.Server.RpcService.Endpoint;
using SatelliteRpc.Server.Transport;

namespace SatelliteRpc.Server.RpcService.DataExchange;

/// <summary>
/// Default implementation of the IRpcDataExchange interface.
/// This class is responsible for converting payloads to parameters and vice versa for RPC calls.
/// </summary>
public class DefaultRpcDataExchange : IRpcDataExchange
{
    /// <summary>
    /// Source of payload converters. Used to convert payloads to parameters and vice versa.
    /// </summary>
    private readonly PayloadConverterSource _converterSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultRpcDataExchange"/> class.
    /// </summary>
    /// <param name="converterSource">The source of payload converters.</param>
    public DefaultRpcDataExchange(PayloadConverterSource converterSource)
    {
        _converterSource = converterSource;
    }

    /// <summary>
    /// Binds the parameters for an RPC service endpoint.
    /// Converts the payload in the raw context to the types expected by the endpoint.
    /// </summary>
    /// <param name="endpoint">The RPC service endpoint.</param>
    /// <param name="rawContext">The raw RPC context.</param>
    /// <returns>An array of parameters to be used in the RPC call.</returns>
    public object?[] BindParameters(RpcServiceEndpoint endpoint, RpcRawContext rawContext)
    {
        try
        {
            var parameters = new object?[endpoint.ParameterTypes.Length];
            for (var i = 0; i < endpoint.ParameterTypes.Length; i++)
            {
                var parameterType = endpoint.ParameterTypes[i];
                
                // Special case for CancellationToken, use the cancellation token from the raw context
                if (parameterType == typeof(CancellationToken) || parameterType == typeof(CancellationToken?))
                {
                    parameters[i] = rawContext.Cancel;
                }
                else
                {
                    var converter = _converterSource.GetConverter(rawContext.Request.PayloadType);
                    parameters[i] = converter.Convert(rawContext.Request.Payload, parameterType);
                }
            }

            return parameters;
        }
        catch (Exception ex)
        {
            throw new ParametersBindException("Failed to bind parameters", ex);
        }
    }

    /// <summary>
    /// Gets a payload writer for the given result and raw context.
    /// The payload writer is used to convert the result of an RPC call to a payload.
    /// </summary>
    /// <param name="result">The result of the RPC call.</param>
    /// <param name="rawContext">The raw RPC context.</param>
    /// <returns>A payload writer for the result.</returns>
    public PayloadWriter GetPayloadWriter(object? result, RpcRawContext rawContext)
    {
        var converter = _converterSource.GetConverter(rawContext.Request.PayloadType);
        return converter.CreatePayloadWriter(result);
    }
}
