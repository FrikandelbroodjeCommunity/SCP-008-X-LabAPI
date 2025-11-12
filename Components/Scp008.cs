using System;
using UnityEngine;
using MEC;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerStatsSystem;
using Logger = LabApi.Features.Console.Logger;

namespace SCP008X.Components
{
    [DisallowMultipleComponent]
    public class Scp008 : MonoBehaviour
    {
        internal static readonly List<Scp008> Instances = new();

        [NonSerialized] public float CurAhp;
        private Player _ply;
        private CoroutineHandle _ahp;
        private CoroutineHandle _s008;

        private static Config Config => Scp008X.Singleton.Config;

        private void Awake()
        {
            _ply = Player.Get(gameObject);
            _ahp = Timing.RunCoroutine(RetainAhp());
            _s008 = Timing.RunCoroutine(Infection());
            
            Instances.Add(this);
        }

        private void OnDestroy()
        {
            _ply = null;
            Timing.KillCoroutines(_ahp);
            Timing.KillCoroutines(_s008);
            
            Instances.Remove(this);
        }

        public void WhenHurt(PlayerHurtEventArgs ev)
        {
            if (ev.Player != _ply || ev.Player.Role != RoleTypeId.Scp0492) return;
            if (ev.DamageHandler is not StandardDamageHandler handler) return;

            if (CurAhp > 0)
                CurAhp -= handler.Damage;
            else
                CurAhp = 0;
        }

        public void WhenRoleChange(PlayerChangedRoleEventArgs ev)
        {
            if (ev.Player != _ply)
                return;
            
            switch (ev.Player.Faction)
            {
                case Faction.SCP:
                    switch (ev.NewRole.RoleTypeId)
                    {
                        case RoleTypeId.Scp0492:
                            Timing.RunCoroutine(RetainAhp());
                            Logger.Debug($"Started coroutine for {_ply.Nickname}: RetainAHP.", Config.DebugMode);
                            break;
                    }

                    break;
                case Faction.FoundationStaff:
                case Faction.FoundationEnemy:
                    Timing.RunCoroutine(Infection());
                    Timing.KillCoroutines(_ahp);
                    Logger.Debug($"Traded coroutines for {_ply.Nickname}: RetainAHP -> Infection.", Config.DebugMode);
                    break;
                case Faction.Unclassified:
                case Faction.Flamingos:
                default:
                    break;
            }
        }

        public IEnumerator<float> RetainAhp()
        {
            for (;;)
            {
                if (_ply.Role == RoleTypeId.Scp0492)
                {
                    if (_ply.ArtificialHealth >= Config.MaxAhp)
                    {
                        CurAhp = Config.MaxAhp;
                    }

                    if (_ply.ArtificialHealth <= CurAhp)
                    {
                        _ply.ArtificialHealth = CurAhp;
                    }
                }

                yield return Timing.WaitForSeconds(0.05f);
            }
        }

        public IEnumerator<float> Infection()
        {
            for (;;)
            {
                if (_ply.Faction == Faction.SCP)
                {
                    yield break;
                }

                _ply.Health -= 2;
                if (_ply.Health <= 0)
                {
                    _ply.Health = 1;
                    break;
                }

                yield return Timing.WaitForSeconds(2f);
            }
        }
    }
}