using ReMod.Core.VRChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase;

namespace ReModCE_ARES.Managers
{
    static class Wrapper
    {

        public static float GetFrames(this Player player) => (player._playerNet.prop_Byte_0 != 0) ? Mathf.Floor(1000f / (float)player._playerNet.prop_Byte_0) : -1f;
        public static short GetPing(this Player player) => player._playerNet.field_Private_Int16_0;
        public static bool ClientDetect(this Player player) => player.GetFrames() > 90 || player.GetFrames() < 1 || player.GetPing() > 665 || player.GetPing() < 0;
        public static bool GetIsMaster(this Player Instance) => Instance.GetVRCPlayerApi().isMaster;
        public static VRCPlayerApi GetVRCPlayerApi(this Player Instance) => Instance?.prop_VRCPlayerApi_0;
        public static ApiAvatar GetAvatarInfo(this Player Instance) => Instance?.prop_ApiAvatar_0;

        public static string GetAvatarStatus(this Player player)
        {
            string status = player.GetAvatarInfo().releaseStatus.ToLower();
            if (status == "public")
                return "<color=green>" + status + "</color>";
            else
                return "<color=red>" + status + "</color>";
        }

        public static string GetFramesColord(this Player player)
        {
            float fps = player.GetFrames();
            if (fps > 80)
                return "<color=green>" + fps + "</color>";
            else if (fps > 30)
                return "<color=yellow>" + fps + "</color>";
            else
                return "<color=red>" + fps + "</color>";
        }

        public static string GetPingColord(this Player player)
        {
            short ping = player.GetPing();
            if (ping > 150)
                return "<color=red>" + ping + "</color>";
            else if (ping > 75)
                return "<color=yellow>" + ping + "</color>";
            else
                return "<color=green>" + ping + "</color>";
        }

        public static string GetPlatform(this Player player)
        {
            if (player.GetAPIUser().IsOnMobile)
            {
                return "<color=green>Q</color>";
            }
            else if (player.GetVRCPlayerApi().IsUserInVR())
            {
                return "<color=#CE00D5>V</color>";
            }
            else
            {
                return "<color=grey>PC</color>";
            }
        }
    }
}
