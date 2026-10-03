using System.Collections.Generic;
namespace WeddingWitchArchipelago;

public static class AchievementCatalog
{
    // Native achievement IDs and AP location names; contract with achievements.py.
    public static readonly (string Id, string Display)[] All = {
        ("WIN1", "Win 1 Run"),
        ("WIN10", "Win 10 Runs"),
        ("ClearNormal", "Clear Normal"),
        ("ClearHard", "Clear Hard"),
        ("ClearNightmare", "Clear Nightmare"),
        ("LOSE1", "Lose 1 Run"),
        ("LOSE10", "Lose 10 Runs"),
        ("HitByGuardian", "Hit by Guardians 10 Times"),
        ("Traveler", "Travel 1000 Units"),
        ("Florist", "Collect 100 Flowers"),
        ("TreasureHunter", "Break 100 Pumpkins"),
        ("SkillMaster", "Use Skills 100 Times"),
        ("Critical1000", "Land 1000 Critical Hits"),
        ("Dodge100", "Dodge 100 Times"),
        ("HPRegen100", "Regenerate HP 100 Times"),
        ("Hit100", "Take 100 Hits"),
        ("NoHitClear", "Clear with at Most 10 Hits"),
        ("KILL1000", "Defeat 1000 Enemies"),
        ("KILL10000", "Defeat 10000 Enemies"),
        ("KILL20000", "Defeat 20000 Enemies"),
        ("BlastMaster", "Master 10 Explosion Spells"),
        ("MissileMaster", "Master 10 Missile Spells"),
        ("MeleeMaster", "Master 10 Melee Spells"),
        ("AreaMaster", "Master 10 Area Spells"),
        ("FamiliarMaster", "Master 10 Familiar Spells"),
        ("DarkMagicMaster", "Master 10 Curse Spells"),
        ("Corruption10", "Drink 10 Corruption Potions"),
        ("BeastPotion10", "Drink 10 Beast Potions"),
        ("BigBreastPotion10", "Drink 10 Big Breast Potions"),
        ("MusclePotion10", "Drink 10 Muscle Potions"),
        ("HipPotion10", "Drink 10 Hip Potions"),
        ("SmallBreastPotion10", "Drink 10 Small Breast Potions"),
        ("CorruptionPotionForm", "Fully Transform into Corruption"),
        ("BeastPotionForm", "Fully Transform into Beast"),
        ("BigBreastPotionForm", "Fully Transform into Big Breasts"),
        ("MusclePotionForm", "Fully Transform into Muscle"),
        ("HipPotionForm", "Fully Transform into Hips"),
        ("SmallBreastPotionForm", "Fully Transform into Small Breasts")
    };
    static readonly Dictionary<string, string> ById = Build();
    static Dictionary<string, string> Build() {
        var result = new Dictionary<string, string>();
        foreach (var entry in All) result.Add(entry.Id, "Achievement: " + entry.Display);
        return result;
    }
    public static string Location(string id) => id != null && ById.TryGetValue(id, out var name) ? name : null;
}
