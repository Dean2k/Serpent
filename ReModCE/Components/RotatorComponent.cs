using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using SerpentCore.Core.VRChat;
using Serpent.ControlSchemes;
using Serpent.Core;
using Serpent.Loader;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;

namespace Serpent.Components
{
    public class RotatorComponent : ModComponent
    {
        private static ReMenuToggle _hotkeyToggle;
        private static ReMenuToggle _rotationToggle;
        private static ReMenuToggle _invertPitchToggle;
        private static ReMenuToggle _rotationLockToggle;
        private static ReMenuToggle _controlSchemeToggle;

        internal static ConfigValue<bool> _RotationHotkeyEnabled;
        internal static ConfigValue<float> _RotationSpeed;
        internal static ConfigValue<float> _RotationFlightSpeed;
        internal static ConfigValue<bool> _RotationInvertPitch;
        internal static ConfigValue<bool> _RotationControlScheme;

        internal static KeyCode _RotationKeybind = KeyCode.G;

        private static bool failedToLoad;

        public readonly string Version = "2.0.5";

        public RotatorComponent()
        {
            _RotationHotkeyEnabled = new ConfigValue<bool>(nameof(_RotationHotkeyEnabled), true);
            _RotationSpeed = new ConfigValue<float>(nameof(_RotationSpeed), 180f);
            _RotationFlightSpeed = new ConfigValue<float>(nameof(_RotationFlightSpeed), 5f);
            _RotationInvertPitch = new ConfigValue<bool>(nameof(_RotationInvertPitch), false);

            _RotationControlScheme = new ConfigValue<bool>(nameof(_RotationControlScheme), false);
            _RotationControlScheme.OnValueChanged += () => RotationSystem.CurrentControlScheme = _RotationControlScheme ? new JanNyaaControlScheme() : new DefaultControlScheme();

            if (!RotationSystem.Initialize())
            {
                ReLogger.Msg("Failed to initialize the rotation system. Instance already exists");
                failedToLoad = true;
                return;
            }

            if (!RotatorPatches.PatchMethods())
            {
                failedToLoad = true;
                ReLogger.Warning("Failed to patch everything, disabling player rotator");
                return;
            }

            if (_RotationControlScheme)
            {
                RotationSystem.CurrentControlScheme = new JanNyaaControlScheme();
            }
            else RotationSystem.CurrentControlScheme = new DefaultControlScheme();

            ReLogger.Msg("SerpentCore-PlayerRotator " + Version);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {

            var rotator = uiManager.MainMenu.AddMenuPage("Rotator Mod", "Access Rotator mod related settings", ResourceManager.GetSprite("remodce.keyboard"));

            var hotkeys = rotator.AddMenuPage("Rotator Mod", "Theme settings", ResourceManager.GetSprite("remodce.mixer"));

            _rotationToggle = rotator.AddToggle("Enable", "Enable/Disable Rotation", RotationSystem.Instance.Toggle, false);
            _rotationLockToggle = rotator.AddToggle("Lock\nRotation", "Lock Rotation", (state) => RotationSystem.LockRotation = state, false);
            _invertPitchToggle = rotator.AddToggle("Invert\nPitch", "Invert Pitch", _RotationInvertPitch.SetValue, false);
            _controlSchemeToggle = rotator.AddToggle("Control\nScheme", "Switch Between Default & JanNyaa's Control Schemes", _RotationControlScheme.SetValue, false);
            var rspeed = rotator.AddButton($"Rotation\nSpeed", "Adjust Speed of Rotation", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Rotation Speed", _RotationSpeed.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        var number = float.TryParse(s, out float n);
                        if (string.IsNullOrEmpty(s) || (!number)) { VRCUiManagerEx.Instance.QueueHudMessage("Invalid Input!", Color.red, 3); ReLogger.Error("Invalid Input!"); return; }
                        _RotationSpeed.SetValue(n);
                    }, null);
            }, ResourceManager.GetSprite("remod.speed"));
            var sspeed = rotator.AddButton($"Flight\nSpeed", "Adjust Speed of Flight While Rotating", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Flight Speed", _RotationFlightSpeed.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        var number = float.TryParse(s, out float n);
                        if (string.IsNullOrEmpty(s) || (!number)) { VRCUiManagerEx.Instance.QueueHudMessage("Invalid Input!", Color.red, 3); ReLogger.Error("Invalid Input!"); return; }
                        _RotationFlightSpeed.SetValue(n);
                    }, null);
            }, ResourceManager.GetSprite("remod.speed"));
            var roll = rotator.AddButton("Barrel\nRoll", "Do a Barrel Roll", RotationSystem.Instance.BarrelRoll, ResourceManager.GetSprite("remod.reload"));

            _hotkeyToggle = hotkeys.AddToggle("Player\nRotator", "Enables/Disable Player Rotator Hotkey (Ctrl+G)", _RotationHotkeyEnabled.SetValue, _RotationHotkeyEnabled);
        }

        public override void OnUpdate()
        {
            if (_RotationHotkeyEnabled
                && Input.GetKey(KeyCode.LeftControl)
                && Input.GetKeyDown(_RotationKeybind))
            {
                if (!RotationSystem.Rotating)
                {
                    _rotationToggle.Toggle(true);
                }
                else _rotationToggle.Toggle(false);
            };

            if (failedToLoad) return;
            RotationSystem.Instance.OnUpdate();

            if (RotationSystem.BarrelRolling) return;
            if (_RotationHotkeyEnabled
                && Input.GetKey(KeyCode.LeftControl)
                && Input.GetKey(KeyCode.LeftShift)
                && Input.GetKeyDown(KeyCode.B))
                RotationSystem.Instance.BarrelRoll();
        }

        public override void OnEnterWorld(ApiWorld world, ApiWorldInstance instance)
        {
            _rotationLockToggle.Toggle(false);
            _rotationToggle.Toggle(false);
            RotationSystem.Instance.OnLeftWorld();
        }

        public override void OnPlayerJoined(Player player)
        {
            if (player.GetAPIUser().IsStaff() && RotationSystem.Rotating)
            {
                _rotationLockToggle.Toggle(false);
                _rotationToggle.Toggle(false);
            };
        }

        public override void OnAvatarIsReady(VRCPlayer player)
        {
            if (player == Utilities.GetLocalVRCPlayer() && RotationSystem.Rotating)
            {
                RotationSystem.Instance.UpdateSettings();
            }
        }

    }
}
