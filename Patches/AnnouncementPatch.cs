using System.Linq;
using System.Reflection;
using Cassie;
using HarmonyLib;
using PlayerRoles;

namespace SCP008X.Patches
{
    [HarmonyPatch]
    public static class AnnouncementPatch
    {
        private static RoleTypeId Scp008 => (RoleTypeId)64;

        [HarmonyTargetMethod]
        public static MethodBase GetMethodBase()
        {
            return typeof(CassieScpTerminationAnnouncement)
                .GetMethods()
                .First(x => x.Name == nameof(CassieScpTerminationAnnouncement.ConvertSCP) &&
                            x.GetParameters().Any(y => y.ParameterType == typeof(RoleTypeId)));
        }

        [HarmonyPrefix]
        public static bool OverrideResult(RoleTypeId role, ref string withoutSpace, ref string withSpace)
        {
            if (Scp008 != role) return true;

            withoutSpace = "008";
            withSpace = "0 0 8";
            return false;
        }
    }
}