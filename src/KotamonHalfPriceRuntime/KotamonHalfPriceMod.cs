using HarmonyLib;
using Il2CppProject.Code.Gameplay.Configs;
using MelonLoader;

[assembly: MelonInfo(typeof(KotamonHalfPriceRuntime.KotamonHalfPriceMod), "Kotamon Half Price Upgrades", "1.0.0", "Codex")]
[assembly: MelonGame("KotaMota Games", "Kotamon")]

namespace KotamonHalfPriceRuntime;

public sealed class KotamonHalfPriceMod : MelonMod
{
    internal const float PriceMultiplier = 0.5f;

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Runtime upgrade-price patch active: prices are 50% of normal.");
    }
}
[HarmonyPatch(typeof(UpgradeData), nameof(UpgradeData.GetPrice))]
internal static class UpgradeDataGetPricePatch
{
    private static void Postfix(ref float __result)
    {
        __result *= KotamonHalfPriceMod.PriceMultiplier;
    }
}
