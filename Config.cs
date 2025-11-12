using System.ComponentModel;

namespace SCP008X
{
    public sealed class Config
    {
        [Description("Only enable this if you're looking for bug sources!")]
        public bool DebugMode { get; set; }

        [Description("Display plugins stats at the end of the round?")]
        public bool SummaryStats { get; set; }

        [Description("The chance of a player being infected when hit by a zombie.")]
        public int InfectionChance { get; set; } = 100;

        [Description("The chance a player cures themselves when using a medkit.")]
        public int CureChance { get; set; } = 50;

        [Description("Toggle players getting infected via area of effect")]
        public bool AoeInfection { get; set; } = false;

        [Description("Set AOE infection to run when infected players turn?")]
        public bool AoeTurned { get; set; } = false;

        [Description("Set the percentage chance players will get infected by area of effect")]
        public int AoeChance { get; set; } = 50;

        [Description("Allow SCP-049 to instantly revive targets?")]
        public bool BuffDoctor { get; set; } = false;

        [Description("Health given to the zombie when they spawn in.")]
        public int ZombieHealth { get; set; } = 320;

        [Description("How much AHP should be given to Zombies?")]
        public int Scp008Buff { get; set; } = 25;

        [Description(
            "Maximum AHP that a zombie can get by damaging other players. This is on top of HS they can get from 049.")]
        public int MaxAhp { get; set; } = 50;

        [Description("Whether to have CASSIE announce a message at the start of the round")]
        public bool CassieAnnounce { get; set; } = true;

        [Description("Message cassie will announce at the start of the round (if enabled)")]
        public string Announcement { get; set; } = "SCP 0 0 8 containment breach detected . Allremaining";

        [Description("Subtitles that will be shown for the cassie message (if enabled)")]
        public string AnnouncementSubtitles { get; set; } =
            "SCP-008 containment breach detected. " +
            "All remaining personnel are advised to proceed with standard evacuation protocols until an MTF squad reaches your destination.";

        public int ZombieDamage { get; set; } = 24;

        [Description("This is the text that will be displayed to SCP-049-2 players on spawn")]
        public string SuicideBroadcast { get; set; } = "";

        [Description("Text displayed to players after they've been infected")]
        public string InfectionAlert { get; set; } = "You've been infected! Use SCP-500 or a medkit to be cured!";

        [Description("Text displayed to newly turned SCP-049-2 players")]
        public string SpawnHint { get; set; } = "Players you hit will be infected with SCP-008!";

        [Description("Should players keep their inventory after turning into a zombie? Items cannot be used by them.")]
        public bool RetainInventory { get; set; } = true;

        [Description("The amount of infected items to spawn if there is no 049.")]
        public int InfectedItems { get; set; } = 1;

        [Description("Hint displayed when infected by an item.")]
        public string InfectedHint { get; set; } = "You picked up an item infected with SCP-008!";
    }
}