using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    InputS input;
    Status status;
    Menu menu;
    Notificaton n;
    public Scrollbar scrollBar;
    public ScrollRect scrollRect;

    public Slider[] Sliders = new Slider[2];

    public GameObject LOGButton;
    public GameObject ResetHighScoresButton;

    public Texture2D cursorNormal;
    public Vector2 ButtonNumber;
    Vector2 ButtonNumberB;
    public bool isOpen;
    bool isOpenB;
    bool enter;
    bool esc;
    int sliderInEdit = -1;
    public AudioSource ChangeSelection;

    bool upB;
    bool downB;
    public bool leftB;
    public bool rightB;

    [Header("Panel")]
    public Image PanelCloseColor;

    private void OnLevelWasLoaded(int level)
    {
        print(level + "level was loaded");
    }

    void Start()
    {
        n = FindObjectOfType<Notificaton>();
        status = FindObjectOfType<Status>();
        menu = FindObjectOfType<Menu>();
        input = FindObjectOfType<InputS>();

        foreach (var s in Sliders)
        {
            s.Start();
        }
        Sliders[0].Select();
    }

    void Update()
    {
        foreach (var s in Sliders)
        {
            s.Update();
        }

        if (menu.panelName == "settings") { isOpen = true; }
        else
        {
            isOpen = false;
        }

        if (isOpen)
        {
            if (!isOpenB)
            {
                SetButton(ResetHighScoresButton, new Vector2(-1, -9), true);
                SetButton(LOGButton, new Vector2(0, -9), true);

                scrollRect.enabled = true;
            }

            WhenOpened();
            if (sliderInEdit == -1) { ChangeSelectedVector(); }
        }

        isOpenB = isOpen;
    }

    void WhenOpened()
    {
        enter = input.enter;
        esc = input.esc;

        if (((enter && ButtonNumber.x == 1) | status.buttonName == "CloseSettings" | esc) && sliderInEdit == -1)
        {
            enter = false;
            esc = false;
            isOpen = false;
            scrollRect.enabled = false;
            menu.SettingsStat = 3;
            print(menu.panelName);
            Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);

            GameDataManager.GameData newGameData = GameDataManager.GetGameData();
            foreach (Slider slider in Sliders)
            {
                newGameData.settingsData.GetType().GetField(slider.GDVariableName).SetValue(newGameData.settingsData, slider.slider.value);
            }
            FindObjectOfType<Methods>().SetGameData(newGameData);
            FindObjectOfType<Quality>().OnSettingsChanged();
        }

        if (enter && ButtonNumber.x == 0 && ButtonNumber.y > -9 && Sliders[Mathf.Abs((int)ButtonNumber.y)] is not null && sliderInEdit == -1)
        {
            enter = false;
            sliderInEdit = Mathf.Abs((int)ButtonNumber.y);
            Sliders[Mathf.Abs((int)ButtonNumber.y)].EnterEdit();
        }

        if (sliderInEdit != -1 && (esc | enter))
        {
            enter = false;
            esc = false;
            Sliders[sliderInEdit].ExitEdit();
            sliderInEdit = -1;
        }

        if ((enter && ButtonNumber.Equals(new Vector2(0, -9))) | status.buttonName == "log")
        {
            enter = false;

            string path = Application.persistentDataPath + "/Player.log";
            System.Diagnostics.Process.Start(path);
        }
        if ((enter && ButtonNumber.Equals(new Vector2(-1, -9))) | status.buttonName == "resetHighScores")
        {
            enter = false;

            FindObjectOfType<HighScore>(PanelCloseColor).ResetHighScores();
        }
    }

    void ChangeSelectedVector()
    {
        if (ButtonNumber != ButtonNumberB)
        {
            ChangeSelection.Play();
            if (ButtonNumberB.x == 1) menu.SetCloseColor(PanelCloseColor, false);

            SetButton(ResetHighScoresButton, new Vector2(-1, -9));
            SetButton(LOGButton, new Vector2(0, -9));

            if (ButtonNumberB.y > -9)
            {
                Sliders[Mathf.Abs((int)ButtonNumberB.y)]?.Deselect();
            }

            if (ButtonNumber.x == 0 && ButtonNumber.y > -9)
            {
                Sliders[Mathf.Abs((int)ButtonNumber.y)]?.Select();
            }

            if (ButtonNumber.x == 1 && ButtonNumberB.x != 1) { menu.SetCloseColor(PanelCloseColor, true); }
        }
        ButtonNumberB = ButtonNumber;

        if (input.up && (input.up != upB) && ButtonNumber.y < 0 && ButtonNumber.x < 1)
        {
            ButtonNumber.y += 1;
            if (ButtonNumber.y == -8) { ButtonNumber.x = 0; }
        }

        if (input.down && (input.down != downB) && ButtonNumber.y > -9 && ButtonNumber.x < 1)
        {
            ButtonNumber.y -= 1;
            if (ButtonNumber.y == -9) { ButtonNumber.x = -1; }
        }

        if (input.right && (input.right != rightB) && ButtonNumber.x < 1)
        {
            ButtonNumber.x += 1;
        }

        if (input.left && (input.left != leftB))
        {
            if (ButtonNumber.x < 1)
            {
                if (ButtonNumber.y == -9 && ButtonNumber.x == 0) { ButtonNumber.x = -1; }
            }
            else
            {
                ButtonNumber.x -= 1;
            }
        }
    }

    private void LateUpdate()
    {
        upB = input.up;
        downB = input.down;
        leftB = input.left;
        rightB = input.right;
    }

    public void SetButton(GameObject Image, Vector2 Pos, bool isStartDeselector = false)
    {
        if (ButtonNumberB == Pos | isStartDeselector)
        {
            Image.GetComponent<UniversalButton>().active[1] = false;
            if (Image.GetComponent<UniversalButton>().active.All(active => active == false))
            {
                LeanTween.scale(Image.GetComponent<RectTransform>(), Image.GetComponent<UniversalButton>().startSize, .25f);
                Image.GetComponent<Image>().color = Color.red;
            }
        }
        if (ButtonNumber == Pos)
        {
            Image.GetComponent<UniversalButton>().active[1] = true;
            LeanTween.scale(Image.GetComponent<RectTransform>(), Image.GetComponent<UniversalButton>().selectedSize, .25f);
            Image.GetComponent<Image>().color = Image.GetComponent<UniversalButton>().TargetColor;
        }
    }

    [System.Serializable]
    public class Slider
    {
        Settings settings;

        public string GDVariableName;
        public UnityEngine.UI.Slider slider;
        public Image image;
        public GameObject currentValueUI;
        public Image Handle;

        GameDataManager.GameData GD;

        TMP_Text[] values;

        float timeSinceValChanged;
        bool editMode;

        public void Start()
        {
            settings = FindObjectOfType<Settings>();

            slider.onValueChanged.AddListener(delegate { OnValueChanged(); });

            if (currentValueUI.transform.childCount > 0)
            {
                values = new TMP_Text[currentValueUI.transform.childCount];

                for (int i = 0; i < currentValueUI.transform.childCount; i++)
                {
                    values[i] = currentValueUI.transform.GetChild(i).GetComponent<TMP_Text>();
                }
            }

            GD = GameDataManager.GetGameData();
            var value = (float)GD.settingsData.GetType().GetField(GDVariableName).GetValue(GD.settingsData);
            slider.value = value;

            CurrentValueSetColor();
        }

        public void Update()
        {
            timeSinceValChanged += Time.deltaTime;

            if (timeSinceValChanged > .5f && values is null)
            {
                LeanTween.cancel(currentValueUI);
                LeanTween.value(currentValueUI, currentValueUI.GetComponent<TMP_Text>().color, Color.white, .25f).setEaseOutCubic().setOnUpdate((Color col) =>
                {
                    currentValueUI.GetComponent<TMP_Text>().color = col;
                });
            }

            if (editMode)
            {
                int factor = 0;
                if (settings.input.right) { factor = 1; }
                if (settings.input.left) { factor = -1; }

                if (values is null)
                {
                    slider.value += factor * ((slider.maxValue - slider.minValue) * 0.01f);
                }
                else
                {
                    if (settings.input.right && !settings.rightB)
                    {
                        slider.value += 1;
                    }

                    if (settings.input.left && !settings.leftB)
                    {
                        slider.value -= 1;
                    }
                }
            }
        }

        public void Select()
        {
            LeanTween.cancel(image.gameObject);
            LeanTween.value(image.gameObject, image.color, new Color(1, 0.345098f, 0.1019608f), .25f).setEaseOutCubic().setOnUpdate((Color col) =>
            {
                image.color = col;
            });
        }

        public void Deselect()
        {
            LeanTween.cancel(image.gameObject);
            LeanTween.value(image.gameObject, image.color, Color.black, .25f).setEaseOutCubic().setOnUpdate((Color col) =>
            {
                image.color = col;
            });
        }

        public void EnterEdit()
        {
            editMode = true;

            LeanTween.cancel(Handle.gameObject);
            LeanTween.value(Handle.gameObject, Handle.color, slider.colors.pressedColor, .25f).setEaseOutCubic().setOnUpdate((Color col) =>
            {
                Handle.color = col;
            });
        }

        public void ExitEdit()
        {
            editMode = false;

            LeanTween.cancel(Handle.gameObject);
            LeanTween.value(Handle.gameObject, Handle.color, Color.white, .25f).setEaseOutCubic().setOnUpdate((Color col) =>
            {
                Handle.color = col;
            });
        }

        public void OnValueChanged()
        {
            timeSinceValChanged = 0;

            if (values is null)
            {
                currentValueUI.GetComponent<TMP_Text>().text = Math.Round(slider.value, 2).ToString();

                LeanTween.cancel(currentValueUI);
                LeanTween.value(currentValueUI, currentValueUI.GetComponent<TMP_Text>().color, new Color(1, 0.6431373f, 0), .25f).setEaseOutCubic().setOnUpdate((Color col) =>
                {
                    currentValueUI.GetComponent<TMP_Text>().color = col;
                });
            }

            if (image.transform.childCount > 0)
            {
                if (slider.value == 0)
                {
                    image.transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    image.transform.GetChild(0).gameObject.SetActive(false);
                }
            }

            CurrentValueSetColor();
        }

        private void CurrentValueSetColor()
        {
            if (values is not null)
            {
                values[(int)slider.value].color = new Color(1, 0.345098f, 0.1019608f);
                for (int i = 0; i < currentValueUI.transform.childCount; i++)
                {
                    if (i == slider.value) { continue; }
                    values[i].color = Color.white;
                }
            }
        }
    }
}