# [PULSAR Mod Loader][0]


[0]: https://github.com/PULSAR-Modders/pulsar-mod-loader "PULSAR Mod Loader"

[![Build Status][1]][2]
[![Download][3]][4]
[![Wiki][5]][6]
[![Discord][7]][8]

[1]: https://github.com/PULSAR-Modders/pulsar-mod-loader/workflows/Build/badge.svg
[2]: https://github.com/PULSAR-Modders/pulsar-mod-loader/actions "Build Status"
[3]: https://img.shields.io/badge/-DOWNLOAD-success
[4]: https://github.com/PULSAR-Modders/pulsar-mod-loader/packages "Download"
[5]: https://img.shields.io/badge/-WIKI-informational
[6]: https://github.com/PULSAR-Modders/pulsar-mod-loader/wiki "Wiki"
[7]: https://img.shields.io/discord/458244416562397184.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2
[8]: https://discord.gg/yBJGv4T "PML Discord"

A mod loader, library, and manager for [*PULSAR: Lost Colony*][10].

[10]: http://www.pulsarthegame.com/ "PULSAR: Lost Colony"

## Installation

Before attempting to mod, ensure your Pulsar installation is a Mono build. On Steam you can change this by right clicking Pulsar: lost colony in your steam library and selecting [properties > betas > mono].

![image](https://github.com/PULSAR-Modders/pulsar-mod-loader/assets/46509577/8aeca171-3cd7-4ffc-8805-77c8ce1400e7)

### Choices
- [Injector](https://github.com/PULSAR-Modders/pulsar-mod-loader/tree/master/PulsarModLoader.Injector) - Custom built injector which loads the manager and mods but is required to be ran every update.

- [Adaptor](https://github.com/PULSAR-Modders/pulsar-mod-loader/tree/master/PulsarModLoader.Adaptor) - BepInEx adaptor which allows the manager and mods to be loaded through BepInEx.

### Mod Locations
Currently, mods are primarily found on the [Pulsar Lost Colony: Crew Matchup Server][8] (Unofficial Discord). They will be coming soon to Thunderstore however.

## Features

- [x] Method to inject mods into the game
- [x] Indicator for modded sessions
- [x] Mod requirement management
- [x] Zip folder mod loading
- [x] Utilities to create local chat commands
- [x] Utilities to create public chat commands
- [x] Autofill for chat commands and names
- [x] Onscreen exception warning messages
- [x] Mod Messages which allow mods to share data
- [x] Keybinds
- [x] Transpiler patch assist methods
- [x] Subscribable events for mods to use
- [x] Utilities to save data based on session or locally
- [x] User Interface to interact with mods (Unloading, config etc)
- [x] Utilities to create new components
- [x] Utilities to create new items
- [ ] Utilities to create new talents
- [ ] Utilities to create new ships
- [ ] Utilities to create new shops
- [ ] Utilities to create new avatars/clothing

## Creating Mods

[Check out the wiki for basic instructions on creating mods.](https://github.com/PULSAR-Modders/pulsar-mod-loader/wiki/Creating-Mods)
