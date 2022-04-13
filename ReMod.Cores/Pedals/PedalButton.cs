﻿using System;
using ReMod.Core.Types;
using UnityEngine;

namespace ReMod.Core.Pedals
{
    public sealed class PedalButton : PedalStruct
    {
        public PedalButton(string text, Texture2D icon, Action triggerEvent, bool locked = false)
        {
            this.text = text;
            this.icon = icon;
            this.triggerEvent = delegate { triggerEvent(); };
            Type = PedalType.Button;
            this.locked = locked;
        }
    }
}