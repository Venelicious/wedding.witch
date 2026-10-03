using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using UnityEngine.SceneManagement;
namespace WeddingWitchArchipelago.Archipelago;
public class ArchipelagoClient
{
    public const string GameName = "Wedding Witch";
    ArchipelagoSession session;
    int generation;
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
    void OnItemReceived(IReceivedItemsHelper helper) {
        if(session==null || !ReferenceEquals(helper,session.Items)) return;
        while(helper.Any()) ApState.EnqueueItem(generation,helper.DequeueItem().ItemName);
    }
    void OnSocketClosed(string reason) {
        // Keep the AP profile active: gameplay during an outage must never write
        // AP upgrades/counters into the vanilla save. Reconnect via the menu.
        Plugin.Logger.LogWarning("AP disconnected; progress retained: " + reason);
    }
}
