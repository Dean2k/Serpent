﻿using ReModAres.Core;
using ReModAres.Core.Managers;
using ReModAres.Core.UI.QuickMenu;
using System;
using System.Linq;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace Serpent.Components
{
    internal class StopSpinComponent : ModComponent
    {
        private ConfigValue<bool> StopAvatarSpinEnabled;
        private ReMenuToggle _stopAvatarSpinToggle;

        public StopSpinComponent()
        {
            StopAvatarSpinEnabled = new ConfigValue<bool>(nameof(StopAvatarSpinEnabled), true);
            StopAvatarSpinEnabled.OnValueChanged += () => _stopAvatarSpinToggle.Toggle(StopAvatarSpinEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage(Page.PageNames.Avatars);
            _stopAvatarSpinToggle = menu.AddToggle("Stop Avatar Spin",
                "Stops Avatar Spinning", ToggleSpin,
                StopAvatarSpinEnabled);
            ToggleSpin(StopAvatarSpinEnabled);
        }

        public void ToggleSpin(bool value)
        {
            StopAvatarSpinEnabled.SetValue(value);

            if (!value) return;

            GameObject scrollerContainer = new GameObject("ScrollerContainer", new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[2] { Il2CppType.Of<RectMask2D>(), Il2CppType.Of<RectTransform>() }));
            RectTransform scrollerContainerRect = scrollerContainer.GetComponent<RectTransform>();
            scrollerContainerRect.SetParent(GameObject.Find("UserInterface/MenuContent/Screens/Avatar").transform);
            scrollerContainerRect.anchoredPosition3D = new Vector3(-565, 20, 1);
            scrollerContainerRect.localScale = Vector3.one;
            scrollerContainerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
            scrollerContainerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 650);

            GameObject scrollerContent = new GameObject("ScrollerContent", new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[2] { Il2CppType.Of<Image>(), Il2CppType.Of<RectTransform>() }));
            RectTransform scrollerContentRect = scrollerContent.GetComponent<RectTransform>();
            scrollerContentRect.SetParent(scrollerContainerRect);

            scrollerContentRect.anchoredPosition3D = Vector3.zero;
            scrollerContentRect.localScale = Vector3.one;
            scrollerContentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 800);
            scrollerContentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1300);
            scrollerContentRect.GetComponent<Image>().color = new Color(0, 0, 0, 0);

            ScrollRect scrollRect = scrollerContainer.AddComponent<ScrollRect>();
            scrollRect.content = scrollerContentRect;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            scrollRect.decelerationRate = 0.03f;
            scrollRect.scrollSensitivity = 6;
            scrollRect.onValueChanged = new ScrollRect.ScrollRectEvent();
            GameObject pedestal = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/AvatarPreviewBase/MainRoot/MainModel");
            MonoBehaviour autoTurn = pedestal.GetComponents<MonoBehaviour>().First(c => c.GetIl2CppType().FullName == "UnityStandardAssets.Utility.AutoMoveAndRotate");
            UnityEngine.Object.DestroyImmediate(autoTurn);

            Vector2 lastPos = Vector2.zero;

            scrollRect.onValueChanged.AddListener(new Action<Vector2>((pos) =>
            {
                Vector2 velocity = pos - lastPos;

                lastPos = pos;
                Vector2 scrollRectVelocity = scrollRect.velocity;
                if (scrollRect.horizontalNormalizedPosition > 0.01f && velocity.x > 0)
                {
                    scrollRect.horizontalNormalizedPosition = 0f;
                    lastPos.x = -1;
                }
                else if (scrollRect.horizontalNormalizedPosition < -0.01f && velocity.x < 0)
                {
                    scrollRect.horizontalNormalizedPosition = 0f;
                    lastPos.x = 1;
                }
                pedestal.transform.Rotate(new Vector2(0, velocity.normalized.x), velocity.magnitude * 375 * Time.deltaTime);
                scrollRect.velocity = scrollRectVelocity;
            }));
        }
    }
}
