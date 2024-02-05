// using System.Buffers;
// using System.Buffers.Binary;
// using System.Diagnostics.CodeAnalysis;
// using System.Runtime.CompilerServices;
// using Google.Protobuf;
//
// namespace SatelliteRpc.Protocol;
//
// public static class BasicProtocolParser
// {
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static bool TryParserRequest(
//         ReadOnlySequence<byte> sequence,
//         out SequencePosition consumed,
//         [MaybeNullWhen(returnValue: false)] out AppRequest request)
//     {
//         // Frame format is defined here:
//         // 8 bytes for length of payload
//         // N bytes of payload
//         request = null;
//         consumed = default;
//
//         if (sequence.Length < 8)
//             return false;
//
//         var length = GetLength(sequence);
//         if (length > sequence.Length)
//             return false;
//
//         var payload = sequence.Slice(8, length);
//         request = AppRequest.Parser.ParseFrom(payload);
//         
//         consumed = sequence.GetPosition(8 + length);
//         return true;
//     }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     private static long GetLength(in ReadOnlySequence<byte> buffer)
//     {
//         if (buffer.First.Length >= 8)
//         {
//             return BinaryPrimitives.ReadInt64LittleEndian(buffer.First.Span[..8]);
//         }
//
//         Span<byte> lengthBuffer = stackalloc byte[8];
//         buffer.Slice(0, 8).CopyTo(lengthBuffer);
//         return BinaryPrimitives.ReadInt64LittleEndian(lengthBuffer);
//     }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static bool TryParserResponse(
//         ReadOnlySequence<byte> sequence,
//         out SequencePosition consumed,
//         [MaybeNullWhen(returnValue: false)] out AppResponse response)
//     {
//         // Frame format is defined here:
//         // 8 bytes for length of payload
//         // N bytes of payload
//         response = null;
//         consumed = default;
//
//         if (sequence.Length < 8)
//             return false;
//
//         var length = GetLength(sequence);
//         if (length > sequence.Length)
//             return false;
//
//         var payload = sequence.Slice(8, length);
//         response = AppResponse.Parser.ParseFrom(payload);
//
//         consumed = sequence.GetPosition(8 + length);
//         return true;
//     }
//     
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static void WriteResponseHeader(IBufferWriter<byte> pipeWriter, AppResponse response)
//     {
//         var length = response.CalculateSize();
//         Span<byte> lengthBuffer = stackalloc byte[8];
//         BinaryPrimitives.WriteInt64LittleEndian(lengthBuffer, length);
//         pipeWriter.Write(lengthBuffer);
//         response.WriteTo(pipeWriter);
//     }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static void WriteRequestHeader(IBufferWriter<byte> pipeWriter, AppRequest request)
//     {
//         var length = request.CalculateSize();
//         Span<byte> lengthBuffer = stackalloc byte[8];
//         BinaryPrimitives.WriteInt64LittleEndian(lengthBuffer, length);
//         pipeWriter.Write(lengthBuffer);
//         request.WriteTo(pipeWriter);
//     }
// }