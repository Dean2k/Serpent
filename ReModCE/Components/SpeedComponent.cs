using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using SerpentCore.Core.VRChat;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Serpent.Components
{
    internal class SpeedComponent : ModComponent
    {
        private ConfigValue<bool> SpeedEnabled;
        private ReMenuToggle _speedToggle;
        private ConfigValue<float> MovementSpeed;
        private ReMenuButton _movementSpeedButton;
        private float defaultWalk;
        private float defaultRun;
        private float defaultStrafe;

        public SpeedComponent()
        {
            SpeedEnabled = new ConfigValue<bool>(nameof(SpeedEnabled), true);
            SpeedEnabled.OnValueChanged += () => _speedToggle.Toggle(SpeedEnabled);

            MovementSpeed = new ConfigValue<float>(nameof(MovementSpeed), 4);
            MovementSpeed.OnValueChanged += () => _movementSpeedButton.Text = $"Movement Speed: {MovementSpeed}";
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage(Page.PageNames.Movement);
            _speedToggle = menu.AddToggle("Speedhack Enabled",
                "Enable / disable the speedhack.", EnableSpeed,
                false);
            _movementSpeedButton = menu.AddButton($"Movement Speed: {MovementSpeed}", "Adjust your speed",
                () =>
                {
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Movement speed",
                        MovementSpeed.ToString(), InputField.InputType.Standard, false, "Submit",
                        (s, k, t) =>
                        {
                            if (string.IsNullOrEmpty(s))
                                return;

                            if (!float.TryParse(s, out var movementSpeed))
                                return;

                            MovementSpeed.SetValue(movementSpeed);
                            OnDisable();
                            OnEnable();
                        }, null);
                }, ResourceManager.GetSprite("remodce.speed"));
        }

        private void EnableSpeed(bool value)
        {
            SpeedEnabled.SetValue(value);
            if (value)
            {
                OnEnable();
            }
            else
            {
                OnDisable();
            }
        }

        public void OnEnable()
        {
            defaultWalk = Networking.LocalPlayer.GetWalkSpeed();
            defaultRun = Networking.LocalPlayer.GetRunSpeed();
            defaultStrafe = Networking.LocalPlayer.GetStrafeSpeed();

            Networking.LocalPlayer.SetWalkSpeed(Networking.LocalPlayer.GetWalkSpeed() + MovementSpeed);
            Networking.LocalPlayer.SetRunSpeed(Networking.LocalPlayer.GetRunSpeed() + MovementSpeed);
            Networking.LocalPlayer.SetStrafeSpeed(Networking.LocalPlayer.GetStrafeSpeed() + MovementSpeed);
        }

        public void OnDisable()
        {
            Networking.LocalPlayer.SetWalkSpeed(defaultWalk);
            Networking.LocalPlayer.SetRunSpeed(defaultRun);
            Networking.LocalPlayer.SetStrafeSpeed(defaultStrafe);
        }
    }
}
