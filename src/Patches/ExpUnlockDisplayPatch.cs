using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
namespace WeddingWitchArchipelago;
[HarmonyPatch(typeof(SelectModeBehaviour), nameof(SelectModeBehaviour.SetSlotColor))]
public static class ExpUnlockDisplayPatch
{
    [HarmonyPostfix]
    static void AfterColors(SelectModeBehaviour __instance) => Refresh(__instance);
    static void Mark(SelectModeBehaviour screen, Image slot, string node, string type) {
        if (slot == null) return;
        var old = slot.GetComponent<ApExpOutline>();
        if (old != null) old.enabled = false;
        var tile = screen.transform.Find("Panel/" + node);
        if (tile == null) return;
        var background = tile.GetComponent<Image>();
        if (background == null) {
            var frame = tile.Find("Frame");
            if (frame != null) background = frame.GetComponent<Image>();
        }
        if (background == null) return;
        var marker = background.GetComponent<ApExpOutline>();
        if (marker == null && ApState.Active) marker = background.gameObject.AddComponent<ApExpOutline>();
        if (marker == null) return;
        marker.effectColor = new Color(0.25f, 1f, 0.45f, 1f);
        marker.effectDistance = new Vector2(5f,-5f);
        marker.useGraphicAlpha = false;
        marker.enabled = ApState.Active && ApState.ExpUnlocked(type);
    }
    public static void Refresh(SelectModeBehaviour screen) {
        Mark(screen,screen.BigBreastSlot,"SlotBigB","BigBreast");
        Mark(screen,screen.SmallBreastSlot,"SlotSmallB","SmallBreast");
        Mark(screen,screen.CorruptionSlot,"SlotCorruption","Corruption");
        Mark(screen,screen.BeastSlot,"SlotBeast","Beast");
        Mark(screen,screen.Muscle,"SlotMuscle","Muscle");
        Mark(screen,screen.HipSlot,"SlotHip","Hip");
    }
    public static void RefreshOpenMenu() {
        foreach (var screen in Resources.FindObjectsOfTypeAll<SelectModeBehaviour>())
            if (screen.gameObject.scene.IsValid()) Refresh(screen);
    }
}
public class ApExpOutline : Outline { }
