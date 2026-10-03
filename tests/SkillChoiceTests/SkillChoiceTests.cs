using System;
using System.Linq;
using System.Reflection;
using WeddingWitchArchipelago;

// Exercise the real Harmony hooks against a small headless native API model.
// Unity scene lifecycle, UI rendering and random weights require an in-game check.
internal static class SkillChoiceTests
{
    private static int assertions;

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        assertions++;
    }

    public static object Hook(string name, params object[] args) =>
        typeof(SkillUnlockPatch).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, args);

    private static EnchantManager Start(bool ap, bool nativeSkills = false)
    {
        SkillUnlockPatch.Forget();
        ApState.Active = ap;
        ApState.Settings.SkillsFromLevelUps = nativeSkills;
        ApState.OwnedSkillLevel = 0;
        UnityEngine.Resources.Items.Clear();
        var manager = new EnchantManager();
        EnchantManager.instance = manager;
        for (int i = 0; i < 6; i++) manager.startEnchantList.Add(new Enchant_Magic());
        manager.startEnchantList.Add(new Enchant_Magic { ConditionMet = false });
        manager.startEnchantList.Add(new BouquetAttackPowerUp());
        foreach (var item in manager.startEnchantList.Take(2)) item.gameObject.SetActive(true);
        Hook("ExpandStartingMagicPool", manager);
        return manager;
    }

    private static void Main()
    {
        var vanilla = Start(false);
        Assert(vanilla.possibleEnchant.Count == 2, "original mode retains its two starter spells");
        Assert(!vanilla.startEnchantList[2].gameObject.activeSelf, "original mode does not enable extra starters");

        var ap = Start(true);
        Assert(ap.possibleEnchant.Count == 6, "AP has a varied eligible starter pool");
        Assert(!ap.possibleEnchant.Contains(ap.startEnchantList[6]), "potion-gated spell remains unavailable");
        Assert(!ap.startEnchantList[7].gameObject.activeSelf, "pool expansion never activates an AP standard skill");
        ap.AddpossibleEnchant(ap.startEnchantList[7]);
        Assert(!ap.possibleEnchant.Contains(ap.startEnchantList[7]), "standard skill remains exclusively AP-owned");

        ap.GetRandomEnchantList();
        Assert(ap.selectedEnchantList.Count == 3, "early level-up offers three options");
        Assert(ap.selectedEnchantList.Distinct().Count() == 3, "options are distinct");
        var mastered = ap.possibleEnchant[0];
        for (int i = 0; i < mastered.enchant.maxEnchantCount; i++) mastered.DoEnchant();
        ap.GetRandomEnchantList();
        Assert(ap.selectedEnchantList.Count == 3, "three options remain after a spell is fully upgraded");
        Assert(!ap.selectedEnchantList.Contains(mastered), "fully upgraded spell is not offered again");
        ap.ChoiceSlots = 4;
        ap.GetRandomEnchantList();
        Assert(ap.selectedEnchantList.Count == 4, "native fourth-choice upgrade is preserved");

        var seen = new System.Collections.Generic.HashSet<Enchant>();
        for (int i = 0; i < 20; i++)
        {
            ap.GetRandomEnchantList();
            seen.UnionWith(ap.selectedEnchantList);
        }
        Assert(seen.Count == 5, "repeated rolls can use the remaining varied pool");
        Assert(!seen.Contains(ap.startEnchantList[6]), "repeated rolls do not bypass potion requirements");
        Assert(!seen.Contains(ap.startEnchantList[7]), "repeated rolls do not bypass AP standard-skill ownership");

        var native = Start(true, true);
        Assert(native.possibleEnchant.Count == 2, "schema 4 retains the native two random starter spells");
        Assert(!native.startEnchantList[2].gameObject.activeSelf, "schema 4 does not enable extra starter spells");
        var standards = SkillCatalog.All.Select(entry => {
            var skill = (Enchant)Activator.CreateInstance(typeof(Enchant).Assembly.GetType(entry.Class));
            skill.enchant.maxEnchantCount = entry.MaxLevel;
            UnityEngine.Resources.Items.Add(skill);
            skill.gameObject.SetActive(true);
            return skill;
        }).ToArray();
        Assert(standards.Length == 13 && standards.All(native.possibleEnchant.Contains), "all thirteen standard skills join the native pool");
        Assert(native.possibleEnchant.Count == 15, "native pool contains standard skills and two starter spells");
        GlobalStat.instance = new GlobalStat();
        PlayableCharacter.instance = new PlayableCharacter();
        PlayerMagnet.instance = new PlayerMagnet();
        ApState.OwnedSkillLevel = 6;
        SkillUnlockPatch.Tick();
        Assert(standards.All(skill => skill.currentEnchantCount == 0), "schema 4 never automatically grants AP skill ranks");
        native.GetRandomEnchantList();
        Assert(native.selectedEnchantList.Count == 3 && native.selectedEnchantList.Distinct().Count() == 3,
            "schema 4 offers three distinct native choices");
        Assert(standards.All(native.possibleEnchant.Contains), "choice hook leaves standard skills eligible");
        var upgraded = standards[0]; upgraded.DoEnchant();
        Assert(upgraded.currentEnchantCount == 1 && native.possibleEnchant.Contains(upgraded), "chosen standard skill advances in this run and remains upgradeable");
        while (upgraded.currentEnchantCount < upgraded.enchant.maxEnchantCount) upgraded.DoEnchant();
        native.GetRandomEnchantList();
        Assert(native.selectedEnchantList.Count == 3 && !native.selectedEnchantList.Contains(upgraded),
            "three choices remain after a standard skill reaches its cap");
        native.ChoiceSlots = 4; native.GetRandomEnchantList();
        Assert(native.selectedEnchantList.Count == 4, "schema 4 preserves the fourth-choice upgrade");
        seen.Clear();
        for (int i = 0; i < 100; i++) { native.GetRandomEnchantList(); seen.UnionWith(native.selectedEnchantList); }
        Assert(seen.Count == 14 && standards.Skip(1).All(seen.Contains), "repeated level-ups draw different standard skills and spells");
        foreach (var spell in native.possibleEnchant.OfType<Enchant_Magic>().ToArray())
            while (spell.currentEnchantCount < spell.enchant.maxEnchantCount) spell.DoEnchant();
        native.ChoiceSlots = 3; native.GetRandomEnchantList();
        Assert(native.selectedEnchantList.Count == 3 && native.selectedEnchantList.All(standards.Contains),
            "three standard-skill choices remain even after both starter spells are fully upgraded");
        Start(true, true);
        var freshSkill = new AbsorptionRadiusUp(); freshSkill.gameObject.SetActive(true);
        SkillUnlockPatch.Tick();
        Assert(freshSkill.currentEnchantCount == 0 && EnchantManager.instance.possibleEnchant.Contains(freshSkill),
            "a fresh run starts the skill at zero with no carried-over rank");
        Console.WriteLine("PASS " + assertions + " skill-choice regression assertions");
    }
}
