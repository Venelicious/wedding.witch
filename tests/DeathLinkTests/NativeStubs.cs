using System;
using System.Collections.Generic;
using System.Reflection;
using WeddingWitchArchipelago;
using WeddingWitchArchipelago.Archipelago;

namespace HarmonyLib {
    [AttributeUsage(AttributeTargets.All, AllowMultiple=true)]
    public sealed class HarmonyPatch : Attribute {
        public HarmonyPatch() { }
        public HarmonyPatch(Type type, string method) { }
    }
    public sealed class HarmonyPrefix : Attribute { }
}
namespace UnityEngine {
    public sealed class GameObject {
        public bool activeSelf = true;
        public void SetActive(bool active) => activeSelf = active;
    }
}
namespace UnityEngine.SceneManagement {
    public struct Scene { public string name; }
    public static class SceneManager {
        public static string Name = "Main";
        public static Scene GetActiveScene() => new Scene { name=Name };
    }
}
public enum Difficulty { Normal, Hard, Nightmare, Hell }
public enum BodyState { Normal, BigBreast, SmallBreast, Corruption, Beast, Muscle, Hip }
public sealed class HitPoint {
    public int CurrentHitPoint = 100;
    public bool invincible = true;
    public void DecreaseHP(int damage) {
        CurrentHitPoint = Math.Max(0, CurrentHitPoint - damage);
        if (CurrentHitPoint == 0) PlayableCharacter.instance.isAlive = false;
    }
}
public sealed class HPBar { public int Updates; public void UpdateHP() => Updates++; }
public sealed class PlayableCharacter {
    public static PlayableCharacter instance;
    public bool isAlive = true;
    public int resurrectionCount = 1;
    public HitPoint hitPoint = new HitPoint();
    public HPBar hPBar = new HPBar();
    public void StopAllCoroutines() { }
}
public sealed class BattleUIManager {
    public static BattleUIManager instance;
    public int Losses;
    public UnityEngine.GameObject pauseCanvas = new UnityEngine.GameObject();
    public UnityEngine.GameObject levelUpCanvas = new UnityEngine.GameObject();
    public UnityEngine.GameObject FlowerCanvas = new UnityEngine.GameObject();
    public UnityEngine.GameObject roadMapCanvas = new UnityEngine.GameObject();
    public void GameOver() {
        bool allowed = (bool)typeof(DeathLinkPatch).GetMethod("GameOver", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] {this});
        if (allowed) Losses++;
        // Model MissionPatch's real postfix, including when a prefix skips native GameOver.
        Plugin.Client.EndDeathLinkRun();
        ApState.Progress.RunActive = false;
        ApState.Progress.PendingEndingRun = "";
    }
}
public sealed class PotionCanvas { public static PotionCanvas instance; public UnityEngine.GameObject gameObject = new UnityEngine.GameObject(); }
public sealed class DialogViewer { public static DialogViewer instance; public UnityEngine.GameObject pannel = new UnityEngine.GameObject(); }
namespace WeddingWitchArchipelago {
    public static class Plugin { public static ArchipelagoClient Client; public static TestLogger Logger=new TestLogger(); }
    public sealed class TestLogger {
        public void LogInfo(string message) => Console.WriteLine(message);
        public void LogWarning(string message) => Console.WriteLine(message);
    }
    public static class ApProfile {
        public static void Enter(string seed, string slot) => ApState.Active = true;
        public static void Leave() => ApState.Active = false;
    }
    public static class ApState {
        public static int Epoch;
        public static bool Active;
        public static SlotSettings Settings = SlotSettings.Defaults();
        public static ProgressLedger Progress = new ProgressLedger();
        public static void AdoptSession(ArchipelagoClient client, IEnumerable<string> checks) { Epoch++; Settings=client.Settings; Progress = new ProgressLedger(); }
        public static void ReleaseSession() { Epoch++; ApProfile.Leave(); }
        public static void RequestReload() { }
        public static void EnqueueItem(int generation, string name) { }
        public static void Sync() { }
    }
}
