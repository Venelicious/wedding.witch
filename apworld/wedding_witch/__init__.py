"""Custom Wedding Witch rules based on chickentuna/WeddingWitchMod."""
from BaseClasses import Item, ItemClassification, Location, Region
from Options import OptionError
from worlds.AutoWorld import World
from .constants import *
from .items import ITEM_NAME_TO_ID, ITEM_NAME_GROUPS, classification, item_pool_names
from .locations import LOCATION_NAME_TO_ID, locations_by_region
from .options import WeddingWitchOptions
class WeddingWitchItem(Item):
    game = GAME_NAME
class WeddingWitchLocation(Location):
    game = GAME_NAME

def allocate_flowers(requested, endings):
    budget = TOTAL_POOL_ITEMS - 18 - 6 - endings
    result = [max(0, n) for n in requested]
    automatic = [i for i, n in enumerate(requested) if n == -1]
    remaining = budget - sum(result)
    if remaining < 0 or (not automatic and remaining):
        raise OptionError(f"Wedding Witch needs exactly {budget} flower checks; configured {sum(result)}. Use -1 for automatic allocation.")
    weights = (16, 19, 21)
    for _ in range(remaining):
        available = [i for i in automatic if result[i] < FLOWER_MAX]
        if not available:
            raise OptionError("Flower budget exceeds the configured maximum")
        # Deterministic weighted allocation, with stable tie breaking.
        chosen = min(available, key=lambda i: (result[i] / weights[i], i))
        result[chosen] += 1
    return result

class WeddingWitchWorld(World):
    game = GAME_NAME
    options_dataclass = WeddingWitchOptions
    options: WeddingWitchOptions
    item_name_to_id = ITEM_NAME_TO_ID
    location_name_to_id = LOCATION_NAME_TO_ID
    item_name_groups = ITEM_NAME_GROUPS
    def generate_early(self):
        choice = self.options.starting_exp_type.value
        self.starting_exp = EXP_TYPES[self.random.randrange(6) if choice == 0 else choice - 1]
        self.flowers = allocate_flowers([self.options.flower_checks_normal.value, self.options.flower_checks_hard.value, self.options.flower_checks_nightmare.value], self.options.transformEnd.value)
        self.goal_difficulty = DIFFICULTIES[self.options.difficulty.value][0]
        self.region_names = locations_by_region(self.flowers, self.options.transformEnd.value, self.goal_difficulty)
        self.multiworld.push_precollected(self.create_item(f"EXP Unlock: {self.starting_exp}"))
    def create_item(self, name):
        return WeddingWitchItem(name, classification(name), ITEM_NAME_TO_ID[name], self.player)
    def create_items(self):
        self.multiworld.itempool += [self.create_item(name) for name in item_pool_names(self.starting_exp)]
    def get_filler_item_name(self):
        return "Witch's Orb"
    def create_regions(self):
        menu = Region("Menu", self.player, self.multiworld)
        self.multiworld.regions.append(menu)
        for name, locations in self.region_names.items():
            region = Region(name, self.player, self.multiworld)
            region.add_locations({n: LOCATION_NAME_TO_ID[n] for n in locations}, WeddingWitchLocation)
            self.multiworld.regions.append(region)
            gate = None if name in ("Normal", "Transformations") else lambda state, n=name: state.has(n + " Wedding", self.player)
            menu.connect(region, rule=gate)
        # An AP event models the gameplay victory for generation/spoilers.
        # The client sends StatusUpdate after the actual distinct ending count.
        goal = self.multiworld.get_region(self.goal_difficulty, self.player)
        event = WeddingWitchLocation(self.player, "Goal Complete", None, goal)
        event.place_locked_item(WeddingWitchItem("Victory", ItemClassification.progression, None, self.player))
        goal.locations.append(event)
    def ending_rule(self, count):
        return lambda state: sum(state.has(f"EXP Unlock: {name}", self.player) for name in EXP_TYPES) >= (1 if count == 1 else max(2, count - 1))
    def set_rules(self):
        for (name, _), exp in zip(FORMS, EXP_TYPES):
            self.multiworld.get_location(f"Full Transformation {name}", self.player).access_rule = lambda state, e=exp: state.has(f"EXP Unlock: {e}", self.player)
        for n in range(1, self.options.transformEnd.value + 1):
            self.multiworld.get_location(f"Transform Ending {n}", self.player).access_rule = self.ending_rule(n)
        self.multiworld.get_location("Goal Complete", self.player).access_rule = self.ending_rule(self.options.transformEnd.value)
        self.multiworld.completion_condition[self.player] = lambda state: state.has("Victory", self.player)
    def fill_slot_data(self):
        return {"schema_version": SCHEMA_VERSION, "integration_mode": "unlock_custom", "transformEnd": self.options.transformEnd.value, "difficulty": self.goal_difficulty.lower(), "starting_exp_type": self.starting_exp, "flower_checks": self.flowers, "map_counts": [5, 6, 7], "skill_classes": [key for key, _, _ in SKILLS], "skill_caps": {key: cap for key, _, cap in SKILLS}, "pool_size": TOTAL_POOL_ITEMS}
