using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;
using WeddingWitchArchipelago.Archipelago;
namespace WeddingWitchArchipelago;
public static class ApState
{
    static ArchipelagoClient client;
    static readonly ConcurrentQueue<(int Epoch, string Item)> Incoming = new ConcurrentQueue<(int,string)>();
    static int epoch;
    static readonly Dictionary<string,int> Counts = new Dictionary<string,int>();
    static readonly Dictionary<string,int> RunLevels = new Dictionary<string,int>();
    public static ProgressLedger Progress { get; private set; } = new ProgressLedger();
    public static bool Connected => client?.Connected == true;
    public static bool Active => ApProfile.Active;
    public static SlotSettings Settings { get; private set; } = SlotSettings.Defaults();
    public static bool GoalComplete { get; private set; }
    public static int Epoch => epoch;
    public static int ItemCount => Counts.Values.Sum();
    public static void AdoptSession(ArchipelagoClient session, IEnumerable<string> checkedNames) {
        client = session;
        Settings = session.Settings;
        epoch++;
        while (Incoming.TryDequeue(out _)) { }
        Progress = ApProfile.TryGet("mod","Progress",out var json) ? JsonConvert.DeserializeObject<ProgressLedger>(json) : new ProgressLedger();
        Progress.RunActive = false; // Connecting is only allowed outside Adventure.
        Progress.Checks.UnionWith(checkedNames);
        Counts.Clear();
        RunLevels.Clear();
        GoalComplete = false;
        Persist();
        RequestReload();
    }
    public static void ReleaseSession() {
        epoch++;
        client = null;
        Counts.Clear();
        RunLevels.Clear();
        ApProfile.Leave();
        GoalComplete = false;
    }
    // A transient socket loss keeps the current AP profile and counters active.
    public static void EnqueueItem(int generation, string name) => Incoming.Enqueue((generation,name));
    public static void DrainItems() {
        bool changed = false;
        while (Incoming.TryDequeue(out var item)) {
            if (item.Epoch != epoch || !Active) continue;
            if (!ApItems.AllNames().Contains(item.Item)) { Plugin.Logger.LogError("Unknown AP item: " + item.Item); continue; }
            Counts[item.Item] = Count(item.Item) + 1;
            Plugin.Logger.LogInfo("[ap] Received " + item.Item + " (" + Count(item.Item) + ")");
            changed = true;
        }
        if (!changed) return;
        SkillUnlockPatch.Refresh();
        RequestReload();
    }
    public static int Count(string name) => Counts.TryGetValue(name,out var n) ? n : 0;
    public static bool Has(string name) => Count(name) > 0;
    public static bool ExpUnlocked(string name) => name == Settings.StartingExp || Has("EXP Unlock: " + name);
    public static int SkillLevel(string className) => SkillCatalog.ByClass.TryGetValue(className,out var name) ? Math.Min(SkillCatalog.Cap(className),Count(name)) : 0;
    public static int LevelOf(string key) {
        if (Progress.RunActive && RunLevels.TryGetValue(key,out var frozen)) return frozen;
        foreach (var upgrade in UpgradeCatalog.All)
            if (upgrade.LoadKey == key) return Math.Min(upgrade.MaxLevel, Count(upgrade.ItemName));
        return 0;
    }
    public static void BeginRun(Difficulty d) {
        if (!Active || !Locations.Valid(d)) return;
        RunLevels.Clear();
        Progress.RunActive = false;
        foreach (var upgrade in UpgradeCatalog.All) RunLevels[upgrade.LoadKey] = LevelOf(upgrade.LoadKey);
        Progress.StartRun((int)d);
        Persist();
    }
    public static void Flower() {
        if (!Active || !Progress.RunActive) return;
        int d = Progress.Difficulty;
        if (Progress.Flowers[d] >= Settings.FlowerChecks[d]) return;
        int n = ++Progress.Flowers[d];
        Check(Locations.Flower((Difficulty)d,n));
        Persist();
    }
    public static void Win(BodyState body) {
        if (!Active) return;
        Progress.Win(body.ToString());
        Persist();
    }
    public static void EndingShown() {
        if (!Active) return;
        if (Progress.ConfirmEnding(out var d,out var n) && string.Equals(((Difficulty)d).ToString(),Settings.GoalDifficulty,StringComparison.OrdinalIgnoreCase) && n <= Settings.GoalForms)
            Check(Locations.Ending(n));
        Persist();
        CheckGoal();
    }
    public static bool IsChecked(string name) => Progress.Checks.Contains(name);
    public static void Check(string name) {
        if (!Active || name == null || !Progress.Checks.Add(name)) return;
        Persist();
        client?.SendCheck(name);
        Plugin.Logger.LogInfo("[ap] CHECK " + name);
        CheckGoal();
    }
    public static void Sync() {
        foreach (var name in Progress.Checks) client?.SendCheck(name);
        CheckGoal();
    }
    static void CheckGoal() {
        if (!Connected || !Locations.TryParseDifficulty(Settings.GoalDifficulty,out var d)) return;
        if (Progress.Endings[(int)d].Count < Settings.GoalForms) return;
        GoalComplete = true;
        client.SendGoal();
    }
    public static void Persist() {
        if (Active) { ApProfile.Set("mod","Progress",JsonConvert.SerializeObject(Progress)); ApProfile.Flush(true); }
    }
    static bool reloadPending;
    public static void RequestReload() => reloadPending = true;
    public static bool TakeReloadRequest() { var value=reloadPending; reloadPending=false; return value; }
}
