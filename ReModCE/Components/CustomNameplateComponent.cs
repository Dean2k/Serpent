using Newtonsoft.Json;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE_ARES.Managers;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using VRC;

namespace ReModCE_ARES.Components
{
    public class CustomNameplate : MonoBehaviour
    {
        public VRC.Player player;
        private byte frames;
        private byte ping;
        private int noUpdateCount = 0;
        private TextMeshProUGUI statsText;
        private ImageThreeSlice background;
        public CustomNameplate(IntPtr ptr) : base(ptr)
        {
        }
        void Start()
        {
            if (this.enabled)
            {
                Transform stats = UnityEngine.Object.Instantiate<Transform>(this.gameObject.transform.Find("Contents/Quick Stats"), this.gameObject.transform.Find("Contents"));
                stats.parent = this.gameObject.transform.Find("Contents");
                stats.gameObject.SetActive(true);
                statsText = stats.Find("Trust Text").GetComponent<TextMeshProUGUI>();
                statsText.color = Color.white;
                stats.Find("Trust Icon").gameObject.SetActive(false);
                stats.Find("Performance Icon").gameObject.SetActive(false);
                stats.Find("Performance Text").gameObject.SetActive(false);
                stats.Find("Friend Anchor Stats").gameObject.SetActive(false);

                background = this.gameObject.transform.Find("Contents/Main/Background").GetComponent<ImageThreeSlice>();

                background._sprite = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.Find("Player Nameplate/Canvas/Nameplate/Contents/Main/Glow").GetComponent<ImageThreeSlice>()._sprite;
                background.color = Color.black;

                frames = player._playerNet.field_Private_Byte_0;
                ping = player._playerNet.field_Private_Byte_1;
            }
        }

        void Update()
        {
            if (this.enabled)
            {
                if (frames == player._playerNet.field_Private_Byte_0 && ping == player._playerNet.field_Private_Byte_1)
                {
                    noUpdateCount++;
                }
                else
                {
                    noUpdateCount = 0;
                }
                frames = player._playerNet.field_Private_Byte_0;
                ping = player._playerNet.field_Private_Byte_1;
                string text = "<color=green>Stable</color>";
                if (noUpdateCount > 30)
                    text = "<color=yellow>Lagging</color>";
                if (noUpdateCount > 150)
                    text = "<color=red>Crashed</color>";
                statsText.text = $"[{player.GetPlatform()}] |" + $" [{player.GetAvatarStatus()}] |" + $"{(player.GetIsMaster() ? " | [<color=#0352ff>HOST</color>] |" : "")}" + $" [{text}] |" + $" [FPS: {player.GetFramesColord()}] |" + $" [Ping: {player.GetPingColord()}] " + $" {(player.ClientDetect() ? " | [<color=red>ClientUser</color>]" : "")}";
            }
        }
        public void Dispose()
        {
            statsText.text = null;
            statsText.OnDisable();
            background.OnDisable();
            this.enabled = false;
        }
    }

    internal sealed class CustomNameplateComponent : ModComponent
    {
        private ConfigValue<bool> CustomNameplateEnabled;
        private ReMenuToggle _CustomNameplateEnabled;

        public CustomNameplateComponent()
        {
            CustomNameplateEnabled = new ConfigValue<bool>(nameof(CustomNameplateEnabled), true);
            CustomNameplateEnabled.OnValueChanged += () => _CustomNameplateEnabled.Toggle(CustomNameplateEnabled);
        }

        public override void OnPlayerJoined(VRC.Player player)
        {
            if (CustomNameplateEnabled)
            {
                CustomNameplate nameplate = player.transform.Find("Player Nameplate/Canvas/Nameplate").gameObject.AddComponent<CustomNameplate>();
                nameplate.player = player;
            }
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);
            var menu = uiManager.MainMenu.GetCategoryPage("Visuals").GetCategory("Nameplate");
            _CustomNameplateEnabled = menu.AddToggle("Custom Nameplates", "Enable/Disable custom nameplates (reload world to fully unload)", ToggleNameplates,
                CustomNameplateEnabled);
        }

        private void ToggleNameplates(bool value)
        {
            CustomNameplateEnabled.SetValue(value);
            try
            {
                if (value)
                {
                    try
                    {
                        foreach (Player player in UnityEngine.Object.FindObjectsOfType<Player>())
                        {

                            CustomNameplate nameplate = player.transform.Find("Player Nameplate/Canvas/Nameplate").gameObject.AddComponent<CustomNameplate>();
                            nameplate.player = player;

                        }
                    }
                    catch { }
                }
                else
                {
                    foreach (Player player in UnityEngine.Object.FindObjectsOfType<Player>())
                    {
                        CustomNameplate disabler = player.transform.Find("Player Nameplate/Canvas/Nameplate").gameObject.GetComponent<CustomNameplate>();
                        disabler.Dispose();
                    }
                }
            }
            catch { }
        }
    }
}
