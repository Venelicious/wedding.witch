using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace WeddingWitchArchipelago;
[HarmonyPatch]
public static class SkillUnlockPatch
{
    static bool scanned;
    static readonly HashSet<Enchant> Skills = new HashSet<Enchant>();
    // Old seeds keep their generated AP skill ranks. Schema 4 uses vanilla level-ups.
    static bool UsesApSkillRanks => ApState.Active && !ApState.Settings.SkillsFromLevelUps;
    [HarmonyPatch(typeof(EnchantManager), "Awake")]
    [HarmonyPrefix]
    public static void Forget() { Skills.Clear(); scanned = false; }
    [HarmonyPatch(typeof(EnchantManager), nameof(EnchantManager.AddpossibleEnchant))]
    [HarmonyPrefix]
    static bool CanAdd(Enchant item) {
        if (!UsesApSkillRanks || item == null || !SkillCatalog.ByClass.ContainsKey(item.GetType().Name)) return true;
        Skills.Add(item);
        return false;
    }
    [HarmonyPatch(typeof(EnchantManager), nameof(EnchantManager.SetFirstMagic))]
    [HarmonyPostfix]
    static void ExpandStartingMagicPool(EnchantManager __instance) {
        if (!UsesApSkillRanks) return;
        // Vanilla enables only two starter spells and fills the other choices
        // with standard skills. AP grants those skills directly, so keep all
        // native starter spells available for the normal random selection.
        // Their Start/AddCheck still enforces native potion conditions.
        int enabled = 0;
        foreach (var item in __instance.startEnchantList) {
            if (item == null || !(item is Enchant_Magic)) continue;
            item.gameObject.SetActive(true);
            enabled++;
        }
        Plugin.Logger.LogInfo("[ap] Enabled " + enabled + " native starter magic candidates");
    }
    public static void Refresh() {
        // Reconciliation happens after all native Start methods have initialized stats.
    }
    public static void Tick() {
        if (!UsesApSkillRanks || SceneManager.GetActiveScene().name != "Adventure" ||
            Time.timeSinceLevelLoad < 0.2f || EnchantManager.instance == null ||
            GlobalStat.instance == null || PlayableCharacter.instance == null || PlayerMagnet.instance == null) return;
        if (!scanned) {
            foreach (var item in Resources.FindObjectsOfTypeAll<Enchant>())
                if (item != null && item.gameObject.scene.IsValid() && SkillCatalog.ByClass.ContainsKey(item.GetType().Name)) Skills.Add(item);
            scanned = true;
        }
        var manager = EnchantManager.instance;
        int budget = 2;
        foreach (var item in new List<Enchant>(Skills)) {
            if (item == null) { Skills.Remove(item); continue; }
            if (!item.gameObject.scene.IsValid() || !item.isActiveAndEnabled || item.enchant == null) continue;
            manager.possibleEnchant.Remove(item);
            manager.newEnchantList.Remove(item);
            int owned = ApState.SkillLevel(item.GetType().Name);
            if (item.currentEnchantCount >= owned || budget <= 0) continue;
            item.DoEnchant(); // Native effect, ownership list and UI notifications.
            manager.possibleEnchant.Remove(item);
            budget--;
            Plugin.Logger.LogInfo("[ap] Applied owned " + item.GetType().Name + " level " + item.currentEnchantCount);
        }
    }
    [HarmonyPatch(typeof(EnchantManager), nameof(EnchantManager.GetRandomEnchantList))]
    [HarmonyPrefix]
    static void BeforeChoices(EnchantManager __instance) {
        if (!UsesApSkillRanks) return;
        __instance.possibleEnchant.RemoveAll(item => item != null && SkillCatalog.ByClass.ContainsKey(item.GetType().Name));
        __instance.FillEnchantList();
    }
}
