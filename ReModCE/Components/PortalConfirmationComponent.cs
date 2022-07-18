using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using SerpentCore.Core.VRChat;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;

namespace Serpent.Components
{
    internal sealed class PortalConfirmationComponent : ModComponent
    {
        private static ReMenuToggle _portalConfirmationToggle;
        private static ConfigValue<bool> PortalConfirmationEnabled;

        private static bool _bypassPortals;

        public PortalConfirmationComponent()
        {
            foreach (var t in typeof(PortalInternal).GetMethods().ToList().FindAll(x =>
            {
                if (!x.Name.Contains("Method_Public_Void_"))
                    return false;
                try
                {
                    if (XrefScanner.XrefScan(x).Any(z => z.Type == XrefType.Global && z.ReadAsObject() != null && z.ReadAsObject().ToString() == " was at capacity, cannot enter."))
                        return true;
                }
                catch
                {
                    return false;
                }
                return false;
            }))
            {
                Serpent.Harmony.Patch(t, GetLocalPatch(nameof(EnterConfirm)));
            }

            PortalConfirmationEnabled = new ConfigValue<bool>(nameof(PortalConfirmationEnabled), true);
            PortalConfirmationEnabled.OnValueChanged += () => _portalConfirmationToggle.Toggle(PortalConfirmationEnabled);
        }


        public override void OnUiManagerInit(UiManager uiManager)
        {
            var menu = uiManager.MainMenu.GetCategoryPage("Utility").GetCategory("Quality of Life");
            _portalConfirmationToggle = menu.AddToggle("Portal Confirmation", "Toggle Portal Confirmation", PortalConfirmationEnabled);
        }


        private static bool EnterConfirm(PortalInternal __instance, MethodBase __originalMethod)
        {
            if (!PortalConfirmationEnabled)
                return true;
            if (!_bypassPortals)
            {
                if (__instance != null)
                {
                    VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.ShowStandardPopupV2(
                        "Confirm Portal Entrance",
                        "Are you sure you want to enter this portal?",
                        "Yes", () =>
                        {
                            _bypassPortals = true;
                            __originalMethod.Invoke(__instance, null);
                        },
                        "No", () => VRCUiManagerEx.Instance.HideScreen("POPUP"), null
                    );

                }

                return false;
            }

            _bypassPortals = false;
            return true;
        }
    }
}
