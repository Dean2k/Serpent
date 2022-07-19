using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.VRChat;
using UnityEngine;
using VRC;
using VRC.DataModel;
using VRC.SDKBase;

namespace Serpent.Components
{
    internal sealed class ItemOrbitPlayerComponent : ModComponent
    {
        private static GameObject target;
        public static VRC_Pickup[] vrc_Pickups;
        public static bool ItemOrbitPlayerEnabled = false;
        public static bool ItemOrbitSwastikaPlayerEnabled = false;
        private static IUser targetPlayer;

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var targetMenu = uiManager.TargetMenu;

            var userInfoTransform = VRCUiManagerEx.Instance.MenuContent().transform.Find("Screens/UserInfo");
            var buttonContainer = userInfoTransform.Find("Buttons/RightSideButtons/RightUpperButtonColumn/");
            targetMenu.AddButton("Item Orbit", "Makes all Pickups spin around the targeted player.", ItemOrbit, ResourceManager.GetSprite("remodce.link"));

        }

        private void ItemOrbit()
        {
            if (ItemOrbitPlayerEnabled)
            {
                ItemOrbitPlayerEnabled = false;
            }
            else
            {
                targetPlayer = QuickMenuEx.SelectedUserLocal.field_Private_IUser_0;
                if (targetPlayer != null)
                {
                    ItemOrbitSwastikaPlayerEnabled = false;
                    ItemOrbitPlayerEnabled = true;
                }
            }
        }

        private void ItemSwastika()
        {
            if (ItemOrbitSwastikaPlayerEnabled)
            {
                ItemOrbitSwastikaPlayerEnabled = false;
            }
            else
            {
                targetPlayer = QuickMenuEx.SelectedUserLocal.field_Private_IUser_0;
                if (targetPlayer != null)
                {
                    ItemOrbitPlayerEnabled = false;
                    ItemOrbitSwastikaPlayerEnabled = true;
                }
            }
        }


        public override void OnUpdate()
        {
            if (ItemOrbitPlayerEnabled)
            {
                if (target == null)
                {
                    target = new GameObject();
                }

                VRCPlayer player = null;
                try
                {
                    player = PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(targetPlayer.prop_String_0)
                        ._vrcplayer;
                }
                catch
                {
                    ItemOrbit();
                }
                if (player == null)
                    return;

                var transform = player.transform;
                var playerPosition = transform.position;
                target.transform.position = playerPosition + new Vector3(0f, 1f, 0f);
                target.transform.Rotate(new Vector3(0f, 380f * Time.time * 1.5f, 0f));
                for (int i = 0; i < vrc_Pickups.Length; i++)
                {
                    VRC_Pickup vrc_Pickup = vrc_Pickups[i];
                    if (Networking.GetOwner(vrc_Pickup.gameObject) != Networking.LocalPlayer)
                    {
                        Networking.SetOwner(Networking.LocalPlayer, vrc_Pickup.gameObject);
                    }
                    vrc_Pickup.transform.position = target.transform.position + target.transform.forward * 1.5f;
                    target.transform.Rotate(new Vector3(0f, 380 / vrc_Pickups.Length, 0f));
                }
            }
        }


        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            vrc_Pickups = Object.FindObjectsOfType<VRC_Pickup>();
        }

    }
}