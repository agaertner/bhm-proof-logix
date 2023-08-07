using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Extended;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.ChatLinks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.ProofLogix.Core.Services.KpWebApi.V2.Models;
using Nekres.ProofLogix.Core.UI.Configs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.ProofLogix.Core.UI.SmartPing {

    public class SmartPingView : View {

        private const char BRACKET_LEFT  = '[';
        private const char BRACKET_RIGHT = ']';

        private SmartPingConfig _config;

        private AsyncTexture2D _cogWheelIcon;
        private AsyncTexture2D _cogWheelIconHover;
        private AsyncTexture2D _cogWheelIconClick;

        public SmartPingView(SmartPingConfig config) {
            _config            = config;
            _cogWheelIcon      = GameService.Content.DatAssetCache.GetTextureFromAssetId(155052);
            _cogWheelIconHover = GameService.Content.DatAssetCache.GetTextureFromAssetId(157110);
            _cogWheelIconClick = GameService.Content.DatAssetCache.GetTextureFromAssetId(157109);
        }

        protected override void Build(Container buildPanel) {

            var cogWheel = new Image(GameService.Content.DatAssetCache.GetTextureFromAssetId(155052)) {
                Parent = buildPanel,
                Width  = 32,
                Height = 32,
                Top    = (buildPanel.ContentRegion.Height - 32) / 2
        };

            cogWheel.MouseEntered += (_, _) => {
                cogWheel.Texture = _cogWheelIconHover;
            };

            cogWheel.MouseLeft += (_, _) => {
                cogWheel.Texture = _cogWheelIcon;
            };

            cogWheel.LeftMouseButtonPressed += (_, _) => {
                cogWheel.Texture = _cogWheelIconClick;
            };

            cogWheel.LeftMouseButtonReleased += (_, _) => {
                cogWheel.Texture = _cogWheelIconHover;
            };

            var labelPanel = new Panel {
                Parent     = buildPanel,
                Width = buildPanel.ContentRegion.Width - 64 - Panel.RIGHT_PADDING - Panel.LEFT_PADDING,
                Left = cogWheel.Right + Panel.RIGHT_PADDING,
                Height = buildPanel.ContentRegion.Height,
                ShowBorder = true
            };

            var quantity = ProofLogix.Instance.PartySync.LocalPlayer.KpProfile.GetToken(_config.SelectedToken).Amount;
            var lbl = BuildItemLabel(quantity, _config.SelectedToken);
            lbl.Parent = labelPanel;
            lbl.Top    = (labelPanel.ContentRegion.Height - lbl.Height) / 2;
            lbl.Left   = (labelPanel.ContentRegion.Width - lbl.Width)  / 2;

            var sendBttn = new Image(GameService.Content.DatAssetCache.GetTextureFromAssetId(784268)) {
                Parent        = buildPanel,
                Width         = 32,
                Height        = 32,
                Left          = labelPanel.Right + Panel.LEFT_PADDING,
                Top           = labelPanel.Top   + (labelPanel.Height - 32) / 2,
                SpriteEffects = SpriteEffects.FlipHorizontally,
                Tint          = Color.White * 0.8f,
                BasicTooltipText = "Send to Chat"
            };

            sendBttn.MouseEntered += (_, _) => {
                sendBttn.Tint = Color.White;
            };

            sendBttn.MouseLeft += (_, _) => {
                sendBttn.Tint = Color.White * 0.8f;
            };

            sendBttn.LeftMouseButtonPressed += (_, _) => {
                sendBttn.Tint = Color.White * 0.85f;
                sendBttn.Size = new Point(31, 31);
                sendBttn.Left = labelPanel.Right + Panel.LEFT_PADDING;
                sendBttn.Top  = labelPanel.Top   + (labelPanel.Height - sendBttn.Height) / 2;
            };

            sendBttn.LeftMouseButtonReleased += (_, _) => {
                sendBttn.Tint = Color.White;
                sendBttn.Size = new Point(32, 32);
                sendBttn.Left = labelPanel.Right + Panel.LEFT_PADDING;
                sendBttn.Top  = labelPanel.Top   + (labelPanel.Height - sendBttn.Height) / 2;
            };

            var lastTotalReachedTime = DateTime.UtcNow;
            var lastSendTime         = DateTime.UtcNow;
            var currentReduction     = 0;
            var currentValue         = 0;
            var currentRepetitions   = 0;

            sendBttn.Click += (_, _) => {
                ProofLogix.Instance.Resources.PlayMenuItemClick();

                if (!GameService.Gw2Mumble.IsAvailable) {
                    ScreenNotification.ShowNotification("Chat unavailable.", ScreenNotification.NotificationType.Error);
                    GameService.Content.PlaySoundEffectByName("error");
                    return;
                }

                if (DateTime.UtcNow.Subtract(lastTotalReachedTime).TotalSeconds < 1) {
                    ScreenNotification.ShowNotification("Your total has been reached. Cooling down.", ScreenNotification.NotificationType.Error);
                    GameService.Content.PlaySoundEffectByName("error");
                    return;
                }

                // Reset values if user paused.
                var hotButtonCooldownTime = DateTime.UtcNow.Subtract(lastSendTime);
                if (hotButtonCooldownTime.TotalMilliseconds > 500) {
                    currentReduction   = 0;
                    currentValue       = 0;
                    currentRepetitions = 0;
                }
                lastSendTime = DateTime.UtcNow;

                var chatLink = new ItemChatLink {
                    ItemId = _config.SelectedToken
                };

                var total = ProofLogix.Instance.PartySync.LocalPlayer.KpProfile.GetToken(_config.SelectedToken).Amount;

                if (total == 0) {
                    ScreenNotification.ShowNotification("No amounts recorded.", ScreenNotification.NotificationType.Error);
                    GameService.Content.PlaySoundEffectByName("error");
                    return;
                }

                chatLink.Quantity = Convert.ToByte(total <= 250 ? total : GetNext(total, 
                                                                                  ref currentReduction, 
                                                                                  ref currentValue, 
                                                                                  ref currentRepetitions, 
                                                                                  ref lastTotalReachedTime));

                ChatUtil.Send(chatLink.ToString(), ProofLogix.Instance.ChatMessageKey.Value);
            };

            var menu = new ContextMenuStrip {
                Parent      = buildPanel,
                ClipsBounds = false
            };

            var generalCategory = new ContextMenuStripItem("General") {
                Parent  = menu,
                Submenu = new ContextMenuStrip()
            };

            AddProofEntries(generalCategory, ProofLogix.Instance.Resources.GetGeneralItems(), labelPanel);

            var coffersCategory = new ContextMenuStripItem("Coffers") {
                Parent  = menu,
                Submenu = new ContextMenuStrip()
            };

            AddProofEntries(coffersCategory, ProofLogix.Instance.Resources.GetCofferItems(), labelPanel);

            var raidsCategory = new ContextMenuStripItem("Raids") {
                Parent  = menu,
                Submenu = new ContextMenuStrip()
            };

            var i = 1;
            foreach (var wing in ProofLogix.Instance.Resources.GetWings()) {
                var wingEntry = new ContextMenuStripItem($"Wing {i++}") {
                    Parent  = raidsCategory.Submenu,
                    Submenu = new ContextMenuStrip()
                };

                AddProofEntries(wingEntry, wing.Events.Where(ev => ev.Token != null).Select(ev => ev.Token), labelPanel);
            }

            var fractalsCategory = new ContextMenuStripItem("Fractals") {
                Parent  = menu,
                Submenu = new ContextMenuStrip()
            };

            AddProofEntries(fractalsCategory, ProofLogix.Instance.Resources.GetItemsForFractals(), labelPanel);

            cogWheel.Click += (_, _) => {
                GameService.Content.PlaySoundEffectByName("button-click");
                menu.Show(GameService.Input.Mouse.Position);
            };
            base.Build(buildPanel);
        }

        private void AddProofEntries(ContextMenuStripItem parent, IEnumerable<Resource> resources, Panel labelPanel) {
            foreach (var resource in resources) {
                var tokenEntry = new ContextMenuStripItem(resource.Name) {
                    Parent   = parent.Submenu
                };

                tokenEntry.Click += (_, _) => {
                    labelPanel.Children.FirstOrDefault()?.Dispose();
                    _config.SelectedToken = resource.Id;
                    var amount = ProofLogix.Instance.PartySync.LocalPlayer.KpProfile.GetToken(resource.Id).Amount;
                    var lbl    = BuildItemLabel(amount, resource.Id);
                    lbl.Parent = labelPanel;
                    lbl.Top    = (labelPanel.ContentRegion.Height - lbl.Height) / 2;
                    lbl.Left   = (labelPanel.ContentRegion.Width  - lbl.Width)  / 2;
                };
            }
        }

        private FormattedLabel BuildItemLabel(int quantity, int tokenId) {
            var str  = $"{BRACKET_LEFT}{quantity} {ProofLogix.Instance.Resources.GetItem(tokenId).Name}{BRACKET_RIGHT}";
            var size = LabelUtil.GetLabelSize(ContentService.FontSize.Size20, str);
            return new FormattedLabelBuilder().SetWidth(size.X)
                                              .SetHeight(size.Y)
                                              .CreatePart(str, o => {
                                                   o.SetFontSize(ContentService.FontSize.Size20);
                                               }).Build();
        }

        private int GetNext(int totalAmount, ref int currentReduction, ref int currentValue, ref int currentRepetitions, ref DateTime lastTotalReachedTime) {
            int rest       = totalAmount - currentValue % totalAmount;
            int tempAmount = Math.Min(250 - currentReduction, rest);
            if (currentRepetitions >= RandomUtil.GetRandom(1,3)) {
                if (rest > 250) {
                    currentValue += tempAmount;
                    ++currentReduction;
                } else {
                    currentReduction     = 0;
                    currentValue         = 0;
                    lastTotalReachedTime = DateTime.UtcNow;
                }
                currentRepetitions = 0;
            } else {
                ++currentRepetitions;
            }
            return tempAmount;
        }
    }

}
