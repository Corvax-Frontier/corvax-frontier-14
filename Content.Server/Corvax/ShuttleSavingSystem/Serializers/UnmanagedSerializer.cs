using System.IO;
using System.Runtime.InteropServices;

namespace Content.Server.Corvax.ShuttleSavingSystem.Serializers;

public static class UnmanagedSerializer
{
    public static void Serialize<T>(Stream stream, T obj) where T : unmanaged
    {
        stream.Write(MemoryMarshal.Cast<T, byte>(stackalloc T[] { obj }));
    }

    public static T Deserialize<T>(Stream stream) where T : unmanaged
    {
        Span<T> span = stackalloc T[1];

        stream.ReadExactly(MemoryMarshal.Cast<T, byte>(span));

        return span[0];
    }
}
