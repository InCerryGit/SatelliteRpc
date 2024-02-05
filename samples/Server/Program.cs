using System.Net;
using Microsoft.Extensions.Hosting;
using SatelliteRpc.Server.Extensions;

await Host
    .CreateDefaultBuilder(args)
    .UseSatelliteRpcServer(builder =>
    {
        // define your rpc server
        builder.ConfigureSatelliteRpcServer(options =>
        {
            // define your options here
            options.Host = IPAddress.Parse("127.0.0.1");
            options.Port = 58886;
        });

        builder.AddRpcConnectionHandler(connMidBuilder =>
        {
            connMidBuilder.Use(async (next, context) =>
            {
                Console.WriteLine($"Before Connection Middleware {context.Request.Id}");
                await next(context);
                Console.WriteLine($"After Connection Middleware  {context.Request.Id}");
            });
        });

        builder.AddRpcServiceMiddleware(serviceMidBuilder =>
        {
            serviceMidBuilder.Use(async (next, context) =>
            {
                Console.WriteLine($"Before Service Middleware {context.RawContext.Request.Id}");
                await next(context);
                Console.WriteLine($"After Service Middleware  {context.RawContext.Request.Id}");
            });
        });
    })
    .Build()
    .RunAsync();