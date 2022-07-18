using System;
using MelonLoader;
using SerpentCore.Core.Helpers;
using SerpentCore.Core.Types;
using UnityEngine;
using UnityEngine.XR;

namespace SerpentCore.Core.Managers
{
    public static class FourAxisPuppetManager
    {
        private static AxisPuppetMenu _fourAxisPuppetMenuRight;
        private static AxisPuppetMenu _fourAxisPuppetMenuLeft;
        private static ActionMenuHand _hand;
        private static bool open;
        public static AxisPuppetMenu Current { get; private set; }

        public static Vector2 FourAxisPuppetValue { get; set; }

        public static Action<Vector2> onUpdate { get; set; }

        public static void Setup()
        {
            _fourAxisPuppetMenuLeft = Utilities
                .CloneGameObject("UserInterface/ActionMenu/Container/MenuL/ActionMenu/AxisPuppetMenu",
                    "UserInterface/ActionMenu/Container/MenuL/ActionMenu").GetComponent<AxisPuppetMenu>();
            _fourAxisPuppetMenuRight = Utilities
                .CloneGameObject("UserInterface/ActionMenu/Container/MenuR/ActionMenu/AxisPuppetMenu",
                    "UserInterface/ActionMenu/Container/MenuR/ActionMenu").GetComponent<AxisPuppetMenu>();
        }

        public static void OnUpdate()
        {
            //Probably a better more efficient way to do all this
            if (Current != null && Current.gameObject.gameObject.active)
            {
                if (XRDevice.isPresent)
                {
                    if (_hand == ActionMenuHand.Right)
                    {
                        if (Input.GetAxis(Constants.RIGHT_TRIGGER) >= 0.4f)
                        {
                            CloseFourAxisMenu();
                            return;
                        }
                    }
                    else if (_hand == ActionMenuHand.Left)
                    {
                        if (Input.GetAxis(Constants.LEFT_TRIGGER) >= 0.4f)
                        {
                            CloseFourAxisMenu();
                            return;
                        }
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    CloseFourAxisMenu();
                    return;
                }

                FourAxisPuppetValue = (_hand == ActionMenuHand.Left ? InputManager.LeftInput : InputManager.RightInput) / 16;
                var x = FourAxisPuppetValue.x;
                var y = FourAxisPuppetValue.y;
                if (x >= 0)
                {
                    Current.GetFillLeft().SetAlpha(0);
                    Current.GetFillRight().SetAlpha(x);
                }
                else
                {
                    Current.GetFillLeft().SetAlpha(Math.Abs(x));
                    Current.GetFillRight().SetAlpha(0);
                }

                if (y >= 0)
                {
                    Current.GetFillDown().SetAlpha(0);
                    Current.GetFillUp().SetAlpha(y);
                }
                else
                {
                    Current.GetFillDown().SetAlpha(Math.Abs(y));
                    Current.GetFillUp().SetAlpha(0);
                }

                UpdateMathStuff();
                CallUpdateAction();
            }
        }

        public static void OpenFourAxisMenu(string title, Action<Vector2> update, PedalOption pedalOption)
        {
            if (open) return;
            switch (_hand = Utilities.GetActionMenuHand())
            {
                case ActionMenuHand.Invalid:
                    return;
                case ActionMenuHand.Left:
                    Current = _fourAxisPuppetMenuLeft;
                    open = true;
                    break;
                case ActionMenuHand.Right:
                    Current = _fourAxisPuppetMenuRight;
                    open = true;
                    break;
            }
            Input.ResetInputAxes();
            InputManager.ResetMousePos();
            onUpdate = update;
            Current.gameObject.SetActive(true);
            Current.GetTitle().text = title;
            var actionMenu = Utilities.GetActionMenuOpener().GetActionMenu();
            actionMenu.DisableInput();
            actionMenu.SetMainMenuOpacity(0.5f);
            Current.transform.localPosition = pedalOption.GetActionButton().transform.localPosition;
        }

        private static void CallUpdateAction()
        {
            try
            {
                onUpdate?.Invoke(FourAxisPuppetValue);
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Exception caught in onUpdate action passed to Four Axis Puppet: {e}");
            }
        }

        public static void CloseFourAxisMenu()
        {
            if (Current == null) return;
            CallUpdateAction();
            Current.gameObject.SetActive(false);
            Current = null;
            open = false;
            _hand = ActionMenuHand.Invalid;
            var actionMenu = Utilities.GetActionMenuOpener().GetActionMenu();
            actionMenu.SetMainMenuOpacity();
            actionMenu.EnableInput();
        }

        private static void UpdateMathStuff()
        {
            var mousePos = _hand == ActionMenuHand.Left ? InputManager.LeftInput : InputManager.RightInput;
            Current.GetCursor().transform.localPosition = mousePos * 4;
        }
    }
}