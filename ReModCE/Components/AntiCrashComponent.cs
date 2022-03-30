using System;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReModCE_ARES.Loader;
using ExitGames.Client.Photon;
using VRC;
using System.Collections.Generic;
using VRC.SDKBase;
using UnityEngine;
using ReMod.Core.VRChat;
using UnityEngine.UI;
using UnhollowerBaseLib;

namespace ReModCE_ARES.Components
{
    internal class AntiCrashComponent : ModComponent
    {
        public static Dictionary<int, Player> PlayersActorID = new Dictionary<int, Player>();
        public static int eventcount = 0;
        public static int eventlimit = 375;

        private ConfigValue<bool> AntiAvatarCrashEnabled;
        private ReMenuToggle _antiAvatarCrashToggle;

        private ConfigValue<int> MaxAudioSources;
        private ReMenuButton _maxAudioSourcesButton;
        private ConfigValue<int> MaxLightSources;
        private ReMenuButton _maxLightSourcesButton;
        private ConfigValue<int> MaxDynamicBonesColliders;
        private ReMenuButton _maxDynamicBonesCollidersButton;
        private ConfigValue<int> MaxPolys;
        private ReMenuButton _maxPolysButton;
        private ConfigValue<int> MaxMaterials;
        private ReMenuButton _maxMaterialsButton;
        private ConfigValue<int> MaxCloth;
        private ReMenuButton _maxClothButton;
        private ConfigValue<int> MaxColliders;
        private ReMenuButton _maxCollidersButton;
        private Shader defaultShader;

        private static string[] meshList = new string[5] { "125k", "medusa", "inside", "outside", "mill" };
        private static string[] shaderList = new string[141]
        {
            "dbtc", "crash", "nigger", "nigga", "n1gga", "n1gg@", "nigg@", "go home", "byebye", "distance based",
            "tess", "tessellation", "cr4sh", "die", "get fucked", "spryth", "hidden/", ".hidden/", "nigg", "distancebased",
            "fuck:screen:fuckery:fuckyou:vilar", "bluescreen", "butterfly:vrchat:particle", "bluescream", "custom/hyu", "ebola", "yeet", "kill:xiexe", "lag ", "/die",
            " die ", "tessel:fur:poiyomi:standard:noise", "thot", "eatingmy", "undetected", "retard", "retrd", "standard on", "kyuzu", "oof ",
            ".Star/Bacon", ".Woofaa/Medusa", "Custom/Custom", "DistanceBased", "Knuckles_Gang/Free Crash", "Kyran/E  G  G", "Medusa Crash/Standard", "onion", "Pretty", "Sprythu/Why is my screen black",
            "custom/oof", "kys", "kos", "??", "yeeet", "got em", "medusa", "nigs", "sfere", " rip ",
            "/rip:/ripple", "sanity", "school", "shooter", "bacon", ".star:metal", "umbrella", "_bpu", "clap", "cooch:mochie",
            "sprythu", "bpu", "atençao", "izzyfrag", "izzy", "fragm", "shinigami:vhs:eye:vision", "clap shader", "clapped", "clapper",
            " clap ", "/clap", "world clap", "killer", ".blaze", "gang:troll:doppel", "makochill", "dead:sins", "death", "coffin",
            "onion", "suspicious", "darkwing", "keylime", "efrag", "yfrag", "brr", "temmie", "basically standard", "rampag",
            "reap", "uber shader 1.2", "C4", "2edgy", "lag:plague", "thotkyuzu", "loops", "overwatch:shader", "slay", "90hz:glasses",
            "autism", "penis", "randomname", "careful", "hurts", "truepink", "aнти", "Уфф", "рендер", "Это",
            "Ñoño", "nuke:almgp", "login", "go home", "ban:band", "buddy", "üõõüõ", "卍", "no sharing", "luka/megaae10",
            ".NEK0/Screen/Radial Blurr Zoom", "DocMe/BeautifulShaderv0.21", "Huyami/Ultrashader", "Leviant's Shaders/ScreenSpace Ubershader v2.7", "Leviant's Shaders/ScreenSpace Ubershader v2.7.3", "Leviant's Shaders/UberShader v2.9", "Magic3000/ScreenSpace", "Magic3000/ScreenSpacePub", "Magic3000/RGB-Glitch", "NEK0/Screen/Fade Screen v1",
            "VoidyShaders/Cave"
        };

        public AntiCrashComponent()
        {
            defaultShader = Shader.Find("VRChat/PC/Toon Lit Cutout");

            AntiAvatarCrashEnabled = new ConfigValue<bool>(nameof(AntiAvatarCrashEnabled), true);
            AntiAvatarCrashEnabled.OnValueChanged += () => _antiAvatarCrashToggle.Toggle(AntiAvatarCrashEnabled);

            MaxAudioSources = new ConfigValue<int>(nameof(MaxAudioSources), 10);
            MaxAudioSources.OnValueChanged += () => _maxAudioSourcesButton.Text = $"Max Audio Sources: {MaxAudioSources}";

            MaxLightSources = new ConfigValue<int>(nameof(MaxLightSources), 0);
            MaxLightSources.OnValueChanged += () => _maxLightSourcesButton.Text = $"Max Light Sources: {MaxLightSources}";

            MaxDynamicBonesColliders = new ConfigValue<int>(nameof(MaxDynamicBonesColliders), 5);
            MaxDynamicBonesColliders.OnValueChanged += () => _maxDynamicBonesCollidersButton.Text = $"Max Light Sources: {MaxDynamicBonesColliders}";

            MaxPolys = new ConfigValue<int>(nameof(MaxPolys), 200000);
            MaxPolys.OnValueChanged += () => _maxPolysButton.Text = $"Max Polys: {MaxPolys}";

            MaxMaterials = new ConfigValue<int>(nameof(MaxMaterials), 20);
            MaxMaterials.OnValueChanged += () => _maxMaterialsButton.Text = $"Max Materials: {MaxMaterials}";

            MaxCloth = new ConfigValue<int>(nameof(MaxCloth), 1);
            MaxCloth.OnValueChanged += () => _maxClothButton.Text = $"Max Cloth: {MaxCloth}";

            MaxColliders = new ConfigValue<int>(nameof(MaxColliders), 0);
            MaxColliders.OnValueChanged += () => _maxCollidersButton.Text = $"Max Colliders: {MaxColliders}";
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);         

            var aresMenu = uiManager.MainMenu.GetMenuPage("ARES").GetMenuPage("Anti-Crash");

            _antiAvatarCrashToggle = aresMenu.AddToggle("Anti Avatar Crash",
                "Enable/Disable Avatar Anti Crash", AntiAvatarCrashEnabled.SetValue,
                AntiAvatarCrashEnabled);
            _maxAudioSourcesButton = aresMenu.AddButton($"Max Audio Sources: {MaxAudioSources}", "Limit Avatar Audio Sources", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max Audio Sources", MaxAudioSources.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        MaxAudioSources.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));
            _maxLightSourcesButton = aresMenu.AddButton($"Max Light Sources: {MaxLightSources}", "Limit Avatar Light Sources", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max Light Sources", MaxLightSources.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        MaxLightSources.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));
            _maxDynamicBonesCollidersButton = aresMenu.AddButton($"Max Dynamic Bone Colliders: {MaxDynamicBonesColliders}", "Limit Dynamic Bone Colliders", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max Dynamic Bone Colliders", MaxDynamicBonesColliders.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        MaxDynamicBonesColliders.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));
            _maxPolysButton = aresMenu.AddButton($"Max Polys: {MaxPolys}", "Limit Poly count", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max Polys", MaxPolys.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        MaxPolys.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));
            _maxMaterialsButton = aresMenu.AddButton($"Max Materials: {MaxMaterials}", "Limit material count", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max material", MaxMaterials.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        MaxMaterials.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));
            _maxClothButton = aresMenu.AddButton($"Max Cloth: {MaxCloth}", "Limit Cloth count", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max Cloth", MaxCloth.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        MaxCloth.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));
            _maxCollidersButton = aresMenu.AddButton($"Max Colliders: {MaxColliders}", "Limit Colliders count", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max material", MaxColliders.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        MaxColliders.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));

        }

        public override bool OnEvent(EventData eventData)
        {
            return true;
        }

        public override bool OnAvatarAssetBundleLoad(GameObject avatar, string avatarID)
        {
            if (AntiAvatarCrashEnabled)
            {
                SkinnedMeshRenderer[] array = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
                MeshFilter[] array2 = avatar.GetComponentsInChildren<MeshFilter>(includeInactive: true);
                foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
                {
                    bool flag = false;
                    if (!skinnedMeshRenderer.sharedMesh.isReadable)
                    {
                        UnityEngine.Object.DestroyImmediate(skinnedMeshRenderer, allowDestroyingAssets: true);
                        ReLogger.Msg("[AnitCrash] deleted unreadable Mesh");
                        continue;
                    }
                    for (int j = 0; j < meshList.Length; j++)
                    {
                        if (skinnedMeshRenderer.name.ToLower().Contains(meshList[j]))
                        {
                            ReLogger.Msg("[AnitCrash] deleted blackListed Mesh " + skinnedMeshRenderer.name);
                            UnityEngine.Object.DestroyImmediate(skinnedMeshRenderer, allowDestroyingAssets: true);
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        continue;
                    }
                    int num = 0;
                    for (int k = 0; k < skinnedMeshRenderer.sharedMesh.subMeshCount; k++)
                    {
                        num += skinnedMeshRenderer.sharedMesh.GetTriangles(k).Length / 3;
                        if (num >= MaxPolys)
                        {
                            UnityEngine.Object.DestroyImmediate(skinnedMeshRenderer, allowDestroyingAssets: true);
                            ReLogger.Msg("[AnitCrash] deleted Mesh with too many polys");
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        continue;
                    }
                    Material[] array3 = skinnedMeshRenderer.materials;
                    if (array3.Length >= MaxMaterials)
                    {
                        UnityEngine.Object.DestroyImmediate(skinnedMeshRenderer, allowDestroyingAssets: true);
                        ReLogger.Msg("[AnitCrash] deleted Mesh with " + array3.Length + " materials");
                        continue;
                    }
                    for (int l = 0; l < array3.Length; l++)
                    {
                        Shader shader = array3[l].shader;
                        for (int m = 0; m < shaderList.Length; m++)
                        {
                            if (shader.name.ToLower().Contains(shaderList[m]))
                            {
                                ReLogger.Msg("[AnitCrash] replaced Shader " + shader.name);
                                shader = defaultShader;
                            }
                        }
                    }
                }
                foreach (MeshFilter meshFilter in array2)
                {
                    if (!meshFilter.sharedMesh.isReadable)
                    {
                        UnityEngine.Object.DestroyImmediate(meshFilter, allowDestroyingAssets: true);
                        ReLogger.Msg("[AnitCrash] deleted unreadable Mesh");
                        continue;
                    }
                    bool flag2 = false;
                    for (int num2 = 0; num2 < meshList.Length; num2++)
                    {
                        if (meshFilter.name.ToLower().Contains(meshList[num2]))
                        {
                            ReLogger.Msg("[AnitCrash] deleted blackListed Mesh " + meshFilter.name);
                            UnityEngine.Object.DestroyImmediate(meshFilter, allowDestroyingAssets: true);
                            flag2 = true;
                            break;
                        }
                    }
                    if (flag2)
                    {
                        continue;
                    }
                    int num3 = 0;
                    for (int num4 = 0; num4 < meshFilter.sharedMesh.subMeshCount; num4++)
                    {
                        num3 += meshFilter.sharedMesh.GetTriangles(num4).Length / 3;
                        if (num3 >= MaxPolys)
                        {
                            UnityEngine.Object.DestroyImmediate(meshFilter, allowDestroyingAssets: true);
                            ReLogger.Msg("[AnitCrash] deleted Mesh with too many polys");
                            flag2 = true;
                            break;
                        }
                    }
                    if (flag2)
                    {
                        continue;
                    }
                    MeshRenderer component = meshFilter.gameObject.GetComponent<MeshRenderer>();
                    Material[] array4 = component.materials;
                    if (array4.Length >= MaxMaterials)
                    {
                        UnityEngine.Object.DestroyImmediate(meshFilter, allowDestroyingAssets: true);
                        ReLogger.Msg("[AnitCrash] deleted Mesh with " + array4.Length + " materials");
                        continue;
                    }
                    for (int num5 = 0; num5 < array4.Length; num5++)
                    {
                        Shader shader2 = array4[num5].shader;
                        for (int num6 = 0; num6 < shaderList.Length; num6++)
                        {
                            if (shader2.name.ToLower().Contains(shaderList[num6]))
                            {
                                ReLogger.Msg("[AnitCrash] replaced Shader " + shader2.name);
                                shader2 = defaultShader;
                            }
                        }
                    }
                }
                AudioSource[] array5 = avatar.GetComponentsInChildren<AudioSource>();
                if (array5.Length >= MaxAudioSources)
                {
                    for (int num7 = 0; num7 < MaxAudioSources; num7++)
                    {
                        UnityEngine.Object.DestroyImmediate(array5[num7].gameObject, allowDestroyingAssets: true);
                    }
                    ReLogger.Msg("[AnitCrash] deleted " + MaxAudioSources + " AudioSources");
                }
                Light[] array6 = avatar.GetComponentsInChildren<Light>();
                if (array6.Length >= MaxLightSources)
                {
                    for (int num8 = 0; num8 < MaxLightSources; num8++)
                    {
                        UnityEngine.Object.DestroyImmediate(array6[num8].gameObject, allowDestroyingAssets: true);
                    }
                    ReLogger.Msg("[AnitCrash] deleted " + MaxLightSources + " Lights");
                }
                Cloth[] array7 = avatar.GetComponentsInChildren<Cloth>();
                if (array7.Length >= MaxCloth)
                {
                    for (int num9 = 0; num9 < MaxCloth; num9++)
                    {
                        UnityEngine.Object.DestroyImmediate(array7[num9].gameObject, allowDestroyingAssets: true);
                    }
                    ReLogger.Msg("[AnitCrash] deleted " + MaxCloth + " Cloth");
                }
                Collider[] array8 = avatar.GetComponentsInChildren<Collider>();
                if (array8.Length >= MaxColliders)
                {
                    for (int num10 = 0; num10 < MaxColliders; num10++)
                    {
                        UnityEngine.Object.DestroyImmediate(array8[num10].gameObject, allowDestroyingAssets: true);
                    }
                    ReLogger.Msg("[AnitCrash] deleted " + MaxColliders + " Colliders");
                }
                DynamicBoneCollider[] array9 = avatar.GetComponentsInChildren<DynamicBoneCollider>();
                if (array9.Length >= MaxDynamicBonesColliders)
                {
                    for (int num11 = 0; num11 < MaxDynamicBonesColliders; num11++)
                    {
                        UnityEngine.Object.DestroyImmediate(array9[num11].gameObject, allowDestroyingAssets: true);
                    }
                    ReLogger.Msg("[AnitCrash] deleted " + MaxDynamicBonesColliders + " DynamicBoneColliders");
                }
                return true;
            }
            return true;
        }

    }
}
