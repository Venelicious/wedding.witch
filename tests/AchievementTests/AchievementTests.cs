using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeddingWitchArchipelago;
using WeddingWitchArchipelago.Archipelago;

// Real profile, slot parser and Harmony hooks, with native counters/Steam stubbed.
internal static class AchievementTests
{
    static int assertions;
    static void Assert(bool condition, string message) {
        if (!condition) throw new Exception(message);
        assertions++;
    }
    static void Reject(Action action, string message) {
        bool rejected = false;
        try { action(); } catch (InvalidOperationException) { rejected = true; }
        Assert(rejected, message);
    }
    public static bool Hook(string method, Achievement achievement) => (bool)
        typeof(AchievementPatch).GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { achievement });

    static Dictionary<string, object> Slot(int schema) {
        var data = new Dictionary<string, object> {
        {"schema_version", schema}, {"difficulty", "normal"}, {"transformEnd", 3},
        {"starting_exp_type", "Beast"}, {"flower_checks", schema >= 4 ? new[] {4,5,6} : schema == 3 ? new[] {22,26,29} : new[] {34,40,41}},
        {"achievement_checks", AchievementCatalog.All.Select(a => a.Id).ToArray()}, {"pool_size", schema >= 4 ? 80 : 142}
        };
        if (schema >= 4) data["skill_mode"] = "level_up";
        else data["skill_caps"] = SkillCatalog.All.ToDictionary(a => a.Class, a => a.MaxLevel);
        return data;
    }
    static void Enter(string seed, int schema = 4) {
        ApProfile.Leave();
        ApProfile.Enter(seed + ":0:1", "AchievementTester");
        ApState.Epoch++;
        ApState.Settings = SlotSettings.FromSlotData(Slot(schema));
        ApState.Checks.Clear();
        if (ApProfile.TryGet("test", "checks", out var checks)) ApState.Checks.UnionWith(checks.Split(';'));
    }
    static int Counter => ApProfile.TryGet(ApProfile.GameDataFile, "EnemiesKilled", out var saved) ? int.Parse(saved) : 0;
    static void Poll() { UnityEngine.Time.realtimeSinceStartup += 1; AchievementPatch.Tick(); }

    static void Main(string[] args) {
        foreach (var file in args) {
            var generated = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(System.IO.File.ReadAllText(file));
            var slot = SlotSettings.FromSlotData(generated);
            Assert(slot.AchievementChecks.Count == 38 && slot.FlowerChecks.Sum() + slot.GoalForms == slot.PoolSize - 62,
                "actual generated slot data is accepted by the client");
        }
        var legacy = SlotSettings.FromSlotData(Slot(2));
        Assert(legacy.AchievementChecks.Count == 0 && legacy.FlowerChecks.Sum() == 115, "legacy seed retains its flower layout");
        var current = SlotSettings.FromSlotData(Slot(3));
        Assert(current.AchievementChecks.Count == 38 && current.FlowerChecks.Sum() == 77, "38 achievements replace exactly 38 flowers");
        Assert(!legacy.SkillsFromLevelUps && !current.SkillsFromLevelUps && legacy.PoolSize == 142 && current.PoolSize == 142,
            "old seeds retain their AP skill ranks and 142-item pool");
        var native = SlotSettings.FromSlotData(Slot(4));
        Assert(native.SkillsFromLevelUps && native.PoolSize == 80 && native.FlowerChecks.Sum() == 15 && native.AchievementChecks.Count == 38,
            "schema 4 removes 62 items and flower checks, retaining all achievements");
        var bad = Slot(4); bad["pool_size"] = 142;
        Reject(() => SlotSettings.FromSlotData(bad), "schema 4 rejects the old item budget");
        bad = Slot(4); bad["flower_checks"] = new[] {22,26,29};
        Reject(() => SlotSettings.FromSlotData(bad), "schema 4 rejects the old flower budget");
        bad = Slot(4); bad.Remove("skill_mode");
        Reject(() => SlotSettings.FromSlotData(bad), "native skill mode must be explicit");
        bad = Slot(4); bad["skill_caps"] = SkillCatalog.All.ToDictionary(a => a.Class, a => a.MaxLevel);
        Reject(() => SlotSettings.FromSlotData(bad), "schema 4 rejects AP skill rank metadata");
        var data = Slot(3); data.Remove("achievement_checks");
        Reject(() => SlotSettings.FromSlotData(data), "missing achievement list rejected");
        data = Slot(3); data["achievement_checks"] = Enumerable.Repeat("WIN1", 38).ToArray();
        Reject(() => SlotSettings.FromSlotData(data), "duplicate achievement IDs rejected");
        data = Slot(3); data["flower_checks"] = new[] {34,40,41};
        Reject(() => SlotSettings.FromSlotData(data), "old flower budget with new achievements rejected");
        data = Slot(5);
        Reject(() => SlotSettings.FromSlotData(data), "unsupported schema rejected");

        Enter("seed-a");
        var kill = new Achievement { Id="KILL1000", CurrentCount=() => Counter, RequiredCount=1000 };
        var manager = new AchievementManager(); manager.achievements.Add(kill); AchievementManager.instance=manager;
        kill.GetState();
        Assert(kill.State == AchivementState.Incompleted && Achievement.NativeReads == 0, "vanilla claimed achievement does not carry into AP");
        // Model the frame between login and ReloadAll, when native cached counters can still be from vanilla.
        kill.CurrentCount=() => 50000;
        Poll();
        Assert(ApState.Checks.Count == 0, "cached vanilla counters cannot grant checks before reload");
        kill.CurrentCount=() => Counter;
        AchievementPatch.AfterReload(); Poll();
        Assert(ApState.Checks.Count == 0, "fresh seed does not grant an old achievement");
        ApProfile.Set(ApProfile.GameDataFile, "EnemiesKilled", "999"); Poll();
        Assert(ApState.Checks.Count == 0, "achievement threshold is respected");
        ApProfile.Set(ApProfile.GameDataFile, "EnemiesKilled", "1000"); Poll();
        string location = AchievementCatalog.Location("KILL1000");
        Assert(ApState.Checks.SetEquals(new[] {location}), "fulfilled native condition sends the mapped AP check");
        Assert(kill.State == AchivementState.Completed, "achievement panel state becomes completed");
        Assert(Steamworks.Data.Achievement.Triggered.Contains("KILL1000"), "Steam achievement unlock is retained");
        Assert(Achievement.NativeWrites == 0, "completion leaves vanilla achievement file and cloud upload untouched");
        Poll(); kill.Complete();
        Assert(ApState.SentChecks == 1, "polling and repeated callbacks grant the check once");
        kill.ClaimedReward(); kill.ClaimedReward();
        Assert(TestReward.Coins == 0 && kill.State == AchivementState.Claimed, "AP achievement claims give no coins");
        Assert(Achievement.NativeWrites == 0, "claim also leaves the vanilla achievement file untouched");

        Enter("seed-b"); AchievementPatch.AfterReload(); Poll();
        Assert(kill.State == AchivementState.Incompleted && Counter == 0 && ApState.Checks.Count == 0, "new seed resets counters, achievement state and checks");
        Enter("seed-a"); AchievementPatch.AfterReload(); Poll();
        Assert(kill.State == AchivementState.Claimed && Counter == 1000, "reconnect restores this seed's claimed state and counters");
        Assert(ApState.Checks.Contains(location) && ApState.SentChecks == 1, "reconnect does not duplicate achievement rewards");
        ApProfile.Set("achievements", "KILL1000", "999"); kill.GetState();
        Assert(kill.State == AchivementState.Incompleted, "invalid stored enum does not appear claimed");

        var florist = new Achievement { Id="Florist", RequiredCount=100, CurrentCount=() =>
            ApProfile.TryGet(ApProfile.GameDataFile, "EarnedFlowerCount", out var flowers) ? int.Parse(flowers) : 0 };
        manager.achievements.Add(florist); florist.GetState();
        ApProfile.Set(ApProfile.GameDataFile, "EarnedFlowerCount", "99"); Poll();
        Assert(!ApState.Checks.Contains(AchievementCatalog.Location("Florist")), "Florist requires 100 actual flower pickups");
        ApProfile.Set(ApProfile.GameDataFile, "EarnedFlowerCount", "100"); Poll();
        Assert(ApState.Settings.FlowerChecks.Sum() == 15 && ApState.Checks.Contains(AchievementCatalog.Location("Florist")),
            "Florist counts natural flowers beyond the reduced AP flower-check budget");

        Enter("legacy", 2); AchievementPatch.AfterReload(); Poll();
        Assert(kill.State == AchivementState.Claimed && Achievement.NativeReads > 0, "legacy seed keeps native achievement-state behavior");
        Assert(ApState.Checks.Count == 0, "legacy seed receives no new achievement locations");
        kill.State = AchivementState.Completed; kill.ClaimedReward();
        Assert(TestReward.Coins == 10, "legacy seed retains its native coin reward");
        ApProfile.Leave(); kill.Complete();
        Assert(Achievement.NativeWrites == 2, "original mode retains native achievement completion");
        Console.WriteLine("PASS " + assertions + " achievement and slot-contract assertions");
    }
}
