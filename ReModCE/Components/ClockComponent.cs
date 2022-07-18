using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using SerpentCore.Core.VRChat;
using Serpent.Managers;
using System;
using UnityEngine;
using VRC.UI.Elements;

namespace Serpent.Components
{
    internal class ClockComponent : ModComponent
    {
        private ConfigValue<bool> ClockEnabled;
        private ReMenuToggle _clockToggle;
        private bool firstRun = true;
        public ClockComponent()
        {
            ClockEnabled = new ConfigValue<bool>(nameof(ClockEnabled), true);
            ClockEnabled.OnValueChanged += () => _clockToggle.Toggle(ClockEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetCategoryPage(Page.PageNames.Utility).AddCategory("Clock");
            _clockToggle = menu.AddToggle("QM Clock",
                "Enable clock in QM", ToggleClock,
                ClockEnabled);
        }

        public void ToggleClock(bool value)
        {
            ClockEnabled.SetValue(value);
            drawOverlay();
        }

        public override void OnUpdate()
        {
            if (Serpent._readyQA)
            {
                if (firstRun)
                {
                    drawOverlay();
                    firstRun = false;
                }
                if (ClockEnabled)
                {                 
                    Serpent._hudClock.text = DateTime.Now.ToString("HH:mm:ss");
                }
            }         
        }

        public void drawOverlay()
        {
            if (ClockEnabled)
            {
                Serpent._hudObj.SetActive(true);
            }
            else
            {
                Serpent._hudObj.SetActive(false);
            }
        }
    }
}
