# [PULSAR Mod Loader - Adaptor][0]


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

BepInEx patcher plugin which repeatedly injects the mod loader into [*PULSAR: Lost Colony*][10].

[10]: http://www.pulsarthegame.com/ "PULSAR: Lost Colony"

> [!CAUTION]  
> For backwards support of the old mods, HarmonyX is altered to not normalise transpiler instructions. Therefore, some mods (which are unaware of this) may not patch properly as `OpCodes.Beq_S` for instance will not be changed to `OpCodes.Beq`

## Installation

Before running ensure your Pulsar installation is a Mono build. On Steam you can change this by right clicking Pulsar: lost colony in your steam library and selecting [properties > betas > mono].

![image](https://github.com/PULSAR-Modders/pulsar-mod-loader/assets/46509577/8aeca171-3cd7-4ffc-8805-77c8ce1400e7)


Next, install BepInEx either manually or through a mod manager such as: Gale, R2ModMan and Thunderstore.

![image](https://github.com/user-attachments/assets/df1d3a43-34f9-4d20-96d4-67a08584c853)


Then, place `PulsarModLoader.dll` and `PulsarModLoader.Adaptor.dll` into the `BepInEx\patchers` folder. Then run PULSAR normally, if it is installed, using F5 will bring up a UI interface.

![image](https://github.com/user-attachments/assets/2cfb714f-6024-4480-b9c3-d2fc24c84fa2)

> [!NOTE]  
> You can also place `PulsarModLoader` into the `PULSARLostColony\Managed` directory, however it will prioritise loading from the patcher location.

Afterwards, add mods to the `PULSARLostColony\Mods` directory or `BepInEx\plugins` directory and run PULSAR normally.

![image](https://github.com/user-attachments/assets/85e92f5b-adeb-46e3-9934-2170abef1c14)

### Removal

Disable the patcher or remove `PulsarModLoader.dll` and `PulsarModLoader.Adaptor.dll` from `BepInEx\plugins`

## Creating Mods

[Check out the wiki for basic instructions on creating mods.](https://github.com/PULSAR-Modders/pulsar-mod-loader/wiki/Creating-Mods)

Currently, the adaptor is only used for loading `PulsarModLoader` and the old mods. In the future, changes will be made to download the most recent `PulsarModLoader` and
a template will be added for BepInEx plugins to be made and use the mod loaders addons.
