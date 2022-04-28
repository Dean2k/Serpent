using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase;

namespace ReModCE_ARES.Core
{
    public class PlayerDetails
    {
        internal string id;
        internal string displayName;
        internal bool isLocalPlayer;
        internal bool isInstanceMaster;
        internal bool isVRUser;
        internal bool isQuestUser;

        internal bool blockedLocalPlayer;

        internal Player player;
        internal VRCPlayerApi playerApi;
        internal VRCPlayer vrcPlayer;
        internal APIUser apiUser;
        internal VRCNetworkBehaviour networkBehaviour;
    };
}
