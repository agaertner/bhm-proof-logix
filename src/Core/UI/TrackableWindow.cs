using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;

namespace Nekres.ProofLogix.Core.UI {
    internal class TrackableWindow : StandardWindow {

        private static ConcurrentDictionary<string, TrackableWindow> _windows;

        public static bool TryGetById(string id, out TrackableWindow wnd) {
            ValidateDictionary();
            return _windows.TryGetValue(id, out wnd);
        }

        public static void Unset() {
            if (_windows == null) {
                return;
            }
            foreach (var wnd in _windows.Values) {
                wnd?.Hide();
            }
            _windows.Clear();
            _windows = null;
        }

        private static void ValidateDictionary() {
            _windows ??= new ConcurrentDictionary<string, TrackableWindow>();
        }

        private readonly string _trackId;

        public TrackableWindow(string id, AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion) {
            ValidateDictionary();
            _trackId = id ?? string.Empty;
            _windows.TryAdd(_trackId, this);
        }

        public TrackableWindow(string id, Texture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion) {
            ValidateDictionary();
            _trackId = id ?? string.Empty;
            _windows.TryAdd(_trackId, this);
        }

        public TrackableWindow(string id, AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Point windowSize) : base(background, windowRegion, contentRegion, windowSize) {
            ValidateDictionary();
            _trackId = id ?? string.Empty;
            _windows.TryAdd(_trackId, this);
        }

        public TrackableWindow(string id, Texture2D background, Rectangle windowRegion, Rectangle contentRegion, Point windowSize) : base(background, windowRegion, contentRegion, windowSize) {
            ValidateDictionary();
            _trackId = id ?? string.Empty;
            _windows.TryAdd(_trackId, this);
        }

        protected override void OnHidden(EventArgs e) {
            this.Dispose();
            base.OnHidden(e);
        }

        protected override void DisposeControl() {
            _windows?.TryRemove(_trackId ?? string.Empty, out _);
            base.DisposeControl();
        }
    }
}
