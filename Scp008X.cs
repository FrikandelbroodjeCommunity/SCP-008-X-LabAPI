using System;
using HarmonyLib;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;

namespace SCP008X
{
    public class Scp008X : Plugin<Config>
    {
        public override string Name => "Scp008X";
        public override string Description => "Adds SCP-008 into the game";
        public override string Author => "DGvagabond";
        public override Version Version => new Version(1, 0, 1);
        public override Version RequiredApiVersion => new Version(LabApiProperties.CompiledVersion);

        public static Scp008X Singleton;

        private readonly Harmony _harmony = new("gamendegamer.SCP008X");

        public override void Enable()
        {
            Singleton = this;
            _harmony.PatchAll();
            EventHandlers.RegisterEvents();
        }

        public override void Disable()
        {
            _harmony.UnpatchAll();
            EventHandlers.UnregisterEvents();
        }
    }
}