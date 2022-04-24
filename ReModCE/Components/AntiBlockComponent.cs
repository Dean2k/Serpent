using System;
using ExitGames.Client.Photon;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Photon.Realtime;
using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;
using ReModAres.Core.VRChat;
using ReModCE_ARES.Core;
using ReModCE_ARES.Loader;
using ReModCE_ARES.Managers;
using UnityEngine;
using VRC.Core;
using VRC.Management;
using Object = UnityEngine.Object;
using Player = VRC.Player;

namespace ReModCE_ARES.Components
{
    internal class AntiBlockComponent : ModComponent
    {
        private static ConfigValue<bool> AntiBlockEnabled;
        private ReMenuToggle _antiblockToggle;

        private ConfigValue<bool> StateBlockedEnabled;
        private ReMenuToggle _stateBlockedToggle;

        public AntiBlockComponent()
        {
            AntiBlockEnabled = new ConfigValue<bool>(nameof(AntiBlockEnabled), true);
            AntiBlockEnabled.OnValueChanged += () => _antiblockToggle.Toggle(AntiBlockEnabled);

            StateBlockedEnabled = new ConfigValue<bool>(nameof(StateBlockedEnabled), true);
            StateBlockedEnabled.OnValueChanged += () => _stateBlockedToggle.Toggle(StateBlockedEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("ARES");
            var subMenu = menu.AddMenuPage("Anti-Block", "", ResourceManager.GetSprite("remodce.shield"));

            _antiblockToggle = subMenu.AddToggle("Anti-Block",
                "Enable / Disable the anti-block", AntiBlockEnabled.SetValue,
                AntiBlockEnabled);

            ReModCE_ARES.Harmony.Patch(typeof(LoadBalancingClient).GetMethod(nameof(LoadBalancingClient.OnEvent)), GetLocalPatch(nameof(OnEventPatch)), null);
        }

        private static bool OnEventPatch(ref EventData __0)
        {
            if (__0.Code == 33)
            {
                Dictionary<byte, Il2CppSystem.Object> moderationData = __0.Parameters[__0.CustomDataKey].Cast<Dictionary<byte, Il2CppSystem.Object>>();

                byte moderationType = moderationData[0].Unbox<byte>();

                switch (moderationType)
                {
                    case 21:
                        {
                            if (moderationData.ContainsKey(1) == true)
                            {
                                bool isBlocked = moderationData[10].Unbox<bool>();

                                PlayerDetails playerDetails = Wrapper.GetPlayerInformationByID(moderationData[1].Unbox<int>());

                                if (playerDetails != null)
                                {
                                    if (isBlocked)
                                    {
                                        VRCUiManagerEx.Instance.QueueHudMessage(playerDetails.displayName + " has blocked you.", Color.red);
                                        ReLogger.Msg(playerDetails.displayName + " has blocked you.", Color.red);
                                        ReModCE_ARES.LogDebug(playerDetails.displayName + " has blocked you.");

                                        if (AntiBlockEnabled)
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        VRCUiManagerEx.Instance.QueueHudMessage(playerDetails.displayName + " has unblocked you.", Color.green);
                                        ReLogger.Msg(playerDetails.displayName + " has unblocked you.", Color.green);
                                        ReModCE_ARES.LogDebug(playerDetails.displayName + " has unblocked you.");
                                    }
                                }
                            }

                            break;
                        }
                }
            }

            return true;
        }

    }
}
