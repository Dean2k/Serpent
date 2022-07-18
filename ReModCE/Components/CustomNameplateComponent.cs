using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;
using Serpent.Core;
using Serpent.Loader;
using Serpent.Managers;
using Serpent.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using VRC;

namespace Serpent.Components
{
    public class CustomNameplate : MonoBehaviour
    {
        public VRC.Player player;
        private byte frames;
        private byte ping;
        private int noUpdateCount = 0;
        private TextMeshProUGUI statsText;
        public bool OverRender;
        public bool Enabled = true;

        public CustomNameplate(IntPtr ptr) : base(ptr)
        {
        }

        private void Start()
        {
            if (Enabled)
            {
                Transform stats = UnityEngine.Object.Instantiate<Transform>(this.gameObject.transform.Find("Contents/Quick Stats"), this.gameObject.transform.Find("Contents"));
                stats.parent = this.gameObject.transform.Find("Contents");
                stats.name = "Nameplate";
                stats.localPosition = new Vector3(0f, 62f, 0f);
                stats.gameObject.SetActive(true);
                statsText = stats.Find("Trust Text").GetComponent<TextMeshProUGUI>();
                statsText.color = Color.white;
                if (OverRender && Enabled)
                {
                    statsText.isOverlay = true;
                }
                else
                {
                    statsText.isOverlay = false;
                }

                stats.Find("Trust Icon").gameObject.SetActive(false);
                stats.Find("Performance Icon").gameObject.SetActive(false);
                stats.Find("Performance Text").gameObject.SetActive(false);
                stats.Find("Friend Anchor Stats").gameObject.SetActive(false);

                frames = player._playerNet.field_Private_Byte_0;
                ping = player._playerNet.field_Private_Byte_1;
            }
        }

        private int skipX = 0;

        private void Update()
        {
            if (Enabled)
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
                if (skipX >= 50)
                {
                    string text = "<color=green>Stable</color>";
                    if (noUpdateCount > 30)
                        text = "<color=yellow>Lagging</color>";
                    if (noUpdateCount > 150)
                        text = "<color=red>Crashed</color>";
                    statsText.text = $"[{player.GetPlatform()}] |" + $" [{player.GetAvatarStatus()}] |" + $"{(player.GetIsMaster() ? " | [<color=#0352ff>HOST</color>] |" : "")}" + $" [{text}] |" + $" [FPS: {player.GetFramesColord()}] |" + $" [Ping: {player.GetPingColord()}] " + $" {(player.ClientDetect() ? " | [<color=red>ClientUser</color>]" : "")}";
                    skipX = 0;
                }
                else
                {
                    skipX++;
                }
            }
        }

        public void Dispose()
        {
            Enabled = false;
            statsText.gameObject.SetActive(false);
            statsText.text = null;
        }
    }

    internal sealed class CustomNameplateComponent : ModComponent
    {
        private ConfigValue<bool> CustomNameplateEnabled;
        private ReMenuToggle _CustomNameplateEnabled;

        private ConfigValue<bool> NamePlateOverRenderEnabled;
        private ReMenuToggle _namePlateOverRenderEnabled;

        private ConfigValue<bool> VRamShowEnabled;
        private ReMenuToggle _vRamShowEnabled;

        public CustomNameplateComponent()
        {
            CustomNameplateEnabled = new ConfigValue<bool>(nameof(CustomNameplateEnabled), true);
            CustomNameplateEnabled.OnValueChanged += () => _CustomNameplateEnabled.Toggle(CustomNameplateEnabled);

            NamePlateOverRenderEnabled = new ConfigValue<bool>(nameof(NamePlateOverRenderEnabled), true);
            NamePlateOverRenderEnabled.OnValueChanged += () => _namePlateOverRenderEnabled.Toggle(NamePlateOverRenderEnabled);

            VRamShowEnabled = new ConfigValue<bool>(nameof(VRamShowEnabled), false);
            VRamShowEnabled.OnValueChanged += () => _vRamShowEnabled.Toggle(VRamShowEnabled);
        }

        public override void OnPlayerJoined(VRC.Player player)
        {
            if (CustomNameplateEnabled)
            {
                CustomNameplate nameplate = player.transform.Find("Player Nameplate/Canvas/Nameplate").gameObject.AddComponent<CustomNameplate>();
                nameplate.player = player;
                nameplate.OverRender = NamePlateOverRenderEnabled;
            }

            player.gameObject.AddComponent<Mono.Platemanager>();
            Task.Run(() => GetTags(player));
        }

        public override void OnPlayerLeft(Player player)
        {
            try
            {
                alreadyGenerated.Remove(player.field_Private_APIUser_0.id);
            }
            catch { }
        }

        private static List<string> alreadyGenerated = new List<string>();
        private static Task GetTags(Player _Player)
        {
            if (alreadyGenerated.Contains(_Player.field_Private_APIUser_0.id)) return null;
            var _Req = (HttpWebRequest)WebRequest.Create($"https://api.ares-mod.com/records/VitalityPlates?filter=UserId,cs,{_Player.field_Private_APIUser_0.id}");
            using (var res = (HttpWebResponse)_Req.GetResponse())
            using (var stream = res.GetResponseStream())
            using (var Reader = new StreamReader(stream))
            {
                var ReaderValue = Reader.ReadToEnd();

                Serpent._Queue.Enqueue(new Action(() =>
                {
                    if (_Player == null)
                        return;
                    if (ReaderValue == "{\"records\":[]}")
                        return;
                    var _UserPlate = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(ReaderValue);

                    for (int i = 0; i < _UserPlate.records.Count; i++)
                    {
                        if (_UserPlate.records[i].Text.StartsWith("#animatedtag#"))
                        {
                            var AnimatedTag = _Player.GeneratePlate(_UserPlate.records[i].Text.Replace("#animatedtag#", String.Empty));
                            AnimatedTag.AddComponent<Mono.TagAnimation>()._Text = _UserPlate.records[i].Text.Replace("#animatedtag#", "");
                            continue;
                        }
                        if (_UserPlate.records[i].Text.StartsWith("#rainbow#"))
                        {
                            var AnimatedTag = _Player.GeneratePlate(_UserPlate.records[i].Text.Replace("#rainbow#", String.Empty));
                            AnimatedTag.AddComponent<Mono.TagRainbow>()._Text = _UserPlate.records[i].Text.Replace("#rainbow#", "");
                            continue;
                        }
                        _Player.GeneratePlate(_UserPlate.records[i].Text);
                    }
                    _Player.GeneratePlate(" ");
                    alreadyGenerated.Add(_Player.field_Private_APIUser_0.id);
                }));
                return null;
            }
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);
            var menu = uiManager.MainMenu.GetCategoryPage(Page.PageNames.Visuals).GetCategory(Page.Categories.Visuals.Nameplate);
            _CustomNameplateEnabled = menu.AddToggle("Custom Nameplates", "Enable/Disable custom nameplates (reload world to fully unload)", ToggleNameplates,
                CustomNameplateEnabled);
            _namePlateOverRenderEnabled = menu.AddToggle("Nameplate Over render", "Enable/Disable the over render of nameplates (reload world to fully unload)", NamePlateOverRenderEnabled.SetValue,
                NamePlateOverRenderEnabled);
            _vRamShowEnabled = menu.AddToggle("Show VRam usage", "This can cause more memory usage and lag spikes", VRamShowEnabled.SetValue,
                VRamShowEnabled);
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
                            nameplate.OverRender = NamePlateOverRenderEnabled;
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
            catch (Exception ex) { ReLogger.Msg(ex.Message); }
        }
    }


    public class Record
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }
    }

    public class Root
    {
        public List<Record> records { get; set; }
    }
}