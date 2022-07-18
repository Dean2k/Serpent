﻿using System;
using UnityEngine;

namespace Serpent.Mono
{
    internal class TagAnimation : MonoBehaviour
    {
        public string _Color { get; set; }
        public string _Text { get; set; }
        public string _CurentText { get; set; }
        public int _TextPoz { get; set; }
        public bool _Deacreasing { get; set; }

        public TMPro.TextMeshProUGUI _TextMeshPro;

        public TagAnimation(IntPtr ptr) : base(ptr)
        {
        }

        private void Start()
        {
            _Deacreasing = false;
            _TextPoz = 0;
            _Color = "";
            if (_Text.StartsWith("<color="))
            {
                _Color = _Text.Substring(0, 15) + " ";
                _Text = _Text.Substring(15).Trim();
            }
            _TextMeshPro = this.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            InvokeRepeating(nameof(updatehud), -1, 0.5f);
        }

        private void updatehud()
        {
            if (_TextPoz >= _Text.Length)
                _Deacreasing = true;
            else if (!_Deacreasing)
            {
                _CurentText = _CurentText + _Text[_TextPoz];
                _TextPoz += 1;
            }
            if (_Deacreasing)
            {
                _CurentText = _CurentText.Substring(0, _CurentText.Length - 1);
                _TextPoz -= 1;

                if (_TextPoz == 0)
                    _Deacreasing = false;
            }

            if (_CurentText.Length == 0)
            {
                _TextMeshPro.text = "";
                return;
            }

            _TextMeshPro.text = _Color + _CurentText;
        }
    }
}