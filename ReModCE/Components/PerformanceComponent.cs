using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;
using ReModCE_ARES.Loader;
using System;
using System.Diagnostics;
using UnityEngine;

namespace ReModCE_ARES.Components
{
    internal class PerformanceComponent : ModComponent
    {
        private ConfigValue<bool> HighPriorityEnabled;
        private ReMenuToggle _highPriorityToggle;

        private ConfigValue<bool> FPS144Enabled;
        private ReMenuToggle _fps144Toggle;

        private ConfigValue<bool> FPS240Enabled;
        private ReMenuToggle _fps240Toggle;

        private ConfigValue<bool> AdaptiveGraphicsEnabled;
        private ReMenuToggle _adaptiveGraphicsToggle;

        public PerformanceComponent()
        {
            HighPriorityEnabled = new ConfigValue<bool>(nameof(HighPriorityEnabled), true);
            HighPriorityEnabled.OnValueChanged += () => _highPriorityToggle.Toggle(HighPriorityEnabled);

            FPS144Enabled = new ConfigValue<bool>(nameof(FPS144Enabled), false);
            FPS144Enabled.OnValueChanged += () => _fps144Toggle.Toggle(FPS144Enabled);

            FPS240Enabled = new ConfigValue<bool>(nameof(FPS240Enabled), false);
            FPS240Enabled.OnValueChanged += () => _fps240Toggle.Toggle(FPS240Enabled);

            AdaptiveGraphicsEnabled = new ConfigValue<bool>(nameof(AdaptiveGraphicsEnabled), true);
            AdaptiveGraphicsEnabled.OnValueChanged += () => _adaptiveGraphicsToggle.Toggle(AdaptiveGraphicsEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("ARES");
            var subMenu = menu.AddMenuPage("Performance", "Performance related settings", ResourceManager.GetSprite("remodce.speedometer"));
            _highPriorityToggle = subMenu.AddToggle("High Priority",
                "Enable whether player joins/leaves should be logged in console.", SetHighPriority,
                HighPriorityEnabled);

            _fps144Toggle = subMenu.AddToggle("FPS 144 Max",
                "Sets FPS limit to 144", SetFPS144,
                FPS144Enabled);

            _fps240Toggle = subMenu.AddToggle("FPS 240 Max",
                "Sets FPS limit to 240", SetFPS240,
                FPS240Enabled);

            _adaptiveGraphicsToggle = subMenu.AddToggle("Adaptive Graphics",
                "Auto Adjust Graphics depending on FPS", AdjustGraphics,
                AdaptiveGraphicsEnabled);

            SetHighPriority(HighPriorityEnabled);
            SetFPS144(FPS144Enabled);
            SetFPS240(FPS240Enabled);
        }

        private void SetFPS240(bool value)
        {
            FPS240Enabled.SetValue(value);

            if (value)
            {
                Application.targetFrameRate = 240;
                _fps144Toggle.Toggle(false);
            }
            if (!FPS144Enabled && !value)
            {
                Application.targetFrameRate = 90;
            }
        }

        private void SetFPS144(bool value)
        {
            FPS144Enabled.SetValue(value);

            if (value)
            {
                Application.targetFrameRate = 144;
                _fps240Toggle.Toggle(false);
            }
            if (!FPS240Enabled && !value)
            {
                Application.targetFrameRate = 90;
            }
        }

        private void SetHighPriority(bool value)
        {
            HighPriorityEnabled.SetValue(value);
            if (value)
            {
                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;

                Process[] processes = Process.GetProcessesByName("vrserver");
                foreach (Process proc in processes)
                {
                    Console.WriteLine("Changing Priority for: " + proc.Id + " To RealTime");
                    proc.PriorityClass = ProcessPriorityClass.High;
                    if (proc.PriorityClass == ProcessPriorityClass.High)
                    {
                        Console.WriteLine("Worked");
                    }
                }

                Process[] processes2 = Process.GetProcessesByName("OVRServer_x64");
                foreach (Process proc in processes2)
                {
                    Console.WriteLine("Changing Priority for: " + proc.Id + " To RealTime");
                    proc.PriorityClass = ProcessPriorityClass.High;
                    if (proc.PriorityClass == ProcessPriorityClass.High)
                    {
                        Console.WriteLine("Worked");
                    }
                }
            }
        }

        private void AdjustGraphics(bool value)
        {
            AdaptiveGraphicsEnabled.SetValue(value);
        }

        public static float Average = 0f;
        public static float FPS = 0f;
        private static float RefreshRate = 0f;
        private static string lastMessage = "";
        public override void OnUpdate()
        {
            if (AdaptiveGraphicsEnabled)
            {
                Average += ((Time.deltaTime / Time.timeScale) - Average) * 0.03f;
                FPS = 1 / Average;

                RefreshRate += Time.deltaTime;
                if (RefreshRate < 8f) return;
                RefreshRate = 0f;

                if (FPS > 60f)
                {
                    ultra();
                }
                else if (FPS > 50f)
                {
                    high();
                }
                else if (FPS > 40f)
                {
                    medium();
                }
                else if (FPS > 30f)
                {
                    low();
                }
                else if (FPS < 20f)
                {
                    ultraLow();
                }
            }
        }

        public static void ultra()
        {
            QualitySettings.antiAliasing = 8;
            QualitySettings.pixelLightCount = 8;
            QualitySettings.lodBias = 2f;
            QualitySettings.shadowDistance = 200f;
            if (lastMessage != "Ultra")
            {
                ReLogger.Msg("Setting graphics to Ultra");
                ReModCE_ARES.LogDebug("Setting graphics to Ultra");
                lastMessage = "Ultra";
            }
        }
        public static void high()
        {
            QualitySettings.antiAliasing = 6;
            QualitySettings.pixelLightCount = 6;
            QualitySettings.lodBias = 1.5f;
            QualitySettings.shadowDistance = 180f;
            if (lastMessage != "High")
            {
                ReLogger.Msg("Setting graphics to High");
                ReModCE_ARES.LogDebug("Setting graphics to High");
                lastMessage = "High";
            }
        }

        public static void medium()
        {
            QualitySettings.antiAliasing = 4;
            QualitySettings.pixelLightCount = 4;
            QualitySettings.lodBias = 1.2f;
            QualitySettings.shadowDistance = 120f;
            if (lastMessage != "Med")
            {
                ReLogger.Msg("Setting graphics to Med");
                ReModCE_ARES.LogDebug("Setting graphics to Med");
                lastMessage = "Med";
            }
        }

        public static void low()
        {
            QualitySettings.antiAliasing = 2;
            QualitySettings.pixelLightCount = 2;
            QualitySettings.lodBias = 0.8f;
            QualitySettings.shadowDistance = 80f;
            if (lastMessage != "Low")
            {
                ReLogger.Msg("Setting graphics to Low");
                ReModCE_ARES.LogDebug("Setting graphics to Low");
                lastMessage = "Low";
            }
        }

        public static void ultraLow()
        {
            QualitySettings.antiAliasing = 0;
            QualitySettings.pixelLightCount = 0;
            QualitySettings.lodBias = 0.4f;
            QualitySettings.shadowDistance = 20f;
            if (lastMessage != "Ultra Low")
            {
                ReLogger.Msg("Setting graphics to Ultra Low");
                ReModCE_ARES.LogDebug("Setting graphics to Ultra Low");
                lastMessage = "Ultra Low";
            }
        }
    }
}
