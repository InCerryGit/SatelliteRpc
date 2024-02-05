// 自动化生成的客户端实例如下类似
//using SatelliteRpc.Client;
//using ServerProto;

//namespace Client.Rpc;

//public class DemoLoginClient : ILoginClient
//{
//    private readonly ISatelliteRpcClient _client;
//
//    public DemoLoginClient(ISatelliteRpcClient client)
//    {
//        _client = client;
//    }
//
//    public async Task<LoginRespProto?> Login(LoginReqProto req, CancellationToken? cancellationToken = null)
//    {
//        return await _client.InvokeAsync<LoginReqProto, LoginRespProto>("LoginService/Login", req, cancellationToken);
//    }
//
//    public Task LogOut(CancellationToken? cancellationToken = null)
//    {
//        return _client.InvokeAsync("LoginService/LogOut", cancellationToken);
//    }
//}