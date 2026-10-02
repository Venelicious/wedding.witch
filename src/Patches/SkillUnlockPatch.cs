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
    [HarmonyPatch(typeof(EnchantManager), "Awake")]
    [HarmonyPrefix]
    public static void Forget() { Skills.Clear(); scanned = false; }
    [HarmonyPatch(typeof(EnchantManager), nameof(EnchantManager.AddpossibleEnchant))]
    [HarmonyPrefix]
    static bool CanAdd(Enchant item) {
        if (!ApState.Active || item == null || !SkillCatalog.ByClass.ContainsKey(item.GetType().Name)) return true;
        Skills.Add(item);
        return false;
    }
    public static void Refresh() {
        // Reconciliation happens after all native Start methods have initialized stats.
    }
    public static void Tick() {
        if (!ApState.Active || SceneManager.GetActiveScene().name != "Adventure" ||
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
        if (!ApState.Active) return;
        __instance.possibleEnchant.RemoveAll(item => item != null && SkillCatalog.ByClass.ContainsKey(item.GetType().Name));
        __instance.FillEnchantList();
    }
}
