using CommandSystem;
using System;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using PlayerRoles;
using SCP008X.Components;

namespace SCP008X.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Infect : ICommand
    {
        public string Command => "infect";
        public string[] Aliases => null;
        public string Description => "Forcefully infect a player with SCP-008";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.HasPermissions("scp008.infect"))
            {
                response = "Missing permissions.";
                return false;
            }

            var ply = Player.Get(arguments.At(0));
            if (ply == null)
            {
                response = "Invalid player.";
                return false;
            }

            switch (ply.Team)
            {
                case Team.SCPs:
                    response = "You can not infect SCP players.";
                    return false;
                case Team.OtherAlive:
                    response = "You can not infect this class.";
                    return false;
            }

            if (ply.ReferenceHub.TryGetComponent(out Scp008 _))
            {
                response = "This player is already infected.";
                return false;
            }

            ply.ReferenceHub.gameObject.AddComponent<Scp008>();
            ply.SendHint($"<color=yellow><b>SCP-008</b></color>\n{Scp008X.Singleton.Config.InfectionAlert}", 10f);
            response = $"{ply.Nickname} has been infected.";
            return true;
        }
    }
}