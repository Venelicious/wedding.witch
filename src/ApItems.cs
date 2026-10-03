using System.Collections.Generic;
namespace WeddingWitchArchipelago;
public static class ApItems
{
    public static readonly string[] ExpTypes = { "BigBreast", "SmallBreast", "Corruption", "Beast", "Muscle", "Hip" };
    public static IEnumerable<string> AllNames() {
        yield return "Hard Wedding";
        yield return "Nightmare Wedding";
        foreach (var upgrade in UpgradeCatalog.All) yield return upgrade.ItemName;
        foreach (var exp in ExpTypes) yield return "EXP Unlock: " + exp;
        // Retained only to recognize items when reconnecting to schema 2/3 seeds.
        foreach (var skill in SkillCatalog.All) yield return "Skill Unlock: " + skill.Display;
    }
}
