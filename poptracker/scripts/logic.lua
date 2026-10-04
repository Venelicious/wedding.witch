-- Rules mirror schema 4 of our APWorld. Accessibility means AP logic, not an
-- estimate of combat strength or of a native achievement's partial progress.
WW.config = {schema_version = 4, transformEnd = 3, difficulty = "normal",
    starting_exp_type = "Beast", flower_checks = {4, 5, 6}}

function ww_supported()
    return WW.config ~= nil
end

function ww_difficulty(difficulty)
    if not ww_supported() then return false end
    return difficulty == "normal" or Tracker:ProviderCountForCode(difficulty) > 0
end

function ww_flower(difficulty, number)
    if not ww_supported() then return false end
    return tonumber(number) <= WW.config.flower_checks[tonumber(difficulty)]
end

function ww_ending_visible(number)
    return ww_supported() and tonumber(number) <= WW.config.transformEnd
end

function ww_ending(number)
    if not ww_supported() or not ww_difficulty(WW.config.difficulty) then return false end
    local count = 0
    for _, code in ipairs(WW.data.exp_codes) do
        if Tracker:ProviderCountForCode(code) > 0 then count = count + 1 end
    end
    local n = tonumber(number)
    return count >= (n == 1 and 1 or math.max(2, n - 1))
end

function WW.enabled(location)
    if not ww_supported() then return false end
    if location.kind == "flower" then
        return location.number <= WW.config.flower_checks[location.difficulty]
    elseif location.kind == "ending" then
        return location.number <= WW.config.transformEnd
    end
    return true
end

function WW.bulk(callback)
    local previous = Tracker.BulkUpdate
    Tracker.BulkUpdate = true
    local ok, message = pcall(callback)
    Tracker.BulkUpdate = previous
    if not ok then error(message) end
end

function WW.validate(slot)
    if type(slot) ~= "table" or slot.schema_version ~= 4 or slot.pool_size ~= 80
        or slot.skill_mode ~= "level_up" or slot.integration_mode ~= "unlock_custom" then
        return nil, "Unsupported seed: this pack needs our schema-4 APWorld (0.5.0+)."
    end
    local goal = slot.transformEnd
    if type(goal) ~= "number" or goal % 1 ~= 0 or goal < 1 or goal > 7 then
        return nil, "Invalid ending goal in slot data."
    end
    if slot.difficulty ~= "normal" and slot.difficulty ~= "hard" and slot.difficulty ~= "nightmare" then
        return nil, "Invalid goal difficulty in slot data."
    end
    if not WW.data.exp_by_name[slot.starting_exp_type] then return nil, "Invalid starting potion type." end
    if type(slot.flower_checks) ~= "table" or #slot.flower_checks ~= 3 then return nil, "Invalid flower counts." end
    local flowers, sum = {}, 0
    for i = 1, 3 do
        local n = slot.flower_checks[i]
        if type(n) ~= "number" or n % 1 ~= 0 or n < 0 or n > 17 then return nil, "Invalid flower count." end
        flowers[i], sum = n, sum + n
    end
    if sum ~= 18 - goal then return nil, "Flower counts do not match the 80-check pool." end
    if type(slot.map_counts) ~= "table" or slot.map_counts[1] ~= 5
        or slot.map_counts[2] ~= 6 or slot.map_counts[3] ~= 7 then return nil, "Invalid map counts." end
    if type(slot.achievement_checks) ~= "table" or #slot.achievement_checks ~= 38 then
        return nil, "Invalid achievement list."
    end
    for i, key in ipairs(WW.data.achievement_keys) do
        if slot.achievement_checks[i] ~= key then return nil, "Unknown achievement contract." end
    end
    return {schema_version = 4, transformEnd = goal, difficulty = slot.difficulty,
        starting_exp_type = slot.starting_exp_type, flower_checks = flowers}
end
