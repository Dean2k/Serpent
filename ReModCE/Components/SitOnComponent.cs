using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE_ARES.Managers;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.DataModel;
using VRC.UI;

namespace ReModCE_ARES.Components
{
    internal sealed class SitOnComponent : ModComponent
    {
        private static ReMenuButton _teleportTargetButton;
        private static IUser target;
        private Vector3 _originalGravity;
        private Vector3 _playerLastPos;

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var targetMenu = uiManager.TargetMenu;

            var userInfoTransform = VRCUiManagerEx.Instance.MenuContent().transform.Find("Screens/UserInfo");

            var buttonContainer = userInfoTransform.Find("Buttons/RightSideButtons/RightUpperButtonColumn/");

            _teleportTargetButton = targetMenu.AddButton("Sit On", "Sit on target (press jump to stop).", TeleportTargetButtonOnClick, ResourceManager.GetSprite("remodce.teleport"));

            var menu = uiManager.MainMenu.GetMenuPage("ARES");
            menu.AddButton("Stop Siton",
                "Stop sitting on incase Jump doesn't work.", StopSit);

        }


        private void TeleportTargetButtonOnClick()
        {
            var user = QuickMenuEx.SelectedUserLocal.field_Private_IUser_0;
            if (user == null)
                return;

            target = user;
            SetGravity();
            TeleportToIUser(user);
        }

        private void StopSit()
        {
            target = null;
            RemoveSetGravity();
        }

        private void SetGravity()
        {
            if (Physics.gravity == Vector3.zero) return;

            _originalGravity = Physics.gravity;
            Physics.gravity = Vector3.zero;
        }

        private void RemoveSetGravity()
        {
            if (_originalGravity == Vector3.zero) return;
            Physics.gravity = _originalGravity;
        }

        private void TeleportToIUser(IUser user)
        {
            try
            {
                var player = PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(user.prop_String_0)._vrcplayer;
                if (player == null)
                    return;

                var transform = player.transform;
                var playerPosition = transform.position;

                var localTransform = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform;
                if (_playerLastPos != null)
                {
                    if (_playerLastPos != localTransform.position)
                    {
                        localTransform.position = playerPosition + new Vector3(0f, 2f);
                    }
                }

                _playerLastPos = playerPosition;
            }
            catch { target = null; RemoveSetGravity(); }
        }

        public override void OnUpdate()
        {
            if (target != null)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    target = null;
                    RemoveSetGravity();
                    return;
                }
                if (Input.GetButton("Oculus_CrossPlatform_Button1"))
                {
                    target = null;
                    RemoveSetGravity();
                    return;
                }
                TeleportToIUser(target);
            }           
        }

        public override void OnSetupUserInfo(APIUser apiUser)
        {
            bool check = APIUser.CurrentUser.id != apiUser.id && PlayerManager.field_Private_Static_PlayerManager_0.GetPlayer(apiUser.id) != null;
            _teleportTargetButton.Active = check;
        }
    }
}