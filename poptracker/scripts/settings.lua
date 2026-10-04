-- Read-only, visible seed settings. Configuration stays authoritative from AP.
WW.settings = {}
function WW.line(code)
    local item = WW.display(code, "", "images/blank.png")
    item:SetOverlayFontSize(14)
    item:SetOverlayAlign("left")
    -- No click callbacks: these LuaItems cannot edit seed configuration.
    return item
end

for n = 1, 8 do WW.settings[n] = WW.line("ww_setting_" .. n) end

function WW.refresh_settings()
    local lines
    if not WW.config then
        lines = {"Seed nicht unterstützt", WW.error or "Keine gültigen Slotdaten."}
    else
        local c = WW.config
        local connection = AutoTracker:GetConnectionState("AP")
        lines = {
            connection == 3 and "Quelle: verbundener AP-Seed" or "Quelle: offline / zuletzt gespeichertes Layout",
            "Ziel: " .. c.transformEnd .. " verschiedene Endings",
            "Ziel-Schwierigkeit: " .. c.difficulty,
            "Start-Tranktyp: " .. c.starting_exp_type,
            string.format("Blumenchecks: Normal %d / Hard %d / Nightmare %d", table.unpack(c.flower_checks)),
            "80 Checks: 18 Karten + 6 Formen + 38 Erfolge + Endings + Blumen",
            "Standard-Skills: normaler Level-up-Pool",
            "APWorld-Schema: 4 · Einstellungen automatisch aus AP"
        }
    end
    for n, item in ipairs(WW.settings) do
        item.Name = lines[n] or ""
        item:SetOverlay(lines[n] or "")
    end
end
