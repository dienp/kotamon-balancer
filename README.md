# Kotamon Faster Progression Mod

A lightweight MelonLoader/Harmony mod for **KOTAMON** designed to make progression faster and less grindy. Its balanced preset reduces repetitive resource gathering while preserving the game's core progression loop.

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

## Requirements

- KOTAMON on Windows
- MelonLoader 0.7.3 installed in the game directory
- .NET 6 SDK to build the mod

MelonLoader must have launched the game successfully at least once so that `MelonLoader/Il2CppAssemblies` exists.

## Build

Set `KotamonGameDir` to the game installation directory, then build:

```powershell
$env:KotamonGameDir = 'C:\path\to\KOTAMON'
dotnet build .\src\KotamonHalfPriceRuntime\KotamonHalfPriceRuntime.csproj -c Release
```

Copy `src/KotamonHalfPriceRuntime/bin/Release/net6.0/KotamonHalfPriceRuntime.dll` into the game's `Mods` directory and launch the game.

## Configuration

Launch the game once with the mod installed. MelonLoader creates `UserData/MelonPreferences.cfg` in the game directory with this section:

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

## Unity 6000.4 compatibility helper

`tools/GeneratedAssemblyFixer` is a narrowly scoped workaround for a duplicate generated `<>O`/`__O` type issue encountered in `UnityEngine.CoreModule.dll`. It operates only on a MelonLoader-generated interop assembly, not on the original game files.

Make a backup before using it. Run the tool against a copy and replace the generated assembly only after it reports successful verification:

```powershell
dotnet run --project .\tools\GeneratedAssemblyFixer -- `
  .\UnityEngine.CoreModule.dll `
  .\UnityEngine.CoreModule.fixed.dll
```

This helper is version-specific and should not be used unless the MelonLoader log reports the matching duplicate generated-type failure.

## Notes

- This repository intentionally contains no game assets, saves, generated game assemblies, or prebuilt mod binaries.
- Use mods only where permitted by the game's terms and keep backups of local files.
