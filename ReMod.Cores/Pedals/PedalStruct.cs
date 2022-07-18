using System;
using SerpentCore.Core.Types;
using UnityEngine;

namespace SerpentCore.Core.Pedals
{
    public abstract class PedalStruct
    {
        public string text { get; set; }
        public Texture2D icon { get; set; }
        public Action<ActionMenu> triggerEvent { get; protected set; }
        public PedalType Type { get; protected set; }
        public bool locked { get; set; }
        public bool shouldAdd { get; set; } = true;
    }
}