local function on_clear(slot)
    WW.bulk(function()
        WW.received, WW.checked = {}, {}
        WW.config, WW.error = WW.validate(slot)
        if Archipelago:GetPlayerGame(Archipelago.PlayerNumber) ~= "Wedding Witch" then
            WW.config, WW.error = nil, "Wrong game: connect your Wedding Witch slot."
        end
        for _, entry in pairs(WW.data.items) do
            local item = Tracker:FindObjectForCode(entry.code)
            item.IgnoreUserInput = true
            if entry.kind == "toggle" then item.Active = false else item.AcquiredCount = 0 end
        end
        for _, location in pairs(WW.data.locations) do
            local section = Tracker:FindObjectForCode(location.code)
            section.AvailableChestCount = 1
        end
        if WW.config then
            Tracker:FindObjectForCode(WW.data.exp_by_name[WW.config.starting_exp_type]).Active = true
        end
        WW.status:Set("revision", (tonumber(WW.status:Get("revision")) or 0) + 1)
    end)
    WW.reset_hints()
    WW.refresh()
    if WW.error then print("Wedding Witch tracker: " .. WW.error) end
end

local function on_item(index, id)
    if not WW.config or WW.received[index] then return end
    local entry = WW.data.items[id]
    if not entry then return end
    WW.received[index] = true
    local item = Tracker:FindObjectForCode(entry.code)
    if entry.kind == "toggle" then
        item.Active = true
    else
        item.AcquiredCount = math.min(entry.cap, item.AcquiredCount + 1)
    end
end

local function on_location(id)
    local location = WW.data.locations[id]
    if not location or not WW.enabled(location) then return end
    WW.checked[id] = true
    Tracker:FindObjectForCode(location.code).AvailableChestCount = 0
end

Archipelago:AddClearHandler("ww_clear", on_clear)
Archipelago:AddItemHandler("ww_item", on_item)
Archipelago:AddLocationHandler("ww_location", on_location)
