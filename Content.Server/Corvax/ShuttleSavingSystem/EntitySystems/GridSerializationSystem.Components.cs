using System.IO;
using Content.Server.Corvax.ShuttleSavingSystem.Serializers;
using Content.Shared.Stacks;

namespace Content.Server.Corvax.ShuttleSavingSystem.EntitySystems;

public sealed partial class GridSerializationSystem
{
    private delegate void ComponentSerializer<T>(Stream stream, T component);

    private delegate void ComponentDeserializer<T>(Stream stream, T component);

    private readonly Dictionary<Type, ComponentSerializer<IComponent>> _serializers = [];

    private readonly Dictionary<Type, ComponentDeserializer<IComponent>> _deserializers = [];

    private void InitializeComponents()
    {
        AddSerializer<StackComponent>(SerializeStackComponent);
        AddDeserializer<StackComponent>(DeserializeStackComponent);
    }

    private void AddSerializer<T>(ComponentSerializer<T> serializer)
    {
        _serializers.Add(typeof(T), (stream, component) => serializer(stream, (T) component));
    }

    private void AddDeserializer<T>(ComponentDeserializer<T> deserializer)
    {
        _deserializers.Add(typeof(T), (stream, component) => deserializer(stream, (T) component));
    }

    private void SerializeComponents(Stream stream, EntityUid entity)
    {
        foreach (var component in AllComps(entity))
        {
            var type = component.GetType();

            if (!_serializers.TryGetValue(type, out var serializer))
                continue;

            StringSerializer.Serialize(stream, type.AssemblyQualifiedName!);

            serializer(stream, component);
        }

        StringSerializer.Serialize(stream, "");
    }

    private void DeserializeComponents(Stream stream, EntityUid entity)
    {
        while (true)
        {
            var componentType = StringSerializer.Deserialize(stream);

            if (componentType == "")
                return;

            var type = Type.GetType(componentType)!;

            var component = EntityManager.GetComponent(entity, type);

            _deserializers[type](stream, component);

            Dirty(entity, component);
        }
    }

    private void SerializeStackComponent(Stream stream, StackComponent component)
    {
        UnmanagedSerializer.Serialize(stream, component.Count);
    }

    private void DeserializeStackComponent(Stream stream, StackComponent component)
    {
        component.Count = UnmanagedSerializer.Deserialize<int>(stream);
    }
}
