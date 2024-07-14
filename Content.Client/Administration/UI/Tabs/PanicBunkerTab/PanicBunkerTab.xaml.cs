using Content.Corvax.Interfaces.Shared;
using Content.Shared.Administration.Events;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Console;

namespace Content.Client.Administration.UI.Tabs.PanicBunkerTab;

[GenerateTypedNameReferences]
public sealed partial class PanicBunkerTab : Control
{
    [Dependency] private readonly IConsoleHost _console = default!;

    private string _minAccountAge;
    private string _minOverallMinutes;

    public PanicBunkerTab()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        DisableAutomaticallyButton.ToolTip = Loc.GetString("admin-ui-panic-bunker-disable-automatically-tooltip");

        MinAccountAge.OnTextEntered += args => SendMinAccountAge(args.Text);
        MinAccountAge.OnFocusExit += args => SendMinAccountAge(args.Text);
        _minAccountAge = MinAccountAge.Text;

        MinOverallMinutes.OnTextEntered += args => SendMinOverallMinutes(args.Text);
        MinOverallMinutes.OnFocusExit += args => SendMinOverallMinutes(args.Text);
        _minOverallMinutes = MinOverallMinutes.Text;
<<<<<<< HEAD
        // Corvax-VPNGuard-Start
        var haveSecrets = IoCManager.Instance!.TryResolveType<ISharedSponsorsManager>(out _); // TODO: Probably need better way to detect Secrets module
        if (haveSecrets)
        {
            VPNContainer.Visible = true;
            DenyVPN.OnPressed += _ => SendDenyVpn(DenyVPN.Pressed);
        }
        // Corvax-VPNGuard-End
=======
>>>>>>> 6a78497d1aaf949d64f9a9ab519e2c2c309d92fa
    }

    private void SendMinAccountAge(string text)
    {
        if (string.IsNullOrWhiteSpace(text) ||
            text == _minAccountAge ||
            !int.TryParse(text, out var minutes))
        {
            return;
        }

        _console.ExecuteCommand($"panicbunker_min_account_age {minutes}");
    }

    private void SendMinOverallMinutes(string text)
    {
        if (string.IsNullOrWhiteSpace(text) ||
            text == _minOverallMinutes ||
            !int.TryParse(text, out var minutes))
        {
            return;
        }

        _console.ExecuteCommand($"panicbunker_min_overall_minutes {minutes}");
    }

    // Corvax-VPNGuard-Start
    private void SendDenyVpn(bool deny)
    {
        _console.ExecuteCommand($"panicbunker_deny_vpn {deny}");
    }
    // Corvax-VPNGuard-End

    public void UpdateStatus(PanicBunkerStatus status)
    {
        EnabledButton.Pressed = status.Enabled;
        EnabledButton.Text = Loc.GetString(status.Enabled
            ? "admin-ui-panic-bunker-enabled"
            : "admin-ui-panic-bunker-disabled"
        );
        EnabledButton.ModulateSelfOverride = status.Enabled ? Color.Red : null;

        DisableAutomaticallyButton.Pressed = status.DisableWithAdmins;
        EnableAutomaticallyButton.Pressed = status.EnableWithoutAdmins;
        CountDeadminnedButton.Pressed = status.CountDeadminnedAdmins;
        ShowReasonButton.Pressed = status.ShowReason;

        MinAccountAge.Text = status.MinAccountAgeMinutes.ToString();
        _minAccountAge = MinAccountAge.Text;

        MinOverallMinutes.Text = status.MinOverallMinutes.ToString();
        _minOverallMinutes = MinOverallMinutes.Text;
<<<<<<< HEAD
        DenyVPN.Pressed = status.DenyVpn; // Corvax-VPNGuard
=======
>>>>>>> 6a78497d1aaf949d64f9a9ab519e2c2c309d92fa
    }
}
