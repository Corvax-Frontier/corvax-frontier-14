using System.IO;
using System.Text;

namespace Content.Server.Corvax.ShuttleSavingSystem.Serializers;

public static class StringSerializer
{
    public static void Serialize(Stream stream, string str)
    {
        Span<byte> span = stackalloc byte[sizeof(int)];

        var length = Encoding.UTF8.GetByteCount(str);

        BitConverter.TryWriteBytes(span, length);

        stream.Write(span);

        span = stackalloc byte[length];

        Encoding.UTF8.GetBytes(str, span);

        stream.Write(span);
    }

    public static string Deserialize(Stream stream)
    {
        Span<byte> span = stackalloc byte[sizeof(int)];

        stream.ReadExactly(span);

        var length = BitConverter.ToInt32(span);

        span = stackalloc byte[length];

        stream.ReadExactly(span);

        return Encoding.UTF8.GetString(span);
    }
}
