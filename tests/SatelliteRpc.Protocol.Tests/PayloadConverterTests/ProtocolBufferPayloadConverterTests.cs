using Google.Protobuf;
using SatelliteRpc.Protocol.PayloadConverters;
using SatelliteRpc.Protocol.Protocol;
using SatelliteRpc.Shared.Collections;
using ServerProto;

namespace SatelliteRpc.Protocol.Tests.PayloadConverterTests;

public class ProtocolBufferPayloadConverterTests
{
    private readonly ProtocolBufferPayloadConverter _converter = new();

    [Fact]
    public void CreatePayloadWriter_NullPayload_ReturnsEmpty()
    {
        var writer = _converter.CreatePayloadWriter(null);
        Assert.Equal(PayloadWriter.Empty, writer);
    }
        
    [Fact]
    public void CreatePayloadWriter_ValidIMessage_ReturnsPayloadWriter()
    {
        var loginReq = new LoginReqProto { User = "test", Password = "password", Sn = 1 };
        var writer = _converter.CreatePayloadWriter(loginReq);
            
        Assert.NotNull(writer.GetPayloadSize);
        Assert.NotNull(writer.PayloadWriteTo);
        Assert.Equal(loginReq.CalculateSize(), writer.GetPayloadSize());
    }
        
    [Fact]
    public void CreatePayloadWriter_InvalidPayload_ThrowsException()
    {
        Assert.Throws<Exception>(() => _converter.CreatePayloadWriter(new object()));
    }
        
    [Fact]
    public void Convert_InvalidType_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => _converter.Convert(new PooledArray<byte>(4), typeof(object)));
    }
        
    [Fact]
    public void Convert_EmptyPayload_ThrowsException()
    {
        Assert.Throws<InvalidProtocolBufferException>(() => _converter.Convert(new PooledArray<byte>(4),
            typeof(LoginRespProto)));
    }
        
    [Fact]
    public void Convert_ValidPayload_ReturnsMessage()
    {
        var loginResp = new LoginRespProto { IsOk = true, ErrMsg = "Success", Sn = 1 };
        var bytes = loginResp.ToByteArray();
        var pooledArray = new PooledArray<byte>(bytes.Length);
        bytes.CopyTo(pooledArray.Memory);
            
        var result = _converter.Convert(pooledArray, typeof(LoginRespProto)) as LoginRespProto;
            
        Assert.NotNull(result);
        Assert.Equal(loginResp.IsOk, result.IsOk);
        Assert.Equal(loginResp.ErrMsg, result.ErrMsg);
        Assert.Equal(loginResp.Sn, result.Sn);
    }
}