using System;
using System.Collections.Generic;
using UnityEngine;
namespace Serpent.Mono
{
    internal class TagRainbow : MonoBehaviour
    {
        public string _Text { get; set; }
        public string _CurentText { get; set; }
        public int _TextPoz { get; set; }
        public bool _Deacreasing { get; set; }
        public List<string> colors = new List<string>();
        public int numColors = 255;


        public TMPro.TextMeshProUGUI _TextMeshPro;

        public TagRainbow(IntPtr ptr) : base(ptr)
        {

        }

        void Start()
        {
            var random = new System.Random();
            for (int i = 0; i < numColors; i++)
            {
                colors.Add(String.Format("#{0:X6}", random.Next(0x1000000)));
            }

            _TextMeshPro = this.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            InvokeRepeating(nameof(updatehud), -1, 0.5f);
        }

        void updatehud()
        {
            _CurentText = "";
            var random = new System.Random();
            foreach (char c in _Text)
            {
                _CurentText += String.Format("<color={0}>{1}</color>", colors[random.Next(numColors)], c.ToString());
            }

            _TextMeshPro.text = _CurentText;
        }
    }
}
