local function display(code, name, icon)
    local item = ScriptHost:CreateLuaItem()
    item.Name = name
    item.Icon = ImageReference:FromPackRelativePath(icon)
    item.PotentialCodes = {code}
    item.CanProvideCodeFunc = function(_, candidate) return candidate == code end
    item.ProvidesCodeFunc = function() return 0 end
    item:SetOverlayFontSize(16)
    item:SetOverlayAlign("center")
    item:SetOverlayBackground("#dd101626")
    return item
end
WW.display = display

WW.status = display("ww_status", "Offline: default reference layout (3 Normal endings; flowers 4/5/6). Connect AP for your seed.", "images/status_offline.png")
WW.progress = display("ww_progress", "Checked AP locations", "images/charm.png")
WW.goal = display("ww_goal", "Distinct endings checked on the goal difficulty", "images/normal.png")

-- Preserve the seed's layout together with PopTracker's item/location save.
-- AP reconnect always clears and replays authoritative state afterwards.
WW.status.SaveFunc = function()
    return {config = WW.config, error = WW.error}
end
WW.status.LoadFunc = function(_, data)
    if type(data) ~= "table" then return end
    WW.clear_hints()
    local c = data.config
    if type(c) == "table" then
        local slot = {schema_version = c.schema_version, pool_size = 80,
            skill_mode = "level_up", integration_mode = "unlock_custom", transformEnd = c.transformEnd,
            difficulty = c.difficulty, starting_exp_type = c.starting_exp_type,
            flower_checks = c.flower_checks, map_counts = {5, 6, 7}, achievement_checks = WW.data.achievement_keys}
        WW.config, WW.error = WW.validate(slot)
    else
        WW.config, WW.error = nil, tostring(data.error or "Unsupported saved seed.")
    end
    WW.status:Set("revision", (tonumber(WW.status:Get("revision")) or 0) + 1)
    WW.refresh()
end

function WW.refresh()
    local complete, total, endings = 0, 0, 0
    for _, location in pairs(WW.data.locations) do
        if WW.enabled(location) then
            total = total + 1
            local section = Tracker:FindObjectForCode(location.code)
            if section and section.AvailableChestCount == 0 then
                complete = complete + 1
                if location.kind == "ending" then endings = endings + 1 end
            end
        end
    end
    WW.progress:SetOverlay(complete .. "/" .. total)
    local connection = AutoTracker:GetConnectionState("AP")
    local connection_icon = WW.error and "status_error" or (connection == 3 and "status" or "status_offline")
    if WW.connection_icon ~= connection_icon then
        WW.status.Icon = ImageReference:FromPackRelativePath("images/" .. connection_icon .. ".png")
        WW.connection_icon = connection_icon
    end
    local goal_icon = WW.config and WW.config.difficulty or "blank"
    if WW.goal_icon ~= goal_icon then
        WW.goal.Icon = ImageReference:FromPackRelativePath("images/" .. goal_icon .. ".png")
        WW.goal_icon = goal_icon
    end
    if WW.error then
        WW.status:SetOverlay("ERROR")
        WW.status.Name = WW.error
        WW.goal:SetOverlay("—")
    else
        WW.status:SetOverlay(connection == 3 and "AP" or "OFF")
        local c = WW.config
        WW.status.Name = string.format("%s · %d checks · flowers %d/%d/%d · starting potion %s",
            connection == 3 and "AP connected" or "Offline / last layout", total,
            c.flower_checks[1], c.flower_checks[2], c.flower_checks[3], c.starting_exp_type)
        WW.goal.Name = "Goal: " .. c.transformEnd .. " distinct endings on " .. c.difficulty
        WW.goal:SetOverlay(endings .. "/" .. c.transformEnd)
    end
    WW.refresh_settings()
    WW.refresh_hints()
end

local elapsed = 0
ScriptHost:AddOnFrameHandler("ww_status", function(delta)
    elapsed = elapsed + delta
    if elapsed >= 1 then elapsed = 0; WW.refresh() end
end)
ScriptHost:AddOnLocationSectionChangedHandler("ww_progress", function() WW.refresh() end)
