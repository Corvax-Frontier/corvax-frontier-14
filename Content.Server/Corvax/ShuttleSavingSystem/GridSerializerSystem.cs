using System.IO;
using System.Linq;
using System.Text.Json;
using Content.Server.Corvax.ShuttleSavingSystem.Serializers;

namespace Content.Server.Corvax.ShuttleSavingSystem;

public sealed class GridSerializerSystem : EntitySystem
{
    public void Serialize(Stream stream, EntityUid grid)
    {
        var query = AllEntityQuery<TransformComponent>();

        while (query.MoveNext(out var entity, out var transform))
        {
            if (transform.GridUid != grid)
                continue;

            var components = EntityManager.GetComponents(entity).ToList();

            UnmanagedSerializer.Serialize(stream, components.Count);

            foreach (var component in components)
                JsonSerializer.Serialize(stream, component);
        }
    }

    public EntityUid Deserialize(Stream stream)
    {
        
    }
}
