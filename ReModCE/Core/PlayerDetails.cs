using VRC;
using VRC.Core;
using VRC.SDKBase;

namespace Serpent.Core
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
