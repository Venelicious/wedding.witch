from BaseClasses import ItemClassification
from .constants import BASE_ID, UPGRADES, EXP_TYPES
ITEM_NAME_TO_ID = {name: BASE_ID + 2000 + i for i, (_, name, _) in enumerate(UPGRADES)}
ITEM_NAME_TO_ID.update({"Hard Wedding": BASE_ID + 2050, "Nightmare Wedding": BASE_ID + 2051})
ITEM_NAME_TO_ID.update({f"EXP Unlock: {name}": BASE_ID + 2100 + i for i, name in enumerate(EXP_TYPES)})
ITEM_NAME_GROUPS = {"Passives": {name for _, name, _ in UPGRADES}, "EXP Types": {f"EXP Unlock: {name}" for name in EXP_TYPES}}
def classification(name):
    # No combat-strength thresholds: skill/survival ability remains player skill.
    return ItemClassification.progression if name.startswith("EXP Unlock:") or name.endswith("Wedding") else ItemClassification.useful
def item_pool_names(starting_exp):
    return [name for _, name, cap in UPGRADES for _ in range(cap)] + ["Hard Wedding", "Nightmare Wedding"] + [f"EXP Unlock: {name}" for name in EXP_TYPES if name != starting_exp]
