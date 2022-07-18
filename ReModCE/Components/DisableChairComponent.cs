using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using SerpentCore.Core.VRChat;
using VRC;
using VRC.Core;

namespace Serpent.Components
{
    internal sealed class DisableChairComponent : ModComponent
    {
        private static ConfigValue<bool> ChairsDisabled;

        private static ReMenuToggle _disableChairToggle;

        public DisableChairComponent()
        {
            Serpent.Harmony.Patch(typeof(VRC_StationInternal).GetMethod(nameof(VRC_StationInternal.Method_Public_Boolean_Player_Boolean_0)),
                GetLocalPatch(nameof(PlayerCanUseStation)));

            ChairsDisabled = new ConfigValue<bool>(nameof(ChairsDisabled), false);
            ChairsDisabled.OnValueChanged += () => _disableChairToggle.Toggle(ChairsDisabled);
        }

        private static bool PlayerCanUseStation(ref bool __result, VRC_StationInternal __instance, Player __0, bool __1)
        {
            if (!ChairsDisabled) return true;
            if (__0 == null) return true;
            if (__0.GetAPIUser().id != APIUser.CurrentUser.id) return true;

            __result = false;
            return false;
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var othersMenu = uiManager.MainMenu.GetCategoryPage(Page.PageNames.Utility).GetCategory(Page.Categories.Utilties.QualityOfLife);
            _disableChairToggle = othersMenu.AddToggle("Disable Chairs", "Toggle Chairs. Because fuck chairs.", ChairsDisabled);
        }
    }
}
