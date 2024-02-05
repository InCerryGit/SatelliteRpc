using System.Reflection;

namespace SatelliteRpc.Server.RpcService.Endpoint;

/// <summary>
/// Represents a remote procedure call (RPC) service endpoint.
/// </summary>
public class RpcServiceEndpoint
{
    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the name of the method.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Gets the type of the service.
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Gets the types of the parameters of the method.
    /// </summary>
    public Type[] ParameterTypes { get; }

    /// <summary>
    /// Gets the return type of the method.
    /// </summary>
    public Type ReturnType { get; }

    /// <summary>
    /// Gets a value indicating whether the return type is a Task.
    /// </summary>
    public bool ReturnIsTask => ReturnType == typeof(Task);

    /// <summary>
    /// Gets a value indicating whether the return type is a generic Task.
    /// </summary>
    public bool ReturnIsTaskT => ReturnType.IsGenericType && ReturnType.GetGenericTypeDefinition() == typeof(Task<>);

    /// <summary>
    /// Gets the method invoker function.
    /// </summary>
    public Func<object, object?[], object> MethodInvoker { get; }
    
    /// <summary>
    /// Gets the path of the RPC service endpoint.
    /// </summary>
    public string Path => $"{ServiceName}/{MethodName}";

    /// <summary>
    /// Initializes a new instance of the <see cref="RpcServiceEndpoint"/> class.
    /// </summary>
    public RpcServiceEndpoint(
        string serviceName,
        string methodName,
        Type[] parameterTypes,
        Type returnType,
        Type serviceType,
        Func<object, object?[], object> methodInvoker)
    {
        ServiceName = serviceName;
        MethodName = methodName;
        ParameterTypes = parameterTypes;
        ReturnType = returnType;
        ServiceType = serviceType;
        MethodInvoker = methodInvoker;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="RpcServiceEndpoint"/> class from a MethodInfo object.
    /// </summary>
    public static RpcServiceEndpoint FromMethodInfo(Type type, MethodInfo methodInfo)
    {
        var serviceName = type.Name;
        var methodName = methodInfo.Name;
        var parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
        var returnType = methodInfo.ReturnType;
        return new RpcServiceEndpoint(
            serviceName,
            methodName,
            parameterTypes,
            returnType,
            type,
            Shared.MethodInvoker.CreateInvoker(type, methodInfo));
    }

    /// <summary>
    /// Invokes the method of this RPC service endpoint asynchronously.
    /// </summary>
    public async Task<object> InvokeAsync(object instance, object?[] parameters)
    {
        // invoke method, if method is async task or Task<T>, await for task complete
        var result = MethodInvoker(instance, parameters);
        if (result is not Task task) return result;

        await task;

        return ReturnIsTaskT ? Shared.MethodInvoker.GetTaskResult(task) : null!;
    }
}
