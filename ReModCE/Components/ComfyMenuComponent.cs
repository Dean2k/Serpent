using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using Serpent.Loader;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using UnityEngine.XR;

namespace Serpent.Components
{
    internal class ComfyMenuComponent : ModComponent
    {
        private ConfigValue<bool> ComfyMenuEnabled;
        private ReMenuToggle _comfyMenuToggle;

        public ComfyMenuComponent()
        {
            ComfyMenuEnabled = new ConfigValue<bool>(nameof(ComfyMenuEnabled), true);
            ComfyMenuEnabled.OnValueChanged += () => _comfyMenuToggle.Toggle(ComfyMenuEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetCategoryPage(Page.PageNames.Utility).GetCategory(Page.Categories.Utilties.QualityOfLife);
            _comfyMenuToggle = menu.AddToggle("Comfy Menu",
                "Open directly in front of your viewpoint when you are in VR, instead of down in it's default position. (must restart game)", ComfyMenuEnabled.SetValue,
                ComfyMenuEnabled);

            if (ComfyMenuEnabled)
            {
                var method = PlaceUiMethod;
                if (method == null)
                {
                    ReLogger.Error("Couldn't find VRCUiManager PlaceUi method to patch.");
                    return;
                }

                Serpent.Harmony.Patch(PlaceUiMethod, GetLocalPatch(nameof(PlaceUiPatch)));
            }
        }
        public static bool IsInVR()
        {
            return XRDevice.isPresent;
        }

        public static VRCPlayer GetVRCPlayer()
        {
            return VRCPlayer.field_Internal_Static_VRCPlayer_0;
        }

        public static VRCTrackingManager GetVRCTrackingManager()
        {
            return VRCTrackingManager.field_Private_Static_VRCTrackingManager_0;
        }

        public static VRCVrCamera GetVRCVrCamera()
        {
            return VRCVrCamera.field_Private_Static_VRCVrCamera_0;
        }

        public static Vector3 GetWorldCameraPosition()
        {
            VRCVrCamera camera = GetVRCVrCamera();
            var type = camera.GetIl2CppType();
            if (type == Il2CppType.Of<VRCVrCameraSteam>())
            {
                VRCVrCameraSteam steam = camera.Cast<VRCVrCameraSteam>();
                Transform transform1 = steam.field_Private_Transform_0;
                Transform transform2 = steam.field_Private_Transform_1;
                if (transform1.name == "Camera (eye)")
                {
                    return transform1.position;
                }
                else if (transform2.name == "Camera (eye)")
                {
                    return transform2.position;
                }
            }
            else if (type == Il2CppType.Of<VRCVrCameraUnity>())
            {
                VRCVrCameraUnity unity = camera.Cast<VRCVrCameraUnity>();
                return unity.field_Public_Camera_0.transform.position;
            }
            else if (type == Il2CppType.Of<VRCVrCameraWave>())
            {
                VRCVrCameraWave wave = camera.Cast<VRCVrCameraWave>();
                return wave.transform.position;
            }
            return camera.transform.parent.TransformPoint(GetLocalCameraPosition());
        }

        public static Vector3 GetLocalCameraPosition()
        {
            VRCVrCamera camera = GetVRCVrCamera();
            var type = camera.GetIl2CppType();
            if (type == Il2CppType.Of<VRCVrCameraSteam>())
            {
                VRCVrCameraSteam steam = camera.Cast<VRCVrCameraSteam>();
                Transform transform1 = steam.field_Private_Transform_0;
                Transform transform2 = steam.field_Private_Transform_1;
                if (transform1.name == "Camera (eye)")
                {
                    return camera.transform.parent.InverseTransformPoint(transform1.position);
                }
                else if (transform2.name == "Camera (eye)")
                {
                    return camera.transform.parent.InverseTransformPoint(transform2.position);
                }
                else
                {
                    return Vector3.zero;
                }
            }
            else if (type == Il2CppType.Of<VRCVrCameraUnity>())
            {
                if (IsInVR())
                {
                    return camera.transform.localPosition + InputTracking.GetLocalPosition(XRNode.CenterEye);
                }
                VRCVrCameraUnity unity = camera.Cast<VRCVrCameraUnity>();
                return camera.transform.parent.InverseTransformPoint(unity.field_Public_Camera_0.transform.position);
            }
            else if (type == Il2CppType.Of<VRCVrCameraWave>())
            {
                VRCVrCameraWave wave = camera.Cast<VRCVrCameraWave>();
                return wave.field_Public_Transform_0.InverseTransformPoint(camera.transform.position);
            }
            return camera.transform.localPosition;
        }

        private static bool PlaceUiPatch(VRCUiManager __instance, bool __0)
        {
            if (!IsInVR()) return true;
            float num = GetVRCTrackingManager() != null ? GetVRCTrackingManager().transform.localScale.x : 1f;
            if (num <= 0f)
            {
                num = 1f;
            }
            var playerTrackingDisplay = __instance.transform;
            var unscaledUIRoot = __instance.transform.Find("UnscaledUI");
            playerTrackingDisplay.position = GetWorldCameraPosition();
            Vector3 rotation = GameObject.Find("Camera (eye)").transform.rotation.eulerAngles;
            Vector3 euler = new Vector3(rotation.x - 30f, rotation.y, 0f);
            //if (rotation.x > 0f && rotation.x < 300f) rotation.x = 0f;
            if (GetVRCPlayer() == null)
            {
                euler.x = euler.z = 0f;
            }
            if (!__0)
            {
                playerTrackingDisplay.rotation = Quaternion.Euler(euler);
            }
            else
            {
                Quaternion quaternion = Quaternion.Euler(euler);
                if (!(Quaternion.Angle(playerTrackingDisplay.rotation, quaternion) < 15f))
                {
                    if (!(Quaternion.Angle(playerTrackingDisplay.rotation, quaternion) < 25f))
                    {
                        playerTrackingDisplay.rotation = Quaternion.RotateTowards(playerTrackingDisplay.rotation, quaternion, 5f);
                    }
                    else
                    {
                        playerTrackingDisplay.rotation = Quaternion.RotateTowards(playerTrackingDisplay.rotation, quaternion, 1f);
                    }
                }
            }
            if (num >= 0f)
            {
                playerTrackingDisplay.localScale = num * Vector3.one;
            }
            else
            {
                playerTrackingDisplay.localScale = Vector3.one;
            }
            if (num > float.Epsilon)
            {
                unscaledUIRoot.localScale = 1f / num * Vector3.one;
            }
            else
            {
                unscaledUIRoot.localScale = Vector3.one;
            }
            return false;
        }

        public static MethodInfo PlaceUiMethod
        {
            get
            {
                if (_placeUi == null)
                {
                    try
                    {
                        var xrefs = XrefScanner.XrefScan(typeof(VRCUiManager).GetMethod(nameof(VRCUiManager.LateUpdate)));
                        foreach (var x in xrefs)
                        {
                            if (x.Type == XrefType.Method && x.TryResolve() != null &&
                                x.TryResolve().GetParameters().Length == 2 &&
                                x.TryResolve().GetParameters().All(a => a.ParameterType == typeof(bool)))
                            {
                                _placeUi = (MethodInfo)x.TryResolve();
                                break;
                            }
                        };
                    }
                    catch
                    {
                    }
                }
                return _placeUi;
            }
        }

        private static MethodInfo _placeUi;
    }
}
