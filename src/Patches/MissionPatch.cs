using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
namespace WeddingWitchArchipelago;
[HarmonyPatch]
public static class MissionPatch
{
    static readonly HashSet<int> Picked = new HashSet<int>();
    [HarmonyPatch(typeof(RoadMapCanvas), "Start")]
    [HarmonyPostfix]
    static void StartRun() { Picked.Clear(); ApState.BeginRun(BattleUIManager.difficulty); }
    [HarmonyPatch(typeof(MissionManager), nameof(MissionManager.MissionComplete))]
    [HarmonyPostfix]
    static void Cleared(ref IEnumerator __result) {
        string run = ApState.Progress.Run;
        int floor = RoadMapCanvas.currentFloor;
        var d = BattleUIManager.difficulty;
        __result = AfterStart(__result, run,d,floor);
    }
    static IEnumerator AfterStart(IEnumerator original,string run,Difficulty d,int floor) {
        bool first = true;
        while (original.MoveNext()) {
            if (first) {
                first = false;
                if (ApState.Active && ApState.Progress.RunActive && run == ApState.Progress.Run)
                    ApState.Check(Locations.Map(d,floor));
            }
            yield return original.Current;
        }
    }
    [HarmonyPatch(typeof(FieldItem_Flower), nameof(FieldItem_Flower.OnSpawned))]
    [HarmonyPostfix]
    static void Spawn(FieldItem_Flower __instance) => Picked.Remove(__instance.GetInstanceID());
    [HarmonyPatch(typeof(FieldItem_Flower), nameof(FieldItem_Flower.ActiveItem))]
    [HarmonyPostfix]
    static void Pickup(FieldItem_Flower __instance) { if (Picked.Add(__instance.GetInstanceID())) ApState.Flower(); }
    [HarmonyPatch(typeof(PotionManager), nameof(PotionManager.FormChange))]
    [HarmonyPostfix]
    static void Form() { if (ApState.Active && ApState.Progress.RunActive) ApState.Check(Locations.Form(PotionManager.bodyState)); }
    [HarmonyPatch(typeof(BattleUIManager), nameof(BattleUIManager.Victory))]
    [HarmonyPrefix]
    static void Victory() => ApState.Win(PotionManager.bodyState);
    [HarmonyPatch(typeof(BedSceneManager), "Start")]
    [HarmonyPostfix]
    static void Ending() => ApState.EndingShown();
    [HarmonyPatch(typeof(BattleUIManager), nameof(BattleUIManager.GameOver))]
    [HarmonyPostfix]
    static void Loss() { if (!ApState.Active) return; Plugin.Client?.EndDeathLinkRun(); ApState.Progress.RunActive=false; ApState.Progress.PendingEndingRun=""; ApState.Persist(); }
}
