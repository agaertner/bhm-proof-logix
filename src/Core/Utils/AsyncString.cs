using System;

namespace Nekres.ProofLogix.Core {
    public class AsyncString {

        public event EventHandler<EventArgs> Changed;

        private string _string;
        public string String {
            get => _string;
            set {
                if (string.Equals(_string, value)) {
                    return;
                }

                _string = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public AsyncString() {
            _string = string.Empty;
        }

        public AsyncString(string str) {
            _string = str;
        }

        public bool Equals(string str) {
            return string.Equals(this, str);
        }

        public bool Equals(AsyncString str) {
            return string.Equals(this, str);
        }

        public static implicit operator string(AsyncString obj) {
            return obj.ToString();
        }

        public static implicit operator AsyncString(string obj) {
            return new AsyncString(obj);
        }

        public override string ToString() {
            return this.String;
        }
    }
}
