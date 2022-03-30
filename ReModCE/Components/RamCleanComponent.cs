using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MelonLoader;
using ReMod.Core;
using ReMod.Core.Managers;
using UnityEngine;
using VRC;
using Object = UnityEngine.Object;

namespace ReModCE_ARES.Components
{
    internal class RamCleanComponent : ModComponent
    {

        public RamCleanComponent()
        {
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("ARES");
            menu.AddButton("Clear VRam",
                "Enable whether player joins/leaves should be logged in console.", CleanRam);
        }

        public void CleanRam()
        {
            var currentAvatars = (from player in PlayerManager.prop_PlayerManager_0.prop_ArrayOf_Player_0 where player != null select player.prop_ApiAvatar_0 into apiAvatar where apiAvatar != null select apiAvatar.assetUrl).ToList();

            var dict = new Dictionary<string, AssetBundleDownload>();
            var abdm = AssetBundleDownloadManager.prop_AssetBundleDownloadManager_0;
            foreach (var key in abdm.field_Private_Dictionary_2_String_AssetBundleDownload_0.Keys)
            {
                dict.Add(key, abdm.field_Private_Dictionary_2_String_AssetBundleDownload_0[key]);
            }

            foreach (var key in dict.Keys)
            {
                var assetBundleDownload = abdm.field_Private_Dictionary_2_String_AssetBundleDownload_0[key];

                if (!assetBundleDownload.field_Private_String_0.Contains("wrld_") && !currentAvatars.Contains(key))
                {
                    abdm.field_Private_Dictionary_2_String_AssetBundleDownload_0.Remove(key);
                    if (assetBundleDownload.field_Private_GameObject_0 != null)
                    {
                        Object.DestroyImmediate(assetBundleDownload.field_Private_GameObject_0, true);
                    }
                }
            }

            dict.Clear();

            Resources.UnloadUnusedAssets();
        }
    }
}
