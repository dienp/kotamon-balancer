# Kotamon Balancer

Kotamon Balancer is a mod for [KOTAMON on Steam](https://store.steampowered.com/app/4294490/) that makes the game less grindy without giving you unlimited money or items.

The default settings make upgrades cheaper, make junk more valuable, improve energy, and speed up card collecting. The mod does not edit your save file.

## What the mod changes

| Game setting | Normal game | With the mod |
| --- | ---: | ---: |
| Upgrade prices | Full price | Half price |
| Value of basic junk | 3 | 5 |
| Junk pickup animation | Normal | Twice as fast |
| Energy price | 30 | 15 |
| Energy regeneration | Normal | Twice as fast |
| Energy restored by a beer can | 25% | 40% |
| Card-box price | 50,000 | 30,000 |
| Card-part appearance | Every 50 pickups | Every 30 pickups |
| Collectible-pile chance | 30% | 45% |
| Sprint control | Hold the Sprint key | Press once to run, press again to stop |

## How to install

You do **not** need Visual Studio, the .NET SDK, or any programming knowledge. Download the ready-to-use mod file directly from this repository.

### 1. Open the KOTAMON folder

In Steam:

1. Open your **Library**.
2. Right-click **KOTAMON**.
3. Select **Manage**, then **Browse local files**.

Keep this folder open.

### 2. Install MelonLoader

1. Download the installer from the [official MelonLoader GitHub page](https://github.com/LavaGang/MelonLoader).
2. Run the installer.
3. Select the KOTAMON game file and choose the **x64** version of **MelonLoader 0.7.3**.

You can also download version 0.7.3 directly from the [official MelonLoader 0.7.3 release](https://github.com/LavaGang/MelonLoader/releases/tag/v0.7.3).

### 3. Install Kotamon Balancer

1. [Download **KotamonBalancer.dll**](https://github.com/dienp/kotamon-balancer/raw/refs/heads/main/KotamonBalancer.dll).
2. Return to the KOTAMON folder you opened through Steam.
3. Open the **Mods** folder. If it does not exist, create a folder named `Mods`.
4. Move **KotamonBalancer.dll** into the **Mods** folder.

Your files should look like this:

```text
KOTAMON game folder
└── Mods
    └── KotamonBalancer.dll
```

### 4. Start the game

Launch KOTAMON normally through Steam.

The first launch after installing MelonLoader can take about **1–3 minutes**. The game may look frozen while MelonLoader prepares its files. This normally happens only once, so give it a few minutes before closing it. Later launches should be much faster.

When the mod loads correctly, `MelonLoader/Latest.log` will contain:

```text
Kotamon Balancer
by ptd
Assembly: KotamonBalancer.dll
```

## Changing the balance

The default settings are applied automatically. You do not need to change anything.

If you want different values:

1. Start the game once with the mod installed, then close it.
2. Open the KOTAMON game folder.
3. Open `UserData`, then open `MelonPreferences.cfg` with Notepad.
4. Find the `[KotamonBalancer]` section.
5. Change a value, save the file, and restart the game.

Examples:

- `0.5` means half.
- `1.0` means no change.
- `2.0` means double.
- `true` turns an option on, and `false` turns it off.

The default section begins like this:

```toml
[KotamonBalancer]
UpgradePriceMultiplier = 0.5
JunkValueMultiplier = 1.6666667
EnergyPriceMultiplier = 0.5
EnergyRegenMultiplier = 2.0
SmallEnergyRecoveryMultiplier = 1.6
CardBoxPriceMultiplier = 0.6
CardPartSpawnIntervalMultiplier = 0.6
CollectiblePileChanceMultiplier = 1.5
JunkPickupSpeedMultiplier = 2.0
ToggleSprint = true
```

`SmallEnergyRecoveryMultiplier` is the amount of energy restored by a beer can. The technical setting name is kept so existing configuration files continue to work.

`JunkPickupSpeedMultiplier` controls how quickly junk flies into your hand or bag. `2.0` means twice as fast, while `1.0` keeps the normal speed.

When `ToggleSprint` is `true`, press the Sprint key once to keep running and press it again to stop. Set it to `false` to restore the game's normal hold-to-sprint control.

Always close the game before editing this file.

## How to check that a setting works

Open `MelonLoader/Latest.log` in the KOTAMON folder and search for the setting name.

For example, these lines prove that the energy setting was loaded and changed the in-game value from 30 to 15:

```text
Configured EnergyPriceMultiplier=0.5
Applied EnergyPriceMultiplier: original=30, multiplier=0.5, result=15
```

The mod normally logs each change only once per launch so the log remains readable. Some settings appear only after you use that feature in the game.

## Troubleshooting

### The game looks frozen on the first launch

Wait a few minutes. MelonLoader may still be preparing the game. Check `MelonLoader/Latest.log`; if new lines are still appearing, it is still working.

### The mod does not appear in the log

Check that:

- The file is named `KotamonBalancer.dll`.
- It is inside the game's `Mods` folder, not next to that folder.
- MelonLoader was installed into the correct KOTAMON folder.
- You selected the x64 version of MelonLoader 0.7.3.

### MelonLoader reports a path-length or extraction error

KOTAMON has an unusually long folder name. On some Windows computers this can cause an error such as `Failure processing application bundle` or `Failed to commit extracted files` during the first launch. This is a MelonLoader setup problem rather than a save-file problem. Include `MelonLoader/Latest.log` when asking for help.

### A setting is listed but does not seem to work

Use the log check described above. A `Configured` line means the mod read your number. An `Applied` line shows the original game value and the changed result. Include the complete `MelonLoader/Latest.log` when reporting a problem.

## Settings intentionally not included

- Special collectible spawn points stay at the normal value of 6. An older experimental setting called `SpecialPointSpawnMultiplier` could crash the game when starting or loading a save, so it was removed.
- The number of card parts required stays at the normal game value. The older `CardPartsRequiredMultiplier` setting was removed because it could not be changed safely.

Old copies of these settings may remain in `MelonPreferences.cfg`. The current mod ignores them, so you may leave them there or delete them while the game is closed.

Older versions also included several optional settings whose normal value was `1.0`. They have been removed to keep the configuration simple. If any of those old lines remain in your preferences file, the current mod ignores them.

## Uninstalling

Close the game and delete `KotamonBalancer.dll` from the `Mods` folder. This removes the mod without changing your save file.

## For developers

Players do not need to build the mod. The ready-to-use `KotamonBalancer.dll` is stored at the top of this repository.

To build from source, install the .NET 6 SDK, let MelonLoader generate the game interop assemblies, set `KotamonGameDir` to your KOTAMON installation, and run:

```powershell
$env:KotamonGameDir = 'C:\path\to\KOTAMON'
dotnet build .\src\KotamonBalancer\KotamonBalancer.csproj -c Release
```

The DLL is created at `src/KotamonBalancer/bin/Release/net6.0/KotamonBalancer.dll`.

The `tools/GeneratedAssemblyFixer` project is a developer-only workaround for a Unity 6000.4 generated-assembly issue. Do not use it unless `MelonLoader/Latest.log` reports `No Support Module Loaded` or the matching duplicate generated-type error. It must be used on a backup copy of the MelonLoader-generated `UnityEngine.CoreModule.dll`, never on original game files.

## Notes

- The ready-to-use `KotamonBalancer.dll` at the top of this repository is rebuilt when the mod changes.
- This repository contains no game assets or save files.
- Keep backups of important saves and use mods only where permitted by the game.
