using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using SerpentCore.Core.VRChat;
using UnityEngine;
using UnityEngine.UI;

namespace Serpent.Components
{
    internal class VRCNewsAdjustment : ModComponent
    {
        private ConfigValue<bool> EnableNews;
        private ReMenuToggle _enableToggle;

        private Transform _carousel;
        private ReMenuHeaderCollapsible _newsHeader;

        public VRCNewsAdjustment()
        {
            EnableNews = new ConfigValue<bool>(nameof(EnableNews), false);
            EnableNews.OnValueChanged += () =>
            {
                if (_newsHeader != null)
                {
                    _newsHeader.Active = EnableNews;
                }
                _carousel?.gameObject.SetActive(EnableNews);
                _enableToggle?.Toggle(EnableNews, false, true);
            };
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var dashboard = QuickMenuEx.Instance.field_Public_Transform_0.Find("Window/QMParent/Menu_Dashboard").GetComponentInChildren<ScrollRect>().content;

            _carousel = dashboard.Find("Carousel_Banners");
            if (_carousel == null) return; // some mod removed the carousel.

            _newsHeader = new ReMenuHeaderCollapsible("VRChat News", dashboard);
            _newsHeader.OnToggle += b => _carousel.gameObject.SetActive(b);

            _newsHeader.RectTransform.SetSiblingIndex(_carousel.GetSiblingIndex());

            _newsHeader.Active = EnableNews;
            _carousel.gameObject.SetActive(EnableNews);

            var menu = uiManager.MainMenu.GetCategoryPage(Page.PageNames.Utility).GetCategory(Page.Categories.Utilties.VRChatNews);
            _enableToggle = menu.AddToggle("Enable", "Enable/Disable VRChat News on the dashboard/launchpad", EnableNews);
        }
    }
}
