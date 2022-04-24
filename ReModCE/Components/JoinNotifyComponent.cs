using System;
using System.Collections.Generic;
using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;
using ReModCE_ARES.Core;
using ReModCE_ARES.Loader;
using UnityEngine;
using VRC;

namespace ReModCE_ARES.Components
{
    internal class JoinNotifyComponent : ModComponent
    {
        private ConfigValue<bool> JoinNotifyEnabled;
        private ReMenuToggle _joinNotifyToggle;

        public JoinNotifyComponent()
        {
            JoinNotifyEnabled = new ConfigValue<bool>(nameof(JoinNotifyEnabled), true);
            JoinNotifyEnabled.OnValueChanged += () => _joinNotifyToggle.Toggle(JoinNotifyEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("ARES");
            _joinNotifyToggle = menu.AddToggle("Join Notifier",
                "Enable whether player joins/leaves a message displays on your hud", JoinNotifyEnabled.SetValue,
                JoinNotifyEnabled);
        }
    }
}
