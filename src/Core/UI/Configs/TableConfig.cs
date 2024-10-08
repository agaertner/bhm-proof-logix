﻿using MonoGame.Extended.Collections;
using Nekres.ProofLogix.Core.Services;
using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.UI.Configs {
    public class TableConfig : ConfigBase {

        public static TableConfig Default => new() {
            _alwaysSortStatus = true,
            _colorGradingMode = PartySyncService.ColorGradingMode.MedianComparison,
            _maxPlayerCount   = 50,
            _requireProfile   = true,
            _profileIds       = new ObservableCollection<string>(),
            _tokenIds = new ObservableCollection<int> {
                77302,
                94020,
                93781
            },
            _columns = new ObservableCollection<Column> {
                Column.Status,
                Column.Timestamp,
                Column.Class,
                Column.Character,
                Column.Account
            }
        };

        public enum Column {
            Timestamp,
            Class,
            Character,
            Account,
            Status
        }

        private int _selectedColumn;
        [JsonProperty("selected_column")]
        public int SelectedColumn {
            get => _selectedColumn;
            set {
                _selectedColumn = value;
                SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private bool _orderDescending;
        [JsonProperty("order_descending")]
        public bool OrderDescending {
            get => _orderDescending;
            set {
                _orderDescending = value;
                SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private bool _keepLeavers;
        [JsonProperty("keep_leavers")]
        public bool KeepLeavers {
            get => _keepLeavers;
            set {
                _keepLeavers = value;
                SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private bool _alwaysSortStatus;
        [JsonProperty("always_sort_status")]
        public bool AlwaysSortStatus {
            get => _alwaysSortStatus;
            set {
                _alwaysSortStatus = value;
                SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private PartySyncService.ColorGradingMode _colorGradingMode;
        [JsonProperty("color_grading_mode")]
        public PartySyncService.ColorGradingMode ColorGradingMode {
            get => _colorGradingMode;
            set {
                _colorGradingMode = value;
                SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private int _maxPlayerCount = 50;
        [JsonProperty("max_player_count")]
        public int MaxPlayerCount {
            get => _maxPlayerCount;
            set {
                _maxPlayerCount = value;
                SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private bool _requireProfile = true;
        [JsonProperty("require_profile")]
        public bool RequireProfile {
            get => _requireProfile;
            set {
                _requireProfile = value;
                SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private ObservableCollection<int> _tokenIds = new();
        [JsonProperty("token_ids")]
        public ObservableCollection<int> TokenIds {
            get => _tokenIds;
            set => _tokenIds = ResetDelegates(_tokenIds, value);
        }

        private ObservableCollection<string> _profileIds = new();
        [JsonProperty("profile_ids")]
        public ObservableCollection<string> ProfileIds {
            get => _profileIds;
            set => _profileIds = ResetDelegates(_profileIds, value);
        }

        private ObservableCollection<Column> _columns = new();
        [JsonProperty("columns")]
        public ObservableCollection<Column> Columns {
            get => _columns;
            set => _columns = ResetDelegates(_columns, value);
        }

        private ObservableCollection<T> ResetDelegates<T>(ObservableCollection<T> oldCollection, ObservableCollection<T> newCollection) {
            if (oldCollection != null) {
                oldCollection.ItemRemoved -= OnAddOrRemove;
                oldCollection.ItemAdded   -= OnAddOrRemove;
            }
            newCollection             ??= new ObservableCollection<T>();
            newCollection.ItemRemoved +=  OnAddOrRemove;
            newCollection.ItemAdded   +=  OnAddOrRemove;
            return newCollection;
        }

        private void OnAddOrRemove<T>(object o, ItemEventArgs<T> e) {
            SaveConfig(ProofLogix.Instance.TableConfig);
        }
    }
}
