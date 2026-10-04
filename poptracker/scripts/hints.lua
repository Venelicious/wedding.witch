-- Standard AP hint storage only: no scouts, checks or server writes.
local PAGE_SIZE = 6
local state = {key = nil, records = {}, page = 1, slot = nil}
WW.hints = state
WW.hint_rows = {}
for n = 1, PAGE_SIZE * 3 do WW.hint_rows[n] = WW.line("ww_hint_" .. n) end
WW.hint_summary = WW.line("ww_hint_summary")
WW.hint_previous = WW.display("ww_hint_previous", "Vorherige Hinweis-Seite", "images/blank.png")
WW.hint_next = WW.display("ww_hint_next", "Nächste Hinweis-Seite", "images/blank.png")
WW.hint_previous:SetOverlay("< Zurück")
WW.hint_next:SetOverlay("Weiter >")

local function integer(value)
    return type(value) == "number" and value % 1 == 0
end

local function found(hint)
    return hint.found or hint.status == 40 or
        (hint.finding_player == state.slot and WW.checked[hint.location] == true)
end

local function name(method, id, game, fallback)
    local result
    if game then result = Archipelago[method](Archipelago, id, game)
    else result = Archipelago[method](Archipelago, id) end
    return type(result) == "string" and result ~= "" and result or fallback .. tostring(id)
end

local function shorten(value)
    -- ASCII control characters never become extra rows. UTF-8 stays intact.
    value = value:gsub("[%c]", " ")
    if utf8.len(value) and utf8.len(value) > 76 then
        return value:sub(1, utf8.offset(value, 74) - 1) .. "…"
    end
    return value
end

function WW.refresh_hints()
    table.sort(state.records, function(a, b)
        local af, bf = found(a), found(b)
        if af ~= bf then return not af end
        local ap, bp = a.status == 30, b.status == 30
        if ap ~= bp then return ap end
        if a.finding_player ~= b.finding_player then return a.finding_player < b.finding_player end
        if a.location ~= b.location then return a.location < b.location end
        return a.receiving_player < b.receiving_player
    end)
    local pages = math.max(1, math.ceil(#state.records / PAGE_SIZE))
    state.page = math.max(1, math.min(state.page, pages))
    local summary
    if not state.key then
        summary = "AP verbinden, um Hinweise für deinen Slot zu laden."
    elseif #state.records == 0 then
        summary = "Noch keine AP-Hinweise für diesen Slot bekannt."
    else
        summary = string.format("%d AP-Hinweise · Seite %d/%d", #state.records, state.page, pages)
        if AutoTracker:GetConnectionState("AP") ~= 3 then summary = summary .. " · letzter Stand (offline)" end
    end
    WW.hint_summary.Name = summary
    WW.hint_summary:SetOverlay(summary)
    for row = 1, PAGE_SIZE do
        local hint = state.records[(state.page - 1) * PAGE_SIZE + row]
        local lines = {}
        local color = "#f5f0df"
        if hint then
            local recipient = name("GetPlayerAlias", hint.receiving_player, nil, "Spieler ")
            local finder = name("GetPlayerAlias", hint.finding_player, nil, "Spieler ")
            local item = name("GetItemName", hint.item, Archipelago:GetPlayerGame(hint.receiving_player), "Item #")
            local location = name("GetLocationName", hint.location, Archipelago:GetPlayerGame(hint.finding_player), "Location #")
            local status = found(hint) and "Gefunden" or
                ({[0] = "Offen", [10] = "Keine Priorität", [20] = "Vermeiden", [30] = "Priorität"})[hint.status] or "Offen"
            lines = {item .. " → " .. recipient, location .. " · bei " .. finder,
                status .. (hint.entrance ~= "" and " · Eingang: " .. hint.entrance or "")}
            color = found(hint) and "#a5abc5" or hint.status == 30 and "#e7bd73" or "#f5f0df"
        end
        for n = 1, 3 do
            local item = WW.hint_rows[(row - 1) * 3 + n]
            item.Name = lines[n] or ""
            item:SetOverlay(shorten(lines[n] or ""))
            item:SetOverlayColor(color)
        end
    end
end

WW.hint_previous.OnLeftClickFunc = function()
    state.page = state.page - 1
    WW.refresh_hints()
end
WW.hint_next.OnLeftClickFunc = function()
    state.page = state.page + 1
    WW.refresh_hints()
end

local function update(key, value)
    if key ~= state.key or not WW.config or type(value) ~= "table" then return end
    local unique = {}
    for _, hint in pairs(value) do
        if type(hint) == "table" and integer(hint.finding_player) and integer(hint.receiving_player)
            and integer(hint.location) and integer(hint.item)
            and (hint.finding_player == state.slot or hint.receiving_player == state.slot) then
            local identity = hint.finding_player .. ":" .. hint.location .. ":" .. hint.receiving_player
            local entry = {finding_player = hint.finding_player, receiving_player = hint.receiving_player,
                location = hint.location, item = hint.item, found = hint.found == true,
                status = integer(hint.status) and hint.status or 0,
                entrance = type(hint.entrance) == "string" and hint.entrance or ""}
            if not unique[identity] or found(entry) or not found(unique[identity]) then unique[identity] = entry end
        end
    end
    state.records = {}
    for _, hint in pairs(unique) do table.insert(state.records, hint) end
    WW.refresh_hints()
end

function WW.clear_hints()
    state.key, state.slot, state.records, state.page = nil, nil, {}, 1
    WW.refresh_hints()
end

function WW.reset_hints()
    WW.clear_hints()
    if WW.config then
        local team, slot = Archipelago.TeamNumber, Archipelago.PlayerNumber
        if integer(team) and team >= 0 and integer(slot) and slot > 0 then
            state.slot = slot
            state.key = "_read_hints_" .. team .. "_" .. slot
            Archipelago:SetNotify({state.key})
            Archipelago:Get({state.key})
        end
    end
    WW.refresh_hints()
end

Archipelago:AddRetrievedHandler("ww_hints", update)
Archipelago:AddSetReplyHandler("ww_hints", update)
