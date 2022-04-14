﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;

namespace ReModCE_ARES.Components
{
    internal class EmojiBlocker : ModComponent
    {
        private static ConfigValue<bool> EmojisEnabled;
        private ReMenuToggle _emojiToggle;


        public EmojiBlocker()
        {
            EmojisEnabled = new ConfigValue<bool>(nameof(EmojisEnabled), true);
            EmojisEnabled.OnValueChanged += () => _emojiToggle?.Toggle(EmojisEnabled);

            ReModCE_ARES.Harmony.Patch(typeof(VRCPlayer).GetMethod(nameof(VRCPlayer.SpawnEmojiRPC)),
                GetLocalPatch(nameof(SpawnEmojiRPCPatch)));
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var menu = uiManager.MainMenu.GetCategoryPage("Utility").AddCategory("Emojis");
            _emojiToggle = menu.AddToggle("Enable", "Disable Emojis from players", EmojisEnabled);
        }

        private static bool SpawnEmojiRPCPatch()
        {
            return EmojisEnabled;
        }
    }
}
