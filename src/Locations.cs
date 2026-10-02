using System;
using System.Collections.Generic;
namespace WeddingWitchArchipelago;
public static class Locations
{
    public static string Map(Difficulty d, int n) => Valid(d) && n >= 1 && n <= 5 + (int)d ? $"{d} Map {n}" : null;
    public static string Flower(Difficulty d, int n) => Valid(d) && n >= 1 && n <= 500 ? $"{d} Flower {n}" : null;
    public static string Ending(int n) => n >= 1 && n <= 7 ? $"Transform Ending {n}" : null;
    public static string Form(BodyState form) => form switch {
        BodyState.BigBreast => "Full Transformation Big Breasts",
        BodyState.SmallBreast => "Full Transformation Small Breasts",
        BodyState.Corruption => "Full Transformation Corruption",
        BodyState.Beast => "Full Transformation Beast",
        BodyState.Muscle => "Full Transformation Muscle",
        BodyState.Hip => "Full Transformation Hips",
        _ => null
    };
    public static bool Valid(Difficulty d) => (int)d >= 0 && (int)d < 3;
    public static bool TryParseDifficulty(string name, out Difficulty d) => Enum.TryParse(name, true, out d) && Valid(d);
    public static IEnumerable<string> AllForDifficulty(Difficulty d) {
        if (!Valid(d)) yield break;
        for (int n = 1; n <= 5 + (int)d; n++) yield return Map(d,n);
        for (int n = 1; n <= ApState.Settings.FlowerChecks[(int)d]; n++) yield return Flower(d,n);
        if (string.Equals(d.ToString(), ApState.Settings.GoalDifficulty, StringComparison.OrdinalIgnoreCase))
            for (int n = 1; n <= ApState.Settings.GoalForms; n++) yield return Ending(n);
    }
}
