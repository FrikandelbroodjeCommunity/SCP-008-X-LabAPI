using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using Discord;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using PlayerRoles;
using SCP008X.Components;

namespace SCP008X
{
    public static class Scp008Handler
    {
        public static int Victims;

        private static Config Config => Scp008X.Singleton.Config;
        private static readonly Random Gen = new();

        public static void ClearScp008(Player player)
        {
            if (player.ReferenceHub.TryGetComponent(out Scp008 scp008))
                UnityEngine.Object.Destroy(scp008);
        }

        public static void Infect(Player target)
        {
            if (target.ReferenceHub.gameObject.TryGetComponent(out Scp008 scp008))
            {
                return;
            }

            target.ReferenceHub.gameObject.AddComponent<Scp008>();
            target.SendHint($"<color=yellow><b>SCP-008</b></color>\n{Config.InfectionAlert}", 10f);
        }

        public static void Turn(Player target)
        {
            Victims++;
            if (!target.ReferenceHub.TryGetComponent(out Scp008 _))
            {
                target.ReferenceHub.gameObject.AddComponent<Scp008>();
            }

            if (target.HasEffect<Scp207>())
            {
                target.DisableEffect<Scp207>();
            }

            if (target.CurrentItem?.Category == ItemCategory.Firearm)
            {
                target.DropEverything();
            }

            if (string.IsNullOrEmpty(Config.SuicideBroadcast))
            {
                target.SendBroadcast(Config.SuicideBroadcast, 10);
            }

            if (!Config.RetainInventory)
            {
                target.ClearInventory();
            }

            if (Config.Scp008Buff >= 0)
            {
                target.ArtificialHealth += Config.Scp008Buff;
            }

            target.Health = Config.ZombieHealth;
            target.SendHint($"<color=yellow><b>SCP-008</b></color>\n{Config.SpawnHint}", 20f);
            if (Config.AoeTurned)
            {
                AoeInfection(target);
            }
        }

        public static bool Scp008Check()
        {
            var check = 0;
            foreach (var ply in Player.List)
            {
                if (ply.ReferenceHub.gameObject.TryGetComponent(out Scp008 _))
                {
                    check++;
                }

                if (ply.Role == RoleTypeId.Scp049)
                {
                    check++;
                }
            }

            return check == 0;
        }

        public static void AoeInfection(Player player)
        {
            // For all other players in the same room
            foreach (var ply in Player.List.Where(x =>
                         x != player && x.CachedRoom == player.CachedRoom && x.Faction != Faction.SCP))
            {
                if (Gen.Next(1, 100) <= Config.AoeChance)
                {
                    Infect(ply);
                    Logger.Debug($"Called Infect() method for {ply.LogName} due to AOE.", Config.DebugMode);
                }
            }
        }
    }
}