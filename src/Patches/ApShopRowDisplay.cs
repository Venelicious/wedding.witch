using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace WeddingWitchArchipelago;
public class ApShopRowDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    Button button;
    Image background;
    SpriteState originalState;
    Sprite originalSprite;
    Outline frame;
    bool captured, hovered, selected;
    public void Configure(Button owner, Image image) {
        if (image == null) return;
        if (!captured) {
            button = owner; background = image;
            originalState = owner.spriteState;
            originalSprite = image.sprite;
            frame = image.gameObject.AddComponent<Outline>();
            frame.effectColor = new Color(1f,0.83f,0.38f,1f);
            frame.effectDistance = new Vector2(2f,-2f);
            frame.useGraphicAlpha = false;
            captured = true;
        }
        if (ApState.Active) {
            var state = originalState;
            state.highlightedSprite = state.pressedSprite = state.selectedSprite = state.disabledSprite = originalSprite;
            button.spriteState = state;
            background.overrideSprite = originalSprite;
        } else {
            button.spriteState = originalState;
            background.overrideSprite = null;
        }
        selected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
        Refresh();
    }
    void Refresh() { if (frame != null) frame.enabled = ApState.Active && (hovered || selected); }
    public void OnPointerEnter(PointerEventData data) { hovered = true; Refresh(); }
    public void OnPointerExit(PointerEventData data) { hovered = false; Refresh(); }
    public void OnSelect(BaseEventData data) { selected = true; Refresh(); }
    public void OnDeselect(BaseEventData data) { selected = false; Refresh(); }
    void OnDisable() { hovered = selected = false; Refresh(); }
}
