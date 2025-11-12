using SCP008X.Components;
using System.Collections.Generic;
using System.Linq;
using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;

namespace SCP008X
{
    public static class EventHandlers
    {
        private static Config Config => Scp008X.Singleton.Config;
        private static readonly Random Gen = new();

        public static void RegisterEvents()
        {
            ServerEvents.RoundStarted += OnRoundStart;
            ServerEvents.RoundEnded += OnRoundEnd;

            PlayerEvents.Left += OnPlayerLeave;
            PlayerEvents.Hurting += OnPlayerHurting;
            PlayerEvents.Hurt += OnPlayerHurt;
            PlayerEvents.UsedItem += OnHealed;
            PlayerEvents.ChangedRole += OnRoleChange;
            PlayerEvents.Death += OnPlayerDied;

            Scp049Events.ResurrectingBody += OnReviving;
            Scp049Events.ResurrectedBody += OnRevived;
        }

        public static void UnregisterEvents()
        {
            ServerEvents.RoundStarted -= OnRoundStart;
            ServerEvents.RoundEnded -= OnRoundEnd;

            PlayerEvents.Left -= OnPlayerLeave;
            PlayerEvents.Hurting -= OnPlayerHurting;
            PlayerEvents.Hurt -= OnPlayerHurt;
            PlayerEvents.UsedItem -= OnHealed;
            PlayerEvents.ChangedRole -= OnRoleChange;
            PlayerEvents.Death -= OnPlayerDied;

            Scp049Events.ResurrectingBody -= OnReviving;
            Scp049Events.ResurrectedBody -= OnRevived;
        }

        private static void OnRoundStart()
        {
            if (Config.CassieAnnounce && !string.IsNullOrEmpty(Config.Announcement))
            {
                Timing.CallDelayed(5f, () =>
                {
                    if (string.IsNullOrEmpty(Config.AnnouncementSubtitles))
                    {
                        Cassie.Message(Config.Announcement);
                    }
                    else
                    {
                        Cassie.Message(Config.Announcement, customSubtitles: Config.AnnouncementSubtitles);
                    }
                });
            }
        }

        private static void OnRoundEnd(RoundEndedEventArgs ev)
        {
            if (Config.SummaryStats)
            {
                var summary =
                    "\n\n\n\n\n\n\n\n\n\n\n\n\n\n<align=left><color=yellow><b>SCP-008 Victims:</b></color> " +
                    $"{Scp008Handler.Victims}/{RoundSummary.ChangedIntoZombies}";

                foreach (var player in Player.List)
                {
                    player.SendHint(summary, 30f);
                }
            }
        }

        private static void OnPlayerLeave(PlayerLeftEventArgs ev)
        {
            if (ev.Player.Role == RoleTypeId.Scp0492 && ev.Player.ReferenceHub.TryGetComponent(out Scp008 s008))
            {
                Scp008Handler.ClearScp008(ev.Player);
            }
        }

        private static void OnPlayerHurting(PlayerHurtingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Attacker.Role != RoleTypeId.Scp0492 || ev.Player == ev.Attacker) return;

            if (ev.DamageHandler is not UniversalDamageHandler handler) return;

            if (Config.ZombieDamage >= 0)
            {
                handler.Damage = Config.ZombieDamage;
                Logger.Debug($"Damage overriden to be {handler.Damage}.", Config.DebugMode);
            }

            if (Config.Scp008Buff >= 0)
            {
                ev.Attacker.ArtificialHealth += Config.Scp008Buff;
                Logger.Debug($"Added {Config.Scp008Buff} AHP to {ev.Attacker.LogName}.", Config.DebugMode);
            }

            if (Gen.Next(1, 100) > Config.InfectionChance || ev.Player.Faction == Faction.SCP) return;
            try
            {
                Scp008Handler.Infect(ev.Player);
                Logger.Debug($"Successfully infected {ev.Player.LogName}.", Config.DebugMode);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to infect {ev.Player.LogName}! {e}");
                throw;
            }
        }

        private static void OnPlayerHurt(PlayerHurtEventArgs ev)
        {
            foreach (var instance in Scp008.Instances)
            {
                instance.WhenHurt(ev);
            }
        }

        private static void OnHealed(PlayerUsedItemEventArgs ev)
        {
            if (ev.UsableItem.Category != ItemCategory.Medical) return;

            if (ev.Player.ReferenceHub.TryGetComponent(out Scp008 scp008))
            {
                switch (ev.UsableItem.Type)
                {
                    case ItemType.SCP500:
                        UnityEngine.Object.Destroy(scp008);
                        Logger.Debug($"{ev.Player.LogName} successfully cured themselves.", Config.DebugMode);
                        break;
                    case ItemType.Medkit:
                        if (Gen.Next(1, 100) <= Config.CureChance)
                        {
                            UnityEngine.Object.Destroy(scp008);
                            Logger.Debug($"{ev.Player.LogName} cured themselves with a medkit.", Config.DebugMode);
                        }

                        break;
                }
            }
        }

        private static void OnRoleChange(PlayerChangedRoleEventArgs ev)
        {
            foreach (var instance in Scp008.Instances)
            {
                instance.WhenRoleChange(ev);
            }

            if (ev.NewRole.RoleTypeId == RoleTypeId.Scp0492)
            {
                Logger.Debug($"Calling Turn() method for {ev.Player.LogName}.", Config.DebugMode);
                Scp008Handler.Turn(ev.Player);
            }

            if (ev.NewRole.RoleTypeId != RoleTypeId.Scp0492 || ev.NewRole.RoleTypeId != RoleTypeId.Scp096)
            {
                Scp008Handler.ClearScp008(ev.Player);
                ev.Player.ArtificialHealth = 0; // TODO: Is this really necessary?!
                Logger.Debug($"Called ClearSCP008() method for {ev.Player.LogName}.", Config.DebugMode);
            }
        }

        private static void OnReviving(Scp049ResurrectingBodyEventArgs ev)
        {
            if (!Config.BuffDoctor) return;
            ev.IsAllowed = false;
            ev.Target.SetRole(RoleTypeId.Scp0492, RoleChangeReason.Resurrected);
        }

        private static void OnRevived(Scp049ResurrectedBodyEventArgs ev)
        {
            if (string.IsNullOrEmpty(Config.SuicideBroadcast))
            {
                ev.Target.ClearBroadcasts();
                ev.Target.SendBroadcast(Config.SuicideBroadcast, 10);
            }

            if (Config.Scp008Buff >= 0)
            {
                ev.Target.ArtificialHealth += Config.Scp008Buff;
                Logger.Debug($"Added {Config.Scp008Buff} AHP to {ev.Target.LogName}.", Config.DebugMode);
            }

            ev.Target.Health = Config.ZombieHealth;
            ev.Target.SendHint($"<color=yellow><b>SCP-008</b></color>\n{Config.SpawnHint}", 20f);
            Logger.Debug($"Set {ev.Target.LogName}'s HP to {Config.Scp008Buff}.", Config.DebugMode);
        }

        private static void OnPlayerDied(PlayerDeathEventArgs ev)
        {
            if (ev.Player.Role == RoleTypeId.Scp0492)
            {
                Scp008Handler.ClearScp008(ev.Player);
                Logger.Debug($"Called ClearSCP008() method for {ev.Player.LogName}.", Config.DebugMode);
            }

            if (ev.Player.ReferenceHub.TryGetComponent(out Scp008 _))
            {
                ev.Player.SetRole(RoleTypeId.Scp0492, RoleChangeReason.Resurrected);
            }

            if (ev.Player.Role == RoleTypeId.Scp049 || ev.Player.Role == RoleTypeId.Scp0492)
            {
                Scp008Handler.Victims--;
                if (Scp008Handler.Scp008Check())
                {
                    Logger.Debug("SCP008Check() passed. Announcing recontainment...", Config.DebugMode);

                    try
                    {
                        var announcement = NineTailedFoxAnnouncer.scpDeaths
                            .First(x => x.announcement == ev.DamageHandler.CassieDeathAnnouncement.Announcement);
                        announcement.scpSubjects.Add((RoleTypeId)64);
                    }
                    catch
                    {
                        NineTailedFoxAnnouncer.scpDeaths.Add(new NineTailedFoxAnnouncer.ScpDeath
                        {
                            scpSubjects = new List<RoleTypeId> { (RoleTypeId)64 },
                            announcement = ev.DamageHandler.CassieDeathAnnouncement.Announcement,
                            subtitleParts = ev.DamageHandler.CassieDeathAnnouncement.SubtitleParts
                        });
                    }
                }
            }

            if (Config.AoeInfection && ev.Player.Role == RoleTypeId.Scp0492)
            {
                Scp008Handler.AoeInfection(ev.Player);
            }
        }
    }
}