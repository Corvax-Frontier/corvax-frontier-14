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

        foreach (var component in components)
            JsonSerializer.Serialize(stream, component);

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

        for (var i = 0; i < count; i++)
        {
            var component = JsonSerializer.Deserialize<Component>(stream);

            EntityManager.AddComponent(grid, component!);
        }

        return grid;
    }
}
