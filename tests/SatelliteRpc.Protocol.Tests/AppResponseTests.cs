using System.Buffers;
using SatelliteRpc.Protocol.Protocol;
using SatelliteRpc.Shared.Collections;

namespace SatelliteRpc.Protocol.Tests;

public class AppResponseTests
{
    [Fact]
    public void Can_Serialize_And_Deserialize_Response()
    {
        var original = new AppResponse
        {
            Id = 12345,
            Status = ResponseStatus.Success,
            PayloadType = PayloadType.Json,
            Payload = new byte[]{ 1, 2, 3, 4, 5 }.ToPooledArray()
        };
        
        var writer = new ArrayBufferWriter<byte>();
        original.Serialize(writer);
        
        var sequence = new ReadOnlySequence<byte>(writer.WrittenMemory);
        var (success,deserialized) = AppResponse.TryDeserialize(sequence, out var consumed);
        
        Assert.Equal(sequence.Length, consumed.GetInteger());
        Assert.True(success);
        Assert.NotNull(deserialized);
            
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Status, deserialized.Status);
        Assert.Equal(original.PayloadType, deserialized.PayloadType);
        Assert.Equal(original.Payload.ToArray(), deserialized.Payload.ToArray());
    }
        
    [Fact]
    public void Can_Serialize_And_Deserialize_Response_With_PayloadWriteTo()
    {
        var payload = Enumerable.Range(0, 10240).Select(c => (byte)c).ToArray();
        var original = new AppResponse
        {
            Id = 12345,
            Status = ResponseStatus.Success,
            PayloadType = PayloadType.Json,
            PayloadWriter = new PayloadWriter
            {
                GetPayloadSize = () => payload.Length,
                PayloadWriteTo = (writer) =>
                {
                    writer.Write(payload);
                }
            }
        };
        
        var writer = new ArrayBufferWriter<byte>();
        original.Serialize(writer);
        
        var sequence = new ReadOnlySequence<byte>(writer.WrittenMemory);
        var (success,deserialized) = AppResponse.TryDeserialize(sequence, out var consumed);
        
        Assert.Equal(sequence.Length, consumed.GetInteger());
        Assert.True(success);
        Assert.NotNull(deserialized);
        
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Status, deserialized.Status);
        Assert.Equal(original.PayloadType, deserialized.PayloadType);
        Assert.Equal(payload.ToArray(), deserialized.Payload.ToArray());
    }
        
    [Fact]
    public void Deserialize_Response_Should_Fail_With_Not_Enough_Length()
    {
        var original = new AppResponse
        {
            Id = 12345,
            Status = ResponseStatus.Success,
            PayloadType = PayloadType.Json,
            Payload = new byte[]{ 1, 2, 3, 4, 5 }.ToPooledArray()
        };
        
        var writer = new ArrayBufferWriter<byte>();
        original.Serialize(writer);
        
        var sequence = new ReadOnlySequence<byte>(writer.WrittenMemory[..^1]);
        var (success,deserialized) = AppResponse.TryDeserialize(sequence, out _);

        Assert.False(success);
        Assert.Null(deserialized);
    }
        
    [Fact]
    public void Deserialize_Response_Should_Fail_With_Not_Length()
    {
        
        var sequence = new ReadOnlySequence<byte>(new byte[] { 1, 2, 3});
        var (success,deserialized) = AppResponse.TryDeserialize(sequence, out _);

        Assert.False(success);
        Assert.Null(deserialized);
    }

    [Fact]
    public void Can_Reuse_Memory_When_Deserializing_Response()
    {
        var original = new AppResponse
        {
            Id = 12345,
            Status = ResponseStatus.Success,
            PayloadType = PayloadType.Json,
            Payload = new byte[]{ 1, 2, 3, 4, 5 }.ToPooledArray()
        };
        
        var writer = new ArrayBufferWriter<byte>();
        original.Serialize(writer);
        
        var sequence = new ReadOnlySequence<byte>(writer.WrittenMemory);
        
        var reuse = new AppResponse();
        var (_,deserialized) = AppResponse.TryDeserialize(sequence, out _, reuse);
        
        Assert.Same(reuse, deserialized);
    }
}