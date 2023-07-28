using MonoGame.Extended.Collections;
using Nekres.ProofLogix.Core.Services;
using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.UI.Configs {
    public class TableConfig : ConfigBase {

        public enum Column {
            Timestamp,
            Class,
            Character,
            Account
        }

        private int _selectedColumn;
        [JsonProperty("selected_column")]
        public int SelectedColumn {
            get => _selectedColumn;
            set {
                _selectedColumn = value;
                this.SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private ObservableCollection<int> _tokenIds;
        [JsonProperty("token_ids")]
        public ObservableCollection<int> TokenIds {
            get => _tokenIds;
            set {
                _tokenIds             =  value ?? new ObservableCollection<int>();
                _tokenIds.ItemRemoved += OnAddOrRemove;
                _tokenIds.ItemAdded   += OnAddOrRemove;
                this.SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private bool _orderDescending;
        [JsonProperty("order_descending")]
        public bool OrderDescending {
            get => _orderDescending;
            set {
                _orderDescending = value;
                this.SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private PartySyncService.ColorGradingMode _colorGradingMode;
        [JsonProperty("color_grading_mode")]
        public PartySyncService.ColorGradingMode ColorGradingMode {
            get => _colorGradingMode;
            set {
                _colorGradingMode = value;
                this.SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private ObservableCollection<string> _profileIds;
        [JsonProperty("profile_ids")]
        public ObservableCollection<string> ProfileIds {
            get => _profileIds;
            set {
                _profileIds             =  value ?? new ObservableCollection<string>();
                _profileIds.ItemRemoved += OnAddOrRemove;
                _profileIds.ItemAdded   += OnAddOrRemove;
                this.SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private ObservableCollection<Column> _columns;
        [JsonProperty("columns")]
        public ObservableCollection<Column> Columns {
            get => _columns;
            set {
                _columns             =  value ?? new ObservableCollection<Column>();
                _columns.ItemRemoved += OnAddOrRemove;
                _columns.ItemAdded   += OnAddOrRemove;
                this.SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        public TableConfig() {
            ColorGradingMode = PartySyncService.ColorGradingMode.MedianComparison;
            ProfileIds = new ObservableCollection<string>();
            TokenIds = new ObservableCollection<int> {
                77302,
                94020,
                93781
            };
            Columns = new ObservableCollection<Column> {
                Column.Timestamp,
                Column.Class,
                Column.Character,
                Column.Account
            };
        }

        private void OnAddOrRemove<T>(object o, ItemEventArgs<T> e) {
            this.SaveConfig(ProofLogix.Instance.TableConfig);
        }
    }
}
