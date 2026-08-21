using HarmonyLib;
using Il2CppProject.Code.Gameplay.Configs;
using Il2CppProject.Code.Gameplay.Controllers;
using MelonLoader;

[assembly: MelonInfo(typeof(KotamonBalancer.KotamonBalancerMod), "Kotamon Balancer", "1.2.1", "ptd")]
[assembly: MelonGame("KotaMota Games", "Kotamon")]

namespace KotamonBalancer;

public sealed class KotamonBalancerMod : MelonMod
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
    internal static MelonPreferences_Entry<float> EnergyCapacityMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> BagCapacityMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> StockLevelMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> PickupRadiusMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> DrinkCapacityMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> PowerMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> MagnetRangeMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> BagFullRewardMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> MagnetPowerMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CardValueMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CommonItemsPerZoneMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CardPartsRequiredMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> JunkZoneCardCountMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CaseSpawnChanceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> TapeSpawnChanceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CardBoxAnimationDurationMultiplier { get; private set; } = null!;

    public override void OnInitializeMelon()
    {
        var category = MelonPreferences.CreateCategory(
            "KotamonBalancer",
            "Kotamon Balancer");

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

        EnergyCapacityMultiplier = CreateEntry(category, "EnergyCapacityMultiplier", 1f,
            "Energy capacity multiplier", "Scales every Energy Level upgrade value. Default 1.00 is unchanged.");
        BagCapacityMultiplier = CreateEntry(category, "BagCapacityMultiplier", 1f,
            "Bag capacity multiplier", "Scales every Bag Level capacity value; this does not alter BagCount.");
        StockLevelMultiplier = CreateEntry(category, "StockLevelMultiplier", 1f,
            "Stock level multiplier", "Scales every Stock Level upgrade value.");
        PickupRadiusMultiplier = CreateEntry(category, "PickupRadiusMultiplier", 1f,
            "Pickup radius multiplier", "Scales every Radius Level upgrade value.");
        DrinkCapacityMultiplier = CreateEntry(category, "DrinkCapacityMultiplier", 1f,
            "Drink capacity multiplier", "Scales every Drink Level upgrade value.");
        PowerMultiplier = CreateEntry(category, "PowerMultiplier", 1f,
            "Power multiplier", "Scales every Power Level upgrade value.");
        MagnetRangeMultiplier = CreateEntry(category, "MagnetRangeMultiplier", 1f,
            "Magnet range multiplier", "Scales every Magnet Level upgrade value.");
        BagFullRewardMultiplier = CreateEntry(category, "BagFullRewardMultiplier", 1f,
            "Bag-full reward multiplier", "Scales the configured BagFullReward values.");
        MagnetPowerMultiplier = CreateEntry(category, "MagnetPowerMultiplier", 1f,
            "Magnet power multiplier", "Scales the configured MagnetPower values.");
        CardValueMultiplier = CreateEntry(category, "CardValueMultiplier", 1f,
            "Card value multiplier", "Scales card prices returned by the card configuration.");
        CommonItemsPerZoneMultiplier = CreateEntry(category, "CommonItemsPerZoneMultiplier", 1f,
            "Common items per zone multiplier", "Scales the configured common-item count for each zone.");
        CardPartsRequiredMultiplier = CreateEntry(category, "CardPartsRequiredMultiplier", 1f,
            "Card parts required multiplier", "Scales the number of dirty card parts needed; minimum one.");
        JunkZoneCardCountMultiplier = CreateEntry(category, "JunkZoneCardCountMultiplier", 1f,
            "Junk-zone card count multiplier", "Scales the number of cards placed in junk zones.");
        CaseSpawnChanceMultiplier = CreateEntry(category, "CaseSpawnChanceMultiplier", 1f,
            "Case spawn chance multiplier", "Scales case spawn chances and caps them at 100%.");
        TapeSpawnChanceMultiplier = CreateEntry(category, "TapeSpawnChanceMultiplier", 1f,
            "Tape spawn chance multiplier", "Scales tape spawn chances and caps them at 100%.");
        CardBoxAnimationDurationMultiplier = CreateEntry(category, "CardBoxAnimationDurationMultiplier", 1f,
            "Card-box animation duration multiplier", "Use 0.50 for animations and delays that take half as long.");

        MelonPreferences.Save();
        LoggerInstance.Msg("Kotamon Balancer preset active. Settings are in UserData/MelonPreferences.cfg.");
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

    internal static float GetUpgradeValueMultiplier(ParameterType parameterType) => parameterType switch
    {
        ParameterType.EnergyLevel => EnergyCapacityMultiplier.Value,
        ParameterType.BagLevel => BagCapacityMultiplier.Value,
        ParameterType.StockLevel => StockLevelMultiplier.Value,
        ParameterType.RadiusLevel => PickupRadiusMultiplier.Value,
        ParameterType.DrinkLevel => DrinkCapacityMultiplier.Value,
        ParameterType.PowerLevel => PowerMultiplier.Value,
        ParameterType.MagnetLevel => MagnetRangeMultiplier.Value,
        ParameterType.BagFullReward => BagFullRewardMultiplier.Value,
        ParameterType.MagnetPower => MagnetPowerMultiplier.Value,
        _ => 1f
    };
}
[HarmonyPatch(typeof(UpgradeData), nameof(UpgradeData.GetPrice))]
internal static class UpgradeDataGetPricePatch
{
    private static void Postfix(ref float __result)
    {
        __result *= KotamonBalancerMod.NonNegative(KotamonBalancerMod.UpgradePriceMultiplier.Value);
    }
}

[HarmonyPatch(typeof(PriceConfig), nameof(PriceConfig.BaseJunkPrice), MethodType.Getter)]
internal static class BaseJunkPricePatch
{
    private static void Postfix(ref float __result)
    {
        __result *= KotamonBalancerMod.NonNegative(KotamonBalancerMod.JunkValueMultiplier.Value);
    }
}

[HarmonyPatch(typeof(PriceConfig), nameof(PriceConfig.EnergyPrice), MethodType.Getter)]
internal static class EnergyPricePatch
{
    private static void Postfix(ref float __result)
    {
        __result *= KotamonBalancerMod.NonNegative(KotamonBalancerMod.EnergyPriceMultiplier.Value);
    }
}

[HarmonyPatch(typeof(UpgradeConfig), nameof(UpgradeConfig.RegenPercent), MethodType.Getter)]
internal static class EnergyRegenPatch
{
    private static void Postfix(ref float __result)
    {
        __result = Math.Clamp(
            __result * KotamonBalancerMod.NonNegative(KotamonBalancerMod.EnergyRegenMultiplier.Value),
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
            __result * KotamonBalancerMod.NonNegative(KotamonBalancerMod.SmallEnergyRecoveryMultiplier.Value),
            0f,
            100f);
    }
}

[HarmonyPatch(typeof(CardBoxSettings), nameof(CardBoxSettings.Price), MethodType.Getter)]
internal static class CardBoxPricePatch
{
    private static void Postfix(ref int __result)
    {
        __result = KotamonBalancerMod.ScaleCount(__result, KotamonBalancerMod.CardBoxPriceMultiplier.Value);
    }
}

[HarmonyPatch(typeof(CardsSettings.DirtyPartSettings), nameof(CardsSettings.DirtyPartSettings.PickupCountToSpawn), MethodType.Getter)]
internal static class CardPartSpawnIntervalPatch
{
    private static void Postfix(ref int __result)
    {
        __result = KotamonBalancerMod.ScaleCount(
            __result,
            KotamonBalancerMod.CardPartSpawnIntervalMultiplier.Value);
    }
}

[HarmonyPatch(typeof(CollectibleSettings), nameof(CollectibleSettings.PileSpawnChanceOnZoneOpen), MethodType.Getter)]
internal static class CollectiblePileChancePatch
{
    private static void Postfix(ref float __result)
    {
        __result = Math.Clamp(
            __result * KotamonBalancerMod.NonNegative(KotamonBalancerMod.CollectiblePileChanceMultiplier.Value),
            0f,
            100f);
    }
}

[HarmonyPatch(typeof(CollectibleSettings), nameof(CollectibleSettings.SpecialPointsSpawnCount), MethodType.Getter)]
internal static class SpecialPointSpawnPatch
{
    private static void Postfix(ref int __result)
    {
        __result = KotamonBalancerMod.ScaleCount(__result, KotamonBalancerMod.SpecialPointSpawnMultiplier.Value);
    }
}

[HarmonyPatch(typeof(UpgradeData), nameof(UpgradeData.GetValue))]
internal static class UpgradeDataGetValuePatch
{
    private static void Postfix(UpgradeData __instance, ref float __result)
    {
        __result *= KotamonBalancerMod.NonNegative(
            KotamonBalancerMod.GetUpgradeValueMultiplier(__instance.ParameterType));
    }
}

[HarmonyPatch(typeof(CardsSettings), nameof(CardsSettings.GetPrice))]
internal static class CardValuePatch
{
    private static void Postfix(ref int __result)
    {
        __result = KotamonBalancerMod.ScaleCount(__result, KotamonBalancerMod.CardValueMultiplier.Value);
    }
}

[HarmonyPatch(typeof(GameConfig), nameof(GameConfig.CommonCountInZone), MethodType.Getter)]
internal static class CommonItemsPerZonePatch
{
    private static void Postfix(ref int __result)
    {
        __result = KotamonBalancerMod.ScaleCount(
            __result,
            KotamonBalancerMod.CommonItemsPerZoneMultiplier.Value);
    }
}

[HarmonyPatch(typeof(CardsSettings.DirtyPartSettings), nameof(CardsSettings.DirtyPartSettings.NeedCount), MethodType.Getter)]
internal static class CardPartsRequiredPatch
{
    private static void Postfix(ref int __result)
    {
        __result = KotamonBalancerMod.ScaleCount(__result, KotamonBalancerMod.CardPartsRequiredMultiplier.Value);
    }
}

[HarmonyPatch(typeof(CardsSettings.JunkZoneCardSettings), nameof(CardsSettings.JunkZoneCardSettings.GetCount))]
internal static class JunkZoneCardCountPatch
{
    private static void Postfix(ref int __result)
    {
        __result = KotamonBalancerMod.ScaleCount(__result, KotamonBalancerMod.JunkZoneCardCountMultiplier.Value);
    }
}

[HarmonyPatch(typeof(CaseSettings), nameof(CaseSettings.GetSpawnChance))]
internal static class CaseSpawnChancePatch
{
    private static void Postfix(ref float __result)
    {
        __result = Math.Clamp(
            __result * KotamonBalancerMod.NonNegative(KotamonBalancerMod.CaseSpawnChanceMultiplier.Value),
            0f,
            100f);
    }
}

[HarmonyPatch(typeof(TapeSettings), nameof(TapeSettings.GetSpawnChance))]
internal static class TapeSpawnChancePatch
{
    private static void Postfix(ref float __result)
    {
        __result = Math.Clamp(
            __result * KotamonBalancerMod.NonNegative(KotamonBalancerMod.TapeSpawnChanceMultiplier.Value),
            0f,
            100f);
    }
}

[HarmonyPatch]
internal static class CardBoxAnimationDurationPatch
{
    private static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        yield return AccessTools.PropertyGetter(typeof(CardBoxSettings), nameof(CardBoxSettings.MoveToOpenPointDuration));
        yield return AccessTools.PropertyGetter(typeof(CardBoxSettings), nameof(CardBoxSettings.OpenDuration));
        yield return AccessTools.PropertyGetter(typeof(CardBoxSettings), nameof(CardBoxSettings.ShowUiDelay));
    }

    private static void Postfix(ref float __result)
    {
        __result *= MathF.Max(
            0.05f,
            KotamonBalancerMod.NonNegative(KotamonBalancerMod.CardBoxAnimationDurationMultiplier.Value));
    }
}
