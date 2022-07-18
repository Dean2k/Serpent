using SerpentCore.Core;
using SerpentCore.Core.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC;
using Object = UnityEngine.Object;

namespace Serpent.Components
{
    internal class RamCleanComponent : ModComponent
    {

        public RamCleanComponent()
        {
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage(Page.PageNames.Optimisation);
            menu.AddButton("Clear VRam",
                "Cleans ram (Shouldn't be needed but just incase).", CleanRam, ResourceManager.GetSprite("remodce.broom"));
        }

        public void CleanRam()
        {
            var currentAvatars = (from player in PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.ToArray() where player != null select player.prop_ApiAvatar_0 into apiAvatar where apiAvatar != null select apiAvatar.assetUrl).ToList();

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

        //public override void OnUpdate()
        //{
        //    GC.Collect();
        //}
    }
}
