using System;
using ReModAres.Core.Helpers;
using ReModAres.Core.Managers;
using ReModAres.Core.Types;
using UnityEngine;

namespace ReModAres.Core.Pedals
{
    public sealed class PedalFourAxis : PedalStruct
    {
        public PedalFourAxis(string text, Texture2D icon, Action<Vector2> onUpdate, string topButtonText,
            string rightButtonText, string downButtonText, string leftButtonText, bool locked = false)
        {
            this.text = text;
            this.icon = icon;
            triggerEvent = delegate
            {
                FourAxisPuppetManager.OpenFourAxisMenu(text, onUpdate, pedal);
                FourAxisPuppetManager.Current.GetButtonUp().SetButtonText(topButtonText);
                FourAxisPuppetManager.Current.GetButtonRight().SetButtonText(rightButtonText);
                FourAxisPuppetManager.Current.GetButtonDown().SetButtonText(downButtonText);
                FourAxisPuppetManager.Current.GetButtonLeft().SetButtonText(leftButtonText);
            };
            Type = PedalType.FourAxisPuppet;
            this.locked = locked;
        }

        public PedalOption pedal { get; set; }
    }
}