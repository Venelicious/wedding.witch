using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WeddingWitchArchipelago;
public static class DifficultyProgress
{
    public static void Refresh(SelectDifficultyCanvas screen) {
        if (screen == null || !ApState.Active) return;
        var original = screen.transform.Find("Panel/PotionInfo") as RectTransform;
        if (original == null) return;
        var node = original.parent.Find("ApProgress");
        TextMeshProUGUI label;
        if (node == null) {
            // Duplicate the native card, including its ornate sprite and text styling.
            var root = Object.Instantiate(original.gameObject,original.parent,false);
            root.name = "ApProgress";
            var rect = (RectTransform)root.transform;
            rect.anchoredPosition = original.anchoredPosition + new Vector2(0f,-original.rect.height-28f);
            rect.localScale = original.localScale;
            var info = rect.Find("Info");
            label = info == null ? null : info.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (label == null) { Object.Destroy(root); return; }
            label.transform.SetParent(info,false);
            label.gameObject.name = "ProgressText";
            for (int i=rect.childCount-1;i>=0;i--) {
                var child = rect.GetChild(i);
                if (child != info) Object.Destroy(child.gameObject);
            }
            for (int i=info.childCount-1;i>=0;i--) {
                var child = info.GetChild(i);
                if (child != label.transform) Object.Destroy(child.gameObject);
            }
            var content = (RectTransform)label.transform;
            content.anchorMin = Vector2.zero;
            content.anchorMax = Vector2.one;
            content.offsetMin = new Vector2(20f,20f);
            content.offsetMax = new Vector2(-20f,-20f);
            label.alignment = TextAlignmentOptions.TopLeft;
            label.enableAutoSizing = true;
            label.fontSizeMin = 22f;
            label.fontSizeMax = 28f;
            label.raycastTarget = false;
            foreach (var image in root.GetComponentsInChildren<Image>(true)) image.raycastTarget = false;
        } else label = node.GetComponentInChildren<TextMeshProUGUI>();
        if (label == null) return;
        var p=ApState.Progress;
        var s=ApState.Settings;
        Locations.TryParseDifficulty(s.GoalDifficulty,out var goal);
        string difficulty = goal == Difficulty.Normal ? "Normal" : goal == Difficulty.Hard ? "Schwer" : "Albtraum";
        label.text = $"<b>Archipelago · {p.Checks.Count}/142 Checks</b>\n" +
            $"Normal: {p.Flowers[0]}/{s.FlowerChecks[0]} Blumen | Siege: {p.Wins[0]}\n" +
            $"Schwer: {p.Flowers[1]}/{s.FlowerChecks[1]} Blumen | Siege: {p.Wins[1]}\n" +
            $"Albtraum: {p.Flowers[2]}/{s.FlowerChecks[2]} Blumen | Siege: {p.Wins[2]}\n" +
            $"Ziel: {p.Endings[(int)goal].Count}/{s.GoalForms} verschiedene Enden\n" +
            $"Schwierigkeit: {difficulty}";
    }
}
