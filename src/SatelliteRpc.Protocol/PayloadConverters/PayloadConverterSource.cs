using SatelliteRpc.Protocol.Protocol;

namespace SatelliteRpc.Protocol.PayloadConverters;

/// <summary>
/// Manage payload converters
/// </summary>
public class PayloadConverterSource
{
    private readonly IReadOnlyDictionary<PayloadType, IPayloadConverter> _converters;

    public PayloadConverterSource(IEnumerable<IPayloadConverter> converters)
    {
        _converters = converters.ToDictionary(c => c.PayloadType);
    }

    /// <summary>
    ///  Get payload converter by payload type
    /// </summary>
    /// <param name="payloadType"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public IPayloadConverter GetConverter(PayloadType payloadType)
    {
        if (_converters.TryGetValue(payloadType, out var converter))
        {
            return converter;
        }

        throw new Exception($"No converter found for type {payloadType}");
    }
}