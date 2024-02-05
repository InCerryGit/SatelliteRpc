using SatelliteRpc.Client;
using ServerProto;

namespace Client.Rpc;

[SatelliteRpc("LoginService")]
public interface ILoginClient
{
    Task<LoginRespProto?> Login(LoginReqProto req, CancellationToken? cancellationToken = null);

    Task LogOut(CancellationToken? cancellationToken = null);
}