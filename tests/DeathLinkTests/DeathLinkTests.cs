using System;
using System.Collections.Concurrent;
using System.Threading;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using WeddingWitchArchipelago;
using WeddingWitchArchipelago.Archipelago;
using UnityEngine.SceneManagement;

static class DeathLinkTests
{
    static int assertions;
    static void Assert(bool condition, string message) { assertions++; if (!condition) throw new Exception(message); }
    static void Wait(Func<bool> condition, string message) {
        var timeout = DateTime.UtcNow.AddSeconds(8);
        while (!condition() && DateTime.UtcNow < timeout) Thread.Sleep(25);
        Assert(condition(), message);
    }
    static void StartRun(ArchipelagoClient client) {
        SceneManager.Name = "Adventure";
        ApState.Progress.StartRun(0);
        client.BeginDeathLinkRun(ApState.Progress.Run);
        BattleUIManager.instance = new BattleUIManager();
        PlayableCharacter.instance = new PlayableCharacter();
        PotionCanvas.instance = new PotionCanvas();
        DialogViewer.instance = new DialogViewer();
        DeathLinkPatch.Reset();
    }
    static void StateTests() {
        var state = new DeathLinkState();
        state.Reset(1, "Self"); state.BeginRun("a");
        state.Receive(1, new DeathLink("Other"));
        Assert(!state.TryTake("a", out _), "disabled DeathLink ignores packets");
        state.SetEnabled(true);
        state.Receive(0, new DeathLink("Other")); state.Receive(1, new DeathLink("Self"));
        Assert(!state.TryTake("a", out _), "old connections and own echoes ignored");
        var death = new DeathLink("Other", "Other died.");
        state.Receive(1, death); state.Receive(1, death);
        Assert(!state.TryTake("wrong", out _), "death tied to its run token");
        Assert(state.TryTake("a", out var received) && received == death, "remote death reaches the main-thread consumer once");
        Assert(!state.TryTake("a", out _) && !state.TrySend("a"), "remote death cannot repeat or echo");
        state.BeginRun("b"); state.Receive(1, death);
        Assert(!state.TryTake("b", out _), "duplicate death cannot kill a later run");
        Assert(state.TrySend("b") && !state.TrySend("b"), "natural death sends once per run");
        state.EndRun(); var menuDeath = new DeathLink("Other"); state.Receive(1, menuDeath);
        state.BeginRun("c"); state.Receive(1, menuDeath);
        Assert(!state.TryTake("c", out _), "menu-time packet never waits for a later run");
        state.Receive(1, new DeathLink("Other")); state.SetEnabled(false); state.SetEnabled(true);
        Assert(!state.TryTake("c", out _), "disabling clears a pending death");
        state.Receive(1, new DeathLink("Other")); state.BeginRun("d");
        Assert(!state.TryTake("d", out _), "new run clears pending death from previous run");
        state.Receive(1, new DeathLink("Other")); state.Reset(2, "Self"); state.SetEnabled(true); state.BeginRun("e");
        Assert(!state.TryTake("e", out _), "reconnect resets the queue");
        state.Receive(1, new DeathLink("Other"));
        Assert(!state.TryTake("e", out _), "late callbacks from old connection rejected");
        var packet = new DeathLink("Other");
        System.Threading.Tasks.Parallel.For(0, 100, _ => state.Receive(2, packet));
        Assert(state.TryTake("e", out _) && !state.TryTake("e", out _), "concurrent callbacks cannot duplicate a death");
    }
    static void NetworkTests(string host, int port, bool seedDeathLink) {
        var client = new ArchipelagoClient(); Plugin.Client = client;
        var peer = ArchipelagoSessionFactory.CreateSession(host, port);
        var received = new ConcurrentQueue<DeathLink>();
        try {
            Assert(client.Connect(host, port, "YamlTest2", "") == null, "actual mod client connects to AP");
            Assert(client.DeathLinkEnabled == seedDeathLink, "login adopts DeathLink from generated slot data, defaulting off for existing seeds");
            Assert(peer.TryConnectAndLogin("Wedding Witch", "YamlTest1", ItemsHandlingFlags.AllItems) is LoginSuccessful,
                "peer connects to actual AP server");
            var link = peer.CreateDeathLinkService(); link.OnDeathLinkReceived += received.Enqueue; link.EnableDeathLink();
            Assert(client.SetDeathLinkEnabled(true) == null && client.DeathLinkEnabled, "F8 toggle enables existing seed");
            // Let the real server process ConnectUpdate before sending the first Bounce.
            Thread.Sleep(350);
            StartRun(client); PlayableCharacter.instance.isAlive = false;
            BattleUIManager.instance.GameOver(); BattleUIManager.instance.GameOver();
            Wait(() => received.Count == 1, "natural terminal loss sends a DeathLink to another slot");
            Assert(received.TryDequeue(out var own) && own.Source == "YamlTest2" && own.Cause.Contains("Wedding Witch"), "outgoing packet uses slot name and cause");
            Thread.Sleep(150);
            Assert(received.IsEmpty, "duplicate native callbacks do not send twice");
            StartRun(client);
            link.SendDeathLink(new DeathLink("YamlTest1", "Peer test death."));
            Wait(() => { DeathLinkPatch.Tick(); return BattleUIManager.instance.Losses == 1; }, "incoming DeathLink ends current run");
            Assert(!PlayableCharacter.instance.isAlive && PlayableCharacter.instance.hitPoint.CurrentHitPoint == 0 &&
                PlayableCharacter.instance.resurrectionCount == 0 && !PlayableCharacter.instance.hitPoint.invincible,
                "remote death bypasses invulnerability and revives");
            Assert(!BattleUIManager.instance.pauseCanvas.activeSelf && !BattleUIManager.instance.levelUpCanvas.activeSelf &&
                !PotionCanvas.instance.gameObject.activeSelf && !DialogViewer.instance.pannel.activeSelf,
                "pause, level-up and selection dialogs close for GameOver");
            BattleUIManager.instance.GameOver();
            Assert(BattleUIManager.instance.Losses == 1, "later death-animation callback does not count a second native loss");
            Thread.Sleep(150); Assert(received.IsEmpty, "incoming death produces no outgoing echo");
            SceneManager.Name = "Main"; client.EndDeathLinkRun();
            link.SendDeathLink(new DeathLink("YamlTest1", "Menu test death.")); Thread.Sleep(200);
            StartRun(client); DeathLinkPatch.Tick();
            Assert(BattleUIManager.instance.Losses == 0, "menu-time DeathLink never carries into next run");
            Assert(client.SetDeathLinkEnabled(false) == null, "DeathLink can be disabled live");
            link.SendDeathLink(new DeathLink("YamlTest1", "Disabled test death.")); Thread.Sleep(200); DeathLinkPatch.Tick();
            Assert(BattleUIManager.instance.Losses == 0, "disabled client survives peer death");
            client.Disconnect(); Assert(!client.DeathLinkEnabled, "disconnect clears DeathLink state");
            SceneManager.Name = "Main";
            Assert(client.Connect(host, port, "YamlTest2", "") == null && client.DeathLinkEnabled == seedDeathLink,
                "reconnect restores seed setting rather than previous F8 override");
        } finally { client.Disconnect(); peer.Socket.DisconnectAsync(); }
    }
    static void Main(string[] args) {
        StateTests();
        if (args.Length >= 2) NetworkTests(args[0], int.Parse(args[1]), args.Length == 3 && args[2] == "on");
        Console.WriteLine("PASS " + assertions + " DeathLink assertions");
    }
}
