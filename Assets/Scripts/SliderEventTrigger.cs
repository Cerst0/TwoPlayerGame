using UnityEngine;
using UnityEngine.EventSystems;


public class SliderEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Texture2D cursorNormal;
    public Texture2D cursorClick;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(cursorClick, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
    }
}
