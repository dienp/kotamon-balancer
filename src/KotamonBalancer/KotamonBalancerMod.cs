using HarmonyLib;
using Il2CppProject.Code.Gameplay.Configs;
using Il2CppProject.Code.Gameplay.Controllers;
using MelonLoader;

[assembly: MelonInfo(typeof(KotamonBalancer.KotamonBalancerMod), "Kotamon Balancer", "1.2.1", "ptd")]
[assembly: MelonGame("KotaMota Games", "Kotamon")]

namespace KotamonBalancer;

public sealed class KotamonBalancerMod : MelonMod
{
    private static readonly object DiagnosticLock = new();
    private static readonly HashSet<int> AppliedGameConfigs = new();
    private static readonly HashSet<string> LoggedAdjustments = new(StringComparer.Ordinal);

    internal static MelonPreferences_Entry<float> UpgradePriceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> JunkValueMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> EnergyPriceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> EnergyRegenMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> SmallEnergyRecoveryMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CardBoxPriceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CardPartSpawnIntervalMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CollectiblePileChanceMultiplier { get; private set; } = null!;
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
        LogConfiguredMultipliers();
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

    internal static void LogAdjustmentOnce(string setting, float original, float multiplier, float adjusted)
    {
        lock (DiagnosticLock)
        {
            if (!LoggedAdjustments.Add(setting))
                return;
        }

        MelonLogger.Msg(
            $"Applied {setting}: original={original:0.###}, multiplier={multiplier:0.###}, result={adjusted:0.###}");
    }

    internal static void LogAdjustmentOnce(string setting, int original, float multiplier, int adjusted) =>
        LogAdjustmentOnce(setting, (float)original, multiplier, adjusted);

    private static object? ReadObject(object target, string propertyName) =>
        AccessTools.Property(target.GetType(), propertyName)?.GetValue(target);

    private static T ReadValue<T>(object target, string propertyName)
    {
        var property = AccessTools.Property(target.GetType(), propertyName)
            ?? throw new MissingMemberException(target.GetType().FullName, propertyName);
        return (T)property.GetValue(target)!;
    }

    private static void WriteValue<T>(object target, string propertyName, T value)
    {
        var property = AccessTools.Property(target.GetType(), propertyName)
            ?? throw new MissingMemberException(target.GetType().FullName, propertyName);
        property.SetValue(target, value);
    }

    internal static void ApplyRuntimeConfig(object config)
    {
        var configId = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(config);
        lock (DiagnosticLock)
        {
            if (!AppliedGameConfigs.Add(configId))
                return;
        }

        try
        {
            var priceConfig = ReadObject(config, "_priceConfig");
            if (priceConfig is not null)
            {
                var originalJunkValue = ReadValue<float>(priceConfig, "_baseJunkPrice");
                var junkMultiplier = NonNegative(JunkValueMultiplier.Value);
                var adjustedJunkValue = originalJunkValue * junkMultiplier;
                WriteValue(priceConfig, "_baseJunkPrice", adjustedJunkValue);
                LogAdjustmentOnce("JunkValueMultiplier", originalJunkValue, junkMultiplier, adjustedJunkValue);

                var originalEnergyPrice = ReadValue<float>(priceConfig, "_energyPrice");
                var energyPriceMultiplier = NonNegative(EnergyPriceMultiplier.Value);
                var adjustedEnergyPrice = originalEnergyPrice * energyPriceMultiplier;
                WriteValue(priceConfig, "_energyPrice", adjustedEnergyPrice);
                LogAdjustmentOnce("EnergyPriceMultiplier", originalEnergyPrice, energyPriceMultiplier, adjustedEnergyPrice);
            }

            var upgradeConfig = ReadObject(config, "_upgradeConfig");
            if (upgradeConfig is not null)
            {
                var originalRegen = ReadValue<float>(upgradeConfig, "_regenPercent");
                var regenMultiplier = NonNegative(EnergyRegenMultiplier.Value);
                var adjustedRegen = Math.Clamp(originalRegen * regenMultiplier, 0f, 100f);
                WriteValue(upgradeConfig, "_regenPercent", adjustedRegen);
                LogAdjustmentOnce("EnergyRegenMultiplier", originalRegen, regenMultiplier, adjustedRegen);

                var originalRecovery = ReadValue<float>(upgradeConfig, "_smallEnergyPercent");
                var recoveryMultiplier = NonNegative(SmallEnergyRecoveryMultiplier.Value);
                var adjustedRecovery = Math.Clamp(originalRecovery * recoveryMultiplier, 0f, 100f);
                WriteValue(upgradeConfig, "_smallEnergyPercent", adjustedRecovery);
                LogAdjustmentOnce("SmallEnergyRecoveryMultiplier", originalRecovery, recoveryMultiplier, adjustedRecovery);
            }

            var cardBoxSettings = ReadObject(config, "_cardBoxSettings");
            if (cardBoxSettings is not null)
            {
                var originalPrice = ReadValue<int>(cardBoxSettings, "_price");
                var priceMultiplier = NonNegative(CardBoxPriceMultiplier.Value);
                var adjustedPrice = ScaleCount(originalPrice, priceMultiplier);
                WriteValue(cardBoxSettings, "_price", adjustedPrice);
                LogAdjustmentOnce("CardBoxPriceMultiplier", originalPrice, priceMultiplier, adjustedPrice);

                var durationMultiplier = MathF.Max(0.05f, NonNegative(CardBoxAnimationDurationMultiplier.Value));
                ApplyFloatProperty(cardBoxSettings, "_moveToOpenPointDuration", "CardBoxMoveDurationMultiplier", durationMultiplier);
                ApplyFloatProperty(cardBoxSettings, "_openDuration", "CardBoxOpenDurationMultiplier", durationMultiplier);
                ApplyFloatProperty(cardBoxSettings, "_showUiDelay", "CardBoxUiDelayMultiplier", durationMultiplier);
            }

            var cardsSettings = ReadObject(config, "_cardsSettings");
            var dirtyPartSettings = cardsSettings is null ? null : ReadObject(cardsSettings, "_dirtyPartSettings");
            if (dirtyPartSettings is not null)
            {
                var originalInterval = ReadValue<int>(dirtyPartSettings, "_pickupCountToSpawn");
                var intervalMultiplier = NonNegative(CardPartSpawnIntervalMultiplier.Value);
                var adjustedInterval = ScaleCount(originalInterval, intervalMultiplier);
                WriteValue(dirtyPartSettings, "_pickupCountToSpawn", adjustedInterval);
                LogAdjustmentOnce("CardPartSpawnIntervalMultiplier", originalInterval, intervalMultiplier, adjustedInterval);
            }

            var collectibleSettings = ReadObject(config, "_collectibleSettings");
            if (collectibleSettings is not null)
            {
                var originalChance = ReadValue<float>(collectibleSettings, "_pileSpawnChanceOnZoneOpen");
                var chanceMultiplier = NonNegative(CollectiblePileChanceMultiplier.Value);
                var adjustedChance = Math.Clamp(originalChance * chanceMultiplier, 0f, 100f);
                WriteValue(collectibleSettings, "_pileSpawnChanceOnZoneOpen", adjustedChance);
                LogAdjustmentOnce("CollectiblePileChanceMultiplier", originalChance, chanceMultiplier, adjustedChance);
            }

            var originalCommonCount = ReadValue<int>(config, "_commonCountInZone");
            var commonCountMultiplier = NonNegative(CommonItemsPerZoneMultiplier.Value);
            var adjustedCommonCount = ScaleCount(originalCommonCount, commonCountMultiplier);
            WriteValue(config, "_commonCountInZone", adjustedCommonCount);
            LogAdjustmentOnce("CommonItemsPerZoneMultiplier", originalCommonCount, commonCountMultiplier, adjustedCommonCount);
        }
        catch (Exception exception)
        {
            lock (DiagnosticLock)
                AppliedGameConfigs.Remove(configId);
            MelonLogger.Error($"Failed to apply runtime configuration: {exception}");
        }
    }

    private static void ApplyFloatProperty(object target, string propertyName, string setting, float multiplier)
    {
        var original = ReadValue<float>(target, propertyName);
        var adjusted = original * multiplier;
        WriteValue(target, propertyName, adjusted);
        LogAdjustmentOnce(setting, original, multiplier, adjusted);
    }

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

    private void LogConfiguredMultipliers()
    {
        var settings = new (string Name, MelonPreferences_Entry<float> Entry)[]
        {
            (nameof(UpgradePriceMultiplier), UpgradePriceMultiplier),
            (nameof(JunkValueMultiplier), JunkValueMultiplier),
            (nameof(EnergyPriceMultiplier), EnergyPriceMultiplier),
            (nameof(EnergyRegenMultiplier), EnergyRegenMultiplier),
            (nameof(SmallEnergyRecoveryMultiplier), SmallEnergyRecoveryMultiplier),
            (nameof(CardBoxPriceMultiplier), CardBoxPriceMultiplier),
            (nameof(CardPartSpawnIntervalMultiplier), CardPartSpawnIntervalMultiplier),
            (nameof(CollectiblePileChanceMultiplier), CollectiblePileChanceMultiplier),
            (nameof(EnergyCapacityMultiplier), EnergyCapacityMultiplier),
            (nameof(BagCapacityMultiplier), BagCapacityMultiplier),
            (nameof(StockLevelMultiplier), StockLevelMultiplier),
            (nameof(PickupRadiusMultiplier), PickupRadiusMultiplier),
            (nameof(DrinkCapacityMultiplier), DrinkCapacityMultiplier),
            (nameof(PowerMultiplier), PowerMultiplier),
            (nameof(MagnetRangeMultiplier), MagnetRangeMultiplier),
            (nameof(BagFullRewardMultiplier), BagFullRewardMultiplier),
            (nameof(MagnetPowerMultiplier), MagnetPowerMultiplier),
            (nameof(CardValueMultiplier), CardValueMultiplier),
            (nameof(CommonItemsPerZoneMultiplier), CommonItemsPerZoneMultiplier),
            (nameof(JunkZoneCardCountMultiplier), JunkZoneCardCountMultiplier),
            (nameof(CaseSpawnChanceMultiplier), CaseSpawnChanceMultiplier),
            (nameof(TapeSpawnChanceMultiplier), TapeSpawnChanceMultiplier),
            (nameof(CardBoxAnimationDurationMultiplier), CardBoxAnimationDurationMultiplier)
        };

        foreach (var setting in settings)
            LoggerInstance.Msg($"Configured {setting.Name}={setting.Entry.Value:0.###}");
    }
}

[HarmonyPatch(typeof(GameConfig), "OnEnable")]
internal static class GameConfigOnEnablePatch
{
    private static void Postfix(object __instance)
    {
        KotamonBalancerMod.ApplyRuntimeConfig(__instance);
    }
}

[HarmonyPatch(typeof(UpgradeData), nameof(UpgradeData.GetPrice))]
internal static class UpgradeDataGetPricePatch
{
    private static void Postfix(ref float __result)
    {
        var original = __result;
        var multiplier = KotamonBalancerMod.NonNegative(KotamonBalancerMod.UpgradePriceMultiplier.Value);
        __result *= multiplier;
        KotamonBalancerMod.LogAdjustmentOnce(nameof(KotamonBalancerMod.UpgradePriceMultiplier), original, multiplier, __result);
    }
}

[HarmonyPatch(typeof(UpgradeData), nameof(UpgradeData.GetValue))]
internal static class UpgradeDataGetValuePatch
{
    private static void Postfix(UpgradeData __instance, ref float __result)
    {
        var original = __result;
        var multiplier = KotamonBalancerMod.NonNegative(
            KotamonBalancerMod.GetUpgradeValueMultiplier(__instance.ParameterType));
        __result *= multiplier;
        KotamonBalancerMod.LogAdjustmentOnce($"UpgradeValue.{__instance.ParameterType}", original, multiplier, __result);
    }
}

[HarmonyPatch(typeof(CardsSettings), nameof(CardsSettings.GetPrice))]
internal static class CardValuePatch
{
    private static void Postfix(ref int __result)
    {
        var original = __result;
        var multiplier = KotamonBalancerMod.NonNegative(KotamonBalancerMod.CardValueMultiplier.Value);
        __result = KotamonBalancerMod.ScaleCount(original, multiplier);
        KotamonBalancerMod.LogAdjustmentOnce(nameof(KotamonBalancerMod.CardValueMultiplier), original, multiplier, __result);
    }
}

[HarmonyPatch(typeof(CardsSettings.JunkZoneCardSettings), nameof(CardsSettings.JunkZoneCardSettings.GetCount))]
internal static class JunkZoneCardCountPatch
{
    private static void Postfix(ref int __result)
    {
        var original = __result;
        var multiplier = KotamonBalancerMod.NonNegative(KotamonBalancerMod.JunkZoneCardCountMultiplier.Value);
        __result = KotamonBalancerMod.ScaleCount(original, multiplier);
        KotamonBalancerMod.LogAdjustmentOnce(nameof(KotamonBalancerMod.JunkZoneCardCountMultiplier), original, multiplier, __result);
    }
}

[HarmonyPatch(typeof(CaseSettings), nameof(CaseSettings.GetSpawnChance))]
internal static class CaseSpawnChancePatch
{
    private static void Postfix(ref float __result)
    {
        var original = __result;
        var multiplier = KotamonBalancerMod.NonNegative(KotamonBalancerMod.CaseSpawnChanceMultiplier.Value);
        __result = Math.Clamp(original * multiplier, 0f, 100f);
        KotamonBalancerMod.LogAdjustmentOnce(nameof(KotamonBalancerMod.CaseSpawnChanceMultiplier), original, multiplier, __result);
    }
}

[HarmonyPatch(typeof(TapeSettings), nameof(TapeSettings.GetSpawnChance))]
internal static class TapeSpawnChancePatch
{
    private static void Postfix(ref float __result)
    {
        var original = __result;
        var multiplier = KotamonBalancerMod.NonNegative(KotamonBalancerMod.TapeSpawnChanceMultiplier.Value);
        __result = Math.Clamp(original * multiplier, 0f, 100f);
        KotamonBalancerMod.LogAdjustmentOnce(nameof(KotamonBalancerMod.TapeSpawnChanceMultiplier), original, multiplier, __result);
    }
}
