using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;

namespace WeddingWitchArchipelago.Archipelago;

// Network callbacks only touch this locked state. A death belongs to the run
// active when it arrived, and is never carried into a later run or connection.
public sealed class DeathLinkState
{
    readonly object gate = new object();
    readonly HashSet<string> seen = new HashSet<string>();
    readonly Queue<string> seenOrder = new Queue<string>();
    int epoch;
    bool enabled;
    string source = "", run = "";
    DeathLink pending;

    public bool Enabled { get { lock (gate) return enabled; } }

    public void Reset(int generation, string player)
    {
        lock (gate) {
            epoch = generation; source = player; enabled = false; run = "";
            pending = null; seen.Clear(); seenOrder.Clear();
        }
    }

    public void SetEnabled(bool value)
    {
        lock (gate) { enabled = value; pending = null; }
    }

    public void BeginRun(string token)
    {
        lock (gate) { run = token ?? ""; pending = null; }
    }

    public void EndRun()
    {
        lock (gate) { run = ""; pending = null; }
    }

    public void Receive(int generation, DeathLink death)
    {
        if (death == null || string.IsNullOrWhiteSpace(death.Source)) return;
        lock (gate) {
            if (generation != epoch || !enabled || death.Source == source) return;
            // Remember even menu-time packets, so a repeated packet cannot
            // become a new death just because a run has started meanwhile.
            string key = death.Timestamp.ToUniversalTime().Ticks + "\0" + death.Source;
            if (!seen.Add(key)) return;
            seenOrder.Enqueue(key);
            if (seenOrder.Count > 256) seen.Remove(seenOrder.Dequeue());
            if (run != "" && pending == null) pending = death;
        }
    }

    public bool TryTake(string token, out DeathLink death)
    {
        lock (gate) {
            death = null;
            if (!enabled || run == "" || token != run || pending == null) return false;
            death = pending;
            // End before Unity's GameOver hook runs: a remote death cannot echo.
            run = ""; pending = null;
            return true;
        }
    }

    public bool TrySend(string token)
    {
        lock (gate) {
            if (!enabled || run == "" || token != run) return false;
            run = ""; pending = null;
            return true;
        }
    }
}
