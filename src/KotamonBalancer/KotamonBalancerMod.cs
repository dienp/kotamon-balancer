using HarmonyLib;
using Il2CppProject.Code.Gameplay.Configs;
using MelonLoader;

[assembly: MelonInfo(typeof(KotamonBalancer.KotamonBalancerMod), "Kotamon Balancer", "1.2.8", "ptd")]
[assembly: MelonGame("KotaMota Games", "Kotamon")]

namespace KotamonBalancer;

public sealed class KotamonBalancerMod : MelonMod
{
    private static readonly object DiagnosticLock = new();
    private static readonly HashSet<int> AppliedGameConfigs = new();
    private static readonly Dictionary<int, float> JunkPickupOriginalDurations = new();
    private static readonly HashSet<string> LoggedAdjustments = new(StringComparer.Ordinal);
    private static bool SprintToggled;

    internal static MelonPreferences_Entry<float> UpgradePriceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> JunkValueMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> EnergyPriceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> EnergyRegenMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> SmallEnergyRecoveryMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CardBoxPriceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CardPartSpawnIntervalMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> CollectiblePileChanceMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> JunkPickupSpeedMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<float> JunkPickupDelayMultiplier { get; private set; } = null!;
    internal static MelonPreferences_Entry<bool> ToggleSprint { get; private set; } = null!;

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
            "Beer energy recovery multiplier", "1.60 raises the energy restored by a beer can from 25% to 40%.");
        CardBoxPriceMultiplier = CreateEntry(category, "CardBoxPriceMultiplier", 0.6f,
            "Card box price multiplier", "0.60 reduces the card-box price from 50,000 to 30,000.");
        CardPartSpawnIntervalMultiplier = CreateEntry(category, "CardPartSpawnIntervalMultiplier", 0.6f,
            "Card-part spawn interval multiplier", "0.60 changes the interval from every 50 pickups to every 30.");
        CollectiblePileChanceMultiplier = CreateEntry(category, "CollectiblePileChanceMultiplier", 1.5f,
            "Collectible pile chance multiplier", "1.50 raises the zone-open pile chance from 30% to 45%.");
        JunkPickupSpeedMultiplier = CreateEntry(category, "JunkPickupSpeedMultiplier", 2f,
            "Junk pickup speed multiplier", "2.00 makes junk fly into your hand or bag twice as fast.");
        JunkPickupDelayMultiplier = CreateEntry(category, "JunkPickupDelayMultiplier", 0.5f,
            "Junk pickup delay multiplier", "0.50 halves the wait before another common-junk pickup can start.");
        ToggleSprint = category.CreateEntry<bool>("ToggleSprint", true,
            "Toggle sprint", "When enabled, press Sprint once to run and press it again to stop.", false, false, null!);

        MelonPreferences.Save();
        LoggerInstance.Msg("Kotamon Balancer preset active. Settings are in UserData/MelonPreferences.cfg.");
        LogConfiguredMultipliers();
        LoggerInstance.Msg($"Configured {nameof(ToggleSprint)}={ToggleSprint.Value}");
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

    private static void LogMessageOnce(string key, string message)
    {
        lock (DiagnosticLock)
        {
            if (!LoggedAdjustments.Add(key))
                return;
        }

        MelonLogger.Msg(message);
    }

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

        }
        catch (Exception exception)
        {
            lock (DiagnosticLock)
                AppliedGameConfigs.Remove(configId);
            MelonLogger.Error($"Failed to apply runtime configuration: {exception}");
        }
    }

    internal static void ApplyJunkPickupSpeed(object pickup)
    {
        try
        {
            var pickupId = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(pickup);
            float originalDuration;
            lock (DiagnosticLock)
            {
                if (!JunkPickupOriginalDurations.TryGetValue(pickupId, out originalDuration))
                {
                    originalDuration = ReadValue<float>(pickup, "_duration");
                    JunkPickupOriginalDurations.Add(pickupId, originalDuration);
                }
            }

            var speedMultiplier = MathF.Max(0.1f, NonNegative(JunkPickupSpeedMultiplier.Value));
            var adjustedDuration = originalDuration / speedMultiplier;
            WriteValue(pickup, "_duration", adjustedDuration);
            LogAdjustmentOnce($"{nameof(JunkPickupSpeedMultiplier)}.MoveToHand", originalDuration, speedMultiplier, adjustedDuration);
        }
        catch (Exception exception)
        {
            MelonLogger.Error($"Failed to apply junk pickup speed: {exception}");
        }
    }

    internal static void ApplyJunkPickupDelay(object pickupController)
    {
        try
        {
            var parametersService = ReadObject(pickupController, "_parametersService")
                ?? throw new MissingMemberException(pickupController.GetType().FullName, "_parametersService");
            var parameterType = AccessTools.TypeByName("Il2CppProject.Code.Gameplay.Controllers.ParameterType")
                ?? throw new TypeLoadException("Could not find ParameterType.");
            var getUpgradeValue = AccessTools.Method(parametersService.GetType(), "GetUpgradeValue", new[] { parameterType })
                ?? throw new MissingMethodException(parametersService.GetType().FullName, "GetUpgradeValue");
            var powerLevel = Convert.ToSingle(
                getUpgradeValue.Invoke(parametersService, new[] { Enum.ToObject(parameterType, 11) }));
            if (powerLevel <= 0f)
                return;

            var originalDelay = 1f / powerLevel;
            var delayMultiplier = NonNegative(JunkPickupDelayMultiplier.Value);
            var adjustedDelay = originalDelay * delayMultiplier;
            var lastPickupTime = ReadValue<float>(pickupController, "_lastCommonPickupTime");
            WriteValue(
                pickupController,
                "_lastCommonPickupTime",
                lastPickupTime - (originalDelay - adjustedDelay));
            LogAdjustmentOnce(nameof(JunkPickupDelayMultiplier), originalDelay, delayMultiplier, adjustedDelay);
        }
        catch (Exception exception)
        {
            MelonLogger.Error($"Failed to apply junk pickup delay: {exception}");
        }
    }

    internal static bool HandleStartSprint(object movementInput)
    {
        if (!ToggleSprint.Value)
            return true;

        try
        {
            SprintToggled = !SprintToggled;
            WriteValue(movementInput, "_runHold", SprintToggled);
            MelonLogger.Msg($"Toggle sprint: {(SprintToggled ? "on" : "off")}");
            return false;
        }
        catch (Exception exception)
        {
            MelonLogger.Error($"Failed to toggle sprint: {exception}");
            return true;
        }
    }

    internal static bool ShouldRunOriginalEndSprint()
    {
        if (!ToggleSprint.Value)
            return true;

        LogMessageOnce("ToggleSprint.EndSprint", "Toggle sprint ignored the physical key release.");
        return false;
    }

    internal static bool ShouldRunOriginalUseRunInput()
    {
        if (!ToggleSprint.Value)
            return true;

        LogMessageOnce("ToggleSprint.UseRunInput", "Toggle sprint latch preserved when the game consumed run input.");
        return false;
    }

    internal static void EnforceSprintState(object movementInput)
    {
        if (!ToggleSprint.Value)
            return;

        try
        {
            WriteValue(movementInput, "_runHold", SprintToggled);
            LogMessageOnce("ToggleSprint.Update", "Toggle sprint state enforcement is active in movement update.");
        }
        catch (Exception exception)
        {
            LogMessageOnce("ToggleSprint.Update.Error", $"Failed to enforce toggle sprint state: {exception}");
        }
    }

    internal static void OverrideSprintInput(ref bool result)
    {
        if (!ToggleSprint.Value)
            return;

        result = SprintToggled;
        LogMessageOnce("ToggleSprint.IsSprintInput", "Toggle sprint is overriding InputService.IsSprintInput.");
    }

    internal static void ResetSprintToggle(object movementInput)
    {
        SprintToggled = false;
        if (!ToggleSprint.Value)
            return;

        try
        {
            WriteValue(movementInput, "_runHold", false);
            MelonLogger.Msg("Toggle sprint reset: off");
        }
        catch (Exception exception)
        {
            MelonLogger.Error($"Failed to reset toggle sprint state: {exception}");
        }
    }

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
            (nameof(JunkPickupSpeedMultiplier), JunkPickupSpeedMultiplier),
            (nameof(JunkPickupDelayMultiplier), JunkPickupDelayMultiplier)
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

[HarmonyPatch]
internal static class JunkPickupMoveToHandPatch
{
    private static System.Reflection.MethodBase TargetMethod() =>
        AccessTools.Method(
            AccessTools.TypeByName("Il2CppProject.Code.Gameplay.Interactions.Pickups.JunkPickup"),
            "MoveToHand");

    private static void Prefix(object __instance)
    {
        KotamonBalancerMod.ApplyJunkPickupSpeed(__instance);
    }
}

[HarmonyPatch]
internal static class MarkCommonPickupStartedPatch
{
    private static System.Reflection.MethodBase TargetMethod() =>
        AccessTools.Method(
            AccessTools.TypeByName("Il2CppProject.Code.Gameplay.Player.PlayerPickupController"),
            "MarkCommonPickupStarted");

    private static void Postfix(object __instance) =>
        KotamonBalancerMod.ApplyJunkPickupDelay(__instance);
}

[HarmonyPatch]
internal static class StartSprintPatch
{
    private static System.Reflection.MethodBase TargetMethod() =>
        AccessTools.Method(
            AccessTools.TypeByName("Il2CppProject.Code.Core.Player.Movement.SimulatorMovementInput"),
            "StartSprint");

    private static bool Prefix(object __instance) =>
        KotamonBalancerMod.HandleStartSprint(__instance);
}

[HarmonyPatch]
internal static class EndSprintPatch
{
    private static System.Reflection.MethodBase TargetMethod() =>
        AccessTools.Method(
            AccessTools.TypeByName("Il2CppProject.Code.Core.Player.Movement.SimulatorMovementInput"),
            "EndSprint");

    private static bool Prefix() =>
        KotamonBalancerMod.ShouldRunOriginalEndSprint();
}

[HarmonyPatch]
internal static class UseRunInputPatch
{
    private static System.Reflection.MethodBase TargetMethod() =>
        AccessTools.Method(
            AccessTools.TypeByName("Il2CppProject.Code.Core.Player.Movement.SimulatorMovementInput"),
            "UseRunInput");

    private static bool Prefix() =>
        KotamonBalancerMod.ShouldRunOriginalUseRunInput();
}

[HarmonyPatch]
internal static class MovementInputUpdatePatch
{
    private static System.Reflection.MethodBase TargetMethod() =>
        AccessTools.Method(
            AccessTools.TypeByName("Il2CppProject.Code.Core.Player.Movement.SimulatorMovementInput"),
            "Update");

    private static void Postfix(object __instance) =>
        KotamonBalancerMod.EnforceSprintState(__instance);
}

[HarmonyPatch]
internal static class IsSprintInputPatch
{
    private static System.Reflection.MethodBase TargetMethod() =>
        AccessTools.Method(
            AccessTools.TypeByName("Il2CppProject.Code.Core.Services.InputService"),
            "IsSprintInput");

    private static void Postfix(ref bool __result) =>
        KotamonBalancerMod.OverrideSprintInput(ref __result);
}

[HarmonyPatch]
internal static class ResetAllInputsPatch
{
    private static System.Reflection.MethodBase TargetMethod() =>
        AccessTools.Method(
            AccessTools.TypeByName("Il2CppProject.Code.Core.Player.Movement.SimulatorMovementInput"),
            "ResetAllInputs");

    private static void Postfix(object __instance) =>
        KotamonBalancerMod.ResetSprintToggle(__instance);
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
