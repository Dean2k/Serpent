using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE_ARES.Loader;
using ReModCE_ARES.Managers;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.DataModel;
using VRC.UI;

namespace ReModCE_ARES.Components
{
    internal sealed class DownloadVRCWComponent : ModComponent
    {
        private static PageWorldInfo _userInfoPage;

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var targetMenu = uiManager.TargetMenu;

            var userInfoTransform = VRCUiManagerEx.Instance.MenuContent().transform.Find("Screens/WorldInfo");
            _userInfoPage = userInfoTransform.GetComponent<PageWorldInfo>();

            var buttonContainer = userInfoTransform.Find("WorldButtons/GoButton").GetParent();

            new ReUiButton("Download VRCW", new Vector2(250f,-10f), new Vector2(0.90f, 1.9f), DownloadVRCW, buttonContainer);

        }

        private void DownloadVRCW()
        {
            Task.Run(delegate
            {
                var world = _userInfoPage.field_Private_ApiWorld_0;
                WebClient webClient = new WebClient
                {
                    Headers =
                    {
                        "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.84 Safari/537.36"
                    }
                };
                if (!Directory.Exists("ARES"))
                {
                    Directory.CreateDirectory("ARES");
                    Directory.CreateDirectory("ARES/VRCW");
                }
                if (!Directory.Exists("ARES/VRCW"))
                {
                    Directory.CreateDirectory("ARES/VRCW");
                }
                webClient.DownloadFileAsync(new Uri(world.assetUrl), "ARES/VRCW/" + world.name + ".vrcw");
                ReLogger.Msg(world.name + " VRCW Downloaded");
                ReModCE_ARES.LogDebug(world.name + " VRCW Downloaded");
            });
        }
    }
}