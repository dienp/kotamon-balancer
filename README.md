# Kotamon Faster Progression Mod

A lightweight MelonLoader/Harmony mod for **KOTAMON** designed to make progression faster and less grindy. It reduces upgrade prices to 50% of their normal value, so players can spend less time repeating resource-gathering tasks and more time collecting cards, upgrading, and exploring the game.

The mod keeps the game's core progression loop intact rather than removing costs or granting unlimited resources. It patches `UpgradeData.GetPrice()` at runtime and multiplies its result by `0.5`. It does not edit the save file or the game's asset bundles.

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

To change the discount, edit `PriceMultiplier` in `KotamonHalfPriceMod.cs`. For example, `0.25f` makes upgrades cost 25% of their normal price.

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
