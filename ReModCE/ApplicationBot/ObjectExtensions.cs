using UnityEngine;

namespace Serpent.ApplicationBot
{
    internal class ObjectExtensions
    {
        private static GameObject CachedPlayerCamera;

        public static GameObject GetPlayerCamera
        {
            get
            {
                if (CachedPlayerCamera == null)
                    CachedPlayerCamera = GameObject.Find("Camera (eye)");
                return CachedPlayerCamera;
            }
        }
    }
}