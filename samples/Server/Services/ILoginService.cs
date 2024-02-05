using SatelliteRpc.Server.RpcService;
using ServerProto;

namespace Server.Services;

public interface ILoginService : IRpcService
{
    Task<LoginRespProto> Login(LoginReqProto req, CancellationToken? cancellationToken);

    Task LogOut(CancellationToken? cancellationToken = null); 
}