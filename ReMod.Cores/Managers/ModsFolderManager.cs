using System;
using System.Collections.Generic;
using ReMod.Core.Api;
using ReMod.Core.Helpers;

namespace ReMod.Core.Managers
{
    internal static class ModsFolderManager
    {
        public static List<Action> mods = new();
        public static List<List<Action>> SplitMods;

        private static readonly Action openFunc = () =>
        {
            if (mods.Count <= Constants.MAX_PEDALS_PER_PAGE)
            {
                foreach (var action in mods) action();
            }
            else
            {
                SplitMods ??= mods.Split(Constants.MAX_PEDALS_PER_PAGE);
                for (var i = 0; i < SplitMods.Count && i < Constants.MAX_PEDALS_PER_PAGE; i++)
                {
                    var index = i;
                    CustomSubMenu.AddSubMenu($"Page {i + 1}", () =>
                    {
                        foreach (var action in SplitMods[index]) action();
                    }, ResourcesManager.GetPageIcon(i + 1));
                }
            }
        };

        public static void AddMod(Action openingAction)
        {
            mods.Add(openingAction);
        }


        public static void AddMainPageButton()
        {
            CustomSubMenu.AddSubMenu(Constants.MODS_FOLDER_NAME, openFunc, ResourcesManager.GetModsSectionIcon());
        }
    }
}