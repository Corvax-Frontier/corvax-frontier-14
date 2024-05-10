using System.IO;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Corvax.ShuttleSavingSystem;

[AdminCommand(AdminFlags.Debug)]
public sealed class SaveShuttleCommand : IConsoleCommand
{
    [Dependency] private readonly EntityManager _entity = default!;
    [Dependency] private readonly IEntitySystemManager _manager = default!;

    public string Command => "saveshuttle";

    public string Description => "Saves shuttle.";

    public string Help => $"Usage: {Command}";

    public void Execute(IConsoleShell shell, string arg, string[] args)
    {
        if (!_entity.TryGetComponent<TransformComponent>(shell.Player?.AttachedEntity, out var xform))
            return;

        if (xform.GridUid is null)
            return;

        using FileStream stream = new("shuttle.sht", FileMode.Create, FileAccess.Write);

        _manager.GetEntitySystem<GridSerializationSystem>().Serialize(stream, xform.GridUid.Value);
    }
}
