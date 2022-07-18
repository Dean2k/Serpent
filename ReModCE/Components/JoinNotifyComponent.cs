using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;

namespace Serpent.Components
{
    internal class JoinNotifyComponent : ModComponent
    {
        private ConfigValue<bool> JoinNotifyEnabled;
        private ReMenuToggle _joinNotifyToggle;

        public JoinNotifyComponent()
        {
            JoinNotifyEnabled = new ConfigValue<bool>(nameof(JoinNotifyEnabled), true);
            JoinNotifyEnabled.OnValueChanged += () => _joinNotifyToggle.Toggle(JoinNotifyEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetCategoryPage(Page.PageNames.Visuals).AddCategory("Join Notify");
            _joinNotifyToggle = menu.AddToggle("Join Notifier",
                "Enable whether player joins/leaves a message displays on your hud", JoinNotifyEnabled.SetValue,
                JoinNotifyEnabled);
        }
    }
}
