using System;
using MelonLoader;
using SerpentCore.Core.Helpers;
using SerpentCore.Core.Types;
using UnityEngine;
using UnityEngine.XR;

namespace SerpentCore.Core.Managers
{
    public static class RadialPuppetManager
    {
        private static RadialPuppetMenu _radialPuppetMenuRight;
        private static RadialPuppetMenu _radialPuppetMenuLeft;
        private static RadialPuppetMenu _current;
        private static ActionMenuHand _hand;
        private static bool open;
        private static bool _restricted;
        private static float _currentValue;

        public static float RadialPuppetValue { get; set; }
        public static Action<float> onUpdate { get; set; }

        public static void Setup()
        {
            _radialPuppetMenuLeft = Utilities
                .CloneGameObject("UserInterface/ActionMenu/Container/MenuL/ActionMenu/RadialPuppetMenu",
                    "UserInterface/ActionMenu/Container/MenuL/ActionMenu").GetComponent<RadialPuppetMenu>();
            _radialPuppetMenuRight = Utilities
                .CloneGameObject("UserInterface/ActionMenu/Container/MenuR/ActionMenu/RadialPuppetMenu",
                    "UserInterface/ActionMenu/Container/MenuR/ActionMenu").GetComponent<RadialPuppetMenu>();
        }

        public static void OnUpdate()
        {
            //Probably a better more efficient way to do all this
            if (_current != null && _current.gameObject.gameObject.active)
            {
                if (XRDevice.isPresent)
                {
                    if (_hand == ActionMenuHand.Right)
                    {
                        if (Input.GetAxis(Constants.RIGHT_TRIGGER) >= 0.4f)
                        {
                            CloseRadialMenu();
                            return;
                        }
                    }
                    else if (_hand == ActionMenuHand.Left)
                    {
                        if (Input.GetAxis(Constants.LEFT_TRIGGER) >= 0.4f)
                        {
                            CloseRadialMenu();
                            return;
                        }
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    CloseRadialMenu();
                    return;
                }

                UpdateMathStuff();
                CallUpdateAction();
            }
        }

        public static void OpenRadialMenu(float startingValue, Action<float> onUpdate, string title, PedalOption pedalOption, bool restricted = false)
        {
            if (open) return;
            switch (Utilities.GetActionMenuHand())
            {
                case ActionMenuHand.Invalid:
                    return;
                case ActionMenuHand.Left:
                    _current = _radialPuppetMenuLeft;
                    _hand = ActionMenuHand.Left;
                    open = true;
                    break;
                case ActionMenuHand.Right:
                    _current = _radialPuppetMenuRight;
                    _hand = ActionMenuHand.Right;
                    open = true;
                    break;
                   
            }

            RadialPuppetManager._restricted = restricted;
            Input.ResetInputAxes();
            InputManager.ResetMousePos();
            _current.gameObject.SetActive(true);
            _current.GetFill().SetFillAngle(startingValue * 360); //Please dont break
            RadialPuppetManager.onUpdate = onUpdate;
            _currentValue = startingValue;
            
            _current.GetTitle().text = title;
            _current.GetCenterText().text = $"{Mathf.Round(startingValue * 100f)}%";
            _current.GetFill().UpdateGeometry();
            _current.transform.localPosition = pedalOption.GetActionButton().transform.localPosition; //new Vector3(-256f, 0, 0); 
            var angleOriginal = Utilities.ConvertFromEuler(startingValue * 360);
            var eulerAngle = Utilities.ConvertFromDegToEuler(angleOriginal);
            var actionMenu = Utilities.GetActionMenuOpener().GetActionMenu();
            actionMenu.DisableInput();
            actionMenu.SetMainMenuOpacity(0.5f);
            _current.UpdateArrow(angleOriginal, eulerAngle);
        }

        public static void CloseRadialMenu()
        {
            if (_current == null) return;
            CallUpdateAction();
            _current.gameObject.SetActive(false);
            _current = null;
            open = false;
            _hand = ActionMenuHand.Invalid;
            var actionMenu = Utilities.GetActionMenuOpener().GetActionMenu();
            actionMenu.EnableInput();
            actionMenu.SetMainMenuOpacity();
        }

        private static void CallUpdateAction()
        {
            try
            {
                onUpdate?.Invoke(_current.GetFill().GetFillAngle() / 360f);
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Exception caught in onUpdate action passed to Radial Puppet: {e}");
            }
        }

        private static void UpdateMathStuff()
        {
            var mousePos = _hand == ActionMenuHand.Left ? InputManager.LeftInput : InputManager.RightInput;
            _radialPuppetMenuRight.GetCursor().transform.localPosition = mousePos * 4;

            if (Vector2.Distance(mousePos, Vector2.zero) > 12)
            {
                var angleOriginal = Mathf.Round(Mathf.Atan2(mousePos.y, mousePos.x) * Constants.RAD_TO_DEG);
                var eulerAngle = Utilities.ConvertFromDegToEuler(angleOriginal);
                var normalisedAngle = eulerAngle / 360f;
                if (Math.Abs(normalisedAngle - _currentValue) < 0.0001f) return;
                if (!_restricted)
                {
                    _current.SetAngle(eulerAngle);
                    _current.UpdateArrow(angleOriginal, eulerAngle);
                }
                else
                {
                    if (_currentValue > normalisedAngle)
                    {
                        if (_currentValue - normalisedAngle < 0.5f)
                        {
                            _current.SetAngle(eulerAngle);
                            _current.UpdateArrow(angleOriginal, eulerAngle);
                            _currentValue = normalisedAngle;
                        }
                        else
                        {
                            _current.SetAngle(360);
                            _current.UpdateArrow(90, 360);
                            _currentValue = 1f;
                        }
                    }
                    else
                    {
                        if (normalisedAngle - _currentValue < 0.5f)
                        {
                            _current.SetAngle(eulerAngle);
                            _current.UpdateArrow(angleOriginal, eulerAngle);
                            _currentValue = normalisedAngle;
                        }
                        else
                        {
                            _current.SetAngle(0);
                            _current.UpdateArrow(90, 0);
                            _currentValue = 0;
                        }
                    }
                }
            }
        }
    }
}