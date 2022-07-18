﻿using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;
using ReModAres.Core.VRChat;
using Serpent.Managers;

namespace Serpent.Components
{
    internal class DebugMenuComponent : ModComponent
    {
        private ConfigValue<bool> DebugMenuEnabled;
        private ReMenuToggle _debugMenuToggle;
        public static QMLable debugLog;

        public DebugMenuComponent()
        {
            DebugMenuEnabled = new ConfigValue<bool>(nameof(DebugMenuEnabled), true);
            DebugMenuEnabled.OnValueChanged += () => _debugMenuToggle.Toggle(DebugMenuEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetCategoryPage(Page.PageNames.Utility).AddCategory("Debug");
            _debugMenuToggle = menu.AddToggle("Debug Console",
                "Enable whether player joins/leaves should be logged in console. (right hand wing)", ToggleDebugRight,
                DebugMenuEnabled);


            debugLog = new QMLable(QuickMenuEx.RightWing.field_Public_Button_0.transform, 609.902f, 457.9203f, "DebugLog");
            drawOverlay();
        }

        public void ToggleDebugRight(bool value)
        {
            DebugMenuEnabled.SetValue(value);
            drawOverlay();
        }

        public void drawOverlay()
        {
            if (DebugMenuEnabled)
            {
                debugLog.lable.SetActive(true);
                //debugLog.lable.transform.localPosition = new Vector3(609.902f, 457.9203f, 0);
                //debugLog.text.enableWordWrapping = false;
                //debugLog.text.fontSizeMin = 30;
                //debugLog.text.fontSizeMax = 30;
                //debugLog.text.alignment = TMPro.TextAlignmentOptions.Left;
                //debugLog.text.verticalAlignment = TMPro.VerticalAlignmentOptions.Top;
                //debugLog.text.color = Color.white;
            }
            else
            {
                debugLog.lable.SetActive(false);
            }
        }
    }
}
