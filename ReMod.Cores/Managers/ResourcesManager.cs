using System.IO;
using System.Reflection;
using MelonLoader;
using SerpentCore.Core.Helpers;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace SerpentCore.Core.Managers
{
    public static class ResourcesManager
    {
        private static GameObject _lockPrefab;
        private static Texture2D _pageOne;
        private static Texture2D _pageTwo;
        private static Texture2D _pageThree;
        private static Texture2D _pageFour;
        private static Texture2D _pageFive;
        private static Texture2D _pageSix;
        private static Texture2D _pageSeven;
        private static Texture2D _locked;
        private static Texture2D _modsSectionIcon;

        public static void LoadTextures()
        {
            AssetBundle iconsAssetBundle;
            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("ActionMenuApi.actionmenuapi.icons"))
            using (var tempStream = new MemoryStream((int) stream.Length))
            {
                stream.CopyTo(tempStream);

                iconsAssetBundle = AssetBundle.LoadFromMemory_Internal(tempStream.ToArray(), 0);
                iconsAssetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            }

            _modsSectionIcon = iconsAssetBundle
                .LoadAsset_Internal("Assets/ActionMenuApi/vrcmg.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
            _modsSectionIcon.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            _pageOne = iconsAssetBundle.LoadAsset_Internal("Assets/ActionMenuApi/1.png", Il2CppType.Of<Texture2D>())
                .Cast<Texture2D>();
            _pageOne.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            _pageTwo = iconsAssetBundle.LoadAsset_Internal("Assets/ActionMenuApi/2.png", Il2CppType.Of<Texture2D>())
                .Cast<Texture2D>();
            _pageTwo.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            _pageThree = iconsAssetBundle.LoadAsset_Internal("Assets/ActionMenuApi/3.png", Il2CppType.Of<Texture2D>())
                .Cast<Texture2D>();
            _pageThree.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            _pageFour = iconsAssetBundle.LoadAsset_Internal("Assets/ActionMenuApi/4.png", Il2CppType.Of<Texture2D>())
                .Cast<Texture2D>();
            _pageFour.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            _pageFive = iconsAssetBundle.LoadAsset_Internal("Assets/ActionMenuApi/5.png", Il2CppType.Of<Texture2D>())
                .Cast<Texture2D>();
            _pageFive.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            _pageSix = iconsAssetBundle.LoadAsset_Internal("Assets/ActionMenuApi/6.png", Il2CppType.Of<Texture2D>())
                .Cast<Texture2D>();
            _pageSix.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            _pageSeven = iconsAssetBundle.LoadAsset_Internal("Assets/ActionMenuApi/7.png", Il2CppType.Of<Texture2D>())
                .Cast<Texture2D>();
            _pageSeven.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            _locked = iconsAssetBundle.LoadAsset_Internal("Assets/ActionMenuApi/locked.png", Il2CppType.Of<Texture2D>())
                .Cast<Texture2D>();
            _locked.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            MelonLogger.Msg("Loaded textures");
        }

        public static void InitLockGameObject()
        {
            _lockPrefab = Object.Instantiate(ActionMenuDriver.prop_ActionMenuDriver_0.GetRightOpener().GetActionMenu()
                .GetPedalOptionPrefab().GetComponent<PedalOption>().GetActionButton().gameObject.GetChild("Inner")
                .GetChild("Folder Icon"));
            Object.DontDestroyOnLoad(_lockPrefab);
            _lockPrefab.active = false;
            _lockPrefab.gameObject.name = Constants.LOCKED_PEDAL_OVERLAY_GAMEOBJECT_NAME;
            _lockPrefab.GetComponent<RawImage>().texture = _locked;
            MelonLogger.Msg("Created lock gameobject");
        }

        public static Texture2D GetPageIcon(int pageIndex)
        {
            return pageIndex switch
            {
                1 => _pageOne,
                2 => _pageTwo,
                3 => _pageThree,
                4 => _pageFour,
                5 => _pageFive,
                6 => _pageSix,
                7 => _pageSeven,
                _ => null
            };
        }

        public static void AddLockChildIcon(GameObject parent)
        {
            var lockedGameObject = Object.Instantiate(_lockPrefab, parent.transform, false);
            lockedGameObject.SetActive(true);
            lockedGameObject.transform.localPosition = new Vector3(50, -25, 0);
            lockedGameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }

        public static Texture2D GetModsSectionIcon()
        {
            return _modsSectionIcon;
        }
    }
}