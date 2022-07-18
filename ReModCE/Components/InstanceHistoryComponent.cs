﻿using Newtonsoft.Json;
using SerpentCore.Core;
using SerpentCore.Core.Managers;
using SerpentCore.Core.UI;
using SerpentCore.Core.UI.QuickMenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;

namespace Serpent.Components
{
    internal sealed class InstanceHistoryComponent : ModComponent
    {
        internal class SavedWorld
        {
            public string Name { get; set; }
            public string JoinId { get; set; }
            public string Type { get; set; }
            public string Region { get; set; }
            public DateTime JoinDate { get; set; }
        }

        private readonly List<SavedWorld> _instanceHistory = new();

        private ReCategoryPage _instanceMenu;
        private ReMenuCategory _instanceHistoryMenu;
        private ReMenuCategory _instanceSettingsMenu;

        private ConfigValue<bool> InstanceHistoryDescendingEnabled;
        private ReMenuToggle _instanceSettingsDescendingToggle;

        private SavedWorld _currentSavedWorld;

        public InstanceHistoryComponent()
        {

            if (File.Exists("UserData/Serpent/instance_history.json"))
            {
                _instanceHistory = JsonConvert.DeserializeObject<List<SavedWorld>>(File.ReadAllText("UserData/Serpent/instance_history.json"));
            }

            _instanceHistory ??= new List<SavedWorld>();

            var history = _instanceHistory.ToList();
            foreach (var instance in history.Where(instance => (DateTime.UtcNow - instance.JoinDate).TotalHours > 8))
            {
                _instanceHistory.Remove(instance);
            }

            InstanceHistoryDescendingEnabled = new ConfigValue<bool>(nameof(InstanceHistoryDescendingEnabled), false);
            InstanceHistoryDescendingEnabled.OnValueChanged += () =>
            {
                _instanceSettingsDescendingToggle?.Toggle(InstanceHistoryDescendingEnabled);
                ReverseButtonOrder();
            };
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            _instanceMenu = uiManager.MainMenu.AddCategoryPage("Instance History", sprite: ResourceManager.GetSprite("remodce.history"));
            _instanceSettingsMenu = _instanceMenu.AddCategory("Settings");
            _instanceHistoryMenu = _instanceMenu.AddCategory("History");


            foreach (var world in _instanceHistory.ToArray())
            {
                if (string.IsNullOrEmpty(world.Region) || string.IsNullOrEmpty(world.Type))
                {
                    _instanceHistory.Remove(world);
                    return;
                }
                AddInstanceButton(world);
            }

            _instanceSettingsMenu.AddButton("Clear History", "Clear Instance History",
                ClearInstanceHistory, ResourceManager.GetSprite("remodce.dust"));

            _instanceSettingsDescendingToggle = _instanceSettingsMenu.AddToggle("Reverse Order",
                "Reverse the order of the instances.", InstanceHistoryDescendingEnabled);
        }

        //TODO: On restart (rejoining last world), queue the button move rather than execute immediately.
        public override void OnEnterWorld(ApiWorld world, ApiWorldInstance instance)
        {
            try
            {
                _currentSavedWorld = new SavedWorld
                {
                    Name = $"{world.name}#{instance.instanceId.Split('~')[0]}",
                    JoinId = $"{world.id}:{instance.instanceId}",
                    Type = $"{InstanceAccessTypeExtensions.ToDisplayString(instance.type)}",
                    Region = $"{NetworkRegionExtensions.ToShortString(instance.region)}",
                    JoinDate = DateTime.UtcNow
                };

                int index = _instanceHistory.FindIndex(sw => sw.JoinId == _currentSavedWorld.JoinId);
                if (index > -1)
                {
                    MoveButton(index);
                    return;
                }

                _instanceHistory.Add(_currentSavedWorld);
                File.WriteAllText("UserData/Serpent/instance_history.json", JsonConvert.SerializeObject(_instanceHistory));

                if (_instanceHistoryMenu != null)
                    AddInstanceButton(_currentSavedWorld);
            }
            catch
            {

            }
        }

        private void MoveButton(int index)
        {
            string searchTerm = "Button_" + UiElement.GetCleanName($"Join {_instanceHistory[index].Name} ({_instanceHistory[index].Region})");
            var child = _instanceHistoryMenu.RectTransform.Find(searchTerm);
            if (!child) return;
            SetAsCorrectSibling(child);
        }

        private void ClearInstanceHistory()
        {
            while (_instanceHistoryMenu.RectTransform.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(_instanceHistoryMenu.RectTransform.GetChild(0).gameObject);
            }

            _instanceHistory.Clear();

            _instanceHistory.Add(_currentSavedWorld);
            AddInstanceButton(_currentSavedWorld);

            File.WriteAllText("UserData/Serpent/instance_history.json", JsonConvert.SerializeObject(_instanceHistory));
        }

        private void ReverseButtonOrder()
        {
            for (var i = 0; i < _instanceHistoryMenu.RectTransform.childCount - 1; i++)
            {
                _instanceHistoryMenu.RectTransform.GetChild(0).SetSiblingIndex(_instanceHistoryMenu.RectTransform.childCount - 1 - i);
            }
        }

        private void SetAsCorrectSibling(Transform child)
        {
            if (InstanceHistoryDescendingEnabled)
            {
                child.SetAsLastSibling();
            }
            else
            {
                child.SetAsFirstSibling();
            }
        }

        private void AddInstanceButton(SavedWorld savedWorld)
        {
            var buttonRectTransform = _instanceHistoryMenu.AddButton(
                $"Join {savedWorld.Name} ({savedWorld.Region})",
                $"Join {savedWorld.Name} ({savedWorld.Region}) [{savedWorld.Type}]",
                () => Networking.GoToRoom(savedWorld.JoinId)).RectTransform;

            SetAsCorrectSibling(buttonRectTransform);
        }
    }
}
