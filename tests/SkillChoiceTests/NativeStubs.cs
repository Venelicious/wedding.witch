using System;
using System.Collections.Generic;
using System.Linq;

namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class HarmonyPatch : Attribute
    {
        public HarmonyPatch() { }
        public HarmonyPatch(Type type, string method) { }
    }
    public sealed class HarmonyPrefix : Attribute { }
    public sealed class HarmonyPostfix : Attribute { }
}

namespace UnityEngine
{
    public static class Time { public static float timeSinceLevelLoad = 1; }
    public static class Resources {
        public static readonly List<object> Items = new List<object>();
        public static T[] FindObjectsOfTypeAll<T>() => Items.OfType<T>().ToArray();
    }
    public sealed class GameObject
    {
        public bool activeSelf;
        public UnityEngine.SceneManagement.Scene scene;
        public Action OnStart;
        public void SetActive(bool active)
        {
            bool starts = active && !activeSelf;
            activeSelf = active;
            if (starts) OnStart?.Invoke();
        }
    }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { public string name; public bool IsValid() => true; }
    public static class SceneManager { public static Scene GetActiveScene() => new Scene { name = "Adventure" }; }
}

public class Enchant
{
    public UnityEngine.GameObject gameObject = new UnityEngine.GameObject();
    public EnchantData enchant = new EnchantData();
    public int currentEnchantCount;
    public bool ConditionMet = true;
    public bool isActiveAndEnabled => gameObject.activeSelf;
    public Enchant()
    {
        gameObject.OnStart = () => { if (ConditionMet) EnchantManager.instance.AddpossibleEnchant(this); };
    }
    public void DoEnchant()
    {
        currentEnchantCount++;
        if (currentEnchantCount >= enchant.maxEnchantCount) EnchantManager.instance.possibleEnchant.Remove(this);
    }
}
public sealed class EnchantData { public int maxEnchantCount = 6; }
public sealed class Enchant_Magic : Enchant { }
public sealed class BouquetAttackPowerUp : Enchant { }
public sealed class AbsorptionRadiusUp : Enchant { }
public sealed class AttackRadiusUp : Enchant { }
public sealed class AttackSpeedUp : Enchant { }
public sealed class BouquetAttackAngleUp : Enchant { }
public sealed class BouquetAttackCriticalUp : Enchant { }
public sealed class BouquetAttackRangeUp : Enchant { }
public sealed class BouquetAttackSpeedUp : Enchant { }
public sealed class CriticalRateUp : Enchant { }
public sealed class HitCountUp : Enchant { }
public sealed class LifeTimeUp : Enchant { }
public sealed class MaxHpUp : Enchant { }
public sealed class MoveSpeedUp : Enchant { }
public sealed class EnchantManager
{
    public static EnchantManager instance;
    public List<Enchant> startEnchantList = new List<Enchant>();
    public List<Enchant> possibleEnchant = new List<Enchant>();
    public List<Enchant> newEnchantList = new List<Enchant>();
    public List<Enchant> selectedEnchantList = new List<Enchant>();
    public int ChoiceSlots = 3;
    private readonly Random random = new Random(42);
    public void AddpossibleEnchant(Enchant item)
    {
        if (!(bool)SkillChoiceTests.Hook("CanAdd", item)) return;
        if (!possibleEnchant.Contains(item)) { possibleEnchant.Add(item); newEnchantList.Add(item); }
    }
    public void FillEnchantList() { }
    public void SetFirstMagic() { }
    public void GetRandomEnchantList()
    {
        SkillChoiceTests.Hook("BeforeChoices", this);
        selectedEnchantList = possibleEnchant.OrderBy(_ => random.Next()).Take(ChoiceSlots).ToList();
    }
}
public sealed class GlobalStat { public static GlobalStat instance; }
public sealed class PlayableCharacter { public static PlayableCharacter instance; }
public sealed class PlayerMagnet { public static PlayerMagnet instance; }

namespace WeddingWitchArchipelago
{
    public sealed class TestSlotSettings { public bool SkillsFromLevelUps; }
    public static class ApState {
        public static bool Active;
        public static TestSlotSettings Settings = new TestSlotSettings();
        public static int OwnedSkillLevel;
        public static int SkillLevel(string className) => OwnedSkillLevel;
    }
    public static class Plugin { public static TestLogger Logger = new TestLogger(); }
    public sealed class TestLogger { public void LogInfo(string message) { } }
}
