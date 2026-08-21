using HarmonyLib;
using Il2CppProject.Code.Gameplay.Configs;
using MelonLoader;

[assembly: MelonInfo(typeof(KotamonHalfPriceRuntime.KotamonHalfPriceMod), "Kotamon Faster Progression", "1.1.0", "Codex")]
[assembly: MelonGame("KotaMota Games", "Kotamon")]

namespace KotamonHalfPriceRuntime;

public sealed class KotamonHalfPriceMod : MelonMod
{
    internal static MelonPreferences_Entry<float> UpgradePriceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> JunkValueMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> EnergyPriceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> EnergyRegenMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> SmallEnergyRecoveryMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CardBoxPriceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CardPartSpawnIntervalMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CollectiblePileChanceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> SpecialPointSpawnMultiplier { get; private set; } = null!;

    public override void OnInitializeMelon()
    {
        var category = MelonPreferences.CreateCategory(
            "KotamonFasterProgression",
            "Kotamon Faster Progression");

        UpgradePriceMultiplier = CreateEntry(category, "UpgradePriceMultiplier", 0.5f,
            "Upgrade price multiplier", "0.50 makes every upgrade cost half price.");
        JunkValueMultiplier = CreateEntry(category, "JunkValueMultiplier", 1.6666667f,
            "Junk value multiplier", "1.6667 raises the base junk value from 3 to approximately 5.");
        EnergyPriceMultiplier = CreateEntry(category, "EnergyPriceMultiplier", 0.5f,
            "Energy price multiplier", "0.50 reduces the configured energy price from 30 to 15.");
        EnergyRegenMultiplier = CreateEntry(category, "EnergyRegenMultiplier", 2f,
            "Energy regeneration multiplier", "2.00 doubles the configured energy regeneration rate.");
        SmallEnergyRecoveryMultiplier = CreateEntry(category, "SmallEnergyRecoveryMultiplier", 1.6f,
            "Small energy recovery multiplier", "1.60 raises small-energy recovery from 25% to 40%.");
        CardBoxPriceMultiplier = CreateEntry(category, "CardBoxPriceMultiplier", 0.6f,
            "Card box price multiplier", "0.60 reduces the card-box price from 50,000 to 30,000.");
        CardPartSpawnIntervalMultiplier = CreateEntry(category, "CardPartSpawnIntervalMultiplier", 0.6f,
            "Card-part spawn interval multiplier", "0.60 changes the interval from every 50 pickups to every 30.");
        CollectiblePileChanceMultiplier = CreateEntry(category, "CollectiblePileChanceMultiplier", 1.5f,
            "Collectible pile chance multiplier", "1.50 raises the zone-open pile chance from 30% to 45%.");
        SpecialPointSpawnMultiplier = CreateEntry(category, "SpecialPointSpawnMultiplier", 1.3333334f,
            "Special-point spawn multiplier", "1.3333 raises special collectible points from 6 to 8.");

        MelonPreferences.Save();
        LoggerInstance.Msg("Faster-progression preset active. Settings are in UserData/MelonPreferences.cfg.");
    }

    private static MelonPreferences_Entry<float> CreateEntry(
        MelonPreferences_Category category,
        string identifier,
        float defaultValue,
        string displayName,
        string description) =>
        category.CreateEntry<float>(identifier, defaultValue, displayName, description, false, false, null!);

    internal static float NonNegative(float value) => MathF.Max(0f, value);

    internal static int ScaleCount(int original, float multiplier) =>
        Math.Max(1, (int)MathF.Round(original * NonNegative(multiplier)));
}
[HarmonyPatch(typeof(UpgradeData), nameof(UpgradeData.GetPrice))]
internal static class UpgradeDataGetPricePatch
{
    private static void Postfix(ref float __result)
    {
        __result *= KotamonHalfPriceMod.NonNegative(KotamonHalfPriceMod.UpgradePriceMultiplier.Value);
    }
}

[HarmonyPatch(typeof(PriceConfig), nameof(PriceConfig.BaseJunkPrice), MethodType.Getter)]
internal static class BaseJunkPricePatch
{
    private static void Postfix(ref float __result)
    {
        __result *= KotamonHalfPriceMod.NonNegative(KotamonHalfPriceMod.JunkValueMultiplier.Value);
    }
}

[HarmonyPatch(typeof(PriceConfig), nameof(PriceConfig.EnergyPrice), MethodType.Getter)]
internal static class EnergyPricePatch
{
    private static void Postfix(ref float __result)
    {
        __result *= KotamonHalfPriceMod.NonNegative(KotamonHalfPriceMod.EnergyPriceMultiplier.Value);
    }
}

[HarmonyPatch(typeof(UpgradeConfig), nameof(UpgradeConfig.RegenPercent), MethodType.Getter)]
internal static class EnergyRegenPatch
{
    private static void Postfix(ref float __result)
    {
        __result = Math.Clamp(
            __result * KotamonHalfPriceMod.NonNegative(KotamonHalfPriceMod.EnergyRegenMultiplier.Value),
            0f,
            100f);
    }
}

[HarmonyPatch(typeof(UpgradeConfig), nameof(UpgradeConfig.SmallEnergyPercent), MethodType.Getter)]
internal static class SmallEnergyRecoveryPatch
{
    private static void Postfix(ref float __result)
    {
        __result = Math.Clamp(
            __result * KotamonHalfPriceMod.NonNegative(KotamonHalfPriceMod.SmallEnergyRecoveryMultiplier.Value),
            0f,
            100f);
    }
}

[HarmonyPatch(typeof(CardBoxSettings), nameof(CardBoxSettings.Price), MethodType.Getter)]
internal static class CardBoxPricePatch
{
    private static void Postfix(ref int __result)
    {
        __result = KotamonHalfPriceMod.ScaleCount(__result, KotamonHalfPriceMod.CardBoxPriceMultiplier.Value);
    }
}

[HarmonyPatch(typeof(CardsSettings.DirtyPartSettings), nameof(CardsSettings.DirtyPartSettings.PickupCountToSpawn), MethodType.Getter)]
internal static class CardPartSpawnIntervalPatch
{
    private static void Postfix(ref int __result)
    {
        __result = KotamonHalfPriceMod.ScaleCount(
            __result,
            KotamonHalfPriceMod.CardPartSpawnIntervalMultiplier.Value);
    }
}

[HarmonyPatch(typeof(CollectibleSettings), nameof(CollectibleSettings.PileSpawnChanceOnZoneOpen), MethodType.Getter)]
internal static class CollectiblePileChancePatch
{
    private static void Postfix(ref float __result)
    {
        __result = Math.Clamp(
            __result * KotamonHalfPriceMod.NonNegative(KotamonHalfPriceMod.CollectiblePileChanceMultiplier.Value),
            0f,
            100f);
    }
}

[HarmonyPatch(typeof(CollectibleSettings), nameof(CollectibleSettings.SpecialPointsSpawnCount), MethodType.Getter)]
internal static class SpecialPointSpawnPatch
{
    private static void Postfix(ref int __result)
    {
        __result = KotamonHalfPriceMod.ScaleCount(__result, KotamonHalfPriceMod.SpecialPointSpawnMultiplier.Value);
    }
}
