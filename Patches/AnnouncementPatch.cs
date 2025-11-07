using HarmonyLib;
using PlayerRoles;

namespace SCP008X.Patches
{
    [HarmonyPatch]
    public static class AnnouncementPatch
    {
        private static RoleTypeId Scp008 => (RoleTypeId)64;

        [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.ConvertSCP),
            typeof(RoleTypeId), typeof(string), typeof(string))]
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