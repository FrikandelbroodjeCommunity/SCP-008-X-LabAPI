using UnityEngine;
using MEC;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerStatsSystem;
using Logger = LabApi.Features.Console.Logger;

namespace SCP008X.Components
{
    public class Scp008 : MonoBehaviour
    {
        internal static readonly List<Scp008> Instances = new();
        
        private float _curAhp;
        private Player _ply;
        private CoroutineHandle _ahp;
        private CoroutineHandle _s008;

        private static Config Config => Scp008X.Singleton.Config;
        
        private void Awake()
        {
            _ply = Player.Get(gameObject);
            _ahp = Timing.RunCoroutine(RetainAHP());
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
            if (ev.DamageHandler is not UniversalDamageHandler handler) return;

            if (_curAhp > 0)
                _curAhp -= handler.Damage;
            else
                _curAhp = 0;
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
                            Timing.RunCoroutine(RetainAHP());
                            Logger.Debug($"Started coroutine for {_ply.Nickname}: RetainAHP.", Config.DebugMode);
                            break;
                        case RoleTypeId.Scp096:
                            Timing.KillCoroutines(_ahp);
                            Logger.Debug($"Killed coroutine for {_ply.Nickname}: RetainAHP.", Config.DebugMode);
                            _ply.ArtificialHealth = 500f;
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

        public IEnumerator<float> RetainAHP()
        {
            for (;;)
            {
                if (_ply.Role == RoleTypeId.Scp0492)
                {
                    if (_ply.ArtificialHealth <= _curAhp)
                    {
                        _ply.ArtificialHealth = _curAhp;
                    }
                    else
                    {
                        if (_ply.ArtificialHealth >= Config.MaxAhp)
                        {
                            _ply.ArtificialHealth = Config.MaxAhp;
                        }

                        _curAhp = _ply.ArtificialHealth;
                    }
                }

                yield return Timing.WaitForSeconds(0.05f);
            }
        }

        public IEnumerator<float> Infection()
        {
            for (;;)
            {
                _ply.Health -= 2;
                if (_ply.Health <= 0)
                {
                    _ply.Damage(1, _ply);
                    _ply.Health++;
                    break;
                }

                yield return Timing.WaitForSeconds(2f);
            }
        }
    }
}