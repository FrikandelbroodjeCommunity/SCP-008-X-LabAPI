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

        private static readonly List<ushort> Scp008ItemIds = new();

        public static void RegisterEvents()
        {
            ServerEvents.RoundStarted += OnRoundStart;
            ServerEvents.RoundEnded += OnRoundEnd;

            PlayerEvents.Left += OnPlayerLeave;
            PlayerEvents.Hurting += OnPlayerHurting;
            PlayerEvents.Hurt += OnPlayerHurt;
            PlayerEvents.UsedItem += OnHealed;
            PlayerEvents.ChangingRole += OnRoleChange;
            PlayerEvents.ChangedRole += OnRoleChanged;
            PlayerEvents.Death += OnPlayerDied;
            PlayerEvents.PickedUpItem += PickedUpItem;

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
            PlayerEvents.ChangingRole -= OnRoleChange;
            PlayerEvents.ChangedRole -= OnRoleChanged;
            PlayerEvents.Death -= OnPlayerDied;
            PlayerEvents.PickedUpItem -= PickedUpItem;

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
                        LabApi.Features.Wrappers.Cassie.Message(Config.Announcement);
                    }
                    else
                    {
                        LabApi.Features.Wrappers.Cassie.Message(Config.Announcement,
                            customSubtitles: Config.AnnouncementSubtitles);
                    }

                    Scp008ItemIds.Clear();

                    // Determine if we should add an SCP-008 item
                    if (Player.List.Any(x => x.Role == RoleTypeId.Scp049 || x.Role == RoleTypeId.Scp0492))
                    {
                        return;
                    }

                    var allItems = Item.List
                        .Where(x => x.Category != ItemCategory.SpecialWeapon)
                        .Select(x => x.Serial)
                        .ToList();

                    for (var i = 0; i < Config.InfectedItems; i++)
                    {
                        Scp008ItemIds.Add(allItems.PullRandomItem());
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
            if (ev.Player.Role == RoleTypeId.Scp0492 && ev.Player.GameObject.TryGetComponent(out Scp008 s008))
            {
                Scp008Handler.ClearScp008(ev.Player);
            }
        }

        private static void OnPlayerHurting(PlayerHurtingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Attacker.Role != RoleTypeId.Scp0492 || ev.Player == ev.Attacker) return;
            if (ev.DamageHandler is not StandardDamageHandler handler) return;

            if (Config.ZombieDamage >= 0)
            {
                handler.Damage = Config.ZombieDamage;
                Logger.Debug($"Damage overriden to be {handler.Damage}.", Config.DebugMode);
            }

            if (Config.Scp008Buff >= 0 && ev.Attacker.GameObject.TryGetComponent(out Scp008 scp008))
            {
                scp008.CurAhp += Config.Scp008Buff;
                Logger.Debug($"Added {Config.Scp008Buff} AHP to {ev.Attacker.LogName}.", Config.DebugMode);
            }

            if (Gen.Next(0, 100) >= Config.InfectionChance || ev.Player.Faction == Faction.SCP) return;
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
            if (ev.Player.GameObject.TryGetComponent(out Scp008 scp008))
            {
                switch (ev.UsableItem.Type)
                {
                    case ItemType.SCP500:
                        UnityEngine.Object.Destroy(scp008);
                        Logger.Debug($"{ev.Player.LogName} successfully cured themselves.", Config.DebugMode);
                        break;
                    case ItemType.Medkit:
                        if (Gen.Next(0, 100) < Config.CureChance)
                        {
                            UnityEngine.Object.Destroy(scp008);
                            Logger.Debug($"{ev.Player.LogName} cured themselves with a medkit.", Config.DebugMode);
                        }

                        break;
                }
            }
        }

        private static void OnRoleChange(PlayerChangingRoleEventArgs ev)
        {
            // Was not a zombie yet, but does have the SCP008 component, so is infected
            if (ev.OldRole.RoleTypeId != RoleTypeId.Scp0492 && ev.Player.GameObject.TryGetComponent(out Scp008 _))
            {
                ev.NewRole = RoleTypeId.Scp0492;
            }

            if (ev.NewRole == RoleTypeId.Scp0492)
            {
                Logger.Debug($"Calling Turn() method for {ev.Player.LogName}.", Config.DebugMode);
                Timing.CallDelayed(0.1f, () => // Small delay so the player is already the zombie when this happens
                {
                    Scp008Handler.Turn(ev.Player);
                });
            }
            else
            {
                Scp008Handler.ClearScp008(ev.Player);
                Logger.Debug($"Called ClearSCP008() method for {ev.Player.LogName}.", Config.DebugMode);
            }
        }

        private static void OnRoleChanged(PlayerChangedRoleEventArgs ev)
        {
            foreach (var instance in Scp008.Instances)
            {
                instance.WhenRoleChange(ev);
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

            if (Config.Scp008Buff >= 0 && ev.Player.GameObject.TryGetComponent(out Scp008 comp))
            {
                comp.CurAhp += Config.Scp008Buff;
                Logger.Debug($"Added {Config.Scp008Buff} AHP to {ev.Target.LogName}.", Config.DebugMode);
            }

            ev.Target.Health = Config.ZombieHealth;
            ev.Target.SendHint($"<color=yellow><b>SCP-008</b></color>\n{Config.SpawnHint}", 20f);
            Logger.Debug($"Set {ev.Target.LogName}'s HP to {Config.Scp008Buff}.", Config.DebugMode);
        }

        private static void OnPlayerDied(PlayerDeathEventArgs ev)
        {
            if (ev.OldRole == RoleTypeId.Scp0492)
            {
                Scp008Handler.ClearScp008(ev.Player);
                Logger.Debug($"Called ClearSCP008() method for {ev.Player.LogName}.", Config.DebugMode);
            }
            else if (ev.Player.GameObject.TryGetComponent(out Scp008 _))
            {
                ev.Player.SetRole(RoleTypeId.Scp0492, RoleChangeReason.Resurrected);
            }

            if (Config.AoeInfection && ev.Player.Role == RoleTypeId.Scp0492)
            {
                Scp008Handler.AoeInfection(ev.Player);
            }
        }

        private static void PickedUpItem(PlayerPickedUpItemEventArgs ev)
        {
            if (!Scp008ItemIds.Contains(ev.Item.Serial))
            {
                return;
            }

            Scp008ItemIds.Remove(ev.Item.Serial);

            Timing.CallDelayed(1f, () =>
            {
                if (ev.Player.GameObject.TryGetComponent(out Scp008 _))
                {
                    return;
                }

                ev.Player.ReferenceHub.gameObject.AddComponent<Scp008>();
                ev.Player.SendHint($"<color=yellow><b>SCP-008</b></color>\n{Config.InfectedHint}", 10f);
            });
        }
    }
}