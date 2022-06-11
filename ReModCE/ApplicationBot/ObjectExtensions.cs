using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ReModCE_ARES.ApplicationBot
{
    class ObjectExtensions
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
