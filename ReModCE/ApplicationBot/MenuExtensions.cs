using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRC;
using VRC.Core;

namespace ReModCE_ARES.ApplicationBot
{
    static class MenuExtensions
    {
        public static QuickMenu GetQuickMenu() =>
            QuickMenu.prop_QuickMenu_0;

        public static APIUser GetSelectedAPIUser() =>
            GetQuickMenu().prop_APIUser_0;

        public static Player GetSelectedPlayer() =>
            PlayerExtensions.GetPlayerByUserID(GetSelectedAPIUser().id);
        
    }
}
