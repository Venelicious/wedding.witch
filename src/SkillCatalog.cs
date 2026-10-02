using System.Collections.Generic;
namespace WeddingWitchArchipelago;
public static class SkillCatalog
{
    // Standard level-up skills, separate from the permanent UpgradeCatalog.
    // Magic weapons keep their normal potion/condition requirements.
    public static readonly (string Class, string Display, int MaxLevel)[] All = {
        ("AbsorptionRadiusUp", "Absorption Radius", 3), ("AttackRadiusUp", "Attack Radius", 5), ("AttackSpeedUp", "Attack Speed", 5),
        ("BouquetAttackAngleUp", "Bouquet Angle", 5),
        ("BouquetAttackCriticalUp", "Bouquet Critical", 5), ("BouquetAttackPowerUp", "Bouquet Power", 6),
        ("BouquetAttackRangeUp", "Bouquet Range", 5), ("BouquetAttackSpeedUp", "Bouquet Speed", 5),
        ("CriticalRateUp", "Critical Rate", 5), ("HitCountUp", "Hit Count", 3),
        ("LifeTimeUp", "Lifetime", 5),
        ("MaxHpUp", "Maximum HP", 5),
        ("MoveSpeedUp", "Move Speed", 5)
    };
    public static int Cap(string className) { foreach(var entry in All) if(entry.Class==className) return entry.MaxLevel; return 0; }
    public static readonly Dictionary<string,string> ByClass = Build();
    static Dictionary<string,string> Build() {
        var result = new Dictionary<string,string>();
        foreach (var entry in All) result.Add(entry.Class, "Skill Unlock: " + entry.Display);
        return result;
    }
}
