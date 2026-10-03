using HarmonyLib;
using UnityEngine;

namespace WeddingWitchArchipelago;

[HarmonyPatch]
public static class AchievementDisplayPatch
{
    [HarmonyPatch(typeof(AchievementSlotBehaviour), nameof(AchievementSlotBehaviour.SetState))]
    [HarmonyPostfix]
    static void ShowReward(AchievementSlotBehaviour __instance) {
        if (__instance.myAchievement == null || __instance.rewardImage == null) return;
        var display = __instance.GetComponent<ApAchievementRewardDisplay>();
        if (display == null) display = __instance.gameObject.AddComponent<ApAchievementRewardDisplay>();
        display.Refresh(__instance);
    }

    [HarmonyPatch(typeof(AchievementSlotBehaviour), nameof(AchievementSlotBehaviour.ClaimReward))]
    [HarmonyPrefix]
    static bool ClaimReward(AchievementSlotBehaviour __instance) {
        var achievement = __instance.myAchievement;
        if (!ApState.Active || achievement == null || !ApState.Settings.AchievementChecks.Contains(achievement.Id)) return true;
        if (achievement.State == AchivementState.Completed) {
            achievement.ClaimedReward();
            __instance.SetState();
            AchivementCanvas.instance?.Resort();
        }
        // The native handler displays the old gold amount in a reward popup.
        // AP completion already sent the check; claiming only acknowledges it.
        return false;
    }

    public static void RefreshOpenRows() {
        foreach (var row in Resources.FindObjectsOfTypeAll<AchievementSlotBehaviour>()) {
            if (row.gameObject.scene.IsValid() && row.myAchievement != null) row.SetState();
        }
    }
}

public sealed class ApAchievementRewardDisplay : MonoBehaviour
{
    bool captured;
    Sprite coin;
    Color colour;
    bool preserveAspect, amountVisible;

    public void Refresh(AchievementSlotBehaviour row) {
        if (!captured) {
            coin = row.rewardImage.sprite;
            colour = row.rewardImage.color;
            preserveAspect = row.rewardImage.preserveAspect;
            amountVisible = row.reward != null && row.reward.gameObject.activeSelf;
            captured = true;
        }
        bool isCheck = ApState.Active && ApState.Settings.AchievementChecks.Contains(row.myAchievement.Id);
        var charm = isCheck ? ApSprite.GetAchievement() : null;
        row.rewardImage.sprite = isCheck && charm != null ? charm : coin;
        row.rewardImage.color = isCheck ? Color.white : colour;
        row.rewardImage.preserveAspect = isCheck || preserveAspect;
        // Retain the native icon rectangle. SetNativeSize would enlarge this
        // 1254px asset across the row and overlap the achievement text.
        if (row.reward != null) row.reward.gameObject.SetActive(!isCheck && amountVisible);
    }
}
