using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Map;

namespace Content.Server.Corvax.ShuttleSavingSystem.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class SpawnShuttleCommand : IConsoleCommand
{
    [Dependency] private readonly EntityManager _entity = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tile = default!;

    public string Command => "spawnshuttle";

    public string Description => "Spawns debug shuttle.";

    public string Help => $"Usage: {Command}";

    public void Execute(IConsoleShell shell, string arg, string[] args)
    {
        if (!_entity.TryGetComponent<TransformComponent>(shell.Player?.AttachedEntity, out var transform))
            return;

        var grid = _mapManager.CreateGridEntity(transform.MapID);
        //var gridXform = _entity.GetComponent<TransformComponent>(grid);

        var transformSystem = _entity.System<SharedTransformSystem>();

        transformSystem.SetWorldPosition(grid, transformSystem.GetWorldPosition(transform));

        //_map.SetTile(grid, new Vector2i(0, 0), new(_tile["FloorSteel"].TileId));
        //_map.SetTile(grid, new Vector2i(), new(_tile["FloorSteel"].TileId));

        _entity.System<SharedMapSystem>().SetTiles(grid, [(new Vector2i(0, 0), new(_tile["FloorSteel"].TileId)), (new Vector2i(1, 0), new(_tile["FloorSteel"].TileId))]);
    }
}
