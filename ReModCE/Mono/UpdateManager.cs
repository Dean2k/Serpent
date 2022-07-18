using Serpent.Loader;
using System;
using UnityEngine;

namespace Serpent.Mono
{
    internal class UpdateManager : MonoBehaviour
    {
        public UpdateManager(IntPtr ptr) : base(ptr)
        {

        }

        void Start()
        {
            ReLogger.Msg("Starting Update manager");
        }

        void LateUpdate()
        {
            try { if (VRC.Player.prop_Player_0.gameObject == null) return; } catch { return; }

            try
            {
                if (Serpent._Queue.Count != 0)
                {
                    for (int i = 0; i < Serpent._Queue.Count; i++)
                    {
                        Serpent._Queue.ToArray()[i].Invoke();
                        Serpent._Queue.Dequeue();
                    }
                }
            }
            catch { }
        }
    }
}
