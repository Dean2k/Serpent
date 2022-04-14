using UnityEngine;

namespace ReModAres.Core.Unity
{
    public static class ColorExtensions
    {
        public static string ToHex(this Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }
    }
}
