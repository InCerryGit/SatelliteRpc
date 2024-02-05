using System.Buffers;
using SatelliteRpc.Protocol.Protocol;
using SatelliteRpc.Shared.Collections;

namespace SatelliteRpc.Protocol.Tests;

public class AppRequestTests
{
    [Fact]
    public void Can_Serialize_And_Deserialize_Request()
    {
        var original = new AppRequest
        {
            Id = 12345,
            Path = "/test/path",
            PayloadType = PayloadType.Json,
            Payload = new byte[] { 1, 2, 3, 4, 5 }.ToPooledArray()
        };

        var writer = new ArrayBufferWriter<byte>();
        original.Serialize(writer);

        var sequence = new ReadOnlySequence<byte>(writer.WrittenMemory);
        var (success,deserialized) = AppRequest.TryDeserialize(sequence, out var consumed);
            
        Assert.Equal(sequence.Length, consumed.GetInteger());
        Assert.True(success);
        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Path, deserialized.Path);
        Assert.Equal(original.PayloadType, deserialized.PayloadType);
        Assert.Equal(original.Payload.ToArray(), deserialized.Payload.ToArray());
    }

    [Fact]
    public void Can_Serialize_And_Deserialize_Request_With_PayloadWriteTo()
    {
        var payload = Enumerable.Range(0, 10240).Select(c => (byte)c).ToArray();
        var original = new AppRequest
        {
            Id = 12345,
            Path = "/test/path",
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
        var (success,deserialized) = AppRequest.TryDeserialize(sequence, out var consumed);
            
        Assert.Equal(sequence.Length, consumed.GetInteger());
        Assert.True(success);
        Assert.NotNull(deserialized);
        
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Path, deserialized.Path);
        Assert.Equal(original.PayloadType, deserialized.PayloadType);
        Assert.Equal(payload.ToArray(), deserialized.Payload.ToArray());
    }

    [Fact]
    public void Deserialize_Request_Should_Fail_With_Not_Enough_Length()
    {
        var original = new AppRequest
        {
            Id = 12345,
            Path = "/test/path",
            PayloadType = PayloadType.Json,
            Payload = new byte[] { 1, 2, 3, 4, 5 }.ToPooledArray()
        };
        
        var writer = new ArrayBufferWriter<byte>();
        original.Serialize(writer);
        
        var sequence = new ReadOnlySequence<byte>(writer.WrittenMemory[..^1]);
        var (success,deserialized) = AppRequest.TryDeserialize(sequence, out _);

        Assert.False(success);
        Assert.Null(deserialized);
    }

    [Fact]
    public void Deserialize_Request_Should_Fail_With_Not_Length()
    {
        
        var sequence = new ReadOnlySequence<byte>(new byte[] { 1, 2, 3});
        var (success,deserialized) = AppRequest.TryDeserialize(sequence, out _);

        Assert.False(success);
        Assert.Null(deserialized);
    }

    [Fact]
    public void Can_Reuse_Memory_When_Deserializing_Request()
    {
        var original = new AppRequest
        {
            Id = 12345,
            Path = "/test/path",
            PayloadType = PayloadType.Json,
            Payload = new byte[] { 1, 2, 3, 4, 5 }.ToPooledArray()
        };
        
        var writer = new ArrayBufferWriter<byte>();
        original.Serialize(writer);
        
        var sequence = new ReadOnlySequence<byte>(writer.WrittenMemory);
        
        var reuse = new AppRequest();
        var (_,deserialized) = AppRequest.TryDeserialize(sequence, out _, reuse);
            
        Assert.Same(reuse, deserialized);
    }
}