using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SatelliteRpc.Server.Configuration;
using SatelliteRpc.Server.Transport;

namespace SatelliteRpc.Server.Tests.Transport;

public partial class RpcConnectionHandlerTests
{
    private readonly Mock<ILogger<RpcConnectionHandler>> _mockLogger = new();
    private readonly Mock<IOptions<SatelliteRpcServerOptions>> _mockRpcOptionsAccessor = new();
    private readonly Mock<IRpcConnectionApplicationHandlerBuilder> _mockHandlerBuilder = new();
    private readonly Mock<ConnectionContext> _mockConnectionContext = new();

    [Fact]
    public async Task OnConnectedAsync_ShouldLogInformation_WhenConnectionIsEstablished()
    {
        var rpcConnectionHandler = new RpcConnectionHandler(_mockLogger.Object, _mockRpcOptionsAccessor.Object,
            _mockHandlerBuilder.Object);
        
        await rpcConnectionHandler.OnConnectedAsync(_mockConnectionContext.Object);
        
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!));
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldAbort_WhenExceptionOccurs()
    {
        var rpcConnectionHandler = new RpcConnectionHandler(_mockLogger.Object, _mockRpcOptionsAccessor.Object,
            _mockHandlerBuilder.Object);

        _mockConnectionContext.Setup(x => x.ConnectionClosed).Throws(new Exception());
        
        await rpcConnectionHandler.OnConnectedAsync(_mockConnectionContext.Object);
        
        _mockConnectionContext.Verify(x => x.Abort(), Times.Once);
    }
    
}