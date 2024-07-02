using System.Linq;
using Content.Client.Administration.Systems;
using Content.Client.UserInterface.Controls;
using Content.Shared.Administration;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;
using static Content.Client.Administration.UI.Tabs.PlayerTab.PlayerTabHeader;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.Administration.UI.Tabs.PlayerTab
{
    [GenerateTypedNameReferences]
    public sealed partial class PlayerTab : Control
    {
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly IPlayerManager _playerMan = default!;

        private const string ArrowUp = "↑";
        private const string ArrowDown = "↓";
        private readonly Color _altColor = Color.FromHex("#292B38");
        private readonly Color _defaultColor = Color.FromHex("#2F2F3B");
        private readonly AdminSystem _adminSystem;
        private IReadOnlyList<PlayerInfo> _players = new List<PlayerInfo>();

        private Header _headerClicked = Header.Username;
        private bool _ascending = true;
        private bool _showDisconnected;

        public event Action<GUIBoundKeyEventArgs, ListData>? OnEntryKeyBindDown;

        public PlayerTab()
        {
            IoCManager.InjectDependencies(this);
            RobustXamlLoader.Load(this);

            _adminSystem = _entManager.System<AdminSystem>();
            _adminSystem.PlayerListChanged += RefreshPlayerList;
            _adminSystem.OverlayEnabled += OverlayEnabled;
            _adminSystem.OverlayDisabled += OverlayDisabled;

            OverlayButton.OnPressed += OverlayButtonPressed;
            ShowDisconnectedButton.OnPressed += ShowDisconnectedPressed;

            ListHeader.BackgroundColorPanel.PanelOverride = new StyleBoxFlat(_altColor);
            ListHeader.OnHeaderClicked += HeaderClicked;

            SearchList.SearchBar = SearchLineEdit;
            SearchList.GenerateItem += GenerateButton;
            SearchList.DataFilterCondition += DataFilterCondition;
            SearchList.ItemKeyBindDown += (args, data) => OnEntryKeyBindDown?.Invoke(args, data);

            RefreshPlayerList(_adminSystem.PlayerList);

        }

        #region Antag Overlay

        private void OverlayEnabled()
        {
            OverlayButton.Pressed = true;
        }

        private void OverlayDisabled()
        {
            OverlayButton.Pressed = false;
        }

        private void OverlayButtonPressed(ButtonEventArgs args)
        {
            if (args.Button.Pressed)
            {
                _adminSystem.AdminOverlayOn();
            }
            else
            {
                _adminSystem.AdminOverlayOff();
            }
        }

        #endregion

        private void ShowDisconnectedPressed(ButtonEventArgs args)
        {
            _showDisconnected = args.Button.Pressed;
            RefreshPlayerList(_players);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _adminSystem.PlayerListChanged -= RefreshPlayerList;
                _adminSystem.OverlayEnabled -= OverlayEnabled;
                _adminSystem.OverlayDisabled -= OverlayDisabled;

                OverlayButton.OnPressed -= OverlayButtonPressed;

                ListHeader.OnHeaderClicked -= HeaderClicked;
            }
        }

        #region ListContainer

        private void RefreshPlayerList(IReadOnlyList<PlayerInfo> players)
        {
            _players = players;
            PlayerCount.Text = $"Players: {_playerMan.PlayerCount}";

            var filteredPlayers = players.Where(info => _showDisconnected || info.Connected).ToList();

            var sortedPlayers = new List<PlayerInfo>(filteredPlayers);
            sortedPlayers.Sort(Compare);

            UpdateHeaderSymbols();

            SearchList.PopulateList(sortedPlayers.Select(info => new PlayerListData(info,
                    $"{info.Username} {info.CharacterName} {info.IdentityName} {info.StartingJob}"))
                .ToList());
        }

        private void GenerateButton(ListData data, ListContainerButton button)
        {
            if (data is not PlayerListData { Info: var player})
                return;

            var entry = new PlayerTabEntry(player, new StyleBoxFlat(button.Index % 2 == 0 ? _altColor : _defaultColor));
            button.AddChild(entry);
            button.ToolTip = $"{player.Username}, {player.CharacterName}, {player.IdentityName}, {player.StartingJob}";
        }

        /// <summary>
        /// Determines whether <paramref name="filter"/> is contained in <paramref name="listData"/>.FilteringString.
        /// If all characters are lowercase, the comparison ignores case.
        /// If there is an uppercase character, the comparison is case sensitive.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="listData"></param>
        /// <returns>Whether <paramref name="filter"/> is contained in <paramref name="listData"/>.FilteringString.</returns>
        private bool DataFilterCondition(string filter, ListData listData)
        {
            if (listData is not PlayerListData {Info: var info, FilteringString: var playerString})
                return false;

            if (!_showDisconnected && !info.Connected)
                return false;

            if (IsAllLower(filter))
            {
                if (!playerString.Contains(filter, StringComparison.CurrentCultureIgnoreCase))
                    return false;
            }
            else
            {
                if (!playerString.Contains(filter))
                    return false;
            }

            return true;
        }

        private bool IsAllLower(string input)
        {
            foreach (var c in input)
            {
                if (char.IsLetter(c) && !char.IsLower(c))
                    return false;
            }

            return true;
        }

        #endregion

        #region Header

        private void UpdateHeaderSymbols()
        {
            ListHeader.ResetHeaderText();
            ListHeader.GetHeader(_headerClicked).Text += $" {(_ascending ? ArrowUp : ArrowDown)}";
        }

        private int Compare(PlayerInfo x, PlayerInfo y)
        {
            if (!_ascending)
            {
                (x, y) = (y, x);
            }

            return _headerClicked switch
            {
                Header.Username => Compare(x.Username, y.Username),
                Header.Character => Compare(x.CharacterName, y.CharacterName),
                Header.Job => Compare(x.StartingJob, y.StartingJob),
                Header.Antagonist => x.Antag.CompareTo(y.Antag),
                Header.Playtime => TimeSpan.Compare(x.OverallPlaytime ?? default, y.OverallPlaytime ?? default),
                Header.Balance => x.Balance.CompareTo(y.Balance),
                _ => 1
            }; ;
        }

        private int Compare(string x, string y)
        {
            return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
        }

        private void HeaderClicked(Header header)
        {
            if (_headerClicked == header)
            {
                _ascending = !_ascending;
            }
            else
            {
                _headerClicked = header;
                _ascending = true;
            }

            RefreshPlayerList(_adminSystem.PlayerList);
        }

        #endregion
    }

    public record PlayerListData(PlayerInfo Info, string FilteringString) : ListData;
}
