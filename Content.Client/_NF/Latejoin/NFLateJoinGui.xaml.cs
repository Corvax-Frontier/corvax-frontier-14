using Content.Client.CrewManifest;
using Content.Client.GameTicking.Managers;
using Content.Client.Players.PlayTimeTracking;
using Content.Client.UserInterface.Controls;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._NF.Latejoin;

[GenerateTypedNameReferences]
public sealed partial class NFLateJoinGui : FancyWindow
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly JobRequirementsManager _playManager = default!;

    private ClientGameTicker _gameTicker;

    private NetEntity _lastSelection;

    private readonly Dictionary<string, NewFrontierLateJoinJobButton> _buttons = new();

    public NFLateJoinGui()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        _gameTicker = EntitySystem.Get<ClientGameTicker>();
        _gameTicker.LobbyJobsAvailableUpdated += UpdateUi;
        VesselSelection.VesselItemList.OnItemSelected += args =>
        {
            UpdateUi(_gameTicker.JobsAvailable);
        };
        CrewManifestButton.OnPressed += _ =>
        {
            EntitySystem.Get<CrewManifestSystem>().RequestCrewManifest(_lastSelection);
        };

        _lastSelection = _gameTicker.JobsAvailable.Keys.FirstOrNull() ?? NetEntity.Invalid;

        UpdateUi(_gameTicker.JobsAvailable);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _gameTicker.LobbyJobsAvailableUpdated -= UpdateUi;
    }

    public void UpdateUi(IReadOnlyDictionary<NetEntity, Dictionary<ProtoId<JobPrototype>, int?>> obj)
    {
        if (VesselSelection.Selected is null)
        {
            CrewManifestButton.Visible = false;
            return;
        }

        CrewManifestButton.Visible = true;

        var station = VesselSelection.Selected.Value;
        var jobs = obj[station];

        if (station != _lastSelection)
        {
            foreach (var (_, button) in _buttons)
            {
                JobList.RemoveChild(button);
            }
            _buttons.Clear();
        }

        _lastSelection = station;

        foreach (var (jobId, _) in jobs)
        {
            if (_buttons.ContainsKey(jobId))
                continue;

            var job = _prototypeManager.Index<JobPrototype>(jobId);



            var newButton = new NewFrontierLateJoinJobButton(station, jobId, _gameTicker, _prototypeManager);
            newButton.OnPressed += args =>
            {
                Logger.InfoS("latejoin", $"Late joining as ID: {jobId}");
                _consoleHost.ExecuteCommand($"joingame {CommandParsing.Escape(jobId)} {station}");
                Close();
            };

            if (!_playManager.IsAllowed(job, out var denyReason))
            {
                newButton.Disabled = true;
                newButton.ToolTip = denyReason.ToString();
            }

            JobList.AddChild(newButton);

            _buttons.Add(jobId, newButton);
        }

    }
}
