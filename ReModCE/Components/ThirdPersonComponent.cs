﻿using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using Serpent.Managers;
using System;
using UnityEngine;

namespace Serpent.Components
{
    internal enum ThirdPersonMode
    {
        Off = 0,
        Back,
        Front
    }

    internal class ThirdPersonComponent : ModComponent
    {
        private Camera _cameraBack;
        private Camera _cameraFront;
        private Camera _referenceCamera;

        private ThirdPersonMode _cameraSetup;

        private ConfigValue<bool> EnableThirdpersonHotkey;
        private ReMenuToggle _hotkeyToggle;

        public ThirdPersonComponent()
        {
            EnableThirdpersonHotkey = new ConfigValue<bool>(nameof(EnableThirdpersonHotkey), true);
            EnableThirdpersonHotkey.OnValueChanged += () => _hotkeyToggle.Toggle(EnableThirdpersonHotkey);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var hotkeyMenu = uiManager.MainMenu.GetMenuPage("Hotkeys");
            _hotkeyToggle = hotkeyMenu.AddToggle("Thirdperson Hotkey", "Enable/Disable thirdperson hotkey", EnableThirdpersonHotkey.SetValue, EnableThirdpersonHotkey);

            var cameraObject = GameObject.Find("Camera (eye)");

            if (cameraObject == null)
            {
                cameraObject = GameObject.Find("CenterEyeAnchor");

                if (cameraObject == null)
                {
                    return;
                }
            }

            _referenceCamera = cameraObject.GetComponent<Camera>();
            if (_referenceCamera == null)
                return;

            _cameraBack = CreateCamera(ThirdPersonMode.Back, Vector3.zero, 75f);
            _cameraFront = CreateCamera(ThirdPersonMode.Front, new Vector3(0f, 180f, 0f), 75f);
        }

        private Camera CreateCamera(ThirdPersonMode cameraType, Vector3 rotation, float fieldOfView)
        {
            var cameraObject = new GameObject($"{cameraType}Camera");
            cameraObject.transform.localScale = _referenceCamera.transform.localScale;
            cameraObject.transform.parent = _referenceCamera.transform;
            cameraObject.transform.rotation = _referenceCamera.transform.rotation;
            cameraObject.transform.Rotate(rotation);
            cameraObject.transform.position = _referenceCamera.transform.position + (-cameraObject.transform.forward * 2f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.fieldOfView = fieldOfView;
            camera.nearClipPlane /= 4f;

            return camera;
        }

        private void HandleHotkeys()
        {
            if (!EnableThirdpersonHotkey) return;

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
            {
                var mode = _cameraSetup;
                if (++mode > ThirdPersonMode.Front)
                {
                    mode = ThirdPersonMode.Off;
                }

                SetThirdPersonMode(mode);
            }
        }

        private void SetThirdPersonMode(ThirdPersonMode mode)
        {
            _cameraSetup = mode;
            switch (mode)
            {
                case ThirdPersonMode.Off:
                    _cameraBack.enabled = false;
                    _cameraFront.enabled = false;
                    break;
                case ThirdPersonMode.Back:
                    _cameraBack.enabled = true;
                    _cameraFront.enabled = false;
                    break;
                case ThirdPersonMode.Front:
                    _cameraBack.enabled = false;
                    _cameraFront.enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleThirdperson()
        {
            if (_cameraSetup == ThirdPersonMode.Off) return;

            var scrollwheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollwheel > 0f)
            {
                _cameraBack.transform.position += _cameraBack.transform.forward * 0.1f;
                _cameraFront.transform.position -= _cameraBack.transform.forward * 0.1f;
            }
            else if (scrollwheel < 0f)
            {
                _cameraBack.transform.position -= _cameraBack.transform.forward * 0.1f;
                _cameraFront.transform.position += _cameraBack.transform.forward * 0.1f;
            }
        }

        public override void OnUpdate()
        {
            if (_cameraBack == null || _cameraFront == null)
            {
                return;
            }

            HandleHotkeys();
            HandleThirdperson();
        }
    }
}