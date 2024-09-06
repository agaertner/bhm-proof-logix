using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.ProofLogix.Core.Services.PartySync.Models;
using Nekres.ProofLogix.Core.UI.Configs;
using Nekres.ProofLogix.Core.UI.KpProfile;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Timers;

namespace Nekres.ProofLogix.Core.UI.Table {
    public class TablePresenter : Presenter<TableView, TableConfig> {
        private readonly Timer _bulkLoadTimer;
        private const    int BULKLOAD_INTERVAL = 1000;
        private readonly ConcurrentDictionary<string, TablePlayerEntry> _bulk;

        public TablePresenter(TableView view, TableConfig model) : base(view, model) {
            _bulk                  =  new ConcurrentDictionary<string, TablePlayerEntry>(Environment.ProcessorCount * 2, this.Model.MaxPlayerCount);
            _bulkLoadTimer         =  new Timer(BULKLOAD_INTERVAL) { AutoReset = false };
            _bulkLoadTimer.Elapsed += OnBulkLoadTimerElapsed;

            ProofLogix.Instance.PartySync.PlayerAdded   += PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerChanged += PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerRemoved += PlayerRemoved;
        }

        private void OnBulkLoadTimerElapsed(object sender, ElapsedEventArgs e) {
            var table = this.View.Table;
            if (table == null) {
                return;
            }

            // Bulk assign children to container.
            // Prepare sorted control collection.
            var bulk = _bulk.Values.ToList();

            var toDisplay = bulk.Where(x => x.Remember || x.Player.Equals(ProofLogix.Instance.PartySync.LocalPlayer)).ToList();
            toDisplay.AddRange(bulk.Except(toDisplay).OrderByDescending(x => x.Player.Created).Take(Math.Abs(this.Model.MaxPlayerCount - 1)));
            toDisplay.Sort(Comparer);

            var list = new ControlCollection<Control>(toDisplay);

            // Assign parent on each child since AddChild (which would assign it) is skipped below.
            foreach (var item in list) {
                // Skip the public setter because it would add to the parent's children individually.
                item.GetPrivateField("_parent").SetValue(item, table);
            }
            
            // Overwrite the old list of children as a whole.
            table.GetPrivateField("_children").SetValue(table, list);

            table.Invalidate();

            this.View.PlayerCountLbl.Text = $"{toDisplay.Count}/{this.Model.MaxPlayerCount}";

            if (toDisplay.Count > this.Model.MaxPlayerCount) {
                this.View.PlayerCountLbl.TextColor = new(255, 57, 57);
            } else if (toDisplay.Count == this.Model.MaxPlayerCount) {
                this.View.PlayerCountLbl.TextColor = new(128, 255, 128);
            } else {
                this.View.PlayerCountLbl.TextColor = Color.White;
            }
        }

        private void ResetBulkLoadTimer() {
            _bulkLoadTimer.Stop();
            _bulkLoadTimer.Interval = BULKLOAD_INTERVAL;
            _bulkLoadTimer.Start();
        }

        public void CreatePlayerEntry(Player player) {
            if (this.Model.RequireProfile && !player.HasKpProfile) {
                return;
            }

            if (TryGetPlayerEntry(player, out var playerEntry)) {
                playerEntry.Player = player; // Reassign just in case it's a new player.
                return;
            }

            ResetBulkLoadTimer();

            var entry = new TablePlayerEntry(player) {
                Height = 32,
                Remember = this.Model.ProfileIds.Any(id => id.Equals(player.KpProfile.Id))
                        || player.Equals(ProofLogix.Instance.PartySync.LocalPlayer)
            };
            
            _bulk[player.AccountName] = entry;

            entry.LeftMouseButtonReleased += (_, _) => {
                if (entry.Player.KpProfile.NotFound) {
                    ScreenNotification.ShowNotification("This player has no profile.");
                    return;
                }
                ProfileView.Open(entry.Player.KpProfile);
            };

            entry.RightMouseButtonReleased += (_, _) => {
                if (player.Equals(ProofLogix.Instance.PartySync.LocalPlayer) || player.KpProfile.NotFound) {
                    return;
                }
                if (entry.Remember) {
                    GameService.Content.PlaySoundEffectByName("button-click");
                    this.Model.ProfileIds.RemoveAll(entry.Player.KpProfile.Id);
                } else {
                    GameService.Content.PlaySoundEffectByName("color-change");
                    this.Model.ProfileIds.Add(entry.Player.KpProfile.Id);
                }
                entry.Remember = !entry.Remember;
            };
        }

        private void PlayerRemoved(object sender, ValueEventArgs<Player> e) {
            if (!TryGetPlayerEntry(e.Value, out var playerEntry)) {
                return;
            }

            if (playerEntry.Remember) {
                return; // Don't remove remembered entries.
            }

            ResetBulkLoadTimer();

            if (_bulk.TryRemove(playerEntry.Player.AccountName, out _)) {
                playerEntry.Dispose();
            }
        }

        private void PlayerAddedOrChanged(object sender, ValueEventArgs<Player> e) {
            CreatePlayerEntry(e.Value);
        }

        private bool TryGetPlayerEntry(Player player, out TablePlayerEntry playerEntry) {
            return _bulk.TryGetValue(player.AccountName, out playerEntry);
        }

        private int Comparer(TablePlayerEntry x, TablePlayerEntry y) {
            if (this.Model.AlwaysSortStatus) {
                var status = x.Player.Status.CompareTo(y.Player.Status);

                // Sort by online status
                if (status != 0) {
                    if (x.Player.Status == Player.OnlineStatus.Unknown) {
                        return 1;
                    }
                    if (y.Player.Status == Player.OnlineStatus.Unknown) {
                        return -1;
                    }
                    if (x.Player.Status == Player.OnlineStatus.Online && y.Player.Status == Player.OnlineStatus.Away) {
                        return -1;
                    }
                    if (x.Player.Status == Player.OnlineStatus.Away && y.Player.Status == Player.OnlineStatus.Online) {
                        return 1;
                    }
                }
            }

            // Sort by selected column
            var column = this.Model.SelectedColumn;
            var comparison = 0;

            if (column == (int)TableConfig.Column.Status) {
                comparison = x.Player.Status.CompareTo(y.Player.Status);
            }

            if (column == (int)TableConfig.Column.Timestamp) {
                comparison = x.Player.Created.CompareTo(y.Player.Created);
            }

            if (column == (int)TableConfig.Column.Class) {
                comparison = string.Compare(x.Player.Class, y.Player.Class, StringComparison.InvariantCultureIgnoreCase);
            }

            if (column == (int)TableConfig.Column.Character) {
                comparison = string.Compare(x.Player.CharacterName, y.Player.CharacterName, StringComparison.InvariantCultureIgnoreCase);
            }

            if (column == (int)TableConfig.Column.Account) {
                comparison = string.Compare(x.Player.AccountName, y.Player.AccountName, StringComparison.InvariantCultureIgnoreCase);
            }

            // All trailing columns are known to be tokens.
            var len = Enum.GetValues(typeof(TableConfig.Column)).Length;
            if (column >= len) {
                var id = ProofLogix.Instance.TableConfig.Value.TokenIds.ElementAtOrDefault(column - len);
                comparison = x.Player.KpProfile.GetToken(id).Amount.CompareTo(y.Player.KpProfile.GetToken(id).Amount);
            }
            return this.Model.OrderDescending ? comparison : -comparison;
        }

        protected override void Unload() {
            ProofLogix.Instance.PartySync.PlayerAdded   -= PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerChanged -= PlayerAddedOrChanged;
            ProofLogix.Instance.PartySync.PlayerRemoved -= PlayerRemoved;

            _bulkLoadTimer.Dispose();

            foreach (var ctrl in _bulk.Values) {
                ctrl?.Dispose();
            }
            base.Unload();
        }

        public void SortEntries() {
            this.View.Table.SortChildren<TablePlayerEntry>(Comparer);
        }
    }
}
