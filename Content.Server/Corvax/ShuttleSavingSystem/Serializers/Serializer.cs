using System.IO;

namespace Content.Server.Corvax.ShuttleSavingSystem.Serializers;

public static class Serializer
{
    public static void Serialize(Stream stream, object? obj)
    {
        if (obj is null)
        {
            StringSerializer.Serialize(stream, "");
            return;
        }

        var type = obj.GetType();

        StringSerializer.Serialize(stream, type.FullName!);

        if(type.)
    }

    public static object Deserialize(Stream stream)
    {
        
    }
}
