﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReModCE.Loader;
using ReModCE.VRChat;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;
using VRC.SDKBase.Validation.Performance.Stats;
using Category = UiAvatarList.EnumNPublicSealedvaInPuMiFaSpClPuLiCrUnique;

namespace ReModCE.UI
{
    internal class ReAvatarList : UIElement
    {
        private static GameObject _legacyAvatarList;
        private static GameObject LegacyAvatarList
        {
            get
            {
                if (_legacyAvatarList == null)
                {
                    _legacyAvatarList = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/Legacy Avatar List");
                }

                return _legacyAvatarList;
            }
        }

        private readonly UiAvatarList _avatarList;
        private readonly ReUiButton _favoriteButton;
        private readonly ReUiButton _refreshButton;
        private readonly ReUiButton _nextPageButton;
        private readonly ReUiButton _prevPageButton;
        private readonly ReUiText _pageCount;

        private readonly Il2CppSystem.Collections.Generic.List<ApiAvatar> _allAvatars =
            new Il2CppSystem.Collections.Generic.List<ApiAvatar>();

        private int _currentPage;

        private const int MaxAvatarsPerPage = 100;

        private SimpleAvatarPedestal AvatarPedestal => _avatarList.field_Public_SimpleAvatarPedestal_0;

        public ReAvatarList(string title) : base(
            LegacyAvatarList,
            LegacyAvatarList.transform.parent,
            $"{title}AvatarList")
        {
            _avatarList = GameObject.GetComponent<UiAvatarList>();
            _avatarList.clearUnseenListOnCollapse = false;
            _avatarList.field_Public_EnumNPublicSealedvaInPuMiFaSpClPuLiCrUnique_0 =
                Category.SpecificList;
            GameObject.transform.SetSiblingIndex(0);
            AvatarPedestal.field_Internal_Action_3_String_GameObject_AvatarPerformanceStats_0 = new Action<string, GameObject, AvatarPerformanceStats>(OnAvatarInstantiated);

            var expandButton = GameObject.GetComponentInChildren<Button>(true);
            var textComponent = expandButton.GetComponentInChildren<Text>();
            textComponent.text = title;

            _favoriteButton = new ReUiButton("Favorite", new Vector2(-600f, 375f), new Vector2(0.7f, 1f), () => FavoriteAvatar(AvatarPedestal.field_Internal_ApiAvatar_0),
                GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Favorite Button").transform.parent);

            _refreshButton = new ReUiButton("↻", new Vector3(980f, 0f),
                new Vector2(0.25f, 1), () => Refresh(), expandButton.transform);

            _nextPageButton = new ReUiButton("→", new Vector2(900f, 0f), new Vector2(0.25f, 1f), () =>
            {
                _currentPage += 1;
                Refresh();
            }, expandButton.transform);

            _prevPageButton = new ReUiButton("←", new Vector2(750f, 0f), new Vector2(0.25f, 1f), () =>
            {
                _currentPage -= 1;
                Refresh();
            }, expandButton.transform);

            _pageCount = new ReUiText("0 / 0", new Vector2(825f, 0f), new Vector2(0.25f, 1f), expandButton.transform);

            Refresh();
        }

        private void OnAvatarInstantiated(string url, GameObject avatar, AvatarPerformanceStats avatarPerformanceStats)
        {
            _favoriteButton.Text = HasAvatarFavorited(AvatarPedestal.field_Internal_ApiAvatar_0.id) ? "Unfavorite" : "Favorite";
        }

        private void FavoriteAvatar(ApiAvatar apiAvatar)
        {
            var hasFavorited = HasAvatarFavorited(apiAvatar.id);
            if (!hasFavorited)
            {
                _allAvatars.Add(apiAvatar);
                Refresh();
                _favoriteButton.Text = "Unfavorite";
            }
            else
            {
                _allAvatars.Remove(apiAvatar);
                Refresh();
                _favoriteButton.Text = "Favorite";
            }

            Refresh();
        }

        private bool HasAvatarFavorited(string id)
        {
            foreach (var avi in _allAvatars)
            {
                if (avi.id == AvatarPedestal.field_Internal_ApiAvatar_0.id)
                {
                    return true;
                }
            }

            return false;
        }

        public void Refresh(Il2CppSystem.Collections.Generic.List<ApiAvatar> avatars = null)
        {
            if (avatars == null)
                avatars = _allAvatars;

            var pagesCount = avatars.Count / MaxAvatarsPerPage;
            _currentPage = Mathf.Clamp(_currentPage, 0, pagesCount);

            _pageCount.Text = $"{_currentPage + 1} / {pagesCount + 1}";
            var cutDown = avatars.GetRange(_currentPage * MaxAvatarsPerPage, Math.Abs(_currentPage * MaxAvatarsPerPage - avatars.Count));
            if (cutDown.Count > MaxAvatarsPerPage)
            {
                cutDown.RemoveRange(MaxAvatarsPerPage, cutDown.Count - MaxAvatarsPerPage);
            }

            _prevPageButton.Interactable = _currentPage > 0;
            _nextPageButton.Interactable = _currentPage < avatars.Count / MaxAvatarsPerPage;

            _avatarList.StartRenderElementsCoroutine(cutDown);
        }
    }
}