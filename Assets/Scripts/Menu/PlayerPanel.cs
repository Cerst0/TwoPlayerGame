using Mirror;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
    public int playerID;
    IdDataApply IDA;
    InputS input;
    public ColorSelector CS;
    Status status;
    public Menu menu;
    float generalDelay = 0.1f;
    public int id;

    public Texture2D cursorNormal;
    public Vector2 ButtonNumber;
    Vector2 ButtonNumberBefore;
    Vector3 pointerPosB;
    public bool isOpen;
    bool enter;
    public AudioSource ChangeSelection;

    bool upB;
    bool downB;
    bool leftB;
    bool rightB;

    [Header("Panel")]
    public Image PanelCloseColor;

    [Header("Name")]
    public TMP_InputField Name;
    public bool nameInEdit;
    public GameObject Pencil;
    string backupName;

    [Header("Image")]
    GameObject CharacterModel;
    GameObject CharacterRotModel;
    public EventTrigger trigger;
    public Image SelectImage;
    public float rotSpeed;
    public Sprite imageSelectedSprite;
    public Sprite imageInEditSprite;

    public bool imageSelected;
    Vector2 Offset;

    [Header("Arrows")]
    public GameObject ArrowRight;
    public Image ArrowRightImage;

    [Header("Arrows")]
    public GameObject ArrowLeft;
    public Image ArrowLeftImage;
    public int meshNumber;
    public int meshes;

    [Header("Colors")]
    public bool[] colorSelected;
    public string[] meshExtraColorsNames0;
    public string[] meshExtraColorsNames1;
    string[] currentMeshExtraColorsNames;
    public bool anyColorSelected;
    public Image[] colorImages;
    public Sprite colorSprite;
    public Sprite colorSpriteSelected;
    public Sprite resetSprite;
    public Sprite resetSpriteSelected;
    public TMP_Text[] extraTexts;
    public Image[] resetImages;

    private void Start()
    {
        status = FindObjectOfType<Status>();
        input = FindObjectOfType<InputS>();


        Pencil.SetActive(false);
        SelectImage.enabled = false;
        CharacterModel = GameObject.Find("Character3D" + playerID);
        CharacterRotModel = GameObject.Find("RotateCharacter" + playerID);
        IDA = CharacterModel.GetComponentInChildren<IdDataApply>();

        colorImages[9].gameObject.transform.parent.gameObject.SetActive(false);
        colorImages[10].gameObject.transform.parent.gameObject.SetActive(false);
        colorImages[11].gameObject.transform.parent.gameObject.SetActive(false);
        if (meshNumber == 0) { currentMeshExtraColorsNames = meshExtraColorsNames0; }
        if (meshNumber == 1) { currentMeshExtraColorsNames = meshExtraColorsNames1; }
        for (int i = 0; i <= currentMeshExtraColorsNames.Length - 1; i++)
        {
            colorImages[i + 9].gameObject.transform.parent.gameObject.SetActive(true);
            extraTexts[i].text = currentMeshExtraColorsNames[i];
        }

        EventTrigger.Entry entry = new();
        EventTrigger.Entry entryBegin = new();
        entry.eventID = EventTriggerType.Drag;
        entryBegin.eventID = EventTriggerType.BeginDrag;
        entry.callback.AddListener((data) => { OnDragDelegate((PointerEventData)data); });
        entryBegin.callback.AddListener((data) => { OnDragBegin((PointerEventData)data); });
        trigger.triggers.Add(entry);
        trigger.triggers.Add(entryBegin);
    }

    private void Update()
    {
        id = playerID;
        if (NetworkClient.active && playerID != 0 && status.clientPlayerID != 0) { id = 0; }
        meshNumber = GameDataManager.GetGameData().PlayerDatas[id].mesh;

        if ((menu.Player0Stat == 2 && playerID == 0) | (menu.Player1Stat == 2 && playerID == 1) | (menu.Player2Stat == 2 && playerID == 2) | (menu.Player3Stat == 2 && playerID == 3)) { isOpen = true; }
        else
        {
            isOpen = false;
        }

        if (isOpen && !nameInEdit && !imageSelected) { WhenOpened(); ChangeSelectedVector(); }

        anyColorSelected = !colorSelected.All(colorSelected => colorSelected == false);

        if (nameInEdit)
        {
            Pencil.SetActive(false);
            Name.image.color = Color.white;
            if (input.enter && generalDelay < 0.1f)
            {
                var eventSystem = EventSystem.current;
                if (!eventSystem.alreadySelecting) eventSystem.SetSelectedGameObject(null);
                generalDelay = 0.1f;
                if (Name.text.Length == 0) { Name.text = backupName; }

                SaveName();
                if (NetworkClient.active)
                {
                    GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().CollectAll();
                }
                else { IDA.IDDataApply(); }
                nameInEdit = false;
            }
        }
        else { Name.text = GameDataManager.GetGameData().PlayerDatas[id].name; }
        if (imageSelected)
        {
            SelectImage.sprite = imageSelectedSprite;
            SelectImage.enabled = true;
            CharacterModel.GetComponent<Animator>().enabled = false;

            if (input.right) CharacterRotModel.transform.Rotate(new Vector3(0, -rotSpeed, 0), Space.Self);
            if (input.left) CharacterRotModel.transform.Rotate(new Vector3(0, rotSpeed, 0), Space.Self);
            if (input.up) CharacterRotModel.transform.Rotate(new Vector3(-rotSpeed, 0, 0), Space.Self);
            if (input.down) CharacterRotModel.transform.Rotate(new Vector3(rotSpeed, 0, 0), Space.Self);

            if ((input.enter | input.esc) && generalDelay < 0f)
            {
                imageSelected = false;
                var eventSystem = EventSystem.current;
                if (!eventSystem.alreadySelecting) eventSystem.SetSelectedGameObject(null);
                generalDelay = 0.1f;
            }
        }
        else { SelectImage.sprite = imageInEditSprite; CharacterModel.GetComponent<Animator>().enabled = true; }

        generalDelay += -Time.deltaTime;
    }

    private void WhenOpened()
    {
        enter = input.enter;

        if (meshNumber == 0) { currentMeshExtraColorsNames = meshExtraColorsNames0; }
        if (meshNumber == 1) { currentMeshExtraColorsNames = meshExtraColorsNames1; }

        if (((enter && ((ButtonNumber.x == 1 && ButtonNumber.y == 0) | (ButtonNumber.x == 2 && ButtonNumber.y == -1))) | status.buttonName == "ClosePlayer1" | input.esc) && !nameInEdit && !imageSelected && generalDelay < 0f && generalDelay < 0f)
        {
            CharacterRotModel.transform.localEulerAngles = Vector3.zero;
            enter = false;
            isOpen = false;
            CS.close = true;
            if (playerID == 0) menu.Player0Stat = 3;
            if (playerID == 1) menu.Player1Stat = 3;
            if (playerID == 2) menu.Player2Stat = 3;
            if (playerID == 3) menu.Player3Stat = 3;
            print(menu.panelName);
            Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
        }
        if ((ButtonNumber.x == 1 && ButtonNumber.y == 0) | (ButtonNumber.x == 2 && ButtonNumber.y == -1))
        {
            menu.SetCloseColor(PanelCloseColor, true);
        }




        if (ButtonNumber.Equals(new Vector2(0, 0)))
        {
            Pencil.SetActive(true);
            Name.image.color = new Color(0.9607843f, 0.9607843f, 0.9607843f, 0.4f);
            if (enter && !nameInEdit && generalDelay < 0f)
            {
                generalDelay = 0.1f;
                nameInEdit = true;
                Name.Select();
                Name.image.color = Color.white;
            }
        }

        if (ButtonNumber.Equals(new Vector2(0, -1)))
        {
            SelectImage.enabled = true;
        }

        if (((enter && ButtonNumber.Equals(new Vector2(0, -1))) | status.buttonName == "Player1Image") && !nameInEdit && !imageSelected && generalDelay < 0f && generalDelay < 0f)
        {
            imageSelected = true;
            generalDelay = 0.1f;
        }

        if (((enter && ButtonNumber.Equals(new Vector2(1, -1))) | status.buttonName == "ArrowR") && !nameInEdit && !imageSelected && generalDelay < 0f && generalDelay < 0f && generalDelay < 0f)
        {
            meshNumber++;
            if (meshNumber > meshes) meshNumber = 0;

            SaveMesh(meshNumber);
            if (NetworkClient.active)
            {
                GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().CollectAll();
            }
            else { IDA.IDDataApply(); }
            generalDelay = 0.1f;

            colorImages[9].gameObject.transform.parent.gameObject.SetActive(false);
            colorImages[10].gameObject.transform.parent.gameObject.SetActive(false);
            colorImages[11].gameObject.transform.parent.gameObject.SetActive(false);
            if (meshNumber == 0) { currentMeshExtraColorsNames = meshExtraColorsNames0; }
            if (meshNumber == 1) { currentMeshExtraColorsNames = meshExtraColorsNames1; }
            for (int i = 0; i <= currentMeshExtraColorsNames.Length - 1; i++)
            {
                colorImages[i + 9].gameObject.transform.parent.gameObject.SetActive(true);
                extraTexts[i].text = currentMeshExtraColorsNames[i];
            }
        }

        if (((enter && ButtonNumber.Equals(new Vector2(-1, -1))) | status.buttonName == "ArrowR") && !nameInEdit && !imageSelected && generalDelay < 0f && generalDelay < 0f && generalDelay < 0f)
        {
            meshNumber--;
            if (meshNumber < 0) meshNumber = meshes;

            SaveMesh(meshNumber);
            if (NetworkClient.active)
            {
                GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().CollectAll();
            }
            else { IDA.IDDataApply(); }
            generalDelay = 0.1f;

            colorImages[9].gameObject.transform.parent.gameObject.SetActive(false);
            colorImages[10].gameObject.transform.parent.gameObject.SetActive(false);
            colorImages[11].gameObject.transform.parent.gameObject.SetActive(false);
            if (meshNumber == 0) { currentMeshExtraColorsNames = meshExtraColorsNames0; }
            if (meshNumber == 1) { currentMeshExtraColorsNames = meshExtraColorsNames1; }
            for (int i = 0; i <= currentMeshExtraColorsNames.Length - 1; i++)
            {
                colorImages[i + 9].gameObject.transform.parent.gameObject.SetActive(true);
                extraTexts[i].text = currentMeshExtraColorsNames[i];
            }
        }


        if (!anyColorSelected)
        {
            ColorUI(new Vector2(-3, -2), "Body", 0);
            ColorUI(new Vector2(-2, -2), "Eye Brows", 1);
            ColorUI(new Vector2(-1, -2), "Nose", 2);
            ColorUI(new Vector2(1, -2), "Mouth", 3);
            ColorUI(new Vector2(2, -2), "Tongue", 4);
            ColorUI(new Vector2(3, -2), "Tooth", 5);
            ColorUI(new Vector2(-3, -4), "Eye", 6);
            ColorUI(new Vector2(-2, -4), "Pupil", 7);
            ColorUI(new Vector2(-1, -4), "Foot", 8);

            ResetUI(new Vector2(-3, -3), "BodyR", 0);
            ResetUI(new Vector2(-2, -3), "EyeBrowsR", 1);
            ResetUI(new Vector2(-1, -3), "NoseR", 2);
            ResetUI(new Vector2(1, -3), "MouthR", 3);
            ResetUI(new Vector2(2, -3), "TongueR", 4);
            ResetUI(new Vector2(3, -3), "ToothR", 5);
            ResetUI(new Vector2(-3, -5), "EyeR", 6);
            ResetUI(new Vector2(-2, -5), "PupilR", 7);
            ResetUI(new Vector2(-1, -5), "FootR", 8);

            for (int i = 0; i < currentMeshExtraColorsNames.Length; i++)
            {
                int number = 9 + i;
                ColorUI(new Vector2(1 + i, -4), "Extra" + (i + 1), number, false, true, currentMeshExtraColorsNames[i]);
                ResetUI(new Vector2(1 + i, -5), "Extra" + (i + 1) + "R", number, false, true);
            }
        }
    }

    private void ColorUI(Vector2 pos, string name, int index, bool isChangeSelectedVector = false, bool isExtra = false, string extraName = "")
    {
        GameDataManager.GameData GD = GameDataManager.GetGameData();

        Color color = Color.magenta;

        if (isExtra)
        {
            ColorUtility.TryParseHtmlString(GD.PlayerDatas[id].extraColors[meshNumber].colors[index - 9], out color);
        }
        else
        {
            ColorUtility.TryParseHtmlString(GD.PlayerDatas[id].colors[index], out color);
        }

        colorImages[index].transform.parent.transform.Find("Mask").transform.GetChild(0).GetComponent<Image>().color = color;


        if (isChangeSelectedVector)
        {
            if (ButtonNumberBefore.Equals(pos))
            {
                colorImages[index].sprite = colorSprite;
                LeanTween.scale(colorImages[index].GetComponent<RectTransform>(), new Vector2(1, 1), 0.2f);
            }
        }
        else
        {
            if (ButtonNumber.Equals(pos))
            {
                colorImages[index].sprite = colorSpriteSelected;
                LeanTween.scale(colorImages[index].GetComponent<RectTransform>(), new Vector2(1.1f, 1.1f), 0.2f);
            }

            if (((input.enter && ButtonNumber.Equals(pos)) | status.buttonName == name) && !nameInEdit && !imageSelected && generalDelay < 0f && generalDelay < 0f)
            {
                for (int i = 0; i <= colorImages.Length - 1; i++)
                {
                    colorImages[i].sprite = colorSprite;
                }
                Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);

                if (isExtra)
                {
                    CS.nameText.text = extraName;
                    CS.isExtra = true;
                }
                else { CS.nameText.text = name; }
                CS.oldColor = color;
                colorSelected[index] = true;
                CS.colorIndex = index;
                generalDelay = 0.1f;
            }
        }
    }

    private void ResetUI(Vector2 pos, string name, int index, bool isChangeSelectedVector = false, bool isExtra = false)
    {
        if (isChangeSelectedVector)
        {
            if (ButtonNumberBefore.Equals(pos))
            {
                resetImages[index].sprite = resetSprite;
                LeanTween.scale(resetImages[index].GetComponent<RectTransform>(), new Vector2(1, 1), 0.2f);
            }

            if (ButtonNumber.Equals(pos))
            {
                resetImages[index].sprite = resetSpriteSelected;
                LeanTween.scale(resetImages[index].GetComponent<RectTransform>(), new Vector2(1.1f, 1.1f), 0.2f);
            }
        }
        else
        {
            if (((input.enter && ButtonNumber.Equals(pos)) | status.buttonName == name) && !nameInEdit && !imageSelected && generalDelay < 0f && generalDelay < 0f)
            {
                GameDataManager.GameData GD = GameDataManager.GetGameData();
                string c = GD.meshes[meshNumber].Defaultcolors[index];

                if (isExtra)
                {
                    GD.PlayerDatas[id].extraColors[meshNumber].colors[index - 9] = c;
                }
                else
                {
                    GD.PlayerDatas[id].colors[index] = c;
                }
                FindObjectOfType<Methods>().SetGameData(GD);

                if (NetworkClient.active)
                {
                    GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().CollectAll();
                }
                else { IDA.IDDataApply(); }
            }
        }
    }

    public void OnNameSelect()
    {
        backupName = Name.text;
        nameInEdit = true;
    }

    public void OnNameDeselect()
    {
        var eventSystem = EventSystem.current;
        if (!eventSystem.alreadySelecting) eventSystem.SetSelectedGameObject(null);
        generalDelay = 0.1f;
        if (Name.text.Length == 0) { Name.text = backupName; }

        SaveName();

        if (NetworkClient.active)
        {
            GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().CollectAll();
        }
        else { IDA.IDDataApply(); }
        nameInEdit = false;
    }

    private void SaveName()
    {
        GameDataManager.GameData GD = GameDataManager.GetGameData();
        GD.PlayerDatas[id].name = Name.text;
        FindObjectOfType<Methods>().SetGameData(GD);
    }

    private void SaveMesh(int newMeshNumber)
    {
        GameDataManager.GameData GD = GameDataManager.GetGameData();
        GD.PlayerDatas[id].mesh = newMeshNumber;
        FindObjectOfType<Methods>().SetGameData(GD);
    }

    private void ChangeSelectedVector()
    {
        if (ButtonNumber != ButtonNumberBefore)
        {
            ChangeSelection.Play();
            if (ButtonNumberBefore == Vector2.zero) Name.image.color = new Color(0.9607843f, 0.9607843f, 0.9607843f, 0.0f); Pencil.SetActive(false);
            if (ButtonNumberBefore.Equals(new Vector2(0, -1))) SelectImage.enabled = false;
            if (ButtonNumberBefore.Equals(new Vector2(1, -1))) { ArrowRightImage.color = Color.white; LeanTween.scale(ArrowRight.GetComponent<RectTransform>(), new Vector2(1, 1), 0.2f); }
            if (ButtonNumberBefore.Equals(new Vector2(-1, -1))) { ArrowLeftImage.color = Color.white; LeanTween.scale(ArrowLeft.GetComponent<RectTransform>(), new Vector2(-1, 1), 0.2f); }
            if ((ButtonNumberBefore.x == 1 && ButtonNumberBefore.y == 0) | (ButtonNumberBefore.x == 2 && ButtonNumberBefore.y == -1)) { menu.SetCloseColor(PanelCloseColor, false); }

            if (ButtonNumber.Equals(new Vector2(1, -1)))
            {
                ArrowRightImage.color = menu.Orange;
                LeanTween.scale(ArrowRight.GetComponent<RectTransform>(), new Vector2(1.2f, 1.2f), 0.2f);
            }

            if (ButtonNumber.Equals(new Vector2(-1, -1)))
            {
                ArrowLeftImage.color = menu.Orange;
                LeanTween.scale(ArrowLeft.GetComponent<RectTransform>(), new Vector2(-1.2f, 1.2f), 0.2f);
            }

            ColorUI(new Vector2(-3, -2), "Body", 0, true);
            ColorUI(new Vector2(-2, -2), "Eye Brows", 1, true);
            ColorUI(new Vector2(-1, -2), "Nose", 2, true);
            ColorUI(new Vector2(1, -2), "Mouth", 3, true);
            ColorUI(new Vector2(2, -2), "Tongue", 4, true);
            ColorUI(new Vector2(3, -2), "Tooth", 5, true);
            ColorUI(new Vector2(-3, -4), "Eye", 6, true);
            ColorUI(new Vector2(-2, -4), "Pupil", 7, true);
            ColorUI(new Vector2(-1, -4), "Foot", 8, true);

            ResetUI(new Vector2(-3, -3), "BodyR", 0, true);
            ResetUI(new Vector2(-2, -3), "EyeBrowsR", 1, true);
            ResetUI(new Vector2(-1, -3), "NoseR", 2, true);
            ResetUI(new Vector2(1, -3), "MouthR", 3, true);
            ResetUI(new Vector2(2, -3), "TongueR", 4, true);
            ResetUI(new Vector2(3, -3), "ToothR", 5, true);
            ResetUI(new Vector2(-3, -5), "EyeR", 6, true);
            ResetUI(new Vector2(-2, -5), "PupilR", 7, true);
            ResetUI(new Vector2(-1, -5), "FootR", 8, true);

            for (int i = 0; i < currentMeshExtraColorsNames.Length; i++)
            {
                int number = 9 + i;
                ColorUI(new Vector2(1 + i, -4), "Extra" + (i + 1), number, true, true, currentMeshExtraColorsNames[i]);
                ResetUI(new Vector2(1 + i, -5), "Extra" + (i + 1) + "R", number, true, true);
            }
        }
        ButtonNumberBefore = ButtonNumber;

        if (!anyColorSelected)
        {
            if (input.up && (input.up != upB) && ButtonNumber.y < 0)
            {
                if (ButtonNumber.y == -2 | ButtonNumber.y == -1)
                {
                    if (ButtonNumber.y == -1) { ButtonNumber = new Vector2(0, 0); }
                    if (ButtonNumber.y == -2) { ButtonNumber = new Vector2(0, -1); }
                }
                else
                {
                    ButtonNumber.y += 1;
                }
            }

            if (input.down && (input.down != downB) && ButtonNumber.y > -5)
            {
                if (ButtonNumber.Equals((new Vector2(0, -1)))) ButtonNumber = new Vector2(-1, -2);
                else ButtonNumber.y -= 1;
            }


            if (input.right && (input.right != rightB) && ButtonNumber.x < 3)
            {
                if (ButtonNumber.y < -1 && ButtonNumber.x == -1) ButtonNumber.x += 2;
                else ButtonNumber.x += 1;
            }

            if (input.left && (input.left != leftB) && ButtonNumber.x > -3)
            {
                if (ButtonNumber.y < -1 && ButtonNumber.x == 1) ButtonNumber.x -= 2;
                else ButtonNumber.x -= 1;
            }
        }
        else { ButtonNumber = new Vector2(CS.ButtonNumber.x, CS.ButtonNumber.y - 2); }


        upB = input.up;
        downB = input.down;
        leftB = input.left;
        rightB = input.right;
    }

    public void SetButtonToSelected(GameObject Image, GameObject DescriptionImage)
    {
        ColorUtility.TryParseHtmlString("#c3c3ed", out Color c);
        ColorUtility.TryParseHtmlString("#bcbcbc", out Color c2);
        Image.GetComponent<Image>().color = c;
        DescriptionImage.GetComponent<Image>().color = c2;
    }

    public void SetButtonToUnSelected(GameObject Image, GameObject DescriptionImage)
    {
        ColorUtility.TryParseHtmlString("#5C4CFF", out Color c);
        ColorUtility.TryParseHtmlString("#797980", out Color c2);
        Image.GetComponent<Image>().color = c;
        DescriptionImage.GetComponent<Image>().color = c2;
    }

    public void OnDragBegin(PointerEventData data)
    {
        Offset = data.position;

        pointerPosB = CharacterRotModel.transform.localEulerAngles;
    }

    public void OnDragDelegate(PointerEventData data)
    {
        CharacterModel.GetComponent<Animator>().enabled = false;

        Vector2 pos = data.position - Offset;
        pos = -pos;
        CharacterRotModel.transform.localEulerAngles = new Vector3(pos.y + pointerPosB.x, pos.x + pointerPosB.y, CharacterRotModel.transform.rotation.z);
    }
}
