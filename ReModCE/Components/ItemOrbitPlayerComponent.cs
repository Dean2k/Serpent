using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE_ARES.Loader;
using ReModCE_ARES.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.DataModel;
using VRC.SDKBase;
using VRC.Udon;
using VRC.UI;

namespace ReModCE_ARES.Components
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
            targetMenu.AddButton("Swastika Orbit", "Makes a Swastika around the targeted player.", ItemSwastika, ResourceManager.GetSprite("remodce.link"));

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
                var player = PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(targetPlayer.prop_String_0)._vrcplayer;
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
            if (ItemOrbitSwastikaPlayerEnabled)
            {

                var player = PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(targetPlayer.prop_String_0)._vrcplayer;
                if (player == null)
                    return;

                var transform = player.transform;
                var playerPosition = transform.position;

                try
                {
                    for (int i = 0; i < vrc_Pickups.Length; i++)
                    {
                        VRC_Pickup vrc_Pickup = vrc_Pickups[i];
                        if (Networking.GetOwner(vrc_Pickup.gameObject) != Networking.LocalPlayer)
                        {
                            Networking.SetOwner(Networking.LocalPlayer, vrc_Pickup.gameObject);
                        }
                    }
                    vrc_Pickups[0].transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
                    vrc_Pickups[1].transform.position = new Vector3(playerPosition.x, playerPosition.y + 0.5f, playerPosition.z);
                    vrc_Pickups[2].transform.position = new Vector3(playerPosition.x, playerPosition.y + 1f, playerPosition.z);
                    vrc_Pickups[3].transform.position = new Vector3(playerPosition.x, playerPosition.y + 1.5f, playerPosition.z);
                    vrc_Pickups[4].transform.position = new Vector3(playerPosition.x, playerPosition.y + 2f, playerPosition.z);
                    vrc_Pickups[17].transform.position = new Vector3(playerPosition.x, playerPosition.y + 0.5f, playerPosition.z);
                    vrc_Pickups[18].transform.position = new Vector3(playerPosition.x, playerPosition.y + 1f, playerPosition.z);
                    vrc_Pickups[19].transform.position = new Vector3(playerPosition.x, playerPosition.y + 1.5f, playerPosition.z);
                    vrc_Pickups[20].transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
                    vrc_Pickups[21].transform.position = new Vector3(playerPosition.x, playerPosition.y + 2.5f, playerPosition.z);
                    vrc_Pickups[22].transform.position = new Vector3(playerPosition.x, playerPosition.y + 3f, playerPosition.z);



                    vrc_Pickups[5].transform.position = new Vector3(playerPosition.x + 0.5f, playerPosition.y + 1.5f, playerPosition.z);
                    vrc_Pickups[6].transform.position = new Vector3(playerPosition.x + 1f, playerPosition.y + 1.5f, playerPosition.z);
                    vrc_Pickups[7].transform.position = new Vector3(playerPosition.x - 0.5f, playerPosition.y + 1.5f, playerPosition.z);
                    vrc_Pickups[8].transform.position = new Vector3(playerPosition.x - 1f, playerPosition.y + 1.5f, playerPosition.z);
                    vrc_Pickups[27].transform.position = new Vector3(playerPosition.x + 1.5f, playerPosition.y + 1.5f, playerPosition.z);
                    vrc_Pickups[28].transform.position = new Vector3(playerPosition.x - 1.5f, playerPosition.y + 1.5f, playerPosition.z);

                    vrc_Pickups[9].transform.position = new Vector3(playerPosition.x + 1.5f, playerPosition.y + 1.7f, playerPosition.z);
                    vrc_Pickups[10].transform.position = new Vector3(playerPosition.x + 1.5f, playerPosition.y + 1.9f, playerPosition.z);
                    vrc_Pickups[23].transform.position = new Vector3(playerPosition.x + 1.5f, playerPosition.y + 2.1f, playerPosition.z);

                    vrc_Pickups[11].transform.position = new Vector3(playerPosition.x - 1.5f, playerPosition.y + 1.3f, playerPosition.z);
                    vrc_Pickups[12].transform.position = new Vector3(playerPosition.x - 1.5f, playerPosition.y + 1.1f, playerPosition.z);
                    vrc_Pickups[24].transform.position = new Vector3(playerPosition.x - 1.5f, playerPosition.y + 1.3f, playerPosition.z);

                    vrc_Pickups[13].transform.position = new Vector3(playerPosition.x - 0.2f, playerPosition.y + 3f, playerPosition.z);
                    vrc_Pickups[14].transform.position = new Vector3(playerPosition.x - 0.4f, playerPosition.y + 3f, playerPosition.z);
                    vrc_Pickups[25].transform.position = new Vector3(playerPosition.x - 0.6f, playerPosition.y + 3f, playerPosition.z);

                    vrc_Pickups[15].transform.position = new Vector3(playerPosition.x + 0.2f, playerPosition.y, playerPosition.z);
                    vrc_Pickups[16].transform.position = new Vector3(playerPosition.x + 0.4f, playerPosition.y, playerPosition.z);
                    vrc_Pickups[26].transform.position = new Vector3(playerPosition.x + 0.6f, playerPosition.y, playerPosition.z);

                }
                catch { }
            }
        }


        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            vrc_Pickups = Object.FindObjectsOfType<VRC_Pickup>();
        }

    }
}