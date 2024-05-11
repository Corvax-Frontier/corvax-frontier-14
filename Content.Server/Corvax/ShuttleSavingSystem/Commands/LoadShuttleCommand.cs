using System.IO;
using Content.Server.Administration;
using Content.Server.Corvax.ShuttleSavingSystem.EntitySystems;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Corvax.ShuttleSavingSystem.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class LoadShuttleCommand : IConsoleCommand
{
    [Dependency] private readonly EntityManager _entity = default!;
    [Dependency] private readonly IEntitySystemManager _manager = default!;

    public string Command => "loadshuttle";

    public string Description => "Loads shuttle.";

    public string Help => $"Usage: {Command}";

    public void Execute(IConsoleShell shell, string arg, string[] args)
    {
        if (!_entity.TryGetComponent<TransformComponent>(shell.Player?.AttachedEntity, out var transform))
            return;

        using FileStream stream = new("shuttle.sht", FileMode.Open, FileAccess.Read);

        var time = System.Diagnostics.Stopwatch.GetTimestamp();

        var grid = _manager.GetEntitySystem<GridSerializationSystem>().Deserialize(stream, transform.MapID);

        shell.WriteLine(((double) (System.Diagnostics.Stopwatch.GetTimestamp() - time) / System.Diagnostics.Stopwatch.Frequency).ToString());

        _manager.GetEntitySystem<SharedTransformSystem>().SetCoordinates(grid, transform.Coordinates);

        //_entity.InitializeAndStartEntity(grid);
    }
}
