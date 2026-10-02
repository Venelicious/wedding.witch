using HarmonyLib;
namespace WeddingWitchArchipelago;
[HarmonyPatch]
public static class ShopPatch
{
    [HarmonyPatch(typeof(UpgradeItem), nameof(UpgradeItem.OnClick))]
    [HarmonyPrefix]
    static bool Buy() => !ApState.Active;
    [HarmonyPatch(typeof(UpgradeItem), nameof(UpgradeItem.Upgrade))]
    [HarmonyPrefix]
    static bool DirectBuy() => !ApState.Active;
    [HarmonyPatch(typeof(UpgradeItem), nameof(UpgradeItem.Reset))]
    [HarmonyPrefix]
    static bool DirectReset() => !ApState.Active;
    [HarmonyPatch(typeof(UpgradeWitchCanvas), nameof(UpgradeWitchCanvas.ResetUpgrade))]
    [HarmonyPrefix]
    static bool Refund() => !ApState.Active;
    [HarmonyPatch(typeof(UpgradeItem), "SetValue")]
    [HarmonyPostfix]
    static void ShowCheckOnly(UpgradeItem __instance) {
        if (__instance.cost != null)
            __instance.cost.gameObject.SetActive(!ApState.Active);
        // Verified scene children: Image is the coin; active is the purchase highlight/plus.
        var coin = __instance.transform.Find("Image");
        if (coin != null) coin.gameObject.SetActive(!ApState.Active);
        var highlight = __instance.transform.Find("active");
        if (highlight != null)
            foreach (var graphic in highlight.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
                graphic.enabled = true;
        foreach (var button in __instance.GetComponentsInChildren<UnityEngine.UI.Button>(true)) {
            button.interactable = true; // Keep controller navigation and scrolling.
            var display = button.GetComponent<ApShopRowDisplay>();
            if (display == null) display = button.gameObject.AddComponent<ApShopRowDisplay>();
            display.Configure(button, highlight == null ? null : highlight.GetComponent<UnityEngine.UI.Image>());
        }
    }
    [HarmonyPatch(typeof(UpgradeWitchCanvas), nameof(UpgradeWitchCanvas.UpdateResetButton))]
    [HarmonyPostfix]
    static void HideRefund(UpgradeWitchCanvas __instance) {
        if (ApState.Active && __instance.resetButton != null)
            __instance.resetButton.gameObject.SetActive(false);
    }
    [HarmonyPatch(typeof(UpgradeItem), "OnEnable")]
    [HarmonyPrefix]
    static void RefreshRow(UpgradeItem __instance) {
        if (ApState.Active && __instance.researchData != null)
            __instance.level = ApState.LevelOf(__instance.researchData.loadKey);
    }
    public static void RefreshOpenShop() {
        foreach (var row in UnityEngine.Resources.FindObjectsOfTypeAll<UpgradeItem>()) {
            if (!row.gameObject.scene.IsValid() || row.researchData == null) continue;
            row.level = ApState.Active ? ApState.LevelOf(row.researchData.loadKey) :
                (ES3.KeyExists(row.researchData.loadKey,"GameData.es3") ? ES3.Load<int>(row.researchData.loadKey,"GameData.es3") : 0);
            if (!row.gameObject.activeInHierarchy) continue;
            AccessTools.Method(typeof(UpgradeItem),"SetValue").Invoke(row,null);
            row.SetLvSlot();
        }
    }
}
