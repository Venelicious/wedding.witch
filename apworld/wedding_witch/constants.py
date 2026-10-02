"""Custom rules, based on chickentuna/WeddingWitchMod e25c120f.
Separate ID range: incompatible with upstream and additive prototype seeds.
"""
GAME_NAME = "Wedding Witch"
BASE_ID = 252982000
DIFFICULTIES = (("Normal", 0, 5), ("Hard", 1, 6), ("Nightmare", 2, 7))
FORMS = (("Big Breasts", 1), ("Small Breasts", 2), ("Corruption", 3), ("Beast", 4), ("Muscle", 5), ("Hips", 6))
EXP_TYPES = ("BigBreast", "SmallBreast", "Corruption", "Beast", "Muscle", "Hip")
UPGRADES = (('Upgrade_WitchHat', "Witch's Hat", 5), ('Upgrade_Orb', "Witch's Orb", 5), ('Upgrade_Glove', "Witch's Gloves", 5), ('Upgrade_Ring', "Witch's Ring", 5), ('Upgrade_Watch', "Witch's Watch", 5), ('Upgrade_Mirror', 'Phantasm Mirror', 1), ('Upgrade_Shoes', "Witch's Shoes", 3), ('Upgrade_Necklace', "Witch's Necklace", 3), ('Upgrade_Earring', "Witch's Earrings", 5), ('Upgrade_Monocle', 'Magical Optics', 5), ('Upgrade_Cape', 'Cape of Protection', 5), ('Upgrade_Bracelet', "Witch's Bracelet", 5), ('Upgrade_HitomiBook', 'Book of Hitomi', 5), ('Upgrade_Maneki', 'Maneki Neko', 5), ('Upgrade_Bag', "Witch's Bag", 1), ('Upgrade_EyeOfBeholder', 'Eye Of The Beholder', 5), ('Upgrade_WitchDoll', "Witch's Doll", 5))
SKILLS = (('AbsorptionRadiusUp', 'Absorption Radius', 3), ('AttackRadiusUp', 'Attack Radius', 5), ('AttackSpeedUp', 'Attack Speed', 5), ('BouquetAttackAngleUp', 'Bouquet Angle', 5), ('BouquetAttackCriticalUp', 'Bouquet Critical', 5), ('BouquetAttackPowerUp', 'Bouquet Power', 6), ('BouquetAttackRangeUp', 'Bouquet Range', 5), ('BouquetAttackSpeedUp', 'Bouquet Speed', 5), ('CriticalRateUp', 'Critical Rate', 5), ('HitCountUp', 'Hit Count', 3), ('LifeTimeUp', 'Lifetime', 5), ('MaxHpUp', 'Maximum HP', 5), ('MoveSpeedUp', 'Move Speed', 5))
TOTAL_UPGRADE_LEVELS = sum(cap for _, _, cap in UPGRADES)
TOTAL_POOL_ITEMS = TOTAL_UPGRADE_LEVELS + 2 + 5 + sum(cap for _, _, cap in SKILLS)
FLOWER_MAX = 500
SCHEMA_VERSION = 2
