# Tebex Space Engineers Plugin
This is a dedicated server plugin is for **vanilla Space Engineers**. For the Torch API variant of this plugin, please see here: https://github.com/tebexio/Tebex-TorchAPI

## What is Tebex?
[Tebex](https://tebex.io/) provides a monetization and donation platform for game servers, allowing server owners to manage in-game purchases, subscriptions, and donations with ease.

This plugin allows you to fulfill purchases on your Space Engineers server, enabling you to offer a wide range of virtual items, packages, and services to your players.

## Commands and Configuration
Vanilla Space Engineers does not support commands. The Tebex plugin must be configured via the Dedicated Server GUI's configuration editor.

These are the configuration options available:

### Secret Key
Your secret key is required to connect your server to your Tebex store. Your secret key can be created and retrieved from your [Tebex panel](https://creator.tebex.io/game-servers)

### Debug Mode
By default the Tebex plugin will notify in the console when a package is delivered, for your records. To show more in-depth logging, enable Debug Mode.

Debug mode will also print a list of valid item IDs to your server's console on startup.

> ⚠️ Do not leave Debug mode enabled for extended sessions. It should only be used for debugging issues with your packages.

### Auto Report Errors
If an error or exception occurs on your server, having this setting on will automatically report the problem to Tebex for a developer to review. This helps us improve our plugins without requiring that you contact support.

## Installation
To install the Tebex Plugin for a Space Engineers dedicated server, follow these steps:

1. Download the latest release of this plugin from [Tebex.io](https://docs.tebex.io/plugin/official-plugins), or choose the latest [Release](https://github.com/tebexio/Tebex-SpaceEngineers/releases) from this repository.
2. Unzip the plugin's archive `Tebex-SpaceEngineers-2.0.0.zip` to your dedicated server's folder.
    - If installed using Steam/SteamCMD, this is usually `C:/Program Files (x86)/Steam/steamapps/common/SpaceEngineersDedicatedServer/DedicatedServer64/`
3. Start the Space Engineers dedicated server and continue to the configuration window.
4. Navigate to the `Plugins` tab and choose `Add Assembly`.
5. Select the plugin `Tebex-SpaceEngineers-VERSION.dll` from your server folder.
6. In the configuration window, set your secret key to the one shown in your [Tebex panel](https://creator.tebex.io/game-servers).
7. Start the server.

You will see a message in your server's console indicating that setup with Tebex was successful:
```
2024-04-16 09:28:42.094: Plugin Init: TebexSpaceEngineersPlugin.TebexPlugin
2024-04-16 09:28:42.110: [Tebex] [INFO] Tebex is starting up...
2024-04-16 09:28:42.111: [Tebex] [INFO] Secret key is set. Loading store info...
2024-04-16 09:28:42.361: Game ready... 
2024-04-16 09:28:42.991: [Tebex] [INFO] Connected to store https://example.tebex.io as game server Example Store
```

## Dev Environment Setup
If you wish to contribute to the development of the plugin, you can set up your development environment as follows:

**Setup Instructions:**
1. Clone the repository to an empty folder.
2. Ensure the assemblies in `Lib` are up-to-date from the latest Space Engineers Dedicated Server.

For updates or additional needed assemblies, always reference assemblies from the Space Engineers Dedicated Server release on Steam.

## Contributions
We welcome contributions from the community. Please refer to the `CONTRIBUTING.md` file for more details. By submitting code to us, you agree to the terms set out in the CONTRIBUTING.md file

## Support
This repository is only used for bug reports via GitHub Issues. If you have found a bug, please [open an issue](https://github.com/tebexio/Tebex-SpaceEngineers/issues).

If you are a user requiring support for Tebex, please contact us at https://tebex.io/contact