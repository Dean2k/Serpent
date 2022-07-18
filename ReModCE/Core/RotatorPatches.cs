using MelonLoader;
using Serpent.Loader;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;

namespace Serpent.Core
{
    internal class RotatorPatches
    {
        private static ApplyPlayerMotion origApplyPlayerMotion;

        private static void ApplyPlayerMotionPatch(Vector3 playerWorldMotion, Quaternion playerWorldRotation)
        {
            origApplyPlayerMotion(playerWorldMotion, RotationSystem.Rotating ? Quaternion.identity : playerWorldRotation);
        }

        internal static bool PatchMethods()
        {
            if (Utilities.IsInVR)
                try
                {
                    // Fixes spinning issue
                    // TL;DR Prevents the tracking manager from applying rotational force
                    MethodInfo applyPlayerMotionMethod = typeof(VRCTrackingManager).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                                   .Where(
                                                                                       m => m.Name.StartsWith("Method_Public_Static_Void_Vector3_Quaternion")
                                                                                            && !m.Name.Contains("_PDM_")).First(
                                                                                       m => XrefScanner.UsedBy(m).Any(
                                                                                           xrefInstance => xrefInstance.Type == XrefType.Method
                                                                                                           && xrefInstance.TryResolve()?.ReflectedType
                                                                                                                          ?.Equals(typeof(VRC_StationInternal))
                                                                                                           == true));
                    origApplyPlayerMotion = Patch<ApplyPlayerMotion>(applyPlayerMotionMethod, GetDetour(nameof(ApplyPlayerMotionPatch)));
                }
                catch (Exception e)
                {
                    ReLogger.Error("Failed to patch ApplyPlayerMotion\n" + e.Message);
                    return false;
                }

            return true;
        }

        private static unsafe TDelegate Patch<TDelegate>(MethodBase originalMethod, IntPtr patchDetour)
        {
            Debug.Assert(typeof(TDelegate).GetCustomAttribute<UnmanagedFunctionPointerAttribute>() != null
                         && typeof(TDelegate).GetCustomAttribute<UnmanagedFunctionPointerAttribute>().CallingConvention
                         == CallingConvention.Cdecl, "You donkey, you fucked up the Native Delegate not having the right attribute");

            IntPtr original = *(IntPtr*)UnhollowerSupport.MethodBaseToIl2CppMethodInfoPointer(originalMethod);
            MelonUtils.NativeHookAttach((IntPtr)(&original), patchDetour);
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(original);
        }

        private static IntPtr GetDetour(string name)
        {
            return typeof(RotatorPatches).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ApplyPlayerMotion(Vector3 playerWorldMotion, Quaternion playerWorldRotation);
    }
}
