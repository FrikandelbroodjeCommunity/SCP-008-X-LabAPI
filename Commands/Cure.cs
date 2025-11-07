using CommandSystem;
using System;
using LabApi.Features.Console;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using SCP008X.Components;

namespace SCP008X.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Cure : ICommand
    {
        public string Command { get; } = "cure";

        public string[] Aliases { get; } = null;

        public string Description { get; } = "Forcefully cure a player from SCP-008";

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

            if (!ply.ReferenceHub.TryGetComponent(out Scp008 scp008))
            {
                response = "This player is not infected.";
                return false;
            }

            try
            {
                UnityEngine.Object.Destroy(scp008);
                response = $"{ply.Nickname} has been cured.";
                return true;
            }
            catch (Exception e)
            {
                Logger.Debug($"Failed to destroy SCP008 component! {e}", Scp008X.Singleton.Config.DebugMode);
                response = $"Failed to cure {ply.Nickname}.";
                return false;
            }
        }
    }
}