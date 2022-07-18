using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using Serpent.Loader;
using System;
using VRC;

namespace Serpent.Components
{
    internal class InfoLogsComponent : ModComponent
    {
        private ConfigValue<bool> JoinLeaveLogsEnabled;
        private ReMenuToggle _joinLeaveLogsToggle;

        public InfoLogsComponent()
        {
            JoinLeaveLogsEnabled = new ConfigValue<bool>(nameof(JoinLeaveLogsEnabled), true);
            JoinLeaveLogsEnabled.OnValueChanged += () => _joinLeaveLogsToggle.Toggle(JoinLeaveLogsEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage(Page.PageNames.Logging);
            _joinLeaveLogsToggle = menu.AddToggle("Join/Leave Logs",
                "Enable whether player joins/leaves should be logged in console.", JoinLeaveLogsEnabled.SetValue,
                JoinLeaveLogsEnabled);
        }

        public override void OnPlayerJoined(Player player)
        {
            if (!JoinLeaveLogsEnabled) return;
            if (player == null) return;
            if (player.field_Private_APIUser_0 == null) return;
            ReLogger.Msg(ConsoleColor.Cyan, $"{player.field_Private_APIUser_0.displayName ?? string.Empty} joined the instance.");
            Serpent.LogDebug($"<color=green>{player.field_Private_APIUser_0.displayName ?? string.Empty} joined the instance.</color>");
        }

        public override void OnPlayerLeft(Player player)
        {
            if (!JoinLeaveLogsEnabled) return;
            if (player == null) return;
            if (player.field_Private_APIUser_0 == null) return;
            ReLogger.Msg(ConsoleColor.White, $"{player.field_Private_APIUser_0.displayName ?? string.Empty} left the instance.");
            Serpent.LogDebug(
                $"<color=#fc4903>{player.field_Private_APIUser_0.displayName ?? string.Empty} left the instance.</color>");
        }
    }
}
