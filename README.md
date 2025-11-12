[![GitHub release](https://flat.badgen.net/github/release/FrikandelbroodjeCommunity/SCP-008-X-LabAPI/)](https://github.com/FrikandelbroodjeCommunity/SCP-008-X-LabAPI/releases/latest)
[![LabAPI Version](https://flat.badgen.net/static/LabAPI%20Version/v1.1.4)](https://github.com/northwood-studios/LabAPI)
[![Original](https://flat.badgen.net/static/Original/DGvagabond?icon=github)](https://github.com/DGvagabond/SCP-008-X)
[![License](https://flat.badgen.net/github/license/FrikandelbroodjeCommunity/SCP-008-X-LabAPI/)](https://github.com/FrikandelbroodjeCommunity/SCP-008-X-LabAPI/blob/master/LICENSE)

# About SCP-008-X

A LabAPI plugin for SCP:SL that adds SCP-008 into the game.

It will give zombies the ability to infect players when hitting them. Whe infected you will slowly drain your HP. The
virus will not actually kill you, keeping you at 1 HP. If you die while infected, you will become a zombie.

A player is able to cure the infection by using `SCP-500`. A `Medkit` will also have a chance of curing the infection,
by default this is set to `50%`.

If no `SCP-049` has spawned, one of the items in the map will become an infected item. When a player picks up this
infected item, they will become infected themselves, after which the item is no longer infected. This way there can be
an SCP-008 outbreak, even if there is no `SCP-049`. (Can be disabled in the config)

# Installation

Install the [latest release](https://github.com/FrikandelbroodjeCommunity/SCP-008-X-LabAPI/releases/latest) of the
SCP-008-X plugin and place it in your LabAPI plugin folder.

# Commands

| Command  | Usage             | Required permission | Description                   |
|----------|-------------------|---------------------|-------------------------------|
| `infect` | `infect <player>` | `scp008.infect`     | Infects `player` with SCP-008 |
| `cure`   | `cure <player>`   | `scp008.infect`     | Cures `player` of SCP-008     |

# Config

| Name                     | Default | Description                                                                                               |
|--------------------------|---------|-----------------------------------------------------------------------------------------------------------|
| `debug_mode`             | `false` | Toggles debug messages to your console.0                                                                  |
| `summary_stats`          | `false` | Toggles round summary stats.                                                                              |
| `infection_chance`       | `25%`   | Percentage chance of infection.                                                                           |
| `cure_chance`            | `50%`   | Percentage chance of being cured when using a medkit.                                                     |
| `aoe_infection`          | `false` | Toggles infecting players near killed zombies.                                                            |
| `aoe_turned`             | `false` | Toggles infecting players near recently turned zombies.                                                   |
| `aoe_chance`             | `50%`   | Percentage chance of players near recently turned zombies being infected.                                 |
| `buff_doctor`            | `false` | Enable instant revives for SCP-049.                                                                       |
| `zombie_health`          | `320`   | Amount of health zombies spawn with.                                                                      |
| `scp008_buff`            | `10`    | Amount of AHP zombies spawn with and gain on each hit.                                                    |
| `max_ahp`                | `50`    | Maximum amount of AHP zombies can reach. This is on top of the HS they can get from SCP-049.              |
| `cassie_announcement`    | `true`  | Toggles the announcement when the round starts.                                                           |
| `announcement`           | ...     | Sets the CASSIE announcement when the round starts.                                                       |
| `announcement_subtitles` | ...     | Subtitles that the players will see when CASSIE announces the message.                                    |
| `zombie_damage`          | `24`    | Set how much damage SCP-049-2 deals on hit.                                                               |
| `suicide_broadcast`      | `null`  | Text that is displayed to all instances of SCP-049-2.                                                     |
| `infection_alert`        | `null`  | A hint that is displayed to players after they're infected.                                               |
| `spawn_hint`             | `null`  | A hint that's displayed to SCP-049-2 on spawn.                                                            |
| `retain_inventory`       | `true`  | Allow players to keep their inventory as zombies. Items can NOT be used by them, this is purely for loot. |
| `infected_items`         | `1`     | The amount of infected items that spawn in the map if there is no SCP-049. Set to 0 to disable.           |
| `infected_hint`          | ...     | Hint shown when picking up an infected item. Can be different from the `infection_alert`.                 |
