using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE_ARES.Core;
using ReModCE_ARES.Loader;
using ReModCE_ARES.Managers;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;
using VRC.Management;

using HarmonyLib;
using Il2CppSystem.Collections;
using VRC.UI;
using VRC.UI.Elements;

namespace ReModCE_ARES.Components
{

    internal class AvatarHiderComponent : ModComponent
    {
        private ConfigValue<bool> HideAvatarsEnabled;
        private ConfigValue<bool> DisableSpawnSoundEnabled;
        private ConfigValue<bool> LimitAudioDistanceEnabled;
        private ConfigValue<float> MaxAudioDistance;
        private ConfigValue<float> HideDistance;
        private ReMenuToggle _hideAvatarsToggle;
        private ReMenuToggle _disableSpawnSoundToggle;
        private ReMenuToggle _limitAudioDistanceToggle;
        private ReMenuButton _maxAudioDistanceButton;
        private ReMenuButton _hideDistanceButton;


        public AvatarHiderComponent()
        {
            HideAvatarsEnabled = new ConfigValue<bool>(nameof(HideAvatarsEnabled), false);
            HideAvatarsEnabled.OnValueChanged += () => _hideAvatarsToggle.Toggle(HideAvatarsEnabled);
            DisableSpawnSoundEnabled = new ConfigValue<bool>(nameof(DisableSpawnSoundEnabled), false);
            DisableSpawnSoundEnabled.OnValueChanged += () => _disableSpawnSoundToggle.Toggle(DisableSpawnSoundEnabled);
            LimitAudioDistanceEnabled = new ConfigValue<bool>(nameof(LimitAudioDistanceEnabled), false);
            LimitAudioDistanceEnabled.OnValueChanged += () => _limitAudioDistanceToggle.Toggle(LimitAudioDistanceEnabled);

            MaxAudioDistance = new ConfigValue<float>(nameof(MaxAudioDistance), 15f);
            HideDistance = new ConfigValue<float>(nameof(HideDistance), 15f);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            try
            {
                var menu = uiManager.MainMenu.GetMenuPage("ARES");
                var subMenu = menu.AddMenuPage("Avatar Hider", "hide avatars at a certain distance", ResourceManager.GetSprite("remodce.arms-up"));
                _hideAvatarsToggle = subMenu.AddToggle("Hide Avatars",
                    "Enable avatar distance hiding", SetAvatarHidden,
                    HideAvatarsEnabled);
                _disableSpawnSoundToggle = subMenu.AddToggle("Spawn Sounds",
                    "Disable spawn sounds", SetSoundDisable,
                    DisableSpawnSoundEnabled);
                _limitAudioDistanceToggle = subMenu.AddToggle("Limit Audio Distance",
                    "Disable spawn sounds", LimitAudioDistance,
                    LimitAudioDistanceEnabled);
                _maxAudioDistanceButton = subMenu.AddButton($"Max Audio Distance: {MaxAudioDistance}",
                    "Set the maximum audio distance (Meters)",
                    () =>
                    {
                        VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Max Audio Distance",
                            MaxAudioDistance.ToString(), InputField.InputType.Standard, true, "Submit",
                            (s, k, t) =>
                            {
                                if (string.IsNullOrEmpty(s))
                                    return;

                                if (!float.TryParse(s, out var maxAudioDist))
                                    return;
                                MaxAudioDistance.SetValue(maxAudioDist);
                                _maxAudioDistanceButton.Text = $"Max Audio Distance: {MaxAudioDistance}";
                            }, null);
                    }, ResourceManager.GetSprite("remodce.max"));
                _hideDistanceButton = subMenu.AddButton($"Max Avatar Distance: {HideDistance}",
                    "Set the maximum Avatar distance (Meters)",
                    () =>
                    {
                        VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Max Audio Distance",
                            HideDistance.ToString(), InputField.InputType.Standard, true, "Submit",
                            (s, k, t) =>
                            {
                                if (string.IsNullOrEmpty(s))
                                    return;

                                if (!float.TryParse(s, out var maxAvatarDist))
                                    return;
                                HideDistance.SetValue(maxAvatarDist);
                                _hideDistanceButton.Text = $"Max Avatar Distance: {HideDistance}";
                            }, null);
                    }, ResourceManager.GetSprite("remodce.max"));
            } catch
            {
                ReLogger.Msg("Menu set error");
            }

            MelonCoroutines.Start(AvatarScanner());
        }

        private void SetAvatarHidden(bool value)
        {
            HideAvatarsEnabled.SetValue(value);
            if (!value)
            {
                UnHideAvatars();
            }
        }

        private void SetSoundDisable(bool value)
        {
            DisableSpawnSoundEnabled.SetValue(value);
        }

        private void LimitAudioDistance(bool value)
        {
            LimitAudioDistanceEnabled.SetValue(value);
        }

        private void UnHideAvatars()
        {
            try
            {
                foreach (VRC.Player player in Wrapper.GetAllPlayers())
                {
                    if (player == null || player.IsMe())
                        continue;

                    GameObject avtrObject = player.GetAvatarObject();
                    if (avtrObject == null || avtrObject.active)
                        continue;

                    avtrObject.SetActive(true);
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg(ConsoleColor.Red, $"Failed to unhide avatar: {e}");
            }
        }

        private System.Collections.IEnumerator AvatarScanner()
        {
            while (true)
            {
                if (HideAvatarsEnabled && Wrapper.GetLocalVRCPlayer() != null)
                {
                    foreach (VRC.Player player in Wrapper.GetAllPlayers().Where(p => p != null && !p.IsMe() && p.prop_APIUser_0 != null))
                    {
                        try
                        {
                            APIUser apiUser = player.prop_APIUser_0;
                            GameObject avtrObject = player.GetAvatarObject();
                            if (avtrObject == null)
                                continue;

                            float dist = Vector3.Distance(Wrapper.GetLocalVRCPlayer().transform.position, avtrObject.transform.position);
                            bool isActive = avtrObject.active;

                            if (HideAvatarsEnabled && isActive && dist > HideDistance)
                                avtrObject.SetActive(false);
                            else if (HideAvatarsEnabled && !isActive && dist <= HideDistance)
                                avtrObject.SetActive(true);
                            else if (!HideAvatarsEnabled && !isActive)
                                avtrObject.SetActive(true);

                            if (DisableSpawnSoundEnabled)
                                avtrObject.StopSpawnSounds();

                        }
                        catch (Exception e)
                        {
                            ReLogger.Msg(ConsoleColor.Red, $"Failed to hide avatar: {e}");
                        }
                        yield return new WaitForSeconds(.19f);
                    }
                }
                yield return new WaitForSeconds(.5f);
            }
        }
    }
}
