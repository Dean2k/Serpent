using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using MelonLoader;
using Newtonsoft.Json;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE_ARES.Core;
using ReModCE_ARES.Loader;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;
using VRC.Core;
using VRC.SDKBase.Validation.Performance.Stats;
using AvatarList = Il2CppSystem.Collections.Generic.List<VRC.Core.ApiAvatar>;
using BuildInfo = ReModCE_ARES.Loader.BuildInfo;

namespace ReModCE_ARES.Components
{
    internal class AvatarFavoritesComponent : ModComponent, IAvatarListOwner
    {
        private ReAvatarList _favoriteAvatarList;
        private ReUiButton _favoriteButton;

        private ReAvatarList _searchedAvatarList;

        private Button.ButtonClickedEvent _changeButtonEvent;

        private const bool EnableApi = true;
        private const string ApiUrl = "https://api.ares-mod.com";
        private string _userAgent = "";
        private HttpClient _httpClient;
        private HttpClientHandler _httpClientHandler;

        private const string PinPath = "UserData/ReModCE_ARES/pin";
        private int _pinCode;
        private ReMenuButton _enterPinButton;

        private ConfigValue<bool> AvatarFavoritesEnabled;
        private ReMenuToggle _enabledToggle;
        private ConfigValue<int> MaxAvatarsPerPage;
        private ReMenuButton _maxAvatarsPerPageButton;

        private List<ReAvatar> _savedAvatars;
        private readonly AvatarList _searchedAvatars;

        private GameObject _avatarScreen;
        private UiInputField _searchBox;
        private UnityAction<string> _searchAvatarsAction;
        private UnityAction<string> _overrideSearchAvatarsAction;
        private UnityAction<string> _emmVRCsearchAvatarsAction;
        private int _updatesWithoutSearch;
        private APIUser user;

        private int _loginRetries;

        public AvatarFavoritesComponent()
        {
            AvatarFavoritesEnabled = new ConfigValue<bool>(nameof(AvatarFavoritesEnabled), true);
            AvatarFavoritesEnabled.OnValueChanged += () =>
            {
                _enabledToggle.Toggle(AvatarFavoritesEnabled);
                _favoriteAvatarList.GameObject.SetActive(AvatarFavoritesEnabled);
                _favoriteButton.GameObject.SetActive(AvatarFavoritesEnabled);
            };
            MaxAvatarsPerPage = new ConfigValue<int>(nameof(MaxAvatarsPerPage), 100);
            MaxAvatarsPerPage.OnValueChanged += () =>
            {
                _favoriteAvatarList.SetMaxAvatarsPerPage(MaxAvatarsPerPage);
            };

            _savedAvatars = new List<ReAvatar>();
            _searchedAvatars = new AvatarList();

            if (File.Exists(PinPath))
            {
                if (!int.TryParse(File.ReadAllText(PinPath), out _pinCode))
                {
                    ReModCE_ARES.LogDebug($"Couldn't read pin file from \"{PinPath}\". File might be corrupted.");
                    ReLogger.Warning($"Couldn't read pin file from \"{PinPath}\". File might be corrupted.");
                }
            }
        }

        private void InitializeNetworkClient()
        {
            if (!EnableApi)
                return;

            _httpClientHandler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };
            _httpClient = new HttpClient(_httpClientHandler);

            var vrHeadset = XRDevice.isPresent ? XRDevice.model : "Desktop";
            vrHeadset = vrHeadset.Replace(' ', '_');

            _userAgent = $"ReModCE/{vrHeadset}.{BuildInfo.Version} (Windows NT 10.0; Win64; x64)";

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
        }

        public override void OnUiManagerInitEarly()
        {
            InitializeNetworkClient();

            _searchedAvatarList = new ReAvatarList("ARES Search", this);

            _favoriteAvatarList = new ReAvatarList("ARES Favorites", this, false);
            _favoriteAvatarList.AvatarPedestal.field_Internal_Action_3_String_GameObject_AvatarPerformanceStats_0 = new Action<string, GameObject, AvatarPerformanceStats>(OnAvatarInstantiated);
            _favoriteAvatarList.OnEnable += () =>
            {
                // make sure it stays off if it should be off.
                _favoriteAvatarList.GameObject.SetActive(AvatarFavoritesEnabled);
            };

            var parent = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Favorite Button").transform.parent;
            _favoriteButton = new ReUiButton("Favorite", new Vector2(-600f, 375f), new Vector2(0.5f, 1f),
                () => FavoriteAvatar(_favoriteAvatarList.AvatarPedestal.field_Internal_ApiAvatar_0),
                parent);
            _favoriteButton.GameObject.SetActive(AvatarFavoritesEnabled);

            var changeButton = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Change Button");
            if (changeButton != null)
            {
                var button = changeButton.GetComponent<Button>();
                _changeButtonEvent = button.onClick;

                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(new Action(ChangeAvatarChecked));
            }


            _searchAvatarsAction = DelegateSupport.ConvertDelegate<UnityAction<string>>(
                (Action<string>)SearchAvatars);
            _overrideSearchAvatarsAction = DelegateSupport.ConvertDelegate<UnityAction<string>>(
                (Action<string>)PromptChooseSearch);

            _avatarScreen = GameObject.Find("UserInterface/MenuContent/Screens/Avatar");
            _searchBox = GameObject.Find("UserInterface/MenuContent/Backdrop/Header/Tabs/ViewPort/Content/Search/InputField").GetComponent<UiInputField>();
            MelonCoroutines.Start(LoginToAPICoroutine());
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            if (ReModCE_ARES.IsRubyLoaded)
            {
                _favoriteButton.Position += new Vector3(420f, 0f);
            }

            var menu = uiManager.MainMenu.GetMenuPage("Avatars");
            _enabledToggle = menu.AddToggle("Avatar Favorites", "Enable/Disable avatar favorites", AvatarFavoritesEnabled);
            _maxAvatarsPerPageButton = menu.AddButton($"Avatars Per Page: {MaxAvatarsPerPage}",
                "Set the maximum amount of avatars shown per page",
                () =>
                {
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Max Avatars Per Page",
                        MaxAvatarsPerPage.ToString(), InputField.InputType.Standard, true, "Submit",
                        (s, k, t) =>
                        {
                            if (string.IsNullOrEmpty(s))
                                return;

                            if (!int.TryParse(s, out var maxAvatarsPerPage))
                                return;

                            MaxAvatarsPerPage.SetValue(maxAvatarsPerPage);
                            _maxAvatarsPerPageButton.Text = $"Max Avatars Per Page: {MaxAvatarsPerPage}";
                        }, null);
                }, ResourceManager.GetSprite("remodce.max"));
            if (_pinCode == 0)
            {
                _enterPinButton = menu.AddButton("Set/Enter Pin", "Set or enter your pin for the ARES API", () =>
                {
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Enter pin",
                        "", InputField.InputType.Standard, true, "Submit",
                        (s, k, t) =>
                        {
                            if (string.IsNullOrEmpty(s))
                                return;

                            if (!int.TryParse(s, out var pinCode))
                                return;

                            _pinCode = pinCode;
                            File.WriteAllText(PinPath, _pinCode.ToString());

                            InitializeNetworkClient();

                            LoginToAPI(APIUser.CurrentUser, FetchAvatars);
                        }, null);
                }, ResourceManager.GetSprite("remodce.padlock"));
            }
        }

        private void LoginToAPI(APIUser user, Action onLogin)
        {
            if (!EnableApi)
            {
                return;
            }
            this.user = user;
            onLogin();
        }

        public override void OnUpdate()
        {
            if (_searchBox == null)
                return;

            if (!_avatarScreen.active)
            {
                return;
            }

            if (!_searchBox.field_Public_Button_0.interactable)
            {
                if (!ReModCE_ARES.IsEmmVRCLoaded || _updatesWithoutSearch >= 10)
                {
                    _searchBox.field_Public_Button_0.interactable = true;
                    _searchBox.field_Public_UnityAction_1_String_0 = _searchAvatarsAction;
                }
                else if (ReModCE_ARES.IsEmmVRCLoaded)
                {
                    ++_updatesWithoutSearch;
                }
                // emmVRC will set it to be interactable. We want to grab their search function
            }
            else
            {
                if (ReModCE_ARES.IsEmmVRCLoaded && _updatesWithoutSearch < 10)
                {
                    if (_searchBox.field_Public_UnityAction_1_String_0 == null)
                        return;

                    if (_searchBox.field_Public_UnityAction_1_String_0.method != _overrideSearchAvatarsAction.method)
                    {
                        if (_emmVRCsearchAvatarsAction == null)
                        {
                            _emmVRCsearchAvatarsAction = _searchBox.field_Public_UnityAction_1_String_0;
                        }
                        _searchBox.field_Public_UnityAction_1_String_0 = _overrideSearchAvatarsAction;
                    }
                }
            }
        }

        private void PromptChooseSearch(string searchTerm)
        {
            MelonCoroutines.Start(PrompSearchDelayed(searchTerm));
        }

        private IEnumerator PrompSearchDelayed(string searchTerm)
        {
            yield return new WaitForSeconds(1f);
            VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowStandardPopupV2("Choose Search",
                "Choose whether you want to search with ARES or emmVRC", "ARES",
                () =>
                {
                    SearchAvatars(searchTerm);
                    VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP");
                }, "emmVRC", () =>
                {
                    _emmVRCsearchAvatarsAction?.Invoke(searchTerm);
                    VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP");
                }, null);
        }

        private void SearchAvatars(string searchTerm)
        {
            var popupManager = VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0;
            if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length < 3)
            {
                popupManager.ShowStandardPopupV2("ARES Search", "That search term is too short. The search term has to be at least 3 characters.", "I'm sorry!",
                    () =>
                    {
                        popupManager.HideCurrentPopup();
                    });
                return;
            }


            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/records/Avatars?include=AvatarID,AvatarName,AvatarDescription,AuthorID,AuthorName,PCAssetURL,ImageURL,ThumbnailURL&size=10000&filter=AvatarName,cs,{searchTerm}&filter=Releasestatus,cs,Public");

            _httpClient.SendAsync(request).ContinueWith(rsp =>
            {
                var searchResponse = rsp.Result;
                if (!searchResponse.IsSuccessStatusCode)
                {
                    searchResponse.Content.ReadAsStringAsync().ContinueWith(errorData =>
                    {
                        var errorMessage = JsonConvert.DeserializeObject<ApiError>(errorData.Result).Error;

                        ReLogger.Error($"Could not search for avatars: \"{errorMessage}\"");
                        ReModCE_ARES.LogDebug($"Could not search for avatars: \"{errorMessage}\"");
                        if (searchResponse.StatusCode == HttpStatusCode.Forbidden)
                        {
                            MelonCoroutines.Start(ShowAlertDelayed($"Could not search for avatars\nReason: \"{errorMessage}\""));
                        }
                    });
                }
                else
                {
                    searchResponse.Content.ReadAsStringAsync().ContinueWith(t =>
                    {
                        var avatars = JsonConvert.DeserializeObject<AvatarGet>(t.Result) ?? new AvatarGet { records = new List<ReAvatar>() };
                        if (avatars.records.Count > 0)
                        {
                            MelonCoroutines.Start(RefreshSearchedAvatars(avatars.records));
                        }
                        else
                        {
                            MelonCoroutines.Start(ShowAlertDelayed($"No avatars found with the name: \"{searchTerm}\""));
                        }
                    });
                }
            });
        }



        private IEnumerator RefreshSearchedAvatars(List<ReAvatar> results)
        {
            yield return new WaitForEndOfFrame();

            _searchedAvatars.Clear();
            foreach (var avi in results.Select(x => x.AsApiAvatar()))
            {
                _searchedAvatars.Add(avi);
            }

            ReModCE_ARES.LogDebug($"Found {_searchedAvatars.Count} avatars");
            ReLogger.Msg($"Found {_searchedAvatars.Count} avatars");
            _searchedAvatarList.RefreshAvatars();
        }

        private void ChangeAvatarChecked()
        {
            if (!AvatarFavoritesEnabled)
            {
                _changeButtonEvent.Invoke();
                return;
            }

            var currentAvatar = _favoriteAvatarList.AvatarPedestal.field_Internal_ApiAvatar_0;
            if (!HasAvatarFavorited(currentAvatar.id)) // this isn't in our list. we don't care about it
            {
                _changeButtonEvent.Invoke();
                return;
            }

            new ApiAvatar { id = currentAvatar.id }.Fetch(new Action<ApiContainer>(ac =>
            {
                var updatedAvatar = ac.Model.Cast<ApiAvatar>();
                switch (updatedAvatar.releaseStatus)
                {
                    case "private" when updatedAvatar.authorId != APIUser.CurrentUser.id:
                        VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ARES", "This avatar is private and you don't own it. You can't switch into it.");
                        break;
                    case "unavailable":
                        VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ARES", "This avatar has been deleted. You can't switch into it.");
                        break;
                    default:
                        _changeButtonEvent.Invoke();
                        break;
                }
            }), new Action<ApiContainer>(ac =>
            {
                VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ARES", "This avatar has been deleted. You can't switch into it.");
            }));
        }

        private void FetchAvatars()
        {

            while (APIUser.CurrentUser == null) { System.Threading.Thread.Sleep(10); };
            user = APIUser.CurrentUser;

            string userId = user.id;
            string pin = _pinCode.ToString();
            string url = $"{ApiUrl}/AvatarFav.php?UserId=" + HttpUtility.UrlEncode(userId) + "&Pin=" + HttpUtility.UrlEncode(pin);
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            _httpClient.SendAsync(request).ContinueWith(rsp =>
            {
                var searchResponse = rsp.Result;
                if (!searchResponse.IsSuccessStatusCode)
                {
                    searchResponse.Content.ReadAsStringAsync().ContinueWith(errorData =>
                    {
                        var errorMessage = JsonConvert.DeserializeObject<ApiError>(errorData.Result).Error;
                        ReModCE_ARES.LogDebug($"Could not search for avatars: \"{errorMessage}\"");
                        ReLogger.Error($"Could not search for avatars: \"{errorMessage}\"");
                        if (searchResponse.StatusCode == HttpStatusCode.Forbidden)
                        {
                            MelonCoroutines.Start(ShowAlertDelayed($"Could not search for avatars\nReason: \"{errorMessage}\""));
                        }
                    });
                }
                else
                {
                    searchResponse.Content.ReadAsStringAsync().ContinueWith(t =>
                    {
                        var avatars = JsonConvert.DeserializeObject<List<ReAvatar>>(t.Result) ?? new List<ReAvatar>();
                        _savedAvatars = avatars;
                    });
                }
            });
        }


        private static IEnumerator ShowAlertDelayed(string message, float seconds = 0.5f)
        {
            if (VRCUiPopupManager.prop_VRCUiPopupManager_0 == null) yield break;

            yield return new WaitForSeconds(seconds);

            VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowAlert("ReMod CE", message);
        }

        private void OnAvatarInstantiated(string url, GameObject avatar, AvatarPerformanceStats avatarPerformanceStats)
        {
            _favoriteButton.Text = HasAvatarFavorited(_favoriteAvatarList.AvatarPedestal.field_Internal_ApiAvatar_0.id) ? "Unfavorite" : "Favorite";
        }

        private void FavoriteAvatar(ApiAvatar apiAvatar)
        {

            var hasFavorited = HasAvatarFavorited(apiAvatar.id);
            if (hasFavorited)
            {
                string userId = user.id;
                string pin = _pinCode.ToString();
                string url = $"{ApiUrl}/AvatarFavRemove.php?UserId=" + HttpUtility.UrlEncode(userId) + "&Pin=" + HttpUtility.UrlEncode(pin) + "&AvatarID=" + HttpUtility.UrlEncode(apiAvatar.id);
                _httpClient.GetStringAsync(url);
            }
            if (!hasFavorited)
            {
                ReAvatar reAvatar = new ReAvatar(apiAvatar);
                reAvatar.AuthorName.replaceThis("Æ");
                reAvatar.AvatarDescription.replaceThis("Æ");
                reAvatar.AvatarName.replaceThis("Æ");
                reAvatar.Pin = _pinCode.ToString();
                reAvatar.UserId = APIUser.CurrentUser.id;

                var httpWebRequest = (HttpWebRequest)WebRequest.Create($"{ApiUrl}/records/AvatarsFav");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                string jsonPost = JsonConvert.SerializeObject(reAvatar);
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonPost);
                }
                try
                {
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    ReModCE_ARES.LogDebug(ex.Message);
                    ReLogger.Error(ex.Message);
                }
            }

            if (_favoriteAvatarList.AvatarPedestal.field_Internal_ApiAvatar_0.id == apiAvatar.id)
            {
                if (!HasAvatarFavorited(apiAvatar.id))
                {
                    _savedAvatars.Insert(0, new ReAvatar(apiAvatar));
                    _favoriteButton.Text = "Unfavorite";
                }
                else
                {
                    _savedAvatars.RemoveAll(a => a.AvatarID == apiAvatar.id);
                    _favoriteButton.Text = "Favorite";
                }
            }

            _favoriteAvatarList.RefreshAvatars();
        }

        private IEnumerator LoginToAPICoroutine()
        {
            while (APIUser.CurrentUser == null) yield return new WaitForEndOfFrame();

            var user = APIUser.CurrentUser;
            LoginToAPI(user, FetchAvatars);
        }

        private bool HasAvatarFavorited(string id)
        {
            return _savedAvatars.FirstOrDefault(a => a.AvatarID == id) != null;
        }

        public AvatarList GetAvatars(ReAvatarList avatarList)
        {
            if (avatarList == _favoriteAvatarList)
            {
                var list = new AvatarList();
                foreach (var avi in _savedAvatars.Select(x => x.AsApiAvatar()).ToList())
                {
                    list.Add(avi);
                }

                return list;
            }
            else if (avatarList == _searchedAvatarList)
            {
                return _searchedAvatars;
            }

            return null;
        }

        public void Clear(ReAvatarList avatarList)
        {
            if (avatarList == _searchedAvatarList)
            {
                _searchedAvatars.Clear();
                avatarList.RefreshAvatars();
            }
        }
    }
}
