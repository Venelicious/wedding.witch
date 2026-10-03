using System;
using HarmonyLib;
using UnityEngine;
namespace WeddingWitchArchipelago;

[HarmonyPatch]
public static class AchievementPatch
{
    const string Store = "achievements";
    static int readyEpoch = -1;
    static float nextPoll;
    static bool Enabled => ApState.Active && ApState.Settings.AchievementChecks.Count > 0;
    static bool Ready => Enabled && readyEpoch == ApState.Epoch;

    public static void AfterReload() {
        // A connection can arrive after Plugin.Update. Do not inspect cached
        // vanilla counters until ReloadAll has loaded this seed's counters.
        readyEpoch = ApState.Epoch;
        nextPoll = 0;
        AchievementManager.instance?.ReloadStates();
    }

    public static void Tick() {
        if (!Ready || Time.realtimeSinceStartup < nextPoll) return;
        nextPoll = Time.realtimeSinceStartup + 0.5f;
        var manager = AchievementManager.instance;
        if (manager == null || manager.achievements == null) return;
        foreach (var achievement in manager.achievements) {
            if (!ApState.Settings.AchievementChecks.Contains(achievement.Id) || !achievement.UnlockCondition()) continue;
            if (achievement.State == AchivementState.Incompleted) achievement.Complete();
            else Check(achievement);
        }
    }

    [HarmonyPatch(typeof(Achievement), nameof(Achievement.GetState))]
    [HarmonyPrefix]
    static bool LoadState(Achievement __instance) {
        if (!Enabled) return true;
        __instance.State = ApProfile.TryGet(Store, __instance.Id, out var saved)
            && Enum.TryParse(saved, out AchivementState state) && Enum.IsDefined(typeof(AchivementState), state)
            ? state : AchivementState.Incompleted;
        if (__instance.State != AchivementState.Incompleted) UnlockSteam(__instance.Id);
        return false;
    }

    [HarmonyPatch(typeof(Achievement), nameof(Achievement.Complete))]
    [HarmonyPrefix]
    static bool Complete(Achievement __instance) {
        if (!Enabled) return true;
        if (!Ready || !__instance.UnlockCondition()) return false;
        __instance.State = AchivementState.Completed;
        ApProfile.Set(Store, __instance.Id, __instance.State.ToString());
        UnlockSteam(__instance.Id);
        Check(__instance);
        // Native Complete writes the shared achievement file and uploads it to
        // Steam Cloud. AP state belongs in the seed profile instead.
        return false;
    }

    [HarmonyPatch(typeof(Achievement), nameof(Achievement.ClaimedReward))]
    [HarmonyPrefix]
    static bool Claim(Achievement __instance) {
        if (!Enabled) return true;
        if (!Ready || __instance.State != AchivementState.Completed) return false;
        __instance.State = AchivementState.Claimed;
        ApProfile.Set(Store, __instance.Id, __instance.State.ToString());
        Check(__instance);
        return false;
    }

    static void Check(Achievement achievement) {
        if (Ready && ApState.Settings.AchievementChecks.Contains(achievement.Id) && achievement.UnlockCondition())
            ApState.Check(AchievementCatalog.Location(achievement.Id));
    }
    static void UnlockSteam(string id) {
        try {
            if (Steamworks.SteamClient.IsValid) new Steamworks.Data.Achievement(id).Trigger();
        } catch (Exception ex) {
            Plugin.Logger.LogWarning("Steam achievement unlock failed for " + id + ": " + ex.Message);
        }
    }
}
