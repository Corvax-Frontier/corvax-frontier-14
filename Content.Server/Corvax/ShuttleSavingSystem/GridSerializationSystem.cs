using System.IO;
using System.Linq;
using System.Text.Json;
using Content.Server.Corvax.ShuttleSavingSystem.Serializers;

namespace Content.Server.Corvax.ShuttleSavingSystem;

public sealed class GridSerializationSystem : EntitySystem
{
    public void Serialize(Stream stream, EntityUid grid)
    {
        var components = EntityManager.GetComponents(grid).ToList();

        UnmanagedSerializer.Serialize(stream, components.Count);

        MemoryStream memory = new();

        foreach (var component in components)
        {
            StringSerializer.Serialize(stream, component.GetType().AssemblyQualifiedName!);
            JsonSerializer.Serialize(memory, component, component.GetType());
            UnmanagedSerializer.Serialize(stream, memory.Length);
            memory.CopyTo(stream);
            memory.SetLength(0);
        }

        /*var query = AllEntityQuery<TransformComponent>();

        while (query.MoveNext(out var entity, out var transform))
        {
            if (transform.GridUid != grid)
                continue;

            var components = EntityManager.GetComponents(entity).ToList();

            UnmanagedSerializer.Serialize(stream, components.Count);

            foreach (var component in components)
                JsonSerializer.Serialize(stream, component);
        }*/
    }

    public EntityUid Deserialize(Stream stream)
    {
        var grid = EntityManager.CreateEntityUninitialized(null);

        var count = UnmanagedSerializer.Deserialize<int>(stream);

        MemoryStream memory = new();

        for (var i = 0; i < count; i++)
        {
            var type = Type.GetType(StringSerializer.Deserialize(stream));

            var buffer = new byte[UnmanagedSerializer.Deserialize<long>(stream)];

            stream.ReadExactly(buffer);

            var component = (Component) JsonSerializer.Deserialize(buffer, type!)!;

            EntityManager.AddComponent(grid, component);
        }

        return grid;
    }
}
