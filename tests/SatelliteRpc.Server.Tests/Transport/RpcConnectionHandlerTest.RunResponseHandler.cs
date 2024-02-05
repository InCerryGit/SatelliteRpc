using System.IO.Pipelines;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Moq;
using SatelliteRpc.Server.Transport;

namespace SatelliteRpc.Server.Tests.Transport;

public partial class RpcConnectionHandlerTests
{
    [Fact]
    public async Task RunResponseHandler_ShouldLogError_WhenExceptionOccurs()
    {
        var rpcConnectionHandler = new RpcConnectionHandler(_mockLogger.Object, _mockRpcOptionsAccessor.Object,
            _mockHandlerBuilder.Object);

        var mockChannelReader = new Mock<ChannelReader<RpcRawContext>>();
        mockChannelReader.Setup(x => x.WaitToReadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mockChannelReader.Setup(x => x.TryRead(out It.Ref<RpcRawContext>.IsAny))
            .Returns(true);

        var mockPipeWriter = new Mock<PipeWriter>();
        mockPipeWriter.Setup(x => x.FlushAsync(It.IsAny<CancellationToken>()))
            .Throws(new Exception());
        
        await rpcConnectionHandler.RunResponseHandler(mockChannelReader.Object, mockPipeWriter.Object,
            new CancellationToken());
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Run response handler error")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!));
    }

}