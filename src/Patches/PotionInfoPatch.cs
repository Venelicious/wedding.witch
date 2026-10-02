using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace WeddingWitchArchipelago;
// Let vanilla roll three different types first. Filtering its candidate list would
// hang its distinct-type while-loop when only one EXP type is unlocked.
[HarmonyPatch(typeof(PotionManager), nameof(PotionManager.GetRandomEnchantList), new[] { typeof(List<Potion>) })]
public static class PotionUnlockPatch
{
    [HarmonyPostfix]
    static void ReplaceLocked(PotionManager __instance) {
        if (!ApState.Active) return;
        var pool = __instance.beastPotion.Concat(__instance.bigBreastPotion).Concat(__instance.smallBreastPotion)
            .Concat(__instance.corruptionPotion).Concat(__instance.hipPotion).Concat(__instance.musclePotion).ToArray();
        for (int i=0; i<__instance.selectedPotionList.Count; i++) {
            var original = __instance.selectedPotionList[i];
            if (ApState.ExpUnlocked(original.potionKind.ToString())) continue;
            var eligible = pool.Where(p => p != null && p.Size == original.Size && ApState.ExpUnlocked(p.potionKind.ToString())).ToArray();
            if (eligible.Length == 0) throw new InvalidOperationException("No unlocked potion of required strength");
            var replacement = eligible[UnityEngine.Random.Range(0,eligible.Length)];
            __instance.selectedPotionList[i] = replacement;
            __instance.EnchantSlots[i].SetInfo(replacement);
        }
    }
}
