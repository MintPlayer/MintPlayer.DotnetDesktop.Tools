using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Bacon.Core.Extensions;

internal static class StreamExtensions
{
    internal static async Task WriteBytesAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        var mem = buffer.AsMemory(offset, count);
        await stream.WriteAsync(mem, cancellationToken);
    }

    internal static async Task WriteNumberAsync<T>(this Stream stream, T number, int byteCount, CancellationToken cancellationToken = default)
        where T : struct, INumber<T>
    {
        var readOnlySpan = MemoryMarshal.CreateReadOnlySpan(ref number, length: byteCount);
        var bytes = MemoryMarshal.AsBytes(readOnlySpan);

        await stream.WriteBytesAsync(bytes, 0, byteCount, cancellationToken);
    }


    internal static async Task WriteNumberAsync<T>(this Stream stream, T number, CancellationToken cancellationToken = default)
        where T : struct, INumber<T>
    {
        var byteCount = sizeof(T);
        await stream.WriteNumberAsync(number, byteCount, cancellationToken);
    }
}