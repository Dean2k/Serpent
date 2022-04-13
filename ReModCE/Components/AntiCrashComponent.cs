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
using HarmonyLib;
using VRC.Core;

namespace ReModCE_ARES.Components
{
    internal class AntiCrashComponent : ModComponent
    {
        public static Dictionary<int, Player> PlayersActorId = new();

        private static ConfigValue<bool> _antiAvatarCrashEnabled;
        private static ReMenuToggle _antiAvatarCrashToggle;

        private static ConfigValue<int> _maxAudioSources;
        private static ReMenuButton _maxAudioSourcesButton;
        private static ConfigValue<int> _maxLightSources;
        private static ReMenuButton _maxLightSourcesButton;
        private static ConfigValue<int> _maxDynamicBonesColliders;
        //private static ReMenuButton _maxDynamicBonesCollidersButton;
        private static ConfigValue<int> _maxPolys;
        private static ReMenuButton _maxPolysButton;
        private static ConfigValue<int> _maxMaterials;
        private static ReMenuButton _maxMaterialsButton;
        private static ConfigValue<int> _maxCloth;
        private static ReMenuButton _maxClothButton;
        private static ConfigValue<int> _maxColliders;
        private static ReMenuButton _maxCollidersButton;
        private static readonly Shader defaultShader = Shader.Find("VRChat/PC/Toon Lit Cutout");

        private static readonly string[] _meshList = new string[5] { "125k", "medusa", "inside", "outside", "mill" };
        private static readonly string[] _shaderList = new string[139]
        {
            "dbtc", "crash", "nigger", "nigga", "n1gga", "n1gg@", "nigg@", "go home", "byebye", "distance based", "rolltheredfire",
            "tess", "tessellation", "cr4sh", "die", "get fucked", "spryth", "nigg", "distancebased",
            "fuck:screen:fuckery:fuckyou:vilar", "bluescreen", "butterfly:vrchat:particle", "bluescream", "custom/hyu", "ebola", "yeet", "kill:xiexe", "lag ", "/die",
            " die ", "thot", "eatingmy", "undetected", "retard", "retrd", "standard on", "kyuzu", "oof ",
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
            ReLogger.Msg("patching OnEvent");

            ReModCE_ARES.Harmony.Patch(typeof(Photon.Realtime.LoadBalancingClient).GetMethod("OnEvent"), new HarmonyMethod(AccessTools.Method(typeof(AntiCrashComponent), nameof(OnEvent))));
            ReModCE_ARES.Harmony.Patch(typeof(VRC.Core.AssetManagement).GetMethod("Method_Public_Static_Object_Object_Boolean_Boolean_Boolean_0"), new HarmonyMethod(AccessTools.Method(typeof(AntiCrashComponent), nameof(OnAvatarAssetBundleLoad))));


            _antiAvatarCrashEnabled = new ConfigValue<bool>(nameof(_antiAvatarCrashEnabled), true);
            _antiAvatarCrashEnabled.OnValueChanged += () => _antiAvatarCrashToggle.Toggle(_antiAvatarCrashEnabled);

            _maxAudioSources = new ConfigValue<int>(nameof(_maxAudioSources), 10);
            _maxAudioSources.OnValueChanged += () => _maxAudioSourcesButton.Text = $"Max Audio Sources: {_maxAudioSources}";

            _maxLightSources = new ConfigValue<int>(nameof(_maxLightSources), 0);
            _maxLightSources.OnValueChanged += () => _maxLightSourcesButton.Text = $"Max Light Sources: {_maxLightSources}";

            _maxDynamicBonesColliders = new ConfigValue<int>(nameof(_maxDynamicBonesColliders), 5);
            //MaxDynamicBonesColliders.OnValueChanged += () => _maxDynamicBonesCollidersButton.Text = $"Max Light Sources: {MaxDynamicBonesColliders}";

            _maxPolys = new ConfigValue<int>(nameof(_maxPolys), 260000);
            _maxPolys.OnValueChanged += () => _maxPolysButton.Text = $"Max Polys: {_maxPolys}";

            _maxMaterials = new ConfigValue<int>(nameof(_maxMaterials), 20);
            _maxMaterials.OnValueChanged += () => _maxMaterialsButton.Text = $"Max Materials: {_maxMaterials}";

            _maxCloth = new ConfigValue<int>(nameof(_maxCloth), 1);
            _maxCloth.OnValueChanged += () => _maxClothButton.Text = $"Max Cloth: {_maxCloth}";

            _maxColliders = new ConfigValue<int>(nameof(_maxColliders), 0);
            _maxColliders.OnValueChanged += () => _maxCollidersButton.Text = $"Max Colliders: {_maxColliders}";
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var aresMenu = uiManager.MainMenu.GetMenuPage("ARES").GetMenuPage("Anti-Crash");

            _antiAvatarCrashToggle = aresMenu.AddToggle("Anti Avatar Crash",
                "Enable/Disable Avatar Anti Crash", _antiAvatarCrashEnabled.SetValue,
                _antiAvatarCrashEnabled);
            _maxAudioSourcesButton = aresMenu.AddButton($"Max Audio Sources: {_maxAudioSources}", "Limit Avatar Audio Sources", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max Audio Sources", _maxAudioSources.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        _maxAudioSources.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));
            _maxLightSourcesButton = aresMenu.AddButton($"Max Light Sources: {_maxLightSources}", "Limit Avatar Light Sources", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max Light Sources", _maxLightSources.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        _maxLightSources.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));
            //_maxDynamicBonesCollidersButton = aresMenu.AddButton($"Max Dynamic Bone Colliders: {MaxDynamicBonesColliders}", "Limit Dynamic Bone Colliders", () =>
            //{
            //    VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max Dynamic Bone Colliders", MaxDynamicBonesColliders.ToString(), InputField.InputType.Standard, false, "Submit",
            //        (s, k, t) =>
            //        {
            //            if (string.IsNullOrEmpty(s))
            //                return;

            //            if (!int.TryParse(s, out var maxSources))
            //                return;

            //            MaxDynamicBonesColliders.SetValue(maxSources);
            //        }, null);
            //}, ResourceManager.GetSprite("remodce.shield"));
            _maxPolysButton = aresMenu.AddButton($"Max Polys: {_maxPolys}", "Limit Poly count", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max Polys", _maxPolys.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        _maxPolys.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));
            _maxMaterialsButton = aresMenu.AddButton($"Max Materials: {_maxMaterials}", "Limit material count", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max material", _maxMaterials.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        _maxMaterials.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));
            _maxClothButton = aresMenu.AddButton($"Max Cloth: {_maxCloth}", "Limit Cloth count", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max Cloth", _maxCloth.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        _maxCloth.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));
            _maxCollidersButton = aresMenu.AddButton($"Max Colliders: {_maxColliders}", "Limit Colliders count", () =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Set Max material", _maxColliders.ToString(), InputField.InputType.Standard, false, "Submit",
                    (s, k, t) =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        if (!int.TryParse(s, out var maxSources))
                            return;

                        _maxColliders.SetValue(maxSources);
                    }, null);
            }, ResourceManager.GetSprite("remodce.shield"));

        }

        private static bool OnEvent(EventData __0)
        {
            var eventCode = __0.Code;
            switch (eventCode)
            {
                case 1:
                case 3:
                    return true;

                case 4:
                    return true;

                case 6:
                    return true;

                case 7:
                    return true;

                case 9:
                    return true;

                case 33:
                    return true;

                case 209:
                    return true;

                case 210:
                    return true;

                case 253:
                    return true;
                default:
                    break;
            }
            return true;
        }

        private static bool OnAvatarAssetBundleLoad(ref UnityEngine.Object __0)
        {
            if (__0 == null) return true;
            GameObject gameObject = __0.TryCast<GameObject>();
                if (gameObject == null)
                {
                    return true;
                }
                if (!gameObject.name.ToLower().Contains("avatar"))
                {
                    return true;
                }
            string avatarId;
            try
            {
                avatarId = gameObject.GetComponent<PipelineManager>().blueprintId;
            } catch {
                return true; }
            return OnAvatarAssetBundleLoadCheck(gameObject, avatarId);

        }

        public static bool OnAvatarAssetBundleLoadCheck(GameObject avatar, string avatarId)
        {
            if (!(_antiAvatarCrashEnabled ?? true)) return true;
            try
            {
                SkinnedMeshRenderer[] array = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
                foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
                {
                    bool flag = false;
                    if (!skinnedMeshRenderer.sharedMesh.isReadable)
                    {
                        UnityEngine.Object.DestroyImmediate(skinnedMeshRenderer, allowDestroyingAssets: true);
                        ReLogger.Msg("[AntiCrash] deleted unreadable Mesh");
                        ReModCE_ARES.LogDebug("<color=red>[AntiCrash] deleted unreadable Mesh</color>");
                        continue;
                    }
                    for (int j = 0; j < _meshList.Length; j++)
                    {
                        if (skinnedMeshRenderer.name.ToLower().Contains(_meshList[j]))
                        {
                            ReLogger.Msg("[AntiCrash] deleted blackListed Mesh " + skinnedMeshRenderer.name);
                            ReModCE_ARES.LogDebug("<color=red>[AntiCrash] deleted blackListed Mesh " + skinnedMeshRenderer.name + "</color>");
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
                        if (num >= _maxPolys)
                        {
                            UnityEngine.Object.DestroyImmediate(skinnedMeshRenderer, allowDestroyingAssets: true);
                            ReLogger.Msg("[AntiCrash] deleted Mesh with too many polys");
                            ReModCE_ARES.LogDebug("<color=red>[AntiCrash] deleted Mesh with too many polys</color>");
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        continue;
                    }
                    Material[] array3 = skinnedMeshRenderer.materials;
                    if (array3.Length >= _maxMaterials)
                    {
                        UnityEngine.Object.DestroyImmediate(skinnedMeshRenderer, allowDestroyingAssets: true);
                        ReLogger.Msg("[AntiCrash] deleted Mesh with " + array3.Length + " materials");
                        ReModCE_ARES.LogDebug("<color=red>[AntiCrash] deleted Mesh with " + array3.Length + " materials</color>");
                        continue;
                    }
                    for (int l = 0; l < array3.Length; l++)
                    {
                        Shader shader = array3[l].shader;
                        for (int m = 0; m < _shaderList.Length; m++)
                        {
                            if (shader.name.ToLower().Contains(_shaderList[m]))
                            {
                                ReLogger.Msg("[AntiCrash] replaced Shader " + shader.name);
                                ReModCE_ARES.LogDebug("<color=yellow>[AntiCrash] replaced Shader " + shader.name + "</color>");
                                shader = defaultShader;
                            }
                        }
                    }
                }
            }
            catch { ReLogger.Msg("Skin mesh error"); }
            try
            {
                MeshFilter[] array2 = avatar.GetComponentsInChildren<MeshFilter>(includeInactive: true);
                foreach (MeshFilter meshFilter in array2)
                {
                    if (!meshFilter.sharedMesh.isReadable)
                    {
                        UnityEngine.Object.DestroyImmediate(meshFilter, allowDestroyingAssets: true);
                        ReLogger.Msg("[AntiCrash] deleted unreadable Mesh");
                        ReModCE_ARES.LogDebug("<color=red>[AntiCrash] deleted unreadable Mesh</color>");
                        continue;
                    }
                    bool flag2 = false;
                    for (int num2 = 0; num2 < _meshList.Length; num2++)
                    {
                        if (meshFilter.name.ToLower().Contains(_meshList[num2]))
                        {
                            ReLogger.Msg("[AntiCrash] deleted blackListed Mesh " + meshFilter.name);
                            ReModCE_ARES.LogDebug("<color=red>[AntiCrash] deleted blackListed Mesh " + meshFilter.name + "</color>");
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
                        if (num3 >= _maxPolys)
                        {
                            UnityEngine.Object.DestroyImmediate(meshFilter, allowDestroyingAssets: true);
                            ReLogger.Msg("[AntiCrash] deleted Mesh with too many polys");
                            ReModCE_ARES.LogDebug("<color=red>[AntiCrash] deleted Mesh with too many polys</color>");
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
                    if (array4.Length >= _maxMaterials)
                    {
                        UnityEngine.Object.DestroyImmediate(meshFilter, allowDestroyingAssets: true);
                        ReLogger.Msg("[AntiCrash] deleted Mesh with " + array4.Length + " materials");
                        ReModCE_ARES.LogDebug("<color=red>[AntiCrash] deleted Mesh with " + array4.Length + " materials</color>");
                        continue;
                    }
                    for (int num5 = 0; num5 < array4.Length; num5++)
                    {
                        Shader shader2 = array4[num5].shader;
                        for (int num6 = 0; num6 < _shaderList.Length; num6++)
                        {
                            if (shader2.name.ToLower().Contains(_shaderList[num6]))
                            {
                                ReLogger.Msg("[AntiCrash] replaced Shader " + shader2.name);
                                ReModCE_ARES.LogDebug("<color=red>[AntiCrash] replaced Shader " + shader2.name + "</color>");

                                shader2 = defaultShader;
                            }
                        }
                    }
                }
            }
            catch { ReLogger.Msg("mesh filter error"); }
            try
            {
                AudioSource[] array5 = avatar.GetComponentsInChildren<AudioSource>();
                if (array5.Length >= _maxAudioSources)
                {
                    for (int num7 = 0; num7 < _maxAudioSources; num7++)
                    {
                        UnityEngine.Object.DestroyImmediate(array5[num7].gameObject, allowDestroyingAssets: true);
                    }
                    ReLogger.Msg("[AntiCrash] deleted " + _maxAudioSources + " AudioSources");
                    ReModCE_ARES.LogDebug("<color=red>[AntiCrash] deleted " + _maxAudioSources + " AudioSources</color>");
                }
            }
            catch { ReLogger.Msg("error in Audio source"); }
            try
            {
                Light[] array6 = avatar.GetComponentsInChildren<Light>();
                if (array6.Length >= _maxLightSources)
                {
                    for (int num8 = 0; num8 < _maxLightSources; num8++)
                    {
                        UnityEngine.Object.DestroyImmediate(array6[num8].gameObject, allowDestroyingAssets: true);
                    }
                    ReLogger.Msg("[AntiCrash] deleted " + _maxLightSources + " Lights");
                    ReModCE_ARES.LogDebug("<color=red>[AntiCrash] deleted " + _maxLightSources + " Lights </color>");
                }
            }
            catch { ReLogger.Msg("Error in light source"); }
            try
            {
                Cloth[] array7 = avatar.GetComponentsInChildren<Cloth>();
                if (array7.Length >= _maxCloth)
                {
                    for (int num9 = 0; num9 < _maxCloth; num9++)
                    {
                        UnityEngine.Object.DestroyImmediate(array7[num9].gameObject, allowDestroyingAssets: true);
                    }
                    ReLogger.Msg("[AntiCrash] deleted " + _maxCloth + " Cloth");
                    ReModCE_ARES.LogDebug("<color=red>[AntiCrash] deleted " + _maxCloth + " Cloth</color>");
                }
            }
            catch { ReLogger.Msg("Error in Cloth Source"); }
            try
            {
                Collider[] array8 = avatar.GetComponentsInChildren<Collider>();
                if (array8.Length >= _maxColliders)
                {
                    for (int num10 = 0; num10 < _maxColliders; num10++)
                    {
                        UnityEngine.Object.DestroyImmediate(array8[num10].gameObject, allowDestroyingAssets: true);
                    }
                    ReLogger.Msg("[AntiCrash] deleted " + _maxColliders + " Colliders");
                    ReModCE_ARES.LogDebug("<color=red>[AntiCrash] deleted " + _maxColliders + " Colliders</color>");
                }
            }
            catch { ReLogger.Msg("Error in collider source"); }
            //try
            //{
            //    DynamicBoneCollider[] array9 = avatar.GetComponentsInChildren<DynamicBoneCollider>();
            //    if (array9.Length >= MaxDynamicBonesColliders)
            //    {
            //        for (int num11 = 0; num11 < MaxDynamicBonesColliders; num11++)
            //        {
            //            UnityEngine.Object.DestroyImmediate(array9[num11].gameObject, allowDestroyingAssets: true);
            //        }
            //        ReLogger.Msg("[AntiCrash] deleted " + MaxDynamicBonesColliders + " DynamicBoneColliders");
            //    }
            //} catch { ReLogger.Msg("Error in Dynamic bones Collider"); }
            return true;
        }

    }
}
