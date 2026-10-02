using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;

namespace WeddingWitchArchipelago;

/// A per-room stand-in for GameData.es3.
///
/// That file holds the upgrade levels, which in a seed are item state and belong to
/// the room rather than to the player: without this, every level the multiworld
/// granted would be written into the save they play unconnected, and their own
/// levels would show up in the room. Love Coins and the lifetime counters live in
/// the same file and come along with it.
///
/// Nothing else is diverted. Config.es3 — language, resolution, volume, key
/// bindings — stays shared, since none of it is progress, and AchivementState.es3
/// is left alone so Steam achievements still work the way they always did.
public static class ApProfile
{
    public const string GameDataFile = "GameData.es3";

    /// The mod's own bookkeeping, kept in the same per-room store. Not a real ES3 file,
    /// so the interception in SaveDataPatch never sees it.
    public const string ModFile = "mod";

    private static readonly Dictionary<string, string> Values =
        new Dictionary<string, string>();

    private static string path;
    private static bool dirty;
    private static DateTime lastFlush;

    /// While false every ES3 call runs untouched, so playing unconnected leaves the
    /// ordinary save exactly as the game left it.
    public static bool Active { get; private set; }

    public static bool Owns(string file) => file == GameDataFile;

    public static void Enter(string room, string slot)
    {
        string name;
        using(var sha=System.Security.Cryptography.SHA256.Create())
            name=BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(room+":"+slot))).Replace("-","")+".txt";
        lock (Values)
        {
            path = Path.Combine(Paths.BepInExRootPath, "WeddingWitchArchipelago", name);
            Values.Clear();
            dirty=false;
            Active = true;
            Load();
            Plugin.Logger.LogInfo($"Save profile: {path} ({Values.Count} keys)");
        }
    }

    public static void Leave()
    {
        lock (Values)
        {
            Flush(true);
            Active = false;
            Values.Clear();
            path = null;
        }
    }

    // ---- the store ------------------------------------------------------
    //
    // Written from two threads: the game reads and writes it through ES3 on the main
    // one, while received items raise levels on the network one.

    public static bool TryGet(string file, string key, out string value)
    {
        lock (Values) return Values.TryGetValue(Compose(file, key), out value);
    }

    public static void Set(string file, string key, string value)
    {
        lock (Values)
        {
            string composite=Compose(file,key);
            if (Values.TryGetValue(composite,out var old) && old==value) return;
            Values[composite] = value;
            dirty=true;
        }
    }

    public static void Remove(string file, string key)
    {
        lock (Values)
        {
            if (Values.Remove(Compose(file, key))) dirty=true;
        }
    }

    private static string Compose(string file, string key) => file + "|" + key;

    // ---- persistence ----------------------------------------------------

    private static void Load()
    {
        if (!File.Exists(path)) return;
        try
        {
            foreach (var line in File.ReadAllLines(path))
            {
                var split = line.IndexOf('=');
                if (split <= 0) continue;
                Values[line.Substring(0, split)] = line.Substring(split + 1);
            }
        }
        catch (Exception ex)
        {
            throw new IOException($"Could not read save profile {path}",ex);
        }
    }

    public static void Flush(bool force)
    {
        lock(Values) {
            if (!dirty || (!force && DateTime.UtcNow-lastFlush<TimeSpan.FromSeconds(1))) return;
            Save();
        }
    }

    private static void Save()
    {
        if (path == null) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp=path+".tmp";
            File.WriteAllLines(temp,
                Values.OrderBy(pair => pair.Key)
                      .Select(pair => pair.Key + "=" + pair.Value)
                      .ToArray());
            if (File.Exists(path)) File.Replace(temp,path,path+".bak");
            else File.Move(temp,path);
            dirty=false; lastFlush=DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Could not write save profile {path}: {ex.Message}");
        }
    }

    private static string Sanitise(string value)
    {
        if (string.IsNullOrEmpty(value)) return "unknown";
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
    }
}

