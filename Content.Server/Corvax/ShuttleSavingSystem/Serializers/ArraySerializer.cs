using System.IO;

namespace Content.Server.Corvax.ShuttleSavingSystem.Serializers;

public static class ArraySerializer
{
    public static void Serialize<T>(Stream stream, T[] array)
    {
        Span<byte> length = stackalloc byte[sizeof(int)];

        BitConverter.TryWriteBytes(length, array.Length);

        stream.Write(length);

        foreach (var obj in array)
            Serializer.Serialize(stream, obj);
    }

    public static T[] Deserialize<T>(Stream stream)
    {
        Span<byte> length = stackalloc byte[sizeof(int)];

        stream.ReadExactly(length);

        var array = new T[BitConverter.ToInt32(length)];

        for (var i = 0; i < array.Length; i++)
            array[i] = (T) Serializer.Deserialize(stream);

        return array;
    }
}
