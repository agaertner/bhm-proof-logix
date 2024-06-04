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

            var cogWheel = new Image(_cogWheelIcon) {
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

            var token = ProofLogix.Instance.PartySync.LocalPlayer.KpProfile.GetToken(_config.SelectedToken);
            var rarity   = ProofLogix.Instance.Resources.GetItem(_config.SelectedToken).Rarity.AsColor();

            var lbl = BuildItemLabel(AssetUtil.GetItemDisplayName(token.Name, token.Amount), rarity);
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
                BasicTooltipText = "Send to Chat\nMouse 1: Proportionately each time until total is reached.\nMouse 2: Total with singular item."
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

            var busy = false;
            sendBttn.LeftMouseButtonReleased += (_, _) => {
                if (busy) {
                    return;
                }
                busy = true;

                var total = ProofLogix.Instance.PartySync.LocalPlayer.KpProfile.GetToken(_config.SelectedToken).Amount;

                if (!CanSend(total, lastTotalReachedTime)) {
                    GameService.Content.PlaySoundEffectByName("error");
                    busy = false;
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
                    ItemId   = _config.SelectedToken,
                    Quantity = Convert.ToByte(total <= 250 ? total : GetNext(total, 
                                                                             ref currentReduction, 
                                                                             ref currentValue, 
                                                                             ref currentRepetitions, 
                                                                             ref lastTotalReachedTime))
                };

                ChatUtil.Send(chatLink.ToString(), ProofLogix.Instance.ChatMessageKey.Value);
                busy = false;
            };

            sendBttn.RightMouseButtonReleased += (_, _) => {
                if (busy) {
                    return;
                }
                busy = true;

                var total = ProofLogix.Instance.PartySync.LocalPlayer.KpProfile.GetToken(_config.SelectedToken).Amount;

                if (!CanSend(total, lastTotalReachedTime)) {
                    GameService.Content.PlaySoundEffectByName("error");
                    busy = false;
                    return;
                }

                var chatLink = new ItemChatLink {
                    ItemId = _config.SelectedToken
                };

                string message;

                if (total > 255) {
                    chatLink.Quantity = 1;
                    message = AssetUtil.GetItemDisplayName(chatLink.ToString(), total);
                } else {
                    chatLink.Quantity = Convert.ToByte(total);
                    message = chatLink.ToString();
                }

                if (_config.SendProfileId) {
                    message += $" » {ProofLogix.Instance.PartySync.LocalPlayer.KpProfile.Id}";
                }

                ChatUtil.Send(message, ProofLogix.Instance.ChatMessageKey.Value);
                lastTotalReachedTime = DateTime.UtcNow;
                busy = false;
            };

            var menu = new ContextMenuStrip {
                Parent      = buildPanel,
                ClipsBounds = false
            };

            var sendProfileIdEntry = new ContextMenuStripItem("Send Profile ID with Mouse 2") {
                Parent = menu,
                CanCheck = true,
                Checked = _config.SendProfileId
            };

            sendProfileIdEntry.CheckedChanged += (_, e) => {
                _config.SendProfileId = e.Checked;
            };

            var playerTokens = ProofLogix.Instance.PartySync.LocalPlayer.KpProfile.GetTokens();
            var generalItems = ProofLogix.Instance.Resources.GetItems(Resources.BANANAS, Resources.BANANAS_IN_BULK, Resources.LEGENDARY_INSIGHT)
                                         .Where(resource => playerTokens.Any(item => item.Id == resource.Id && item.Amount > 0)).ToList();

            if (generalItems.Any()) {
                var generalCategory = new ContextMenuStripItem("General") {
                    Parent  = menu,
                    Submenu = new ContextMenuStrip()
                };

                AddProofEntries(generalCategory, generalItems, labelPanel);
            }

            var cofferItems = ProofLogix.Instance.Resources.GetCofferItems()
                                        .Where(resource => playerTokens.Any(item => item.Id == resource.Id && item.Amount > 0)).ToList();

            if (cofferItems.Any()) {
                var coffersCategory = new ContextMenuStripItem("Coffers") {
                    Parent  = menu,
                    Submenu = new ContextMenuStrip()
                };

                AddProofEntries(coffersCategory, cofferItems, labelPanel);
            }

            var strikeItems = ProofLogix.Instance.Resources.GetItems(Resources.BONESKINNER_RITUAL_VIAL)
                                        .Concat(ProofLogix.Instance.Resources.GetItemsForStrikes())
                                        .Where(resource => playerTokens.Any(item => item.Id == resource.Id && item.Amount > 0)).ToList();

            if (strikeItems.Any()) {
                var strikesCategory = new ContextMenuStripItem("Strikes") {
                    Parent  = menu,
                    Submenu = new ContextMenuStrip()
                };

                AddProofEntries(strikesCategory, strikeItems, labelPanel);
            }

            var wingTokens = ProofLogix.Instance.Resources.GetWings()
                                       .Select(wing => wing.Events.Where(ev => ev.Token != null)
                                                           .Select(ev => ev.Token)
                                                           .Where(resource => playerTokens.Any(item => 
                                                                                                   item.Id == resource.Id && 
                                                                                                   item.Amount > 0))
                                                           .ToList()).ToList();

            var raidItems = ProofLogix.Instance.Resources.GetItems(Resources.LEGENDARY_DIVINATION)
                                      .Where(resource => playerTokens.Any(item => item.Id == resource.Id && item.Amount > 0)).ToList();

            if (wingTokens.Any() || raidItems.Any()) {
                var raidsCategory = new ContextMenuStripItem("Raids") {
                    Parent  = menu,
                    Submenu = new ContextMenuStrip()
                };

                AddProofEntries(raidsCategory, raidItems, labelPanel);

                var i = 1;
                foreach (var wing in wingTokens) {
                    if (wing.Any()) {
                        var wingEntry = new ContextMenuStripItem($"Wing {i}") {
                            Parent  = raidsCategory.Submenu,
                            Submenu = new ContextMenuStrip()
                        };
                        AddProofEntries(wingEntry, wing, labelPanel);
                    }
                    ++i;
                }
            }

            var fractalItems = ProofLogix.Instance.Resources.GetItemsForFractals()
                                         .Where(resource => playerTokens.Any(item => item.Id == resource.Id && item.Amount > 0)).ToList();

            if (fractalItems.Any()) {
                var fractalsCategory = new ContextMenuStripItem("Fractals") {
                    Parent  = menu,
                    Submenu = new ContextMenuStrip()
                };

                AddProofEntries(fractalsCategory, fractalItems, labelPanel);
            }

            cogWheel.Click += (_, _) => {
                GameService.Content.PlaySoundEffectByName("button-click");
                menu.Show(GameService.Input.Mouse.Position);
            };
            base.Build(buildPanel);
        }

        private bool CanSend(int totalAmount, DateTime lastTotalReachedTime) {
            if (!GameService.Gw2Mumble.IsAvailable) {
                ScreenNotification.ShowNotification("Chat unavailable.", ScreenNotification.NotificationType.Error);
                return false;
            }
            if (DateTime.UtcNow.Subtract(lastTotalReachedTime).TotalSeconds < 1) {
                ScreenNotification.ShowNotification("Action recharging.", ScreenNotification.NotificationType.Error);
                return false;
            }
            if (totalAmount == 0) {
                ScreenNotification.ShowNotification("You can't ping empty records.", ScreenNotification.NotificationType.Error);
                return false;
            }
            return true;
        }

        private void AddProofEntries(ContextMenuStripItem parent, IEnumerable<Resource> resources, Container labelPanel) {
            foreach (var resource in resources) {

                var token = ProofLogix.Instance.PartySync.LocalPlayer.KpProfile.GetToken(resource.Id);

                var displayName = AssetUtil.GetItemDisplayName(resource.Name, token.Amount);

                var rarity   = ProofLogix.Instance.Resources.GetItem(resource.Id).Rarity.AsColor();
                var tokenEntry = new ContextMenuStripItemWithColor(displayName) {
                    Parent   = parent.Submenu,
                    TextColor = rarity
                };

                tokenEntry.Click += (_, _) => {
                    labelPanel.Children.FirstOrDefault()?.Dispose();
                    _config.SelectedToken = resource.Id;
                    var lbl = BuildItemLabel(displayName, rarity);
                    lbl.Parent = labelPanel;
                    lbl.Top    = (labelPanel.ContentRegion.Height - lbl.Height) / 2;
                    lbl.Left   = (labelPanel.ContentRegion.Width  - lbl.Width)  / 2;
                    GameService.Content.PlaySoundEffectByName("color-change");
                };
            }
        }

        private Label BuildItemLabel(string displayName, Color color) {
            var size = LabelUtil.GetLabelSize(ContentService.FontSize.Size20, displayName);
            return new Label {
                Text       = displayName,
                TextColor  = color,
                StrokeText = true,
                ShowShadow = true,
                Size       = new Point(size.X, size.Y),
                Font       = GameService.Content.GetFont(ContentService.FontFace.Menomonia , ContentService.FontSize.Size20, ContentService.FontStyle.Regular)
            };
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
