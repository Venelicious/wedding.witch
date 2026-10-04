WW = {data = require("generated"), config = nil, received = {}, checked = {}, error = nil}
Tracker:AddItems("items/items.json")
require("logic")
require("status")
require("settings")
require("hints")
Tracker:AddMaps("maps/maps.json")
Tracker:AddLocations("locations/locations.json")
Tracker:AddLayouts("layouts/tracker.json")
if Tracker.ActiveVariantUID ~= "standard" then
    Tracker:AddLayouts("layouts/" .. Tracker.ActiveVariantUID .. ".json")
end
require("autotracking")
WW.refresh()
