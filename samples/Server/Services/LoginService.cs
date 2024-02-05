using Microsoft.Extensions.Logging;
using ServerProto;

namespace Server.Services;

public class LoginService : ILoginService
{
    private readonly ILogger<LoginService> _logger;

    public LoginService(ILogger<LoginService> logger)
    {
        _logger = logger;
    }

    public Task<LoginRespProto> Login(LoginReqProto req, CancellationToken? cancellationToken)
    {
        _logger.LogInformation("Login: {UserName} {Password}", req.User, req.Password);
        return Task.FromResult(new LoginRespProto
        {
            IsOk = true,
            ErrMsg = "Not Implemented",
            Sn = req.Sn
        });
    }

    public Task LogOut(CancellationToken? cancellationToken = null)
    {
        _logger.LogInformation("LogOut");
        return Task.CompletedTask;
    }
}