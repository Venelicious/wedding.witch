using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
namespace WeddingWitchArchipelago;
[HarmonyPatch(typeof(SelectModeBehaviour), nameof(SelectModeBehaviour.SetSlotColor))]
public static class ExpUnlockDisplayPatch
{
    [HarmonyPostfix]
    static void AfterColors(SelectModeBehaviour __instance) => Refresh(__instance);
    static void Mark(Image slot, string type) {
        if (slot == null) return;
        var icon = slot.GetComponent<ApExpIcon>();
        if (icon == null && ApState.Active) icon = slot.gameObject.AddComponent<ApExpIcon>();
        if (icon != null) icon.Refresh(slot, ApState.Active, ApState.ExpUnlocked(type));
    }
    public static void Refresh(SelectModeBehaviour screen) {
        Mark(screen.BigBreastSlot,"BigBreast");
        Mark(screen.SmallBreastSlot,"SmallBreast");
        Mark(screen.CorruptionSlot,"Corruption");
        Mark(screen.BeastSlot,"Beast");
        Mark(screen.Muscle,"Muscle");
        Mark(screen.HipSlot,"Hip");
    }
    public static void RefreshOpenMenu() {
        foreach (var screen in Resources.FindObjectsOfTypeAll<SelectModeBehaviour>())
            if (screen.gameObject.scene.IsValid()) screen.SetSlotColor();
    }
}
public sealed class ApExpIcon : MonoBehaviour
{
    Sprite original;
    bool captured, preserveAspect;

    public void Refresh(Image image, bool active, bool unlocked) {
        if (!captured) {
            original = image.sprite;
            preserveAspect = image.preserveAspect;
            captured = true;
        }
        bool locked = active && !unlocked;
        var charm = locked ? ApSprite.GetAchievement() : null;
        image.sprite = charm != null ? charm : original;
        image.preserveAspect = locked || preserveAspect;
        // AP unlocks determine availability in this menu. The native greying
        // uses completed transformations instead, which is a separate counter.
        if (active) image.color = Color.white;
    }
}
