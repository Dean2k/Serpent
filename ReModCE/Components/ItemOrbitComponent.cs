﻿using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace Serpent.Components
{
    internal sealed class ItemOrbitComponent : ModComponent
    {
        private ConfigValue<bool> ItemOrbitEnabled;
        private ReMenuToggle _itemOrbitEnabled;
        private GameObject target;
        public static VRC_Pickup[] vrc_Pickups;

        public ItemOrbitComponent()
        {
            ItemOrbitEnabled = new ConfigValue<bool>(nameof(ItemOrbitEnabled), false);
            ItemOrbitEnabled.OnValueChanged += () => _itemOrbitEnabled.Toggle(ItemOrbitEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetCategoryPage(Page.PageNames.Monkey).AddCategory("Item Orbit");
            _itemOrbitEnabled = menu.AddToggle("Item Orbit",
                "Makes all Pickups spin arround you.", ItemOrbit,
                ItemOrbitEnabled);
        }

        private void ItemOrbit(bool enable)
        {
            ItemOrbitEnabled.SetValue(enable);
        }

        public override void OnUpdate()
        {
            if (ItemOrbitEnabled)
            {
                if (target == null)
                {
                    target = new GameObject();
                }
                target.transform.position = Player.prop_Player_0.transform.position + new Vector3(0f, 1f, 0f);
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

        public override void OnPlayerJoined(Player player)
        {
            if (player == Player.prop_Player_0)
            {
                initWorldProps();
            }
        }

        private void initWorldProps()
        {
            vrc_Pickups = Object.FindObjectsOfType<VRC_Pickup>();
        }
    }
}