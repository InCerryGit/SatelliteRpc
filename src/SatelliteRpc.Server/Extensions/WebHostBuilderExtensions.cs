using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SatelliteRpc.Server.Configuration;
using SatelliteRpc.Server.Transport;

namespace SatelliteRpc.Server.Extensions;

/// <summary>
/// Provides extension methods for the IWebHostBuilder interface to configure RPC services.
/// </summary>
public static class WebHostBuilderExtensions
{
    /// <summary>
    /// Configures the specified IWebHostBuilder to use the Kestrel server and RPC services.
    /// </summary>
    /// <param name="builder">The IWebHostBuilder to configure.</param>
    /// <returns>The IWebHostBuilder that was passed into the method, with the configuration applied.</returns>
    public static IWebHostBuilder UseRpcKestrelServer(
        this IWebHostBuilder builder)
    {
        builder.UseKestrel(options =>
        {
            var serverOptions = options.ApplicationServices.GetService<IOptions<SatelliteRpcServerOptions>>();

            // Configure Kestrel to listen on the specified host and port,
            // and to use the RpcConnectionHandler for handling connections
            options.Listen(
                serverOptions!.Value.Host,
                serverOptions.Value.Port,
                listenOptions => { listenOptions.UseConnectionHandler<RpcConnectionHandler>(); });
        });
        
        // Configure the application to use the RpcMvcMiddleware
        // and ignore the default middleware
        builder.Configure(_ => { });

        return builder;
    }
}
