using System.IO;

namespace Content.Server.Corvax.ShuttleSavingSystem.Serializers;

public static class ArraySerializer
{
    public static void Serialize(Stream stream, Array array)
    {
        Span<byte> length = stackalloc byte[sizeof(int)];

        BitConverter.TryWriteBytes(length, array.Length);

        stream.Write(length);

        foreach (var obj in array)
            Serializer.Serialize(stream, obj);
    }

    public static Array Deserialize(Stream stream, Type type)
    {
        Span<byte> length = stackalloc byte[sizeof(int)];

        stream.ReadExactly(length);

        var array = Array.CreateInstance(type.GetElementType()!, BitConverter.ToInt32(length));

        for (var i = 0; i < array.Length; i++)
            array.SetValue(Serializer.Deserialize(stream), i);

        return array;
    }
}
