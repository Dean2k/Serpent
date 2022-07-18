using ReModAres.Core;
using ReModAres.Core.Api;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;

namespace Serpent.Components
{
    internal sealed class LoudMicComponent : ModComponent
    {
        private ConfigValue<bool> LoudMicEnabled;
        private ReMenuToggle _loudMicEnabled;

        private ConfigValue<bool> ActionMenuMicroPhoneEnabled;
        private ReMenuToggle _actionMenuMicroPhoneEnabled;

        public LoudMicComponent()
        {
            LoudMicEnabled = new ConfigValue<bool>(nameof(LoudMicEnabled), false);
            LoudMicEnabled.OnValueChanged += () => _loudMicEnabled.Toggle(LoudMicEnabled);

            ActionMenuMicroPhoneEnabled = new ConfigValue<bool>(nameof(ActionMenuMicroPhoneEnabled), true);
            ActionMenuMicroPhoneEnabled.OnValueChanged += () => _actionMenuMicroPhoneEnabled.Toggle(ActionMenuMicroPhoneEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage(Page.PageNames.Microphone);
            _loudMicEnabled = menu.AddToggle("Loud Mic",
                "Ear Rape.", LoudMic,
                LoudMicEnabled);

            _actionMenuMicroPhoneEnabled = menu.AddToggle("Action Menu Toggle",
                "Enable the action menu quick toggle (restart required).", ToggleMenu,
                ActionMenuMicroPhoneEnabled);

            try
            {
                if (ActionMenuMicroPhoneEnabled)
                {
                    VRCActionMenuPage.AddToggle(ActionMenuPage.Main, "Mic Rape", LoudMicEnabled, ToggleMicQuick,
                        ResourceManager.GetTexture("remodce.skull"));
                }
            }
            catch { }

        }


        private void ToggleMenu(bool value)
        {
            ActionMenuMicroPhoneEnabled.SetValue(value);
        }

        private void ToggleMicQuick(bool value)
        {
            if (LoudMicEnabled)
            {
                LoudMic(false);
            }
            else
            {
                LoudMic(true);
            }
        }

        private void LoudMic(bool enable)
        {
            LoudMicEnabled.SetValue(enable);
            if (enable)
            {
                USpeaker.field_Internal_Static_Single_1 = float.MaxValue;
            }
            else
            {
                USpeaker.field_Internal_Static_Single_1 = 1f;
            }

        }

    }
}