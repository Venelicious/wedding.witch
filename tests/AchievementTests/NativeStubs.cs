using System;
using System.Collections.Generic;
using System.IO;
using WeddingWitchArchipelago.Archipelago;

namespace HarmonyLib {
    [AttributeUsage(AttributeTargets.All, AllowMultiple=true)]
    public sealed class HarmonyPatch : Attribute {
        public HarmonyPatch() { }
        public HarmonyPatch(Type type, string method) { }
    }
    public sealed class HarmonyPrefix : Attribute { }
}
namespace UnityEngine { public static class Time { public static float realtimeSinceStartup; } }
namespace BepInEx { public static class Paths { public static string BepInExRootPath=Path.Combine(Path.GetTempPath(), "WW-achievement-tests-"+Guid.NewGuid().ToString("N")); } }
namespace Steamworks { public static class SteamClient { public static bool IsValid=true; } }
namespace Steamworks.Data {
    public struct Achievement {
        public static HashSet<string> Triggered = new HashSet<string>();
        readonly string id;
        public Achievement(string id) { this.id=id; }
        public void Trigger() => Triggered.Add(id);
    }
}
public enum Difficulty { Normal, Hard, Nightmare, Hell }
public enum BodyState { Normal, BigBreast, SmallBreast, Corruption, Beast, Muscle, Hip }
public enum AchivementState { Incompleted, Completed, Claimed }
public sealed class TestReward {
    public static int Coins;
    public void GiveReward() { Coins+=10; }
}
public sealed class Achievement {
    public static int NativeReads, NativeWrites;
    public string Id;
    public Func<int> CurrentCount;
    public int RequiredCount;
    public AchivementState State;
    public TestReward Reward=new TestReward();
    public bool UnlockCondition() => CurrentCount() >= RequiredCount;
    public void GetState() {
        if (!AchievementTests.Hook("LoadState", this)) return;
        NativeReads++; State=AchivementState.Claimed;
    }
    public void Complete() {
        if (!AchievementTests.Hook("Complete", this)) return;
        NativeWrites++; State=AchivementState.Completed;
    }
    public void ClaimedReward() {
        if (!AchievementTests.Hook("Claim", this)) return;
        NativeWrites++; State=AchivementState.Claimed; Reward.GiveReward();
    }
}
public sealed class AchievementManager {
    public static AchievementManager instance;
    public List<Achievement> achievements=new List<Achievement>();
    public void ReloadStates() { foreach (var achievement in achievements) achievement.GetState(); }
}
namespace WeddingWitchArchipelago {
    public static class Plugin { public static TestLogger Logger=new TestLogger(); }
    public sealed class TestLogger {
        public void LogInfo(string message) { }
        public void LogWarning(string message) { throw new Exception(message); }
        public void LogError(string message) { throw new Exception(message); }
    }
    public static class ApState {
        public static bool Active => ApProfile.Active;
        public static int Epoch;
        public static SlotSettings Settings;
        public static HashSet<string> Checks=new HashSet<string>();
        public static int SentChecks;
        public static void Check(string name) {
            if (name==null || !Checks.Add(name)) return;
            SentChecks++;
            ApProfile.Set("test", "checks", string.Join(";", Checks));
            ApProfile.Flush(true);
        }
    }
}
