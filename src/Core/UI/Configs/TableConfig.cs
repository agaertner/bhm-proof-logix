using MonoGame.Extended.Collections;
using Newtonsoft.Json;

namespace Nekres.ProofLogix.Core.UI.Configs {
    public class TableConfig : ConfigBase {

        private int _selectedColumn;
        [JsonProperty("selected_column")]
        public int SelectedColumn {
            get => _selectedColumn;
            set {
                _selectedColumn                       = value;
                this.SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private ObservableCollection<int> _tokenIds;

        [JsonProperty("token_ids")]
        public ObservableCollection<int> TokenIds {
            get => _tokenIds;
            set {
                if (_tokenIds != null) {
                    _tokenIds.ItemRemoved -= OnAddOrRemove;
                    _tokenIds.ItemAdded   -= OnAddOrRemove;
                }
                _tokenIds             =  value;
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
                _orderDescending                      = value;
                this.SaveConfig(ProofLogix.Instance.TableConfig);
            }
        }

        private void OnAddOrRemove(object o, ItemEventArgs<int> e) {
            this.SaveConfig(ProofLogix.Instance.TableConfig);
        }
    }
}
