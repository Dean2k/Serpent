using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;
using ReModAres.Core.VRChat;
using ReModCE_ARES.SDK;

namespace ReModCE_ARES.Components
{
    internal class HideSelfComponent : ModComponent
    {
        private string pastAviID;
        public bool _HideSelfEnabled;
        private static ReMenuToggle _HideSelfToggled;

        public HideSelfComponent()
        {
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var aviMenu = uiManager.MainMenu.GetCategoryPage(Page.PageNames.Monkey).AddCategory("Avatars");
            _HideSelfToggled = aviMenu.AddToggle("Hide Self", "Prevents download manager, meaning no avatars will load until you turn this off.", PerformHide, _HideSelfEnabled);
        }

        public void PerformHide(bool value)
        {
            _HideSelfEnabled = value;
            _HideSelfToggled?.Toggle(value);

            if (_HideSelfEnabled)
            {
                AssetBundleDownloadManager.field_Private_Static_AssetBundleDownloadManager_0.gameObject.SetActive(false);
                PlayerWrapper.LocalVRCPlayer().prop_VRCAvatarManager_0.gameObject.SetActive(false);
            }

            if (!_HideSelfEnabled)
            {
                AssetBundleDownloadManager.field_Private_Static_AssetBundleDownloadManager_0.gameObject.SetActive(true);
                PlayerWrapper.LocalVRCPlayer().prop_VRCAvatarManager_0.gameObject.SetActive(true);
                PlayerExtensions.ReloadAvatar(PlayerWrapper.LocalVRCPlayer());
            }
        }
    }
}