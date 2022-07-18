﻿using Il2CppSystem.Collections.Generic;
using MelonLoader;
using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;
using System.Collections;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRCSDK2;

namespace Serpent.Exploits
{
    internal class ItemLagger : ModComponent
    {
        public static bool _ItemLagEnabled;
        private static ReMenuToggle _ItemLagToggled;
        private ReMenuButton RespawnPickups;
        public static System.Collections.Generic.List<VRCSDK2.VRC_Pickup> PickupsSDK2 = new System.Collections.Generic.List<VRCSDK2.VRC_Pickup>();
        public static System.Collections.Generic.List<VRCPickup> PickupsSDK3 = new System.Collections.Generic.List<VRCPickup>();
        public static System.Collections.Generic.List<VRC_ObjectSync> PickupsSync = new System.Collections.Generic.List<VRC_ObjectSync>();
        public static List<GameObject> AllPickups = new List<GameObject>();

        public ItemLagger()
        {
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var exploitMenu = uiManager.MainMenu.GetCategoryPage(Page.PageNames.Monkey).AddCategory("Items");
            _ItemLagToggled = exploitMenu.AddToggle("Item Lag", "Lags players by repeatedly respawning objects.", StartItemLag, _ItemLagEnabled);
            RespawnPickups = exploitMenu.AddButton("Respawn Pickups",
                "Respawns all pickups in the instance (useful after using item lagger.)",
                () =>
                {
                    AllPickups = GrabAllPickups();

                    foreach (GameObject allPickup in AllPickups)
                    {
                        Networking.SetOwner(VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCPlayerApi_0,
                            allPickup.gameObject);
                        allPickup.gameObject.transform.position = new Vector3(0f, -500f, 0f);
                    }
                });
        }

        public void StartItemLag(bool value)
        {
            _ItemLagEnabled = value;
            _ItemLagToggled?.Toggle(value);
            PickupsSDK3 = VRCPickups();
            PickupsSDK2 = VRCSDK2Pickups();
            AllPickups = GrabAllPickups();
            PickupsSync = VRCObjectSyncObjects();

            if (_ItemLagEnabled)
            {
                MelonCoroutines.Start(PerformItemLag());
            }
        }

        public static System.Collections.Generic.List<VRCPickup> VRCPickups()
        {
            return Resources.FindObjectsOfTypeAll<VRCPickup>().ToList();
        }

        public static System.Collections.Generic.List<VRCSDK2.VRC_Pickup> VRCSDK2Pickups()
        {
            return Resources.FindObjectsOfTypeAll<VRCSDK2.VRC_Pickup>().ToList();
        }

        public static System.Collections.Generic.List<VRC_ObjectSync> VRCObjectSyncObjects()
        {
            return Resources.FindObjectsOfTypeAll<VRC_ObjectSync>().ToList();
        }

        public static List<GameObject> GrabAllPickups()
        {
            List<GameObject> list = new List<GameObject>();
            foreach (VRCSDK2.VRC_Pickup item in VRCSDK2Pickups())
            {
                list.Add(item.gameObject);
            }
            foreach (VRCPickup item2 in VRCPickups())
            {
                list.Add(item2.gameObject);
            }
            foreach (VRC_ObjectSync item3 in VRCObjectSyncObjects())
            {
                list.Add(item3.gameObject);
            }
            return list;
        }

        public static IEnumerator PerformItemLag()
        {
            Vector3 LastPos = VRCPlayer.field_Internal_Static_VRCPlayer_0._player.transform.position + new Vector3(0f, 0.1f, 0f);


            foreach (VRCSDK2.VRC_Pickup item in Resources.FindObjectsOfTypeAll<VRCSDK2.VRC_Pickup>().ToList())
            {
                try
                {
                    if (item.GetComponent<Rigidbody>() != null)
                    {
                        item.GetComponent<Rigidbody>().mass = 2.14748365E+09f;
                        item.GetComponent<Rigidbody>().useGravity = true;
                        item.GetComponent<Rigidbody>().velocity = new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f);
                        item.GetComponent<Rigidbody>().maxAngularVelocity = 2.14748365E+09f;
                        item.GetComponent<Rigidbody>().maxDepenetrationVelocity = 2.14748365E+09f;
                        item.GetComponent<Rigidbody>().isKinematic = false;
                        item.GetComponent<Rigidbody>().AddForce(new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f), ForceMode.Acceleration);
                        item.GetComponent<Rigidbody>().AddForce(new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f), ForceMode.Force);
                        item.GetComponent<Rigidbody>().AddForce(new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f), ForceMode.Impulse);
                        item.GetComponent<Rigidbody>().AddForce(new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f), ForceMode.VelocityChange);
                        item.GetComponent<Rigidbody>().angularVelocity = new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f);
                        item.ThrowVelocityBoostMinSpeed = 2.14748365E+09f;
                        item.ThrowVelocityBoostScale = 2.14748365E+09f;
                    }
                    if (item.GetComponent<Collider>() != null)
                    {
                        item.GetComponent<Collider>().enabled = false;
                    }
                    item.ThrowVelocityBoostMinSpeed = 2.14748365E+09f;
                    item.ThrowVelocityBoostScale = 2.14748365E+09f;
                }

                catch // probably caused because one of these pickups don't exist, so just throw it away
                {
                }
            }



            foreach (VRCPickup item2 in Resources.FindObjectsOfTypeAll<VRCPickup>().ToList())
            {
                try
                {

                    if (item2.GetComponent<Rigidbody>() != null)
                    {
                        item2.GetComponent<Rigidbody>().mass = 2.14748365E+09f;
                        item2.GetComponent<Rigidbody>().useGravity = true;
                        item2.GetComponent<Rigidbody>().velocity = new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f);
                        item2.GetComponent<Rigidbody>().maxAngularVelocity = 2.14748365E+09f;
                        item2.GetComponent<Rigidbody>().maxDepenetrationVelocity = 2.14748365E+09f;
                        item2.GetComponent<Rigidbody>().isKinematic = false;
                        item2.GetComponent<Rigidbody>().AddForce(new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f), ForceMode.Acceleration);
                        item2.GetComponent<Rigidbody>().AddForce(new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f), ForceMode.Force);
                        item2.GetComponent<Rigidbody>().AddForce(new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f), ForceMode.Impulse);
                        item2.GetComponent<Rigidbody>().AddForce(new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f), ForceMode.VelocityChange);
                        item2.GetComponent<Rigidbody>().angularVelocity = new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f);
                        item2.ThrowVelocityBoostMinSpeed = 2.14748365E+09f;
                        item2.ThrowVelocityBoostScale = 2.14748365E+09f;
                    }
                    if (item2.GetComponent<Collider>() != null)
                    {
                        item2.GetComponent<Collider>().enabled = false;
                    }
                    item2.ThrowVelocityBoostMinSpeed = 2.14748365E+09f;
                    item2.ThrowVelocityBoostScale = 2.14748365E+09f;
                }

                catch // probably caused because one of these pickups don't exist, so just throw it away
                {
                }
            }



            foreach (VRC_ObjectSync item3 in Resources.FindObjectsOfTypeAll<VRC_ObjectSync>().ToList())
            {
                try
                {
                    if (item3.GetComponent<Rigidbody>() != null)
                    {
                        item3.GetComponent<Rigidbody>().mass = 2.14748365E+09f;
                        item3.GetComponent<Rigidbody>().useGravity = true;
                        item3.GetComponent<Rigidbody>().velocity = new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f);
                        item3.GetComponent<Rigidbody>().maxAngularVelocity = 2.14748365E+09f;
                        item3.GetComponent<Rigidbody>().maxDepenetrationVelocity = 2.14748365E+09f;
                        item3.GetComponent<Rigidbody>().isKinematic = false;
                        item3.GetComponent<Rigidbody>().AddForce(new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f), ForceMode.Acceleration);
                        item3.GetComponent<Rigidbody>().AddForce(new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f), ForceMode.Force);
                        item3.GetComponent<Rigidbody>().AddForce(new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f), ForceMode.Impulse);
                        item3.GetComponent<Rigidbody>().AddForce(new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f), ForceMode.VelocityChange);
                        item3.GetComponent<Rigidbody>().angularVelocity = new Vector3(2.14748365E+09f, 2.14748365E+09f, 2.14748365E+09f);
                    }
                    if (item3.GetComponent<Collider>() != null)
                    {
                        item3.GetComponent<Collider>().enabled = false;
                    }
                }
                catch // probably caused because one of these pickups don't exist, so just throw it away
                {
                }
            }


            while (_ItemLagEnabled)
            {
                foreach (GameObject allPickup in AllPickups)
                {
                    Networking.SetOwner(VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCPlayerApi_0, allPickup.gameObject);
                    allPickup.gameObject.transform.position = LastPos + new Vector3(0f, 1f, 0f);
                }
                yield return new WaitForSeconds(1f);
            }
        }
    }
}