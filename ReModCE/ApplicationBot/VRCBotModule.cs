﻿using ExitGames.Client.Photon;

namespace Serpent
{
    public class VRCModule
    {
        public VRCModule()
        {
            Serpent.Modules.Add(this);
        }
        public virtual void OnStart()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual bool OnEventPatch(ref EventData __0)
        {
            return true;
        }

        public virtual void VRChat_OnUiManagerInit()
        {

        }

        public virtual void OnLevelWasLoaded(int buildIndex, string sceneName)
        {

        }
    }
}
