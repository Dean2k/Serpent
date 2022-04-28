using System;
using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;
using ReModCE_ARES.Loader;
using VRC;

namespace ReModCE_ARES.Components
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

            var menu = uiManager.MainMenu.GetMenuPage("Logging");
            _joinLeaveLogsToggle = menu.AddToggle("Join/Leave Logs",
                "Enable whether player joins/leaves should be logged in console.", JoinLeaveLogsEnabled.SetValue,
                JoinLeaveLogsEnabled);
        }

        public override void OnPlayerJoined(Player player)
        {
            if (!JoinLeaveLogsEnabled) return;
            if (player == null) return;
            if (player.field_Private_APIUser_0 == null) return;
            ReLogger.Msg(ConsoleColor.Cyan, $"{player.field_Private_APIUser_0.displayName} joined the instance.");
            ReModCE_ARES.LogDebug($"<color=green>{player.field_Private_APIUser_0.displayName} joined the instance.</color>");
        }

        public override void OnPlayerLeft(Player player)
        {
            if (!JoinLeaveLogsEnabled) return;
            if (player == null) return;
            if (player.field_Private_APIUser_0 == null) return;
            ReLogger.Msg(ConsoleColor.White, $"{player.field_Private_APIUser_0.displayName} left the instance.");
            ReModCE_ARES.LogDebug(
                $"<color=#fc4903>{player.field_Private_APIUser_0.displayName} left the instance.</color>");
        }
    }
}
