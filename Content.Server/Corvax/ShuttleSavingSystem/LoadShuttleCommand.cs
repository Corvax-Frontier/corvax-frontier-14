using System.IO;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Content.Shared.Players;
using Robust.Shared.Console;

namespace Content.Server.Corvax.ShuttleSavingSystem;

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
        if (shell.Player is null)
            return;

        if (!_entity.TryGetComponent<MindComponent>(shell.Player.GetMind(), out var mind))
            return;

        if (!_entity.TryGetComponent<TransformComponent>(mind.CurrentEntity, out var xform))
            return;

        using FileStream stream = new("shuttle.sht", FileMode.Open, FileAccess.Read);

        var grid = _manager.GetEntitySystem<GridSerializationSystem>().Deserialize(stream);

        _manager.GetEntitySystem<SharedTransformSystem>().SetCoordinates(grid, xform.Coordinates);

        _entity.InitializeAndStartEntity(grid);
    }
}
