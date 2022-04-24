using System;
using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;
using ReModCE_ARES.Core;
using ReModCE_ARES.Loader;
using UnityEngine;
using VRC;
using VRC.Udon;

namespace ReModCE_ARES.Components
{
    internal class UdonLoggerComponent : ModComponent
    {
        private ReMenuButton _udonLog;

        public UdonLoggerComponent()
        {
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("ARES");
            _udonLog = menu.AddButton("Log all Udon Events in world",
                "Gets all udon events and logs them to console", LogUdon);
        }

        public void LogUdon()
        {
            foreach (var udonEvent in GameObject.FindObjectsOfType<UdonBehaviour>())
            {
                foreach (var table in udonEvent._eventTable)
                {
                    Console.WriteLine(table.Key);
                }
            }
        }
    }
}
