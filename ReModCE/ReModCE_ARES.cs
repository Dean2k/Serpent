using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.Wings;
using ReModAres.Core.Unity;
using ReModCE_ARES.Components;
using ReModCE_ARES.Core;
using ReModCE_ARES.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnhollowerRuntimeLib;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.DataModel;
using ConfigManager = ReModAres.Core.Managers.ConfigManager;

namespace ReModCE_ARES
{
    public static class ReModCE_ARES
    {
        private static readonly List<ModComponent> Components = new List<ModComponent>();
        private static UiManager _uiManager;
        private static ConfigManager _configManager;

        public static ReMirroredWingMenu WingMenu;
        public static bool IsEmmVrcLoaded { get; private set; }
        public static bool IsRubyLoaded { get; private set; }
        public static bool IsOculus { get; private set; }
        public static HarmonyLib.Harmony Harmony { get; private set; }

        private static string newHWID = "";

        public static List<NameplateModel> NameplateModels;

        public static void UpdateNamePlates()
        {
            ReLogger.Msg("Reloading Nameplates");
            string url = "https://api.ares-mod.com/records/NamePlates";

            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
            webReq.Method = "GET";
            HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
            string jsonString;
            using (Stream stream = webResp.GetResponseStream())   //modified from your code since the using statement disposes the stream automatically when done
            {
                StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                jsonString = reader.ReadToEnd();
            }
            NameplateModelList items = JsonConvert.DeserializeObject<NameplateModelList>(jsonString);
            NameplateModels = items.records;
        }

        public static void OnApplicationStart()
        {
            Harmony = MelonHandler.Mods.First(m => m.Info.Name == "ReModCE_ARES").HarmonyInstance;
            Directory.CreateDirectory("UserData/ReModCE_ARES");
            ReLogger.Msg("Initializing...");

            Directory.CreateDirectory("LoadingScreenMusic");
            if (!File.Exists("LoadingScreenMusic/Music.ogg"))
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile("https://api.ares-mod.com/Music.ogg", "LoadingScreenMusic/Music.ogg");
                }
            }

            IsEmmVrcLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "emmVRCLoader");
            IsRubyLoaded = File.Exists("hid.dll");

            var ourAssembly = Assembly.GetExecutingAssembly();
            var resources = ourAssembly.GetManifestResourceNames();
            foreach (var resource in resources)
            {
                if (!resource.EndsWith(".png"))
                    continue;

                var stream = ourAssembly.GetManifestResourceStream(resource);

                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                var resourceName = Regex.Match(resource, @"([a-zA-Z\d\-_]+)\.png").Groups[1].ToString();
                ResourceManager.LoadSprite("remodce", resourceName, ms.ToArray());
            }

            _configManager = new ConfigManager(nameof(ReModCE_ARES));

            EnableDisableListener.RegisterSafe();
            ClassInjector.RegisterTypeInIl2Cpp<WireframeEnabler>();
            ClassInjector.RegisterTypeInIl2Cpp<CustomNameplate>();

            SetIsOculus();

            ReLogger.Msg($"Running on {(IsOculus ? "Not Steam" : "Steam")}");

            InitializePatches();
            InitializeModComponents();
            MelonCoroutines.Start(WaitForActionMenuInitWheel());
            ReLogger.Msg("Done!");
            ShowLogo();
        }

        private static IEnumerator WaitForActionMenuInitWheel()
        {
            while (ActionMenuDriver.prop_ActionMenuDriver_0 == null) //VRCUIManager Init is too early 
                yield return null;
            ResourcesManager.InitLockGameObject();
            RadialPuppetManager.Setup();
            FourAxisPuppetManager.Setup();
        }

        private static void ShowLogo()
        {
            Console.Title = "ARES";
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(@"=============================================================================================================");
            Console.WriteLine(@"                _____/\\\\\\\\\_______/\\\\\\\\\______/\\\\\\\\\\\\\\\_____/\\\\\\\\\\\___                   ");
            Console.WriteLine(@"                 ___/\\\\\\\\\\\\\___/\\\///////\\\___\/\\\///////////____/\\\/////////\\\_                  ");
            Console.WriteLine(@"                  __/\\\/////////\\\_\/\\\_____\/\\\___\/\\\______________\//\\\______\///__                 ");
            Console.WriteLine(@"                   _\/\\\_______\/\\\_\/\\\\\\\\\\\/____\/\\\\\\\\\\\_______\////\\\_________                ");
            Console.WriteLine(@"                    _\/\\\\\\\\\\\\\\\_\/\\\//////\\\____\/\\\///////___________\////\\\______               ");
            Console.WriteLine(@"                     _\/\\\/////////\\\_\/\\\____\//\\\___\/\\\_____________________\////\\\___              ");
            Console.WriteLine(@"                      _\/\\\_______\/\\\_\/\\\_____\//\\\__\/\\\______________/\\\______\//\\\__             ");
            Console.WriteLine(@"                       _\/\\\_______\/\\\_\/\\\______\//\\\_\/\\\\\\\\\\\\\\\_\///\\\\\\\\\\\/___            ");
            Console.WriteLine(@"                        _\///________\///__\///________\///__\///////////////____\///////////_____           ");
            Console.WriteLine(@"                                                                                                             ");
            Console.WriteLine(@"                                    ARES Client(Modified ReMod) - By ShrekamusChrist                         ");
            Console.WriteLine(@"                                                         HotKeys                                             ");
            Console.WriteLine(@" Noclip      = CTRL + F                                                                                      ");
            Console.WriteLine(@" 3rd Person  = CTRL + T                                                                                      ");
            Console.WriteLine(@" Teleport    = CTRL + Q                                                                                      ");
            Console.WriteLine(@"=============================================================================================================");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void SetIsOculus()
        {
            try
            {
                var steamTracking = typeof(VRCTrackingSteam);
            }
            catch (TypeLoadException)
            {
                IsOculus = true;
                return;
            }

            IsOculus = false;
        }

        private static HarmonyMethod GetLocalPatch(string name)
        {
            return typeof(ReModCE_ARES).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod();
        }
        private static void ForceClone(ref bool __0) => __0 = true;
        public static void InitializePatches()
        {
            Harmony.Patch(typeof(VRCPlayer).GetMethod(nameof(VRCPlayer.Awake)), GetLocalPatch(nameof(VRCPlayerAwakePatch)));
            Harmony.Patch(typeof(RoomManager).GetMethod(nameof(RoomManager.Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0)), postfix: GetLocalPatch(nameof(EnterWorldPatch)));
            try
            {
                Harmony.Patch(typeof(SystemInfo).GetProperty("deviceUniqueIdentifier").GetGetMethod(), new HarmonyLib.HarmonyMethod(AccessTools.Method(typeof(ReModCE_ARES), nameof(FakeHWID))));
            }
            catch
            {
                MelonLogger.Msg("Failed to patch HWID");
            }
            try
            {
                Harmony.Patch(typeof(APIUser).GetProperty(nameof(APIUser.allowAvatarCopying)).GetSetMethod(), new HarmonyLib.HarmonyMethod(typeof(ReModCE_ARES).GetMethod(nameof(ForceClone), BindingFlags.NonPublic | BindingFlags.Static)));
            }
            catch
            {
                MelonLogger.Msg("Failed to patch force cloning");
            }

            ActionMenus.PatchAll(Harmony);

            //foreach (var method in typeof(SelectedUserMenuQM).GetMethods())
            //{
            //    if (!method.Name.StartsWith("Method_Private_Void_IUser_PDM_"))
            //        continue;

            //    if (XrefScanner.XrefScan(method).Count() < 3)
            //        continue;

            //    Harmony.Patch(method, postfix: GetLocalPatch(nameof(SetUserPatch)));
            //}
        }


        private static bool FakeHWID(ref string __result)
        {
            if (newHWID == "")
            {
                newHWID = KeyedHashAlgorithm.Create().ComputeHash(Encoding.UTF8.GetBytes(string.Format("{0}A-{1}{2}-{3}{4}-{5}{6}-3C-1F", new object[]
                {
                    new System.Random().Next(0, 9),
                    new System.Random().Next(0, 9),
                    new System.Random().Next(0, 9),
                    new System.Random().Next(0, 9),
                    new System.Random().Next(0, 9),
                    new System.Random().Next(0, 9),
                    new System.Random().Next(0, 9)
                }))).Select(delegate (byte x)
                {
                    byte b = x;
                    return b.ToString("x2");
                }).Aggregate((string x, string y) => x + y);
                MelonLogger.Msg("[HWID] new " + newHWID);
            }
            __result = newHWID;
            return false;
        }

        private static void InitializeNetworkManager()
        {
            var playerJoinedDelegate = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0;
            var playerLeftDelegate = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_1;
            playerJoinedDelegate.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>(p =>
            {
                if (p != null) OnPlayerJoined(p);
            }));

            playerLeftDelegate.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>(p =>
            {
                if (p != null) OnPlayerLeft(p);
            }));
        }

        public static void OnUiManagerInit()
        {
            ReLogger.Msg("Initializing UI...");

            _uiManager = new UiManager("ReMod <color=#00ff00>CE</color>", ResourceManager.GetSprite("remodce.remod"));
            WingMenu = ReMirroredWingMenu.Create("ReModCE-ARES", "Open the RemodCE menu", ResourceManager.GetSprite("remodce.remod"));

            _uiManager.MainMenu.AddMenuPage("Movement", "Access movement related settings", ResourceManager.GetSprite("remodce.running"));

            _uiManager.MainMenu.AddMenuPage("Microphone", "Microphone Settings", ResourceManager.GetSprite("remodce.mixer"));

            var aresPage = _uiManager.MainMenu.AddMenuPage("ARES", "ARES Functions", ResourceManager.GetSprite("remodce.areslogo"));
            aresPage.AddMenuPage("Anti-Crash", "Anticrash settings", ResourceManager.GetSprite("remodce.shield"));
            aresPage.AddMenuPage("World Cheats", "World Cheats (exploits)", ResourceManager.GetSprite("remodce.admin"));

            var visualPage = _uiManager.MainMenu.AddCategoryPage("Visuals", "Access anything that will affect your game visually", ResourceManager.GetSprite("remodce.eye"));
            visualPage.AddCategory("Nameplate");
            visualPage.AddCategory("ESP/Highlights");
            visualPage.AddCategory("Wireframe");

            _uiManager.MainMenu.AddMenuPage("Avatars", "Access avatar related settings", ResourceManager.GetSprite("remodce.hanger"));

            var utilityPage = _uiManager.MainMenu.AddCategoryPage("Utility", "Access miscellaneous settings", ResourceManager.GetSprite("remodce.tools"));
            utilityPage.AddCategory("Quality of Life");
            utilityPage.AddCategory("VRChat News");

            _uiManager.MainMenu.AddMenuPage("Logging", "Access logging related settings", ResourceManager.GetSprite("remodce.log"));
            _uiManager.MainMenu.AddMenuPage("Hotkeys", "Access hotkey related settings", ResourceManager.GetSprite("remodce.keyboard"));


            foreach (var m in Components)
            {
                try
                {
                    m.OnUiManagerInit(_uiManager);
                }
                catch (Exception e)
                {
                    ReLogger.Error($"{m.GetType().Name} had an error during UI initialization:\n{e}");
                }
            }
        }
        public static void OnUiManagerInitEarly()
        {
            ReLogger.Msg("Initializing early UI...");

            InitializeNetworkManager();

            foreach (var m in Components)
            {
                try
                {
                    m.OnUiManagerInitEarly();
                }
                catch (Exception e)
                {
                    ReLogger.Error($"{m.GetType().Name} had an error during early UI initialization:\n{e}");
                }
            }
        }

        public static void OnFixedUpdate()
        {
            foreach (var m in Components)
            {
                m.OnFixedUpdate();
            }
        }

        public static void OnUpdate()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    Tele2MousePos();
                }
            }

            RadialPuppetManager.OnUpdate();
            FourAxisPuppetManager.OnUpdate();

            foreach (var m in Components)
            {
                m.OnUpdate();
            }
        }

        public static void Tele2MousePos()
        {
            Ray posF = new Ray(Camera.main.transform.position, Camera.main.transform.forward); //pos, directon 
            RaycastHit[] PosData = Physics.RaycastAll(posF);
            if (PosData.Length > 0) { RaycastHit pos = PosData[0]; VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position = pos.point; }
        }

        public static void OnLateUpdate()
        {
            foreach (var m in Components)
            {
                m.OnLateUpdate();
            }
        }

        public static void OnGUI()
        {
            foreach (var m in Components)
            {
                m.OnGUI();
            }
        }

        public static void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            foreach (var m in Components)
            {
                m.OnSceneWasLoaded(buildIndex, sceneName);
            }
        }

        public static void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            foreach (var m in Components)
            {
                m.OnSceneWasInitialized(buildIndex, sceneName);
            }
        }

        public static void OnApplicationQuit()
        {
            foreach (var m in Components)
            {
                m.OnApplicationQuit();
            }

            MelonPreferences.Save();
            Process.GetCurrentProcess().Kill();
        }

        public static void OnPreferencesLoaded()
        {
            foreach (var m in Components)
            {
                m.OnPreferencesLoaded();
            }
        }

        public static void OnPreferencesSaved()
        {
            foreach (var m in Components)
            {
                m.OnPreferencesSaved();
            }
        }

        private static void OnPlayerJoined(Player player)
        {
            foreach (var m in Components)
            {
                m.OnPlayerJoined(player);
            }
        }

        private static void OnPlayerLeft(Player player)
        {
            foreach (var m in Components)
            {
                m.OnPlayerLeft(player);
            }
        }

        private static void AddModComponent(Type type)
        {
            try
            {
                var newModComponent = Activator.CreateInstance(type) as ModComponent;
                Components.Add(newModComponent);
            }
            catch (Exception e)
            {
                ReLogger.Error($"Failed creating {type.Name}:\n{e}");
            }
        }

        private class LoadableModComponent
        {
            public int Priority;
            public Type Component;
        }

        private static void InitializeModComponents()
        {
            var assembly = Assembly.GetExecutingAssembly();
            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException reflectionTypeLoadException)
            {
                types = reflectionTypeLoadException.Types.Where(t => t != null);
            }

            var loadableModComponents = new List<LoadableModComponent>();
            foreach (var t in types)
            {
                if (t.IsAbstract)
                    continue;
                if (t.BaseType != typeof(ModComponent))
                    continue;
                if (t.IsDefined(typeof(ComponentDisabled), false))
                    continue;

                var priority = 0;
                if (t.IsDefined(typeof(ComponentPriority)))
                {
                    priority = ((ComponentPriority)Attribute.GetCustomAttribute(t, typeof(ComponentPriority)))
                        .Priority;
                }

                loadableModComponents.Add(new LoadableModComponent
                {
                    Component = t,
                    Priority = priority
                });
            }

            var sortedComponents = loadableModComponents.OrderBy(component => component.Priority);
            foreach (var modComp in sortedComponents)
            {
                AddModComponent(modComp.Component);
            }

            ReLogger.Msg(ConsoleColor.Cyan, $"Created {Components.Count} mod components.");
        }

        private static void EnterWorldPatch(ApiWorld __0, ApiWorldInstance __1)
        {
            if (__0 == null || __1 == null)
                return;

            foreach (var m in Components)
            {
                m.OnEnterWorld(__0, __1);
            }
        }

        private static void VRCPlayerAwakePatch(VRCPlayer __instance)
        {
            if (__instance == null) return;

            __instance.Method_Public_add_Void_OnAvatarIsReady_0(new Action(() =>
            {
                foreach (var m in Components)
                {
                    m.OnAvatarIsReady(__instance);
                }
            }));
        }
        //private static void SetUserPatch(SelectedUserMenuQM __instance, IUser __0)
        //{
        //    if (__0 == null)
        //        return;

        //    foreach (var m in Components)
        //    {
        //        m.OnSelectUser(__0, __instance.field_Public_Boolean_0);
        //    }
        //}

        private static List<string> DebugLogs = new List<string>();
        private static int duplicateCount = 1;
        private static string lastMsg = "";
        public static void LogDebug(string message)
        {
            if (message == lastMsg)
            {
                DebugLogs.RemoveAt(DebugLogs.Count - 1);
                duplicateCount++;
                DebugLogs.Add($"<color=white><b>[<color=red>ARES</color>] [<color=#ff00ffff>{DateTime.Now.ToString("hh:mm tt")}</color>] {message} <color=red><i>x{duplicateCount}</i></color></b></color>");
            }
            else
            {
                lastMsg = message;
                duplicateCount = 1;
                DebugLogs.Add($"<color=white><b>[<color=red>ARES</color>] [<color=#ff00ffff>{DateTime.Now.ToString("hh:mm tt")}</color>] {message}</b></color>");
                if (DebugLogs.Count == 25)
                {
                    DebugLogs.RemoveAt(0);
                }
            }
            DebugMenuComponent.debugLog.text.text = string.Join("\n", DebugLogs.Take(25));
            DebugMenuComponent.debugLog.text.enableWordWrapping = false;
            DebugMenuComponent.debugLog.text.fontSizeMin = 25;
            DebugMenuComponent.debugLog.text.fontSizeMax = 30;
            DebugMenuComponent.debugLog.text.alignment = TMPro.TextAlignmentOptions.Left;
            DebugMenuComponent.debugLog.text.verticalAlignment = TMPro.VerticalAlignmentOptions.Top;
        }


    }
}
