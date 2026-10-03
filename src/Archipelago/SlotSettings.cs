using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
namespace WeddingWitchArchipelago.Archipelago;

public class SlotSettings
{
    public string GoalDifficulty;
    public int GoalForms;
    public string StartingExp;
    public int[] FlowerChecks;
    public int PoolSize;
    public bool SkillsFromLevelUps;
    public HashSet<string> AchievementChecks = new HashSet<string>();
    public static SlotSettings Defaults() => new SlotSettings { GoalDifficulty="Normal",GoalForms=1,StartingExp="Beast",FlowerChecks=new[]{5,6,6},PoolSize=80,SkillsFromLevelUps=true };
    public static SlotSettings FromSlotData(Dictionary<string,object> data) {
        if(data==null || !data.TryGetValue("schema_version",out var schema))
            throw new InvalidOperationException("Custom-rules seed schema is missing");
        int version = Convert.ToInt32(schema);
        if (version < 2 || version > 4)
            throw new InvalidOperationException("This client requires a custom-rules schema 2, 3 or 4 seed. Keep prototype/upstream seeds with their original client.");
        var s=Defaults();
        s.SkillsFromLevelUps = version == 4;
        s.PoolSize = s.SkillsFromLevelUps ? 80 : 142;
        s.GoalDifficulty=Convert.ToString(data["difficulty"]);
        s.GoalForms=Convert.ToInt32(data["transformEnd"]);
        s.StartingExp=Convert.ToString(data["starting_exp_type"]);
        s.FlowerChecks=JArray.FromObject(data["flower_checks"]).Values<int>().ToArray();
        if (version >= 3) {
            if (!data.TryGetValue("achievement_checks", out var achievements))
                throw new InvalidOperationException("Achievement check list is missing");
            var ids = JArray.FromObject(achievements).Values<string>().ToArray();
            s.AchievementChecks.UnionWith(ids);
            if (ids.Length != AchievementCatalog.All.Length || !s.AchievementChecks.SetEquals(AchievementCatalog.All.Select(entry => entry.Id)))
                throw new InvalidOperationException("Achievement checks differ from this client build");
        }
        if(!Locations.TryParseDifficulty(s.GoalDifficulty,out _) || s.GoalForms<1 || s.GoalForms>7 || !ApItems.ExpTypes.Contains(s.StartingExp) || s.FlowerChecks.Length!=3 || s.FlowerChecks.Any(n=>n<0||n>500))
            throw new InvalidOperationException("Invalid custom slot options");
        if (Convert.ToInt32(data["pool_size"])!=s.PoolSize) throw new InvalidOperationException("Item pool differs from this seed schema");
        if (18 + 6 + s.GoalForms + s.FlowerChecks.Sum() + s.AchievementChecks.Count != s.PoolSize)
            throw new InvalidOperationException("Location budget differs from this client build");
        if (s.SkillsFromLevelUps) {
            if (!data.TryGetValue("skill_mode", out var mode) || Convert.ToString(mode) != "level_up" || data.ContainsKey("skill_caps") || data.ContainsKey("skill_classes"))
                throw new InvalidOperationException("Schema 4 requires native level-up skills without AP skill ranks");
        } else {
            var caps=JObject.FromObject(data["skill_caps"]);
            if(caps.Count!=SkillCatalog.All.Length || SkillCatalog.All.Any(skill => (int?)caps[skill.Class]!=skill.MaxLevel))
                throw new InvalidOperationException("Skill rank limits differ from this client build");
        }
        return s;
    }
}
