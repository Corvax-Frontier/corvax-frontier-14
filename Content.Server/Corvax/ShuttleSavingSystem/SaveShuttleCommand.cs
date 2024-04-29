using System.IO;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Content.Shared.Players;
using NetSerializer;
using Robust.Shared.Console;
using Robust.Shared.Map;

namespace Content.Server.Corvax.ShuttleSavingSystem;

[AdminCommand(AdminFlags.Debug)]
public sealed class SaveShuttleCommand : IConsoleCommand
{
    [Dependency] private readonly EntityManager _entity = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tile = default!;

    public string Command => "saveshuttle";

    public string Description => "Saves shuttle.";

    public string Help => $"Usage: {Command}";

    public void Execute(IConsoleShell shell, string arg, string[] args)
    {
        if (shell.Player is null)
            return;

        if (!_entity.TryGetComponent<MindComponent>(shell.Player.GetMind(), out var mind))
            return;

        if (!_entity.TryGetComponent<TransformComponent>(mind.CurrentEntity, out var xform))
            return;

        if (xform.GridUid is null)
            return;

        using FileStream stream = new("shuttle.sht", FileMode.Create, FileAccess.Write);

        Serializer serializer = new();

        foreach (var component in _entity.GetComponents(xform.GridUid.Value))
        {
            serializer.Serialize(stream, component);
        }
    }
}
