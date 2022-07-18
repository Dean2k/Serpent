using Il2CppSystem.IO;
using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using System;
using UnityEngine;
using VRC.Udon;

namespace Serpent.Components
{
    internal class UdonLoggerComponent : ModComponent
    {
        private ReMenuButton _udonLog;
        private ReMenuButton _itemLog;

        public UdonLoggerComponent()
        {
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage(Page.PageNames.WorldCheats);
            _udonLog = menu.AddButton("Log all Udon Events in world",
                "Gets all udon events and logs them to console", LogUdon);
            _itemLog = menu.AddButton("Log all item names in world",
                "Gets all item names and logs them to console", LogItems);
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

        public void LogItems()
        {
            string itemList = null;
            foreach (GameObject item in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                itemList = itemList + Environment.NewLine + item.name;
            }
            File.WriteAllText("Items.txt", itemList);
        }
    }
}
