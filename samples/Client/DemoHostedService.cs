using Client.Rpc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerProto;

namespace Client;

public class DemoHostedService : BackgroundService
{
    private readonly ILogger<DemoHostedService> _logger;
    private readonly ILoginClient _client;

    public DemoHostedService(ILogger<DemoHostedService> logger, ILoginClient client)
    {
        _logger = logger;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DemoHostedService is starting");
        
        var rp = await _client.Login(new LoginReqProto
        {
            User = "admin",
            Password = "123456",
            Sn = 1024
        }, stoppingToken);
        _logger.LogInformation("Received1: {IsOk},{ErrMsg},{Sn}", rp!.IsOk, rp.ErrMsg, rp.Sn);        
        
        rp = await _client.Login(new LoginReqProto
        {
            User = "guest",
            Password = "654321",
            Sn = 2048
        }, stoppingToken);
        _logger.LogInformation("Received2: {IsOk},{ErrMsg},{Sn}", rp!.IsOk, rp.ErrMsg, rp.Sn);
        
        await _client.LogOut(stoppingToken);
        _logger.LogInformation("DemoHostedService is stopping");
    }
}