namespace SatelliteRpc.Client;

/// <summary>
/// Rpc service attribute, used to mark the interface as a rpc service
/// Generate the client and dependency injection by default
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class SatelliteRpcAttribute : Attribute
{
    public SatelliteRpcAttribute(string serviceName)
    {
        ServiceName = serviceName;
    }

    public string ServiceName { get; set; }

    public bool GenerateClient { get; set; } = true;
    
    public bool GenerateDependencyInjection { get; set; } = true;
}