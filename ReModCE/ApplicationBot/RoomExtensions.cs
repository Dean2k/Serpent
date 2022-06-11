using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRC.Core;

namespace ReModCE_ARES.ApplicationBot
{
    static class RoomExtensions
    {
        public static ApiWorld Current_World { get { return RoomManager.field_Internal_Static_ApiWorld_0; } }
        public static string Current_World_ID { get { return $"{RoomManager.field_Internal_Static_ApiWorld_0.id}:{RoomManager.field_Internal_Static_ApiWorldInstance_0.instanceId}"; } }
        public static bool In_World { get { return Current_World != null; } }
    }
}
