﻿using System.Net;
using Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SatelliteRpc.Client;
using SatelliteRpc.Client.Extensions;

await Host
    .CreateDefaultBuilder()
    .UseRpcClient(options =>
    {
        options.ServerAddress = IPAddress.Parse("127.0.0.1");
        options.ServerPort = 58886;
    }, middlewareBuilder =>
    {
        middlewareBuilder.Use(async (next, context) =>
        {
            Console.WriteLine($"Before {context.Request.Id}");
            await next(context);
            Console.WriteLine($"After {context.Request.Id}");
        });
    })
    .ConfigureServices(services =>
    {
        services.AddAutoGeneratedClients();
        services.AddHostedService<DemoHostedService>();
    })
    .Build()
    .RunAsync();