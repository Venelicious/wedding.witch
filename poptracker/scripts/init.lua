WW = {data = require("generated"), config = nil, received = {}, checked = {}, error = nil}
Tracker:AddItems("items/items.json")
require("logic")
require("status")
Tracker:AddMaps("maps/maps.json")
Tracker:AddLocations("locations/locations.json")
Tracker:AddLayouts("layouts/tracker.json")
require("autotracking")
WW.refresh()
