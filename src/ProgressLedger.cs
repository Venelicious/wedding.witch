using System;
using System.Collections.Generic;
namespace WeddingWitchArchipelago;

// Pure bookkeeping: every counter belongs to one seed, team and slot.
public class ProgressLedger
{
    public HashSet<string> Checks = new HashSet<string>();
    public HashSet<string>[] Endings = { new HashSet<string>(), new HashSet<string>(), new HashSet<string>() };
    public int[] Flowers = new int[3];
    public int[] Wins = new int[3];
    public string Run = "";
    public bool RunActive;
    public int Difficulty;
    public string PendingEndingRun = "";
    public string PendingEndingForm = "";
    public int PendingEndingDifficulty;
    public HashSet<string> CompletedRuns = new HashSet<string>();
    public void StartRun(int difficulty)
    {
        if (difficulty < 0 || difficulty > 2) throw new ArgumentOutOfRangeException(nameof(difficulty));
        Run = Guid.NewGuid().ToString("N");
        Difficulty = difficulty;
        RunActive = true;
        PendingEndingRun = "";
    }
    public void Win(string form)
    {
        if (!RunActive) return;
        if (CompletedRuns.Add(Run)) Wins[Difficulty]++;
        PendingEndingRun = Run;
        PendingEndingDifficulty = Difficulty;
        PendingEndingForm = form;
        RunActive = false;
    }
    public bool ConfirmEnding(out int difficulty, out int distinct)
    {
        difficulty = PendingEndingDifficulty;
        distinct = 0;
        if (PendingEndingRun == "" || !CompletedRuns.Contains(PendingEndingRun)) return false;
        var form = PendingEndingForm;
        PendingEndingRun = "";
        if (!Endings[difficulty].Add(form)) return false;
        distinct = Endings[difficulty].Count;
        return true;
    }
}
