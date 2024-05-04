using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Content.Server.Corvax.ShuttleSavingSystem.Serializers;

public static class Serializer
{
    private static readonly MethodInfo UnmanagedSerialize = typeof(UnmanagedSerializer).GetMethod(nameof(UnmanagedSerializer.Serialize))!;
    private static readonly MethodInfo UnmanagedDeserialize = typeof(UnmanagedSerializer).GetMethod(nameof(UnmanagedSerializer.Deserialize))!;

    public static void Serialize(Stream stream, object? obj)
    {
        if (obj is null)
        {
            StringSerializer.Serialize(stream, "");
            return;
        }

        var type = obj.GetType();

        StringSerializer.Serialize(stream, type.AssemblyQualifiedName!);

        if (type.IsPrimitive)
            UnmanagedSerialize.MakeGenericMethod(type).Invoke(null, [stream, obj]);
        else if (obj is string str)
            StringSerializer.Serialize(stream, str);
        else if (obj is Array array)
            ArraySerializer.Serialize(stream, array);
        else
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            UnmanagedSerializer.Serialize(stream, fields.Length);

            foreach (var field in fields)
            {
                StringSerializer.Serialize(stream, field.Name);
                Serialize(stream, field.GetValue(obj));
            }
        }
    }

    public static object? Deserialize(Stream stream)
    {
        var name = StringSerializer.Deserialize(stream);

        if (name == "")
            return null;

        var type = Type.GetType(name)!;

        if (type.IsPrimitive)
            return UnmanagedDeserialize.MakeGenericMethod(type).Invoke(null, [stream]);
        if (type == typeof(string))
            return StringSerializer.Deserialize(stream);
        if (type.IsAssignableTo(typeof(Array)))
            return ArraySerializer.Deserialize(stream, type);

        var obj = RuntimeHelpers.GetUninitializedObject(type);

        for (var i = UnmanagedSerializer.Deserialize<int>(stream); i > 0; i--)
        {
            var field = StringSerializer.Deserialize(stream);

            type.GetField(field)!.SetValue(obj, Deserialize(stream));
        }

        return obj;
    }
}
