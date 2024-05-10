using System.IO;
using System.Linq;
using System.Numerics;
using Content.Server.Atmos.Components;
using Content.Server.Corvax.ShuttleSavingSystem.Serializers;
using Content.Shared.Atmos;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.Corvax.ShuttleSavingSystem;

public sealed class GridSerializationSystem : EntitySystem
{
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tile = default!;

    public void Serialize(Stream stream, EntityUid grid)
    {
        var mapGrid = EntityManager.GetComponent<MapGridComponent>(grid);

        var tiles = _map.GetAllTiles(grid, mapGrid).ToList();

        UnmanagedSerializer.Serialize(stream, tiles.Count);

        foreach (var tile in tiles)
        {
            StringSerializer.Serialize(stream, _tile[tile.Tile.TypeId].ID);
            UnmanagedSerializer.Serialize(stream, new Vector2i(tile.X, tile.Y));
        }

        var atmosphere = EntityManager.GetComponent<GridAtmosphereComponent>(grid);

        foreach (var tile in tiles)
        {
            var gas = atmosphere.Tiles[new(tile.X, tile.Y)].Air;

            if (gas is null)
            {
                UnmanagedSerializer.Serialize(stream, float.NaN);
                continue;
            }

            UnmanagedSerializer.Serialize(stream, gas.Temperature);

            for (var i = 0; i < Atmospherics.AdjustedNumberOfGases; i++)
                UnmanagedSerializer.Serialize(stream, gas[i]);
        }

        List<Entity<TransformComponent, MetaDataComponent>> entities = [];

        var query = AllEntityQuery<TransformComponent, MetaDataComponent>();

        while (query.MoveNext(out var entity, out var transform, out var meta))
            if (transform.GridUid == grid && meta.EntityPrototype is not null)
                entities.Add(new(entity, transform, meta));

        UnmanagedSerializer.Serialize(stream, entities.Count);

        foreach (var entity in entities)
        {
            StringSerializer.Serialize(stream, entity.Comp2.EntityPrototype!.ID);

            UnmanagedSerializer.Serialize(stream, entity.Comp1.Coordinates.Position);

            UnmanagedSerializer.Serialize(stream, entity.Comp1.LocalRotation);
        }
    }

    public EntityUid Deserialize(Stream stream, MapId id)
    {
        var grid = _mapManager.CreateGridEntity(id);

        List<(Vector2i, Tile)> tiles = new(UnmanagedSerializer.Deserialize<int>(stream));

        for (var i = 0; i < tiles.Capacity; i++)
        {
            var tile = _tile[StringSerializer.Deserialize(stream)];

            tiles.Add((UnmanagedSerializer.Deserialize<Vector2i>(stream), new(tile.TileId)));
        }

        _map.SetTiles(grid, tiles);

        var atmosphere = EntityManager.AddComponent<GridAtmosphereComponent>(grid);

        foreach ((var tile, _) in tiles)
        {
            var temperature = UnmanagedSerializer.Deserialize<float>(stream);

            if (float.IsNaN(temperature))
                continue;

            var moles = new float[Atmospherics.AdjustedNumberOfGases];

            for (var i = 0; i < moles.Length; i++)
                moles[i] = UnmanagedSerializer.Deserialize<float>(stream);

            var atmosphereTiles = atmosphere.Tiles;

            atmosphereTiles.Add(tile, new(grid, tile, new(moles, temperature)));
        }

        var count = UnmanagedSerializer.Deserialize<int>(stream);

        for (var i = 0; i < count; i++)
        {
            var entity = SpawnAtPosition(StringSerializer.Deserialize(stream), new(grid, UnmanagedSerializer.Deserialize<Vector2>(stream)));

            var transform = EntityManager.GetComponent<TransformComponent>(entity);

            transform.LocalRotation = UnmanagedSerializer.Deserialize<Angle>(stream);
        }

        return grid;
    }
}
