using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using SerpentCore.Core.VRChat;
using Serpent.Loader;
using Serpent.Managers;
using System;
using UnityEngine;
using UnityEngine.UI;
using VRCSDK2;

namespace Serpent.Components
{
    internal class PortableMirrorComponent : ModComponent
    {
        private ConfigValue<float> MirrorScaleX;
        private ConfigValue<float> MirrorScaleY;
        private bool PortableMirrorEnabled;
        private ConfigValue<bool> OptimizedMirror;
        private ConfigValue<bool> CanPickupMirror;
        private ReMenuToggle _portableMirrorToggle;
        private ReMenuToggle _optimizedMirrorToggle;
        private ReMenuToggle _pickupToggle;
        private ReMenuButton _mirrorScaleX;
        private ReMenuButton _mirrorScaleY;
        private GameObject _mirror;

        public PortableMirrorComponent()
        {
            PortableMirrorEnabled = false;

            OptimizedMirror = new ConfigValue<bool>(nameof(OptimizedMirror), true);
            OptimizedMirror.OnValueChanged += () => _optimizedMirrorToggle.Toggle(OptimizedMirror);

            CanPickupMirror = new ConfigValue<bool>(nameof(CanPickupMirror), true);
            CanPickupMirror.OnValueChanged += () => _pickupToggle.Toggle(CanPickupMirror);

            MirrorScaleX = new ConfigValue<float>(nameof(MirrorScaleX), 5f);
            MirrorScaleY = new ConfigValue<float>(nameof(MirrorScaleY), 3f);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetCategoryPage(Page.PageNames.Utility).GetCategory(Page.Categories.Utilties.QualityOfLife);
            var subMenu = menu.AddMenuPage("Portable Mirror", "spawn a portable mirror", ResourceManager.GetSprite("remodce.mirror"));
            _portableMirrorToggle = subMenu.AddToggle("Portable Mirror",
                "Portable Mirror", ButtonToggleMirror,
                PortableMirrorEnabled);

            _optimizedMirrorToggle = subMenu.AddToggle("Optimized Mirror",
                "Optimized Mirror", ButtonToggleOptimized,
                OptimizedMirror);

            _pickupToggle = subMenu.AddToggle("Pickup",
                "Allow the mirror to be picked up", ButtonTogglePickup,
                CanPickupMirror);

            _mirrorScaleX = subMenu.AddButton($"Mirror Scale X: {MirrorScaleX}",
                    "Set the mirror scale X",
                    () =>
                    {
                        VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Mirror Scale X",
                            MirrorScaleX.ToString(), InputField.InputType.Standard, true, "Submit",
                            (s, k, t) =>
                            {
                                if (string.IsNullOrEmpty(s))
                                    return;

                                if (!float.TryParse(s, out var maxAudioDist))
                                    return;
                                MirrorScaleX.SetValue(maxAudioDist);
                                _mirrorScaleX.Text = $"Mirror Scale X: {MirrorScaleX}";
                            }, null);
                    }, ResourceManager.GetSprite("remodce.max"));

            _mirrorScaleY = subMenu.AddButton($"Mirror Scale Y: {MirrorScaleY}",
                   "Set the mirror scale Y",
                   () =>
                   {
                       VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Mirror Scale Y",
                           MirrorScaleY.ToString(), InputField.InputType.Standard, true, "Submit",
                           (s, k, t) =>
                           {
                               if (string.IsNullOrEmpty(s))
                                   return;

                               if (!float.TryParse(s, out var maxAudioDist))
                                   return;
                               MirrorScaleY.SetValue(maxAudioDist);
                               _mirrorScaleY.Text = $"Mirror Scale X: {MirrorScaleY}";
                           }, null);
                   }, ResourceManager.GetSprite("remodce.max"));
        }

        public void ButtonTogglePickup(bool value)
        {
            CanPickupMirror.SetValue(value);
            ToggleMirror();
        }

        public void ButtonToggleOptimized(bool value)
        {
            OptimizedMirror.SetValue(value);
            ToggleMirror();
        }

        public void ButtonToggleMirror(bool value)
        {
            PortableMirrorEnabled = value;
            if (value)
            {
                ToggleMirror();
            }
            else
            {
                if (_mirror != null)
                {
                    UnityEngine.Object.Destroy(_mirror);
                    _mirror = null;
                }
            }
        }

        private void ToggleMirror()
        {
            if (_mirror != null)
            {
                UnityEngine.Object.Destroy(_mirror);
                _mirror = null;
            }

            VRCPlayer player = GetVRCPlayer();
            Vector3 pos = player.transform.position + player.transform.forward;
            pos.y += MirrorScaleY / 2;
            GameObject mirror = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mirror.transform.position = pos;
            mirror.transform.rotation = player.transform.rotation;
            mirror.transform.localScale = new Vector3(MirrorScaleX, MirrorScaleY, 1f);
            mirror.name = "PortableMirror";
            UnityEngine.Object.Destroy(mirror.GetComponent<Collider>());
            mirror.GetOrAddComponent<BoxCollider>().size = new Vector3(1f, 1f, 0.05f);
            mirror.GetOrAddComponent<BoxCollider>().isTrigger = true;
            try
            {
                mirror.GetOrAddComponent<MeshRenderer>().material.shader = Shader.Find("FX/MirrorReflection");

                mirror.GetOrAddComponent<VRC_MirrorReflection>().m_ReflectLayers = new LayerMask
                {
                    value = OptimizedMirror ? 263680 : -1025
                };
            }
            catch (Exception ex) { ReLogger.Msg(ex.Message + "part 1"); }

            try
            {
                mirror.GetOrAddComponent<VRC_Pickup>().proximity = 0.3f;
                mirror.GetOrAddComponent<VRC_Pickup>().pickupable = CanPickupMirror;
                mirror.GetOrAddComponent<VRC_Pickup>().allowManipulationWhenEquipped = false;
            }
            catch (Exception ex) { ReLogger.Msg(ex.Message + "part 2"); }
            try
            {

                mirror.GetOrAddComponent<Rigidbody>().useGravity = false;
                mirror.GetOrAddComponent<Rigidbody>().isKinematic = true;
            }
            catch (Exception ex) { ReLogger.Msg(ex.Message + "part 3"); }

            _mirror = mirror;
        }

        public VRCPlayer GetVRCPlayer()
        {
            return VRCPlayer.field_Internal_Static_VRCPlayer_0;
        }
    }
}
