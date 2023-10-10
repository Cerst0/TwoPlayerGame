using HSVPicker;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour
{
    IdDataApply IDA;
    public PlayerPanel PP;
    public ColorPicker picker;
    public ColorSlider colorSlider;
    InputS input;
    Menu menu;
    GameObject CharacterModel;
    public Texture2D cursorNormal;
    float generalDelay = 0.1f;
    public Image CloseColor;
    public Image oldColorImage;
    public Image[] colorImages;
    public GameObject PanelClose;
    public TMP_Text nameText;
    public Vector2 ButtonNumber;
    public int colorIndex;
    public Color oldColor;
    public bool isExtra;
    public bool close;
    Vector2 ButtonNumberBefore;
    Status status;
    Color pickerColorB;
    bool isOpen;

    bool downB;
    bool rightB;
    bool leftB;
    bool upB;

    private void Start()
    {
        status = FindObjectOfType<Status>();
        input = FindObjectOfType<InputS>();
        CharacterModel = GameObject.Find("Character3D" + PP.playerID);
        IDA = CharacterModel.GetComponentInChildren<IdDataApply>();
        menu = FindObjectOfType<Menu>();

        foreach (Transform child in transform)
        {
            if (child.GetComponent<Image>() != null)
            {
                Image img = child.GetComponent<Image>();
                Color c = img.color;
                c.a = 0;
                img.color = c;
                img.gameObject.SetActive(false);
            }

            foreach (Transform child2 in child.transform)
            {
                if (child2.GetComponent<Image>() != null)
                {
                    Image img = child2.GetComponent<Image>();
                    Color c = img.color;
                    c.a = 0;
                    img.color = c;
                    img.gameObject.SetActive(false);
                }

                foreach (Transform child3 in child2.transform)
                {
                    if (child3.GetComponent<Image>() != null)
                    {
                        Image img = child3.GetComponent<Image>();
                        Color c = img.color;
                        c.a = 0;
                        img.color = c;
                        img.gameObject.SetActive(false);
                    }
                }
            }
        }
        nameText.color = new Color(0, 0, 0, 0);
    }

    private void Update()
    {

        if (PP.anyColorSelected && !isOpen)
        {
            close = false;
            isOpen = true;

            foreach (Transform child in transform)
            {
                if (child.GetComponent<Image>() != null)
                {
                    Image img = child.GetComponent<Image>();
                    LeanTween.alpha(img.gameObject.GetComponent<RectTransform>(), 1, 0.25f);
                    img.gameObject.SetActive(true);
                }

                foreach (Transform child2 in child.transform)
                {
                    if (child2.GetComponent<Image>() != null)
                    {
                        Image img = child2.GetComponent<Image>();
                        LeanTween.alpha(img.gameObject.GetComponent<RectTransform>(), 1, 0.25f);
                        img.gameObject.SetActive(true);
                    }

                    foreach (Transform child3 in child2.transform)
                    {
                        if (child3.GetComponent<Image>() != null)
                        {
                            Image img = child3.GetComponent<Image>();
                            LeanTween.alpha(img.gameObject.GetComponent<RectTransform>(), 1, 0.25f);
                            img.gameObject.SetActive(true);
                        }
                    }
                }

                CloseColor.color = new Color(0.4f, 0.1294118f, 1);
                oldColorImage.color = oldColor;
                picker.CurrentColor = oldColor;
                colorSlider.UpdateColor();
            }
            LeanTween.value(nameText.gameObject, 0, 1, 0.25f).setOnUpdate((float val) =>
            {
                Color c = Color.black;
                c.a = val;
                nameText.color = c;
            });

            PanelClose.SetActive(true);
            foreach (Image img in PP.colorImages)
            {
                img.gameObject.SetActive(false);
                img.transform.parent.GetChild(3).GetComponent<UniversalButton>().enabled = false;
            }
        }

        if (isOpen && !PP.nameInEdit && !PP.imageSelected)
        {
            ChangeSelectedVector();

            if (pickerColorB != picker.CurrentColor)
            {
                string c = ColorUtility.ToHtmlStringRGB(picker.CurrentColor);
                SaveColor(null, c);
            }
            pickerColorB = picker.CurrentColor;

            bool enter = input.enter;

            if (((enter && ButtonNumber.Equals(new Vector2(1, 0))) | status.buttonName == "CloseColorSelector" | input.esc | close) && generalDelay < 0f)
            {
                CloseColor.color = new Color(0.4f, 0.1294118f, 1);
                foreach (Image img in PP.colorImages)
                {
                    img.gameObject.SetActive(true);
                    img.transform.parent.GetChild(3).GetComponent<UniversalButton>().enabled = true;
                }
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<Image>() != null)
                    {
                        Image img = child.GetComponent<Image>();
                        LeanTween.alpha(img.gameObject.GetComponent<RectTransform>(), 0, 0.25f);
                    }

                    foreach (Transform child2 in child.transform)
                    {
                        if (child2.GetComponent<Image>() != null)
                        {
                            Image img = child2.GetComponent<Image>();
                            LeanTween.alpha(img.gameObject.GetComponent<RectTransform>(), 0, 0.25f);
                        }

                        foreach (Transform child3 in child2.transform)
                        {
                            if (child3.GetComponent<Image>() != null)
                            {
                                Image img = child3.GetComponent<Image>();
                                LeanTween.alpha(img.gameObject.GetComponent<RectTransform>(), 0, 0.25f);
                            }
                        }
                    }
                }
                LeanTween.value(nameText.gameObject, 1, 0, 0.25f).setOnUpdate((float val) =>
                {
                    Color c = Color.black;
                    c.a = val;
                    nameText.color = c;
                });
                PanelClose.SetActive(false);
                isExtra = false;
                PP.colorSelected = new bool[PP.colorSelected.Length];
                enter = false;
                Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
                isOpen = false;
                ButtonNumber = new Vector2(-1, 0);
                close = false;
            }

            if (ButtonNumber.Equals(new Vector2(1, 0)))
            {
                CloseColor.color = menu.CloseGold;
            }

            AssetColorUI(new Vector2(-7, 0), "BlueC1", 0);
            AssetColorUI(new Vector2(-7, -1), "BlueC2", 1);
            AssetColorUI(new Vector2(-7, -2), "BlueC3", 2);
            AssetColorUI(new Vector2(-6, 0), "AquaC1", 3);
            AssetColorUI(new Vector2(-6, -1), "AquaC2", 4);
            AssetColorUI(new Vector2(-6, -2), "AquaC3", 5);
            AssetColorUI(new Vector2(-5, 0), "GreenC1", 6);
            AssetColorUI(new Vector2(-5, -1), "GreenC2", 7);
            AssetColorUI(new Vector2(-5, -2), "GreenC3", 8);
            AssetColorUI(new Vector2(-4, 0), "YellowC1", 9);
            AssetColorUI(new Vector2(-4, -1), "YellowC2", 10);
            AssetColorUI(new Vector2(-4, -2), "YellowC3", 11);
            AssetColorUI(new Vector2(-3, 0), "OrangeC1", 12);
            AssetColorUI(new Vector2(-3, -1), "OrangeC2", 13);
            AssetColorUI(new Vector2(-3, -2), "OrangeC3", 14);
            AssetColorUI(new Vector2(-2, 0), "RedC1", 15);
            AssetColorUI(new Vector2(-2, -1), "RedC2", 16);
            AssetColorUI(new Vector2(-2, -2), "RedC3", 17);
            AssetColorUI(new Vector2(-1, 0), "PinkC1", 18);
            AssetColorUI(new Vector2(-1, -1), "PinkC2", 19);
            AssetColorUI(new Vector2(-1, -2), "PinkC3", 20);
            AssetColorUI(new Vector2(0, 0), "Black", 21);
            AssetColorUI(new Vector2(0, -1), "Black", 21);
            AssetColorUI(new Vector2(0, -2), "White", 22);
        }
        if (!isOpen)
        {
            if (colorImages[0].color.a == 0)
            {
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<Image>() != null)
                    {
                        Image img = child.GetComponent<Image>();
                        img.gameObject.SetActive(false);
                    }

                    foreach (Transform child2 in child.transform)
                    {
                        if (child2.GetComponent<Image>() != null)
                        {
                            Image img = child2.GetComponent<Image>();
                            img.gameObject.SetActive(false);
                        }

                        foreach (Transform child3 in child2.transform)
                        {
                            if (child3.GetComponent<Image>() != null)
                            {
                                Image img = child3.GetComponent<Image>();
                                img.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
        generalDelay += -Time.deltaTime;
    }

    private void AssetColorUI(Vector2 pos, string name, int number, bool isChangeSelectedVector = false)
    {
        if (isOpen)
        {
            if (isChangeSelectedVector)
            {
                if (ButtonNumberBefore.Equals(pos))
                {
                    colorImages[number].sprite = PP.colorSprite; LeanTween.scale(colorImages[number].GetComponent<RectTransform>(), new Vector2(1, 1), 0.2f);
                }
            }
            else
            {
                if (ButtonNumber.Equals(pos))
                {
                    colorImages[number].sprite = PP.colorSpriteSelected;
                    LeanTween.scale(colorImages[number].GetComponent<RectTransform>(), new Vector2(1.1f, 1.1f), 0.2f);
                }

                if (((input.enter && ButtonNumber.Equals(pos)) | status.buttonName == name) && generalDelay < 0f)
                {
                    SaveColor(colorImages[number]);
                    generalDelay = 0.1f;
                }
            }
        }
    }

    private void ChangeSelectedVector()
    {
        if (ButtonNumber != ButtonNumberBefore)
        {
            PP.ChangeSelection.Play();
            if (ButtonNumberBefore.Equals(new Vector2(1, 0))) CloseColor.color = new Color(0.4f, 0.1294118f, 1);

            AssetColorUI(new Vector2(-7, 0), "BlueC1", 0, true);
            AssetColorUI(new Vector2(-7, -1), "BlueC2", 1, true);
            AssetColorUI(new Vector2(-7, -2), "BlueC3", 2, true);
            AssetColorUI(new Vector2(-6, 0), "AquaC1", 3, true);
            AssetColorUI(new Vector2(-6, -1), "AquaC2", 4, true);
            AssetColorUI(new Vector2(-6, -2), "AquaC3", 5, true);
            AssetColorUI(new Vector2(-5, 0), "GreenC1", 6, true);
            AssetColorUI(new Vector2(-5, -1), "GreenC2", 7, true);
            AssetColorUI(new Vector2(-5, -2), "GreenC3", 8, true);
            AssetColorUI(new Vector2(-4, 0), "YellowC1", 9, true);
            AssetColorUI(new Vector2(-4, -1), "YellowC2", 10, true);
            AssetColorUI(new Vector2(-4, -2), "YellowC3", 11, true);
            AssetColorUI(new Vector2(-3, 0), "OrangeC1", 12, true);
            AssetColorUI(new Vector2(-3, -1), "OrangeC2", 13, true);
            AssetColorUI(new Vector2(-3, -2), "OrangeC3", 14, true);
            AssetColorUI(new Vector2(-2, 0), "RedC1", 15, true);
            AssetColorUI(new Vector2(-2, -1), "RedC2", 16, true);
            AssetColorUI(new Vector2(-2, -2), "RedC3", 17, true);
            AssetColorUI(new Vector2(-1, 0), "PinkC1", 18, true);
            AssetColorUI(new Vector2(-1, -1), "PinkC2", 19, true);
            AssetColorUI(new Vector2(-1, -2), "PinkC3", 20, true);
            AssetColorUI(new Vector2(0, 0), "Black", 21, true);
            AssetColorUI(new Vector2(0, -1), "Black", 21, true);
            AssetColorUI(new Vector2(0, -2), "White", 22, true);
        }
        ButtonNumberBefore = ButtonNumber;


        if (input.up && (input.up != upB) && ButtonNumber.y < 2)
        {
            if (ButtonNumber.y == 0) ButtonNumber = new Vector2(0, 1);
            else ButtonNumber.y += 1;
            if (ButtonNumber.Equals(new Vector2(0, -1))) ButtonNumber.y += 1;
        }

        if (input.down && (input.down != downB) && ButtonNumber.y > -2)
        {
            ButtonNumber.y -= 1;
            if (ButtonNumber.Equals(new Vector2(0, -1))) ButtonNumber.y -= 1;
        }


        if (input.right && (input.right != rightB) && ButtonNumber.x < 1)
        {
            ButtonNumber.x += 1;
        }

        if (input.left && (input.left != leftB) && ButtonNumber.x > -7)
        {
            ButtonNumber.x -= 1;
        }


        upB = input.up;
        downB = input.down;
        leftB = input.left;
        rightB = input.right;
    }

    private void SaveColor(Image img, string hex = null)
    {
        if (hex == null)
        {
            hex = ColorUtility.ToHtmlStringRGB(img.transform.parent.GetChild(0).GetChild(0).GetComponent<Image>().color);
        }
        hex = "#" + hex;

        GameDataManager.GameData GD = GameDataManager.GetGameData();

        if (isExtra)
        {
            GD.PlayerDatas[PP.id].extraColors[PP.meshNumber].colors[colorIndex - 9] = hex;
        }
        else
        {
            GD.PlayerDatas[PP.id].colors[colorIndex] = hex;
        }
        FindObjectOfType<Methods>().SetGameData(GD);

        if (NetworkClient.active)
        {
            GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().CollectAll();
        }
        else { IDA.IDDataApply(); }

        //print("Saved Color with hex: " + hex + " Of Player" + PP.id + " with ColorIndex: " + colorIndex);
    }
}
