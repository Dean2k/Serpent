using MelonLoader;
using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI.QuickMenu;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Serpent.Components
{
    internal class CustomLoadingMusicComponent : ModComponent
    {
        private ConfigValue<bool> CustomMusicEnabled;
        private ReMenuToggle _customMusicEnabledToggle;

        public CustomLoadingMusicComponent()
        {
            CustomMusicEnabled = new ConfigValue<bool>(nameof(CustomMusicEnabled), true);
            CustomMusicEnabled.OnValueChanged += () => _customMusicEnabledToggle.Toggle(CustomMusicEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage(Page.PageNames.Theme);
            _customMusicEnabledToggle = menu.AddToggle("Custom Loading Music",
                "Enable The custom loading music.", CustomMusicEnabled.SetValue,
                CustomMusicEnabled);
            MelonCoroutines.Start(WebRequest());
        }

        private AudioClip audiofile;
        private AudioSource audiosource;
        private AudioSource audiosource2;


        IEnumerator WebRequest()
        {
            // I used some code of Knah´s join notifier for the unitywebrequest.
            if (CustomMusicEnabled)
            {
                MelonLogger.Msg("Loading custom loading screen music");
                var uwr = UnityWebRequest.Get($"file://{Path.Combine(Environment.CurrentDirectory, "LoadingScreenMusic/Music.ogg")}");
                uwr.SendWebRequest();

                while (!uwr.isDone) yield return null;

                audiofile = WebRequestWWW.InternalCreateAudioClipUsingDH(uwr.downloadHandler, uwr.url, false, false, AudioType.UNKNOWN);
                audiofile.hideFlags |= HideFlags.DontUnloadUnusedAsset;

                while (audiosource == null)
                {
                    audiosource = GameObject.Find("LoadingBackground_TealGradient_Music/LoadingSound")?.GetComponent<AudioSource>();

                    yield return null;
                }
                audiosource.clip = audiofile;
                audiosource.Stop();
                audiosource.Play();

                while (audiosource2 == null)
                {
                    audiosource2 = GameObject.Find("UserInterface/MenuContent/Popups/LoadingPopup/LoadingSound")?.GetComponent<AudioSource>();

                    yield return null;
                }
                audiosource2.clip = audiofile;
                audiosource2.Stop();
                audiosource2.Play();
            }

        }

    }
}
