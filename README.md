# Kotamon Balancer

A lightweight MelonLoader/Harmony mod for [**KOTAMON** on Steam](https://store.steampowered.com/app/4294490/) designed to make progression faster and less grindy. Its balanced preset reduces repetitive resource gathering while preserving the game's core progression loop.

The mod adjusts values at runtime. It does not grant unlimited resources, edit the save file, or modify the game's asset bundles.

## Balanced preset

| Setting | Original | Preset |
| --- | ---: | ---: |
| Upgrade prices | 100% | 50% |
| Base junk value | 3 | 5 |
| Energy price | 30 | 15 |
| Energy regeneration | 1x | 2x |
| Small-energy recovery | 25% | 40% |
| Card-box price | 50,000 | 30,000 |
| Card-part spawn interval | 50 pickups | 30 pickups |
| Collectible-pile chance | 30% | 45% |
| Special collectible points | 6 | 8 |

## Install

You do not need the .NET SDK or Visual Studio to install the mod.

1. Install [KOTAMON from Steam](https://store.steampowered.com/app/4294490/).
2. Install the x64 build of [MelonLoader 0.7.3](https://github.com/LavaGang/MelonLoader/releases/tag/v0.7.3) in the KOTAMON game directory. See the [official MelonLoader repository](https://github.com/LavaGang/MelonLoader) for its installer and manual installation instructions.
3. Download the precompiled [`KotamonHalfPriceRuntime.dll`](https://github.com/dienp/kotamon-balancer/releases/latest/download/KotamonHalfPriceRuntime.dll) from the [latest Kotamon Balancer release](https://github.com/dienp/kotamon-balancer/releases/latest).
4. Copy `KotamonHalfPriceRuntime.dll` into the game's `Mods` directory. Create the directory if MelonLoader has not created it yet.
5. Launch KOTAMON through Steam.

### First launch

The first modded launch can take roughly 1–3 minutes while MelonLoader generates KOTAMON's IL2CPP interop assemblies. The game may appear unresponsive during this one-time process. Do not close it while `MelonLoader/Latest.log` is still showing assembly-generation progress. Later launches should be much faster.

KOTAMON's unusually long default Steam directory name can cause a Windows path-length error during generation. If `MelonLoader/Latest.log` reports `Failure processing application bundle` or `Failed to commit extracted files`, launch the game once through a shorter path or directory junction, then return to launching it normally through Steam after generation succeeds.

If the log ends with `No Support Module Loaded` on Unity 6000.4, see the [Unity 6000.4 compatibility helper](#unity-60004-compatibility-helper).

## Configuration

After the mod initializes, MelonLoader creates `UserData/MelonPreferences.cfg` in the game directory with this section:

```toml
[KotamonFasterProgression]
UpgradePriceMultiplier = 0.5
JunkValueMultiplier = 1.6666667
EnergyPriceMultiplier = 0.5
EnergyRegenMultiplier = 2.0
SmallEnergyRecoveryMultiplier = 1.6
CardBoxPriceMultiplier = 0.6
CardPartSpawnIntervalMultiplier = 0.6
CollectiblePileChanceMultiplier = 1.5
SpecialPointSpawnMultiplier = 1.3333334
```

Close the game before editing the file, then restart it to apply the new values. Non-negative values are accepted; percentage results are capped at 100%, and count/interval results have a minimum of one.

### Optional advanced settings

Version 1.2 adds neutral-by-default multipliers for:

- Energy, bag, stock, drink, power, radius, and magnet upgrade effectiveness
- Bag-full rewards and magnet power
- Card values
- Common items per zone
- Card parts required
- Cards placed in junk zones
- Case and tape spawn chances
- Card-box animation duration

These entries default to `1.0`, so they do not change the balanced preset until edited. `BagCapacityMultiplier` affects the `BagLevel` upgrade curve only; it never edits the separate `BagCount` save counter. For faster card-box animations, set `CardBoxAnimationDurationMultiplier` below `1.0`, such as `0.5`.

## Building from source

Building is only required for development. Install the .NET 6 SDK, allow MelonLoader to generate `MelonLoader/Il2CppAssemblies`, set `KotamonGameDir` to the game installation directory, then build:

```powershell
$env:KotamonGameDir = 'C:\path\to\KOTAMON'
dotnet build .\src\KotamonHalfPriceRuntime\KotamonHalfPriceRuntime.csproj -c Release
```

The output is `src/KotamonHalfPriceRuntime/bin/Release/net6.0/KotamonHalfPriceRuntime.dll`.

## Unity 6000.4 compatibility helper

`tools/GeneratedAssemblyFixer` is a narrowly scoped workaround for a duplicate generated `<>O`/`__O` type issue encountered in `UnityEngine.CoreModule.dll`. It operates only on a MelonLoader-generated interop assembly, not on the original game files.

Make a backup before using it. Run the tool against a copy and replace the generated assembly only after it reports successful verification:

```powershell
dotnet run --project .\tools\GeneratedAssemblyFixer -- `
  .\UnityEngine.CoreModule.dll `
  .\UnityEngine.CoreModule.fixed.dll
```

This helper is version-specific and should not be used unless `MelonLoader/Latest.log` ends with `No Support Module Loaded` or reports the matching duplicate generated-type failure after successful interop generation.

## Notes

- Precompiled mod binaries are published as GitHub Release assets, not committed to the source tree.
- This repository intentionally contains no game assets, saves, or generated game assemblies.
- Use mods only where permitted by the game's terms and keep backups of local files.
