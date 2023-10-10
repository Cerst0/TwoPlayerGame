using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UniversalButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    Status status;

    [Header("Basics")]
    Texture2D cursorNormal;
    Texture2D cursorClick;
    public bool MainButton;
    public bool ignoreAlpha;
    public string buttonName;

    [Header("Set Color")]
    public bool setColorOnHover;
    public Image ColorTargetImage;
    public Color TargetColor;
    Color startColor;
    public bool ignoreStartColorAlpha;

    [Header("Set Active On Hover")]
    public bool setObjectActiveOnHover;
    public GameObject ReferenceObject;
    public bool isImage;
    public Sprite sprite;
    Sprite spriteStart;
    public Material material;
    Material materialStart;

    [Header("Change Text Color")]
    public TMP_Text text;
    public Color targetTextColor;
    public bool textColorIgnoreActiveStatus;
    Color textStartColor;

    [Header("Animation")]
    public bool animation;
    public Color clickColor;

    [Header("Size")]
    public Vector2 selectedSize;
    public bool isFactor;
    public Vector2 startSize;

    [Header("Don't Edit")]
    public bool[] active = new bool[2];

    private void Start()
    {
        cursorNormal = Resources.Load<Texture2D>("cursorNormal");
        cursorClick = Resources.Load<Texture2D>("cursorClick");

        if (MainButton) { Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto); }

        if (text != null) { textStartColor = text.color; }

        if (setColorOnHover | animation)
        {
            if (ColorTargetImage == null) { ColorTargetImage = ReferenceObject.GetComponent<Image>(); }
            startColor = ColorTargetImage.color;
            if (ignoreStartColorAlpha) { startColor.a = 1; }
        }

        if (sprite != null) spriteStart = ReferenceObject.GetComponent<Image>().sprite;

        if (material != null) materialStart = ReferenceObject.GetComponent<Image>().material;

        if (!selectedSize.Equals(Vector2.zero))
        {
            if (startSize == Vector2.zero)
            {
                startSize = ReferenceObject.GetComponent<RectTransform>().localScale;
            }
            if (isFactor) { selectedSize = startSize * selectedSize; }
        }

        if (!ignoreAlpha) { GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f; }
    }

    private void Update()
    {
        if (status == null) { status = GameObject.FindGameObjectWithTag("Status").GetComponent<Status>(); }

        if (animation)
        {
            if (gameObject.GetComponent<Image>().color == clickColor)
            {
                LeanTween.value(gameObject, ColorTargetImage.color, startColor, 0.1f).setIgnoreTimeScale(true).setOnUpdate((Color color) =>
                {
                    ColorTargetImage.color = color;
                });
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(cursorClick, Vector2.zero, CursorMode.Auto);
        if (setColorOnHover)
        {
            LeanTween.value(ColorTargetImage.gameObject, ColorTargetImage.color, TargetColor, 0.25f).setIgnoreTimeScale(true).setOnUpdate((Color c) =>
            {
                ColorTargetImage.color = c;
            });
        }
        if (setObjectActiveOnHover)
        {
            if (isImage) ReferenceObject.GetComponent<Image>().enabled = true;
            else ReferenceObject.SetActive(true);
        }
        if (sprite != null) ReferenceObject.GetComponent<Image>().sprite = sprite;
        if (material != null) ReferenceObject.GetComponent<Image>().material = material;
        if (!selectedSize.Equals(Vector2.zero))
        {
            LeanTween.scale(ReferenceObject.GetComponent<RectTransform>(), selectedSize, 0.2f).setIgnoreTimeScale(true);
        }
        if (text != null)
        {
            LeanTween.value(gameObject, text.color, targetTextColor, 0.2f).setIgnoreTimeScale(true).setOnUpdate((Color c) =>
            {
                text.color = c;
            });
        }

        active[0] = true;
    }

    public void OnMouseOver()
    {
        Cursor.SetCursor(cursorClick, Vector2.zero, CursorMode.Auto);
        if (setObjectActiveOnHover)
        {
            if (isImage) ReferenceObject.GetComponent<Image>().enabled = true;
            else ReferenceObject.SetActive(true);
        }
        if (sprite != null) ReferenceObject.GetComponent<Image>().sprite = sprite;
        if (material != null) ReferenceObject.GetComponent<Image>().material = material;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);

        active[0] = false;
        if (active.All(active => active == false))
        {
            if (setColorOnHover)
            {
                LeanTween.value(ColorTargetImage.gameObject, ColorTargetImage.color, startColor, 0.25f).setIgnoreTimeScale(true).setOnUpdate((Color c) =>
                {
                    ColorTargetImage.color = c;
                });
            }
            if (setObjectActiveOnHover)
            {
                if (isImage) ReferenceObject.GetComponent<Image>().enabled = false;
                else ReferenceObject.SetActive(false);
            }
            if (sprite != null) ReferenceObject.GetComponent<Image>().sprite = spriteStart;
            if (material != null) ReferenceObject.GetComponent<Image>().material = materialStart;
            if (!selectedSize.Equals(Vector2.zero))
            {
                LeanTween.scale(ReferenceObject.GetComponent<RectTransform>(), startSize, 0.2f).setIgnoreTimeScale(true);
            }
            if (text != null)
            {
                LeanTween.value(gameObject, text.color, textStartColor, 0.2f).setIgnoreTimeScale(true).setOnUpdate((Color c) =>
                {
                    text.color = c;
                });
            }
        }
        if (text != null && textColorIgnoreActiveStatus)
        {
            LeanTween.value(gameObject, text.color, textStartColor, 0.2f).setIgnoreTimeScale(true).setOnUpdate((Color c) =>
            {
                text.color = c;
            });
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        print(buttonName + " was clicked;    " + "dragging?: " + eventData.dragging);

        if (!eventData.dragging)
        {
            status.buttonNamePreview = buttonName;
        }

        if (animation)
        {
            LeanTween.value(gameObject, ColorTargetImage.color, clickColor, 0.1f).setIgnoreTimeScale(true).setOnUpdate((Color color) =>
           {
               ColorTargetImage.color = color;
           });
        }
    }
}
