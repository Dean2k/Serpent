using System;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;

namespace Serpent.Core
{
    internal static class Utilities
    {

        private static MethodInfo alignTrackingToPlayerMethod;

        // Yes that's a lot of xref scanning but gotta make sure xD
        // Only grabs once anyway ¯\_(ツ)_/¯
        internal static AlignTrackingToPlayerDelegate GetAlignTrackingToPlayerDelegate
        {
            get
            {
                alignTrackingToPlayerMethod ??= typeof(VRCPlayer).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).First(
                    m => m.ReturnType == typeof(void)
                         && m.GetParameters().Length == 0
                         && m.Name.IndexOf("PDM", StringComparison.OrdinalIgnoreCase) == -1
                         && m.XRefScanForMethod("get_Transform")

                         //&& m.XRefScanForMethod(reflectedType: "Player")
                         //&& m.XRefScanForMethod("Vector3_Quaternion")
                         && m.XRefScanForMethod(reflectedType: nameof(VRCTrackingManager))
                         && m.XRefScanForMethod(reflectedType: nameof(InputStateController)));

                return (AlignTrackingToPlayerDelegate)Delegate.CreateDelegate(
                    typeof(AlignTrackingToPlayerDelegate),
                    GetLocalVRCPlayer(),
                    alignTrackingToPlayerMethod);
            }
        }

        public static bool GetStreamerMode =>
            VRCInputManager.Method_Public_Static_Boolean_InputSetting_0(
                VRCInputManager.InputSetting.StreamerModeEnabled);

        internal static bool IsInVR
        {
            get
            {
                try
                {
                    return VRC.Player.prop_Player_0.prop_VRCPlayerApi_0.IsUserInVR();
                }
                catch
                {
                    return Environment.GetCommandLineArgs().All(args => !args.Equals("--no-vr", StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        // Borrowed from https://github.com/gompocp/ActionMenuUtils/blob/69f1fe1852810ee977f23dceee5cff0e7b4528d7/ActionMenuAPI.cs#L251
        internal static bool AnyActionMenuesOpen()
        {
            return ActionMenuDriver.field_Public_Static_ActionMenuDriver_0.field_Public_ActionMenuOpener_0.field_Private_Boolean_0
                   || ActionMenuDriver.field_Public_Static_ActionMenuDriver_0.field_Public_ActionMenuOpener_1.field_Private_Boolean_0;
        }

        internal static VRCPlayer GetLocalVRCPlayer()
        {
            return VRCPlayer.field_Internal_Static_VRCPlayer_0;
        }

        internal static bool XRefScanForMethod(this MethodBase methodBase, string methodName = null, string reflectedType = null)
        {
            var found = false;
            foreach (XrefInstance xref in XrefScanner.XrefScan(methodBase))
            {
                if (xref.Type != XrefType.Method) continue;

                MethodBase resolved = xref.TryResolve();
                if (resolved == null) continue;

                if (!string.IsNullOrEmpty(methodName))
                    found = !string.IsNullOrEmpty(resolved.Name) && resolved.Name.IndexOf(methodName, StringComparison.OrdinalIgnoreCase) >= 0;

                if (!string.IsNullOrEmpty(reflectedType))
                    found = !string.IsNullOrEmpty(resolved.ReflectedType?.Name)
                            && resolved.ReflectedType.Name.IndexOf(reflectedType, StringComparison.OrdinalIgnoreCase) >= 0;

                if (found) return true;
            }

            return false;
        }

        internal delegate void AlignTrackingToPlayerDelegate();

        public static VRC.Player GetLocalPlayer() => VRC.Player.prop_Player_0;

        public static bool IsFriend(VRC.Player p_player)
        {
            bool l_result = false;
            if (p_player.field_Private_APIUser_0 != null)
                l_result = VRC.Core.APIUser.IsFriendsWith(p_player.field_Private_APIUser_0.id);
            if (p_player.field_Private_VRCPlayerApi_0 != null)
                l_result = (l_result && !p_player.field_Private_VRCPlayerApi_0.isLocal);
            return l_result;
        }

        public static Il2CppSystem.Collections.Generic.List<VRC.Player> GetPlayers() => VRC.PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0;


        public static System.Collections.Generic.List<VRC.Player> GetFriendsInInstance()
        {
            System.Collections.Generic.List<VRC.Player> l_result = new System.Collections.Generic.List<VRC.Player>();
            var l_remotePlayers = GetPlayers();
            if (l_remotePlayers != null)
            {
                foreach (VRC.Player l_remotePlayer in l_remotePlayers)
                {
                    if ((l_remotePlayer != null) && IsFriend(l_remotePlayer))
                        l_result.Add(l_remotePlayer);
                }
            }
            return l_result;
        }

        public static VRCTrackingManager GetVRCTrackingManager() => VRCTrackingManager.field_Private_Static_VRCTrackingManager_0;

        public static float GetTrackingScale() => GetVRCTrackingManager().transform.localScale.x;

        // RootMotion.FinalIK.IKSolverVR extensions
        public static void SetLegIKWeight(this RootMotion.FinalIK.IKSolverVR p_solver, HumanBodyBones p_leg, float p_weight)
        {
            var l_leg = (p_leg == HumanBodyBones.LeftFoot) ? p_solver.leftLeg : p_solver.rightLeg;
            if (l_leg != null)
            {
                l_leg.positionWeight = p_weight;
                l_leg.rotationWeight = p_weight;
            }
        }

        // Math extensions
        public static Matrix4x4 GetMatrix(this Transform p_transform, bool p_pos = true, bool p_rot = true, bool p_scl = false)
        {
            return Matrix4x4.TRS(p_pos ? p_transform.position : Vector3.zero, p_rot ? p_transform.rotation : Quaternion.identity, p_scl ? p_transform.localScale : Vector3.one);
        }
        public static Matrix4x4 AsMatrix(this Quaternion p_quat)
        {
            return Matrix4x4.Rotate(p_quat);
        }
    }
}
