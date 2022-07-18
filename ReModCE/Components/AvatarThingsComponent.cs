
using MelonLoader;
using SerpentCore.Core;
using SerpentCore.Core.Managers;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UnityEngine;
using VRC.Core;
using VRC.UI;

namespace Serpent.Components
{
    internal sealed class AvatarThingsComponent : ModComponent
    {
        private static GameObject SocialMenuInstance;

        public static GameObject GetSocialMenuInstance()
        {
            if (SocialMenuInstance == null)
            {
                SocialMenuInstance = GameObject.Find("UserInterface/MenuContent/Screens");
            }
            return SocialMenuInstance;
        }

        public AvatarThingsComponent()
        {

        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage(Page.PageNames.Avatars);
            menu.AddButton("Clone by ID",
                "Clone avatar by Avatar ID", CloneID);
        }

        private void CloneID()
        {
            Regex Avatar = new Regex("avtr_[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}");
            if (Avatar.IsMatch(Clipboard.GetText()))
            {
                new ApiAvatar { id = Clipboard.GetText() }.Get(new Action<ApiContainer>(x =>
                {
                    GetSocialMenuInstance().transform.Find("Avatar").GetComponent<PageAvatar>().field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0 = x.Model.Cast<ApiAvatar>();
                    GetSocialMenuInstance().transform.Find("Avatar").GetComponent<PageAvatar>().ChangeToSelectedAvatar();
                }), new Action<ApiContainer>(x =>
                {
                    MelonLogger.Msg($"Failed to change to avatar: {Clipboard.GetText()} | Error Message: {x.Error}");
                }));
            }
            else
            {
                MelonLogger.Msg($"Invalid Avatar ID!");
            }

        }

    }
}