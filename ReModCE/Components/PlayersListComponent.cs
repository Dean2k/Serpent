﻿using System;
using MelonLoader;
using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;
using ReModAres.Core.VRChat;
using ReModCE_ARES.Loader;
using ReModCE_ARES.Managers;
using TMPro;
using UnityEngine;
using VRC;

namespace ReModCE_ARES.Components
{
	internal class PlayersListComponent : ModComponent
	{
		private ConfigValue<bool> PlayerListEnabled;
		private ReMenuToggle _playerListToggle;
		private QMLablePlayer playerList;

		public PlayersListComponent()
		{
            PlayerListEnabled = new ConfigValue<bool>(nameof(PlayerListEnabled), true);
            PlayerListEnabled.OnValueChanged += () => _playerListToggle.Toggle(PlayerListEnabled);
		}

		public override void OnUiManagerInit(UiManager uiManager)
		{
			base.OnUiManagerInit(uiManager);

			var menu = uiManager.MainMenu.GetMenuPage("ARES");
            _playerListToggle = menu.AddToggle("Player List",
				"Enable/Disable player List", TogglePlayerList,
                PlayerListEnabled);


			playerList = new QMLablePlayer(QuickMenuEx.LeftWing.field_Public_Button_0.transform, 609.902f, 457.9203f, "PlayerList");
            drawOverlay();
        }

        public void drawOverlay()
        {
            if (PlayerListEnabled)
            {
                playerList.lable.SetActive(true);
                playerList.lable.transform.localPosition = new Vector3(-106.6402f, -30f, 0f);
                playerList.text.enableWordWrapping = false;
                playerList.text.fontSizeMin = 30f;
                playerList.text.fontSizeMax = 30f;
                playerList.text.alignment = TextAlignmentOptions.Right;
                playerList.text.color = Color.white;
                //debugLog.lable.transform.localPosition = new Vector3(609.902f, 457.9203f, 0);
                //debugLog.text.enableWordWrapping = false;
                //debugLog.text.fontSizeMin = 30;
                //debugLog.text.fontSizeMax = 30;
                //debugLog.text.alignment = TMPro.TextAlignmentOptions.Left;
                //debugLog.text.verticalAlignment = TMPro.VerticalAlignmentOptions.Top;
                //debugLog.text.color = Color.white;
            }
            else
            {
                playerList.lable.SetActive(false);
            }
        }

		public void TogglePlayerList(bool value)
		{
            PlayerListEnabled.SetValue(value);
			drawOverlay();
		}


        public override void OnUpdate()
        {
			try
			{
                string text = "";
                for (int i = 0; i < Wrapper.GetAllPlayers().Count; i++)
                {
                    VRC.Player player = Wrapper.GetAllPlayers()[i];
                    if (player.GetIsMaster())
                    {
                        text += " [<color=#FFB300>H</color>]";
                    }
                    text = text + " [" + player.GetPlatform() + "]";
                    text = text + " [<color=#FFB300>P</color>] " + player.GetPingColord();
                    text = text + " [<color=#FFB300>F</color>] " + player.GetFramesColord();
                    text = text + " <color=#" + ColorUtility.ToHtmlStringRGB(player.GetTrustColor()) + ">" + player.GetAPIUser().displayName + "</color>\n";
                }
                playerList.text.text = text;
            }
            catch
            {
            }
		}
        
    }
}