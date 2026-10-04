using System;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace WeddingWitchArchipelago;

[HarmonyPatch]
public static class DeathLinkPatch
{
    static BattleUIManager remoteGameOver;
    static bool remoteGameOverCommitted;

    public static void Reset() { remoteGameOver = null; remoteGameOverCommitted = false; }

    [HarmonyPatch(typeof(BattleUIManager), nameof(BattleUIManager.GameOver))]
    [HarmonyPrefix]
    static bool GameOver(BattleUIManager __instance)
    {
        if (ReferenceEquals(remoteGameOver, __instance)) {
            // The animation's later callback must not count this forced loss twice.
            if (remoteGameOverCommitted) return false;
            remoteGameOverCommitted = true;
        }
        var player = PlayableCharacter.instance;
        if (ApState.Active && ApState.Progress.RunActive && player != null && !player.isAlive)
            Plugin.Client?.SendDeathLink(ApState.Progress.Run);
        return true;
    }

    // Called only from Plugin.Update, never from a socket callback.
    public static void Tick()
    {
        if (!ApState.Active || !ApState.Progress.RunActive || SceneManager.GetActiveScene().name != "Adventure") return;
        var battle = BattleUIManager.instance;
        var player = PlayableCharacter.instance;
        if (battle == null || player == null || player.hitPoint == null) return;
        if (Plugin.Client == null || !Plugin.Client.TryTakeDeathLink(ApState.Progress.Run, out var death)) return;

        Plugin.Logger.LogInfo("[DeathLink] " + (string.IsNullOrWhiteSpace(death.Cause) ? death.Source + " died." : death.Cause));
        remoteGameOver = battle; remoteGameOverCommitted = false;
        // DeathLink ends the run, including during pause/level-up screens.
        // Native revives still apply to normal damage, but not remote deaths.
        player.resurrectionCount = 0;
        player.StopAllCoroutines();
        player.hitPoint.invincible = false;
        player.hitPoint.DecreaseHP(Math.Max(1, player.hitPoint.CurrentHitPoint));
        if (player.hPBar != null) player.hPBar.UpdateHP();
        Hide(battle.pauseCanvas); Hide(battle.levelUpCanvas); Hide(battle.FlowerCanvas); Hide(battle.roadMapCanvas);
        if (PotionCanvas.instance != null) Hide(PotionCanvas.instance.gameObject);
        if (DialogViewer.instance != null) Hide(DialogViewer.instance.pannel);
        battle.GameOver();
    }

    static void Hide(UnityEngine.GameObject panel) { if (panel != null) panel.SetActive(false); }
}
