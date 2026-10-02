using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

internal static class Launcher
{
    [STAThread]
    private static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        try
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            if (args.Contains("--self-test"))
            {
                if (DisableValue(false) != "1" || DisableValue(true) != "0") return 1;
                return 0;
            }
            if (!File.Exists(Path.Combine(dir, "Wedding Witch.exe")))
                throw new InvalidOperationException("Den Launcher in den Wedding-Witch-Spielordner installieren.");
            if (args.Contains("--original") || args.Contains("--ap"))
                Start(dir, args.Contains("--ap"));
            else
            {
                using (var form = new Form { Text = "Wedding Witch · Archipelago", ClientSize = new Size(420, 190), FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, StartPosition = FormStartPosition.CenterScreen })
                {
                    form.Controls.Add(new Label { Text = "Wie möchtest du Wedding Witch starten?", Location = new Point(22, 20), AutoSize = true });
                    var original = new Button { Text = "Original starten", Location = new Point(22, 60), Size = new Size(180, 48) };
                    var ap = new Button { Text = "Archipelago starten", Location = new Point(218, 60), Size = new Size(180, 48) };
                    form.Controls.Add(original); form.Controls.Add(ap);
                    form.Controls.Add(new Label { Text = "Archipelago: Verbindung im Spiel mit F8 öffnen.\nSpielstände und AP-Fortschritt bleiben erhalten.", Location = new Point(22, 130), Size = new Size(380, 45) });
                    original.Click += (s, e) => TryStart(form, dir, false);
                    ap.Click += (s, e) => TryStart(form, dir, true);
                    Application.Run(form);
                }
            }
            return 0;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Wedding Witch Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error); return 1; }
    }

    private static string DisableValue(bool ap) { return ap ? "0" : "1"; }
    private static void TryStart(Form form, string dir, bool ap)
    {
        try { Start(dir, ap); form.Close(); }
        catch (Exception ex) { MessageBox.Show(form, ex.Message, "Start nicht möglich", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private static void Start(string dir, bool ap)
    {
        if (Process.GetProcessesByName("Wedding Witch").Length != 0)
            throw new InvalidOperationException("Wedding Witch läuft bereits. Erst das Spiel schließen.");
        string config = Path.Combine(dir, "doorstop_config.ini");
        if (File.Exists(config))
        {
            string text = File.ReadAllText(config);
            if (Regex.IsMatch(text, @"(?im)^\s*ignore_disable_switch\s*=\s*true\s*$"))
                throw new InvalidOperationException("doorstop_config.ini: ignore_disable_switch muss false sein, damit Original ohne Mods starten kann.");
            if (ap && !Regex.IsMatch(text, @"(?im)^\s*enabled\s*=\s*true\s*$"))
                throw new InvalidOperationException("Der Mod-Loader ist deaktiviert. Install.cmd erneut ausführen.");
        }
        else if (ap) throw new InvalidOperationException("Der Mod-Loader fehlt. Zuerst Install.cmd ausführen.");
        if (ap && !File.Exists(Path.Combine(dir, "BepInEx", "plugins", "WeddingWitchCustom", "WeddingWitchArchipelago.dll")))
            throw new InvalidOperationException("Der AP-Mod fehlt. Zuerst Install.cmd ausführen.");
        if (Process.GetProcessesByName("steam").Length == 0)
        {
            string steam = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamExe", null);
            if (String.IsNullOrEmpty(steam)) throw new InvalidOperationException("Steam zuerst starten und anmelden.");
            Process.Start(new ProcessStartInfo(steam) { UseShellExecute = true });
            throw new InvalidOperationException("Steam wird gestartet. Sobald Steam angemeldet ist, den gewünschten Startknopf erneut drücken.");
        }
        var start = new ProcessStartInfo(Path.Combine(dir, "Wedding Witch.exe")) { WorkingDirectory = dir, UseShellExecute = false };
        start.EnvironmentVariables["DOORSTOP_DISABLE"] = DisableValue(ap);
        start.EnvironmentVariables["SteamAppId"] = "2529820";
        start.EnvironmentVariables["SteamGameId"] = "2529820";
        Process.Start(start);
    }
}
