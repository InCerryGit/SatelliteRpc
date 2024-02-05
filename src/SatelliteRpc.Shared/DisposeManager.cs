using SatelliteRpc.Shared.Collections;

namespace SatelliteRpc.Shared;

/// <summary>
/// A class to manage the lifecycle of IDisposable resources.
/// It provides a centralized way of disposing of multiple resources.
/// </summary>
public class DisposeManager : IDisposable
{
    /// <summary>
    /// A list of resources that implement IDisposable.
    /// These resources will be disposed of when the DisposeManager is disposed.
    /// </summary>
    private readonly PooledList<IDisposable> _resources = new();

    /// <summary>
    /// Register an IDisposable resource to be disposed of when the DisposeManager is disposed.
    /// </summary>
    /// <param name="resource">The IDisposable resource to register.</param>
    public void RegisterForDispose(IDisposable resource)
    {
        _resources.Add(resource);
    }

    /// <summary>
    /// Disposes of all registered resources and clears the list.
    /// This method should be called when the resources are no longer needed.
    /// </summary>
    public void Dispose()
    {
        foreach (var resource in _resources)
        {
            resource.Dispose();
        }
        
        _resources.Clear();
        _resources.Dispose();
    }
}
