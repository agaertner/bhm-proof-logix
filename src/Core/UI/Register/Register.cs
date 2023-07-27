using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.ProofLogix.Core.UI.KpProfile;

namespace Nekres.ProofLogix.Core.UI {

    public class RegisterView : View {

        private AsyncTexture2D _kpLogo;

        public RegisterView() {
            _kpLogo = ProofLogix.Instance.ContentsManager.GetTexture("killproof_logo.png");
        }

        protected override void Build(Container buildPanel) {

            var image = new Image(_kpLogo) {
                Parent = buildPanel,
                Width = 128,
                Height = 128,
                Left = (buildPanel.ContentRegion.Width - 128) / 2 
            };
            image.Disposed += (_, _) => _kpLogo.Dispose();

            var text  = "You do not appear to have added a key to www.killproof.me yet.\nYou can add yourself now to track your progress.";
            var size  = LabelUtil.GetLabelSize(GameService.Content.DefaultFont16, text);
            var label = new FormattedLabelBuilder().SetWidth(buildPanel.ContentRegion.Width)
                                                   .SetHeight(size.Y)
                                                   .SetHorizontalAlignment(HorizontalAlignment.Center)
                                                   .CreatePart(text, o => {
                                                        o.SetFontSize(ContentService.FontSize.Size16);
                                                    }).Build();
            label.Top    = image.Bottom;
            label.Parent = buildPanel;

            var perms = string.Join(", ", ProofLogix.Instance.KpWebApi.RequiredPermissions);
            var apiInput = new TextBox {
                Parent = buildPanel,
                Width  = buildPanel.ContentRegion.Width / 2,
                Height = 32,
                Left   = buildPanel.ContentRegion.Width / 4,
                Top = label.Bottom + Control.ControlStandard.ControlOffset.Y,
                PlaceholderText = "Guild Wars 2 API key",
                BasicTooltipText = $"A Guild Wars 2 API key.\nRequired permissions: {perms}"
            };

            var checkmark = new Image {
                Parent  = buildPanel,
                Top     = apiInput.Top,
                Left    = apiInput.Right + Panel.RIGHT_PADDING,
                Height  = 32,
                Width   = 32,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(1234939)
            };

            apiInput.TextChanged += (_, _) => {
                var valid = ProofLogix.Instance.Gw2WebApi.HasCorrectFormat(apiInput.Text);
                checkmark.Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(valid ? 1234938 : 1234939);
            };

            var openerCb = new Checkbox {
                Parent = buildPanel,
                Height = 40,
                Top    = apiInput.Bottom + Control.ControlStandard.ControlOffset.Y,
                Text   = "I would be willing to open instances for people. (Optional)",
                BasicTooltipText = "This feature will give your Guild Wars 2 account name to "
                                 + "people if you are able to open an instance to a specific boss.\n"
                                 + "For example someone might want to skip Escort and if that's all you've done in Wing 3, you "
                                 + "could be one of the accounts shown. It will show your account name so they can contact you in-game."
            }; 
            openerCb.Left = (buildPanel.ContentRegion.Width - openerCb.Width) / 2;

            var agreeCb = new Checkbox {
                Parent = buildPanel,
                Height = 60,
                Text   = "I hereby grant www.killproof.me permission to track and store my raid related progress.\n"
                       + "I understand that I can claim ownership of my records at anytime by creating an account\nat www.killproof.me.",
                Top = openerCb.Bottom + Control.ControlStandard.ControlOffset.Y
            };
            agreeCb.Left = (buildPanel.ContentRegion.Width - agreeCb.Width) / 2;

            var acceptBttn = new StandardButton {
                Parent = buildPanel,
                Top    = agreeCb.Bottom + Control.ControlStandard.ControlOffset.Y,
                Width  = 120, 
                Height = 36,
                Left   = (buildPanel.ContentRegion.Width - 120) / 2,
                Text   = "Add yourself now!"
            };

            acceptBttn.Click += async (_, _) => {
                
                if (!ProofLogix.Instance.Gw2WebApi.HasCorrectFormat(apiInput.Text)) {
                    GameService.Content.PlaySoundEffectByName("error");
                    ScreenNotification.ShowNotification("Please enter a valid Guild Wars 2 API key.", ScreenNotification.NotificationType.Error);
                    return;
                }

                if (!agreeCb.Checked) {
                    GameService.Content.PlaySoundEffectByName("error");
                    ScreenNotification.ShowNotification("Your consent is required to proceed.", ScreenNotification.NotificationType.Error);
                    return;
                }
                
                GameService.Content.PlaySoundEffectByName("button-click");

                var window = (StandardWindow)buildPanel;

                window.Show(new LoadingView("Adding key...", "Please, wait."));

                var response = await ProofLogix.Instance.KpWebApi.AddKey(apiInput.Text, openerCb.Checked);
                if (response.IsError) {
                    ProofLogix.Logger.Warn(response.Error);
                    GameService.Content.PlaySoundEffectByName("error");
                    ScreenNotification.ShowNotification("Something went wrong. Please, try again.", ScreenNotification.NotificationType.Error);
                    window.Show(this);
                    ProofLogix.Instance.ToggleRegisterWindow();
                    return;
                }

                GameService.Content.PlaySoundEffectByName("color-change");
                ScreenNotification.ShowNotification($"Profile added successfully! ID: {response.KpId}", ScreenNotification.NotificationType.Green);
                var profile = await ProofLogix.Instance.KpWebApi.GetProfile(response.KpId);
                ProofLogix.Instance.PartySync.LocalPlayer.AttachProfile(profile);
                ProfileView.Open(profile);
                window.Dispose();
            };

            buildPanel.ContentResized += (_, e) => {
                image.Left     = (e.CurrentRegion.Width - image.Width) / 2;
                label.Width    = e.CurrentRegion.Width;
                label.Top      = image.Bottom;

                apiInput.Width = e.CurrentRegion.Width / 2;
                apiInput.Left  = e.CurrentRegion.Width / 4;
                apiInput.Top   = label.Bottom + Control.ControlStandard.ControlOffset.Y;

                checkmark.Top  = apiInput.Top;
                checkmark.Left = apiInput.Right + Panel.RIGHT_PADDING;

                openerCb.Top  = apiInput.Bottom + Control.ControlStandard.ControlOffset.Y;
                openerCb.Left = (e.CurrentRegion.Width - openerCb.Width) / 2;
                agreeCb.Top   = openerCb.Bottom + Control.ControlStandard.ControlOffset.Y;
                agreeCb.Left  = (e.CurrentRegion.Width - agreeCb.Width) / 2;

                acceptBttn.Top  = agreeCb.Bottom + Control.ControlStandard.ControlOffset.Y;
                acceptBttn.Left = (buildPanel.ContentRegion.Width - 120) / 2;
            };

            base.Build(buildPanel);
        }

    }

}
