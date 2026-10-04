using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using UnityEngine.SceneManagement;
namespace WeddingWitchArchipelago.Archipelago;
public class ArchipelagoClient
{
    public const string GameName = "Wedding Witch";
    ArchipelagoSession session;
    int generation;
    readonly DeathLinkState deaths = new DeathLinkState();
    DeathLinkService deathLinkService;
    DeathLinkService.DeathLinkReceivedHandler deathLinkHandler;
    string deathLinkSource;
    public bool DeathLinkEnabled => deaths.Enabled;
    public bool Connected => session?.Socket?.Connected == true;
    public SlotSettings Settings { get; private set; } = SlotSettings.Defaults();
    public string Connect(string host,int port,string slot,string password) {
        if (SceneManager.GetActiveScene().name == "Adventure") return "Connect or change seeds from the main menu.";
        Disconnect();
        try {
            session=ArchipelagoSessionFactory.CreateSession(host,port);
            var result=session.TryConnectAndLogin(GameName,slot,ItemsHandlingFlags.AllItems,password:string.IsNullOrEmpty(password)?null:password);
            if (result is LoginFailure failure) { Disconnect(); return string.Join("; ",failure.Errors); }
            var success=(LoginSuccessful)result;
            Settings=SlotSettings.FromSlotData(success.SlotData);
            ApProfile.Enter(session.RoomState.Seed + ":" + success.Team + ":" + success.Slot,slot);
            ApState.AdoptSession(this,CheckedNames());
            generation=ApState.Epoch;
            deaths.Reset(generation, slot);
            deathLinkSource = slot;
            var currentSession = session;
            int currentGeneration = generation;
            deathLinkService = session.CreateDeathLinkService();
            deathLinkHandler = death => {
                if (ReferenceEquals(session, currentSession) && Connected) deaths.Receive(currentGeneration, death);
            };
            deathLinkService.OnDeathLinkReceived += deathLinkHandler;
            var deathLinkError = SetDeathLinkEnabled(Settings.DeathLink);
            if (deathLinkError != null) throw new InvalidOperationException(deathLinkError);
            session.Items.ItemReceived+=OnItemReceived;
            session.Socket.SocketClosed+=OnSocketClosed;
            OnItemReceived(session.Items);
            ApState.Sync();
            Plugin.Logger.LogInfo($"Connected: {slot} | {Settings.GoalForms} endings on {Settings.GoalDifficulty}");
            return null;
        } catch(Exception ex) { Disconnect(); return ex.GetBaseException().Message; }
    }
    IEnumerable<string> CheckedNames() {
        foreach(var id in session.Locations.AllLocationsChecked) {
            string name=session.Locations.GetLocationNameFromId(id,GameName);
            if (!string.IsNullOrEmpty(name)) yield return name;
        }
    }
    public void Disconnect() {
        var old=session;
        session=null;
        if (deathLinkService != null && deathLinkHandler != null)
            deathLinkService.OnDeathLinkReceived -= deathLinkHandler;
        deathLinkService = null; deathLinkHandler = null;
        deaths.Reset(-1, "");
        if(old!=null) {
            old.Items.ItemReceived-=OnItemReceived;
            old.Socket.SocketClosed-=OnSocketClosed;
            old.Socket.DisconnectAsync();
        }
        ApState.ReleaseSession();
        ApState.RequestReload();
    }
    public void SendCheck(string name) {
        if (!Connected) return;
        long id=session.Locations.GetLocationIdFromName(GameName,name);
        if(id>0 && (session.Locations.AllMissingLocations.Contains(id) || session.Locations.AllLocationsChecked.Contains(id)))
            session.Locations.CompleteLocationChecks(id);
    }
    public void SendGoal() { if(Connected) session.SetGoalAchieved(); }
    public string SetDeathLinkEnabled(bool enabled) {
        if (!Connected || deathLinkService == null) return "Connect before changing DeathLink.";
        try {
            if (enabled) deathLinkService.EnableDeathLink(); else deathLinkService.DisableDeathLink();
            deaths.SetEnabled(enabled);
            Plugin.Logger.LogInfo("[DeathLink] " + (enabled ? "enabled" : "disabled"));
            return null;
        } catch (Exception ex) { return ex.GetBaseException().Message; }
    }
    public void BeginDeathLinkRun(string run) => deaths.BeginRun(run);
    public void EndDeathLinkRun() => deaths.EndRun();
    public bool TryTakeDeathLink(string run, out DeathLink death) {
        death = null;
        return Connected && deaths.TryTake(run, out death);
    }
    public void SendDeathLink(string run) {
        if (!Connected || deathLinkService == null || !deaths.TrySend(run)) return;
        try {
            deathLinkService.SendDeathLink(new DeathLink(deathLinkSource, deathLinkSource + " died in Wedding Witch."));
            Plugin.Logger.LogInfo("[DeathLink] sent");
        } catch (Exception ex) { Plugin.Logger.LogWarning("[DeathLink] could not send: " + ex.GetBaseException().Message); }
    }
    void OnItemReceived(IReceivedItemsHelper helper) {
        if(session==null || !ReferenceEquals(helper,session.Items)) return;
        while(helper.Any()) ApState.EnqueueItem(generation,helper.DequeueItem().ItemName);
    }
    void OnSocketClosed(string reason) {
        deaths.EndRun();
        // Keep the AP profile active: gameplay during an outage must never write
        // AP upgrades/counters into the vanilla save. Reconnect via the menu.
        Plugin.Logger.LogWarning("AP disconnected; progress retained: " + reason);
    }
}
