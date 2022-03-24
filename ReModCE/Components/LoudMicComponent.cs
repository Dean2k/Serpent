﻿using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE_ARES.Loader;
using ReModCE_ARES.Managers;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.DataModel;
using VRC.UI;

namespace ReModCE_ARES.Components
{
    internal sealed class LoudMicComponent : ModComponent
    {
        private ConfigValue<bool> LoudMicEnabled;
        private ReMenuToggle _loudMicEnabled;

        public LoudMicComponent()
        {
            LoudMicEnabled = new ConfigValue<bool>(nameof(LoudMicEnabled), false);
            LoudMicEnabled.OnValueChanged += () => _loudMicEnabled.Toggle(LoudMicEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("Microphone");
            _loudMicEnabled = menu.AddToggle("Loud Mic",
                "Ear Rape.", LoudMic,
                LoudMicEnabled);
        }

        private void LoudMic(bool enable)
        {
            LoudMicEnabled.SetValue(enable);
            if (enable)
            {
                USpeaker.field_Internal_Static_Single_1 = float.MaxValue;
            } else
            {
                USpeaker.field_Internal_Static_Single_1 = 1f;
            }

        }
        
    }
}