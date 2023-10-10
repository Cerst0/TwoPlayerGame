using Mirror;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class Menu : MonoBehaviour
{
    [Header("GameInfos")]
    public string[] GameNames;
    public Vector2[] Positions;
    public string[] maxGamePlayerCount;
    public string[] maxGamePlayerCountText;
    public float[] ScrollbarValue;
    public int maxYPos;

    [Header("GameInfos-AutoCollect/Other")]
    GameObject[] VideoMasks = new GameObject[100];
    VideoPlayer[] VideoPlayer = new VideoPlayer[100];
    GameObject[] DescriptionImages = new GameObject[100];
    GameObject[] Images = new GameObject[100];
    GameObject[] Thumbnails = new GameObject[100];
    Sprite[] GamePreviewSprites = new Sprite[100];
    GameObject[] TickBoxes = new GameObject[100];

    bool[] playVideo = new bool[100];
    bool[] activeGames = new bool[100];
    int[] tweeningState = new int[100];

    [Header("Other")]
    InputS input;
    Status status;
    public ConfirmExit ce;
    public Scrollbar Sb;
    public InputAction scroll;
    public PlayerEditor[] PEs;
    public PlayerPanel[] PlayerPanelScripts;
    public PlayerCountSelector PCS;

    public Vector2 ButtonNumber;
    Vector2 ButtonNumberBefore;

    string buttonName;
    public string panelName = "menu";
    public string panelNameB = "menu";

    public float animationSpeed;

    bool upB;
    bool downB;
    bool leftB;
    bool rightB;
    bool enter;

    int playerNumB = -5;
    int playerNumB2 = -5;

    int partyEditorStat = -1;

    float scrollB;

    public int BindingsStat;
    public int ConfirmExitStat;
    public int SettingsStat;
    public int PlayerCountSelectorStat = 2;
    public int Player0Stat;
    public int Player1Stat;
    public int Player2Stat;
    public int Player3Stat;

    public bool networkWait;

    public KeyBindings KeyBindingsS;
    public Settings settings;
    public NetworkManager manager;
    Sync sync;

    public AudioSource ThemeMusic;
    public AudioSource ChangeSelection;

    public TMP_Text[] PointsPlayers;
    public TextMeshProUGUI Player0Name;
    public TextMeshProUGUI Player1Name;
    public TextMeshProUGUI Player2Name;
    public TextMeshProUGUI Player3Name;
    public TMP_Text PartyButton;
    public TMP_Text PartyText;

    public GameObject BindingsPanel;
    public GameObject ConfirmExitPanel;
    public GameObject SettingsPanel;
    public GameObject Player0Panel;
    public GameObject Player1Panel;
    public GameObject MenuPanel;
    public GameObject MainPanel;
    public GameObject PartyGameButtons;
    public GameObject PartyFieldButtons;
    public GameObject PlayerCountSelectorPanel;

    public GameObject[] Pencils;

    public Texture2D cursorNormal;

    public Color CloseGold;
    public Color CloseBlue;
    public Color ButtonSelected;
    public Color ButtonUnSelected;
    public Color ButtonDescriptionSelected;
    public Color ButtonDescriptionUnSelected;
    public Color Orange;


    [Header("Exit")]
    public GameObject ExitDescriptionImage;
    public GameObject ExitImage;

    [Header("Settings")]
    public GameObject SettingsDescriptionImage;
    public GameObject SettingsImage;

    [Header("Party")]
    public GameObject PartyImage;
    public GameObject FakeDescriptionImage;
    public GameObject PartyColorImage;

    [Header("ResetScore")]
    public GameObject ResetScoreDescriptionImage;
    public GameObject ResetScoreImage;

    [Header("KeyBindings")]
    public GameObject BindingsDescriptionImage;
    public GameObject BindingsImage;

    [Header("StartGame")]
    public GameObject StartGameDescriptionImage;
    public GameObject StartGameImage;

    [Header("SelectAll")]
    public GameObject SelectAllDescriptionImage;
    public GameObject SelectAllImage;

    [Header("DeselectAll")]
    public GameObject DeselectAllDescriptionImage;
    public GameObject DeselectAllImage;

    [Header("Fields")]
    public GameObject ArrowRight;
    public GameObject ArrowLeft;
    public GameObject Count;
    public GameObject SpecialFieldsBox;

    private void Awake()
    {
        if (GameObject.Find("@DontDestroyOnLoadObjects") == null)
        {
            UnityEngine.Object obj = Instantiate(Resources.Load("@DontDestroyOnLoadObjects"));
            obj.name = "@DontDestroyOnLoadObjects";
        }
        FindObjectOfType<Methods>().DeleteAllNetworkPlayers();
    }

    void Start()
    {
        status = FindObjectOfType<Status>();
        input = FindObjectOfType<InputS>();

        MenuPanel.SetActive(true);
        foreach (var (s, index) in GameNames.Select((value, i) => (value, i)))
        {
            Images[index] = GameObject.Find("/Canvas/Menu/Games/Scroll View/Viewport/Content/" + s + "/ButtonEdges");
            DescriptionImages[index] = GameObject.Find("/Canvas/Menu/Games/Scroll View/Viewport/Content/" + s + "/ButtonDescription Image");
            VideoMasks[index] = GameObject.Find("/Canvas/Menu/Games/Scroll View/Viewport/Content/" + s + "/Mask");
            Thumbnails[index] = VideoMasks[index].transform.GetChild(0).gameObject;
            VideoPlayer[index] = GameObject.Find("/Canvas/Menu/Games/Scroll View/Viewport/Content/" + s + "/Video").GetComponent<VideoPlayer>();
            GamePreviewSprites[index] = FindObjectOfType<LoadingScreen>().GamePreviews[index];
            TickBoxes[index] = GameObject.Find("/Canvas/Menu/Games/Scroll View/Viewport/Content/" + s + "/TickBox");
        }

        Cursor.lockState = CursorLockMode.None;
        foreach (var (s, index) in GameNames.Select((value, i) => (value, i)))
        {
            VideoMasks[index].transform.GetChild(1).gameObject.SetActive(false);
            VideoPlayer[index].Prepare();
            TickBoxes[index].SetActive(false);
        }

        BindingsPanel.SetActive(false);
        foreach (GameObject go in Pencils)
        {
            go.SetActive(false);
        }
        SettingsPanel.SetActive(false);
        Player0Panel.SetActive(false);
        Player1Panel.SetActive(false);
        FindObjectOfType<Methods>().SetServerListnening(true);

        UpdateButtonState();

        SetButtonToSelected(ExitDescriptionImage, ExitImage, false, 0, new Vector2(3, 1), true);
        SetButtonToSelected(FakeDescriptionImage, PartyImage, false, 0, new Vector2(3, 1), true, new Vector2(5.45f, 5.45f), new Vector2(5.45f * 1.1f, 5.45f * 1.1f), true, PartyColorImage);
        SetButtonToSelected(SettingsDescriptionImage, SettingsImage, false, 0, new Vector2(2, 1), true);
        SetButtonToSelected(BindingsDescriptionImage, BindingsImage, false, 0, new Vector2(1, 1), true);
        SetButtonToSelected(StartGameDescriptionImage, StartGameImage, false, 0, new Vector2(0, 0), true, new Vector2(1.5f, 1.5f), new Vector2(1.5f * 1.1f, 1.5f * 1.1f), true);
        SetButtonToSelected(ResetScoreDescriptionImage, ResetScoreImage, false, 0, new Vector2(0, 1), true);
        SetButtonToSelected(SelectAllDescriptionImage, SelectAllImage, false, 0, new Vector2(1, 1), true);
        SetButtonToSelected(DeselectAllDescriptionImage, DeselectAllImage, false, 0, new Vector2(3, 1), true);
        SetButtonToSelected(FakeDescriptionImage, ArrowRight, false, 0, new Vector2(3, 0), true, new Vector2(3, 3), new Vector2(3 * 1.1f, 3 * 1.1f), true, null, Orange, Color.white);
        SetButtonToSelected(FakeDescriptionImage, ArrowLeft, false, 0, new Vector2(2, 0), true, new Vector2(3, 3), new Vector2(3 * 1.1f, 3 * 1.1f), true, null, Orange, Color.white);
        SetButtonToSelected(FakeDescriptionImage, SpecialFieldsBox, false, 0, new Vector2(2, -1), true, new Vector2(1.2f, 1.2f), new Vector2(1.2f * 1.1f, 1.2f * 1.1f), true, FakeDescriptionImage);

        if (networkWait) { ButtonNumber = new Vector2(2, 1); }

        if (status.clientPlayerID > 0 || status.playerNumber == 1)
        {
            PartyImage.SetActive(false);
        }

        FindObjectOfType<LoadingScreen>().DisableLoadingScreen();
    }
    private void OnEnable() { scroll.Enable(); }

    void Update()
    {
        buttonName = status.buttonName;

        float scrollVal = Mathf.Lerp(scroll.ReadValue<Vector2>().y, scrollB, 0.95f);
        Sb.value += scrollVal / 750;
        scrollB = scrollVal;

        if (!ThemeMusic.isPlaying) { ThemeMusic.Play(); }

        if (panelName == "menu") { ChangeSelectedVector(); KeyPressed(); }

        ChangePanel(BindingsPanel, BindingsImage, ref BindingsStat, ref KeyBindingsS.ButtonNumber);
        ChangePanel(PlayerCountSelectorPanel, PlayerCountSelectorPanel, ref PlayerCountSelectorStat, ref PCS.buttonNumber);
        ChangePanel(SettingsPanel, SettingsImage, ref SettingsStat, ref settings.ButtonNumber);
        ChangePanel(ConfirmExitPanel, ExitImage, ref ConfirmExitStat, ref GetComponent<ConfirmExit>().ButtonNumber);
        ChangePanel(Player0Panel, Player0Name.gameObject, ref Player0Stat, ref PlayerPanelScripts[0].ButtonNumber);
        ChangePanel(Player1Panel, Player1Name.gameObject, ref Player1Stat, ref PlayerPanelScripts[1].ButtonNumber);

        foreach (var (s, index) in GameNames.Select((value, i) => (value, i)))
        {
            if (playVideo[index] == true && activeGames[index] == true)
            {
                if (VideoPlayer[index].isPrepared)
                {
                    VideoMasks[index].transform.GetChild(1).gameObject.SetActive(true);
                    VideoPlayer[index].Play();
                }
            }
            else
            {
                VideoPlayer[index].Stop();
                VideoMasks[index].transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            Pencils[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(PEs[i].textOverflowX + (Pencils[i].GetComponent<RectTransform>().rect.width / 2), Pencils[i].GetComponent<RectTransform>().anchoredPosition.y);
        }

        if (MenuPanel.GetComponent<CanvasGroup>().alpha == 0) { MenuPanel.GetComponent<CanvasGroup>().blocksRaycasts = false; }
        else { MenuPanel.GetComponent<CanvasGroup>().blocksRaycasts = true; }

        if (NetworkClient.isConnected)
        {
            if (sync != null)
            {
                if (status.playerNumber != playerNumB2)
                {
                    TMP_Text[] PlayerTexts = { Player0Name, Player1Name, Player2Name, Player3Name };

                    foreach (TMP_Text text in PlayerTexts)
                    {
                        GameObject go = text.transform.parent.gameObject;
                        if (!go.name.Contains(status.clientPlayerID.ToString()))
                        {
                            Destroy(go.GetComponent<UniversalButton>());
                            go.AddComponent<UniversalButton>().ignoreAlpha = true;
                            LeanTween.scale(go.GetComponent<RectTransform>(), new Vector3(.75f, .75f, .75f), 0.5f);
                            go.transform.parent.GetComponent<HorizontalLayoutGroup>().spacing = -300;
                        }
                    }
                    playerNumB2 = status.playerNumber;
                }
            }
            else
            {
                    sync = GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>();
            }
        }

        if (status.playerNumber != playerNumB)
        {
            FindObjectOfType<Methods>().SetPointsColorOrder(PointsPlayers);

            foreach (var (s, index) in GameNames.Select((value, i) => (value, i)))
            {
                bool isGameMatchingWitchNum = false;

                string allowedPlayers = maxGamePlayerCount[index];

                int[] allowedPlayersInt = new int[4];
                allowedPlayersInt[0] = int.Parse(allowedPlayers[0].ToString());
                allowedPlayersInt[1] = int.Parse(allowedPlayers[1].ToString());
                allowedPlayersInt[2] = int.Parse(allowedPlayers[2].ToString());
                allowedPlayersInt[3] = int.Parse(allowedPlayers[3].ToString());

                for (int i = 0; i < 4; i++)
                {
                    if (allowedPlayersInt[i] == status.playerNumber)
                    {
                        isGameMatchingWitchNum = true;
                    }
                }

                if (!isGameMatchingWitchNum)
                {
                    activeGames[index] = false;

                    GameObject ThumbNail = Thumbnails[index];
                    ThumbNail.transform.parent.GetComponent<UniversalButton>().enabled = false;
                    ThumbNail.GetComponent<Image>().sprite = null;

                    TMP_Text text = Images[index].transform.parent.GetChild(4).GetComponent<TMP_Text>();
                    string numbers = maxGamePlayerCountText[index];
                    string newText = text.text;
                    int startChar = newText.IndexOf("<size=50>") + 9;
                    int endChar = newText.IndexOf("<br><color=");
                    newText = newText.Remove(startChar, endChar - startChar);
                    newText = newText.Insert(startChar, numbers);
                    text.SetText(newText);
                    text.parseCtrlCharacters = false;
                    text.parseCtrlCharacters = true;
                    text.gameObject.SetActive(true);

                }
                else
                {
                    activeGames[index] = true;

                    Thumbnails[index].transform.parent.GetComponent<UniversalButton>().enabled = true;
                    Color c = Thumbnails[index].GetComponent<Image>().color; c.a = 1;
                    Thumbnails[index].GetComponent<Image>().color = c;
                    LeanTween.alpha(Thumbnails[index].GetComponent<RectTransform>(), 1, 0);
                    Thumbnails[index].GetComponent<Image>().sprite = GamePreviewSprites[index];

                    TMP_Text text = Images[index].transform.parent.GetChild(4).GetComponent<TMP_Text>();
                    text.gameObject.SetActive(false);
                }
            }
            playerNumB = status.playerNumber;
        }
    }

    private void LateUpdate()
    {
        panelNameB = panelName;
    }

    private void KeyPressed()
    {
        enter = input.enter;

        foreach (var (s, index) in GameNames.Select((value, i) => (value, i)))
        {
            Vector2 pos2 = Positions[index];
            pos2 = new Vector2(pos2.x + 1, pos2.y);
            bool rightPos = ButtonNumber.Equals(Positions[index]) | ButtonNumber.Equals(pos2);

            if (((enter && rightPos) | buttonName == s) && !networkWait && activeGames[index] == true)
            {
                if (partyEditorStat == -1)
                {
                    FindObjectOfType<LoadingScreen>().SetLoadingScreen(index);
                    FindObjectOfType<Methods>().SetServerListnening(false);
                    print("activated Loadingscreen: " + GamePreviewSprites[index]);
                    status.gameIndex = index;

                    if (NetworkClient.active)
                    {
                        sync.LoadScene(index + 2, true, index);
                    }
                    else { SceneManager.LoadSceneAsync(index + 2); }
                    Destroy(GetComponent<Menu>());
                }
                if (partyEditorStat == 1)
                {
                    if (TickBoxes[index].GetComponent<Image>().color == Color.white)
                    {
                        SelectTickBox(index);
                    }
                    else
                    {
                        SelectTickBox(index, true);
                    }
                }
            }
        }

        OnButtonPressed();

        if (networkWait && status.clientPlayerID == 0 && status.playerNumber > 1)
        {
            StartGameImage.SetActive(true);
        }
        else
        {
            StartGameImage.SetActive(false);
        }
    }

    private void SelectTickBox(int index, bool deselect = false)
    {
        if (TickBoxes[index] is null) { return; }
        if (deselect || !activeGames[index])
        {
            TickBoxes[index].GetComponent<Image>().color = Color.white;
            FindObjectOfType<Party>().selectedGames[index] = false;
        }
        else
        {
            TickBoxes[index].GetComponent<Image>().color = Orange;
            FindObjectOfType<Party>().selectedGames[index] = true;
        }
    }

    private void UpdateButtonState()
    {
        foreach (var (s, index) in GameNames.Select((value, i) => (value, i)))
        {
            Vector2 pos2 = Positions[index];
            pos2 = new Vector2(pos2.x + 1, pos2.y);
            bool rightPos = ButtonNumber.Equals(Positions[index]) | ButtonNumber.Equals(pos2);

            if (rightPos)
            {
                SetButtonToSelected(DescriptionImages[index], Images[index], true, index);
                playVideo[index] = true;
                VideoPlayer[index].Prepare();
            }
            else
            {
                SetButtonToUnSelected(DescriptionImages[index], Images[index], true, index);
                playVideo[index] = false;
            }
        }
    }

    private void OnButtonPressed()
    {
        if (partyEditorStat == -1)
        {
            if ((enter && ButtonNumber.Equals(new Vector2(4, 1))) | buttonName == "Exit")
            {
                ConfirmExitPanel.transform.localScale = new Vector3(0, 0, 0);
                ConfirmExitStat = 1;
                panelName = "exit";
                Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
            }

            if ((enter && ButtonNumber.Equals(new Vector2(3, 1))) | buttonName == "Settings")
            {
                SettingsPanel.transform.localScale = new Vector3(0, 0, 0);
                SettingsStat = 1;
                panelName = "settings";
                Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
            }

            if ((enter && ButtonNumber.Equals(new Vector2(1, 1))) | buttonName == "KeyBindings")
            {
                BindingsPanel.transform.localScale = new Vector3(0, 0, 0);
                BindingsStat = 1;
                panelName = "bindings";
                Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
            }

            if ((input.enter && ButtonNumber.Equals(new Vector2(0, 1))) | buttonName == "ResetScoreButton")
            {
                if (NetworkClient.active)
                {
                    for (int i = 0; i < status.points.Length; i++)
                    {
                        status.points[i] = 0;
                    }
                }
                else
                {
                    if (status.playerNumber == 1)
                    {
                        PlayerPrefs.SetInt("SinglePlayerPoint0", 0);
                    }
                    else
                    {
                        PlayerPrefs.SetInt("Point0", 0);
                        PlayerPrefs.SetInt("Point1", 0);
                    }
                }
                FindObjectOfType<Methods>().SetPointsColorOrder(PointsPlayers);
            }
        }
        if (partyEditorStat == 1)
        {
            if ((input.enter && ButtonNumber.Equals(new Vector2(1, 1))) | buttonName == "SelectAll")
            {
                for (int i = 0; i < TickBoxes.Length; i++)
                {
                    SelectTickBox(i);
                }
            }

            if ((input.enter && ButtonNumber.Equals(new Vector2(3, 1))) | buttonName == "DeselectAll")
            {
                for (int i = 0; i < TickBoxes.Length; i++)
                {
                    SelectTickBox(i, true);
                }
            }
        }
        if (partyEditorStat == 2)
        {
            if ((input.enter && ButtonNumber.Equals(new Vector2(2, 0))) | buttonName == "FieldArrowLeft")
            {
                ChangeFieldNumber(true);
            }

            if ((input.enter && ButtonNumber.Equals(new Vector2(3, 0))) | buttonName == "FieldArrowRight")
            {
                ChangeFieldNumber(false);
            }

            if ((input.enter && ButtonNumber.Equals(new Vector2(2, -1))) | buttonName == "SpecialFields")
            {
                int alpha = FindObjectOfType<Party>().specialFields ? 0 : 1;
                LeanTween.cancel(SpecialFieldsBox.transform.GetChild(0).gameObject);
                LeanTween.value(SpecialFieldsBox.transform.GetChild(0).gameObject, SpecialFieldsBox.transform.GetChild(0).GetComponent<Image>().color.a, alpha, animationSpeed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
                {
                    Color c = SpecialFieldsBox.transform.GetChild(0).GetComponent<Image>().color;
                    c.a = val;
                    SpecialFieldsBox.transform.GetChild(0).GetComponent<Image>().color = c;
                });

                FindObjectOfType<Party>().specialFields = !FindObjectOfType<Party>().specialFields;
            }
        }


        if (((enter && ButtonNumber.Equals(new Vector2(0, 0))) | buttonName == "StartGame") && networkWait)
        {
            PCS.connectionStat = 4;
        }

        if ((enter && ButtonNumber.Equals(new Vector2(0, -3))) | buttonName == "Player0" && (!NetworkClient.active | status.clientPlayerID == 0))
        {
            Player0Panel.transform.localScale = new Vector3(0, 0, 0);
            Player0Stat = 1;
            panelName = "player0";
            Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
        }
        if (ButtonNumber.Equals(new Vector2(0, -3))) { Player0Name.color = new Color(1, 0.3490196f, 0.1058824f, 1); Pencils[0].SetActive(true); }
        else { Player0Name.color = Color.white; }

        if ((enter && ButtonNumber.Equals(new Vector2(1, -3))) | buttonName == "Player1" && (!NetworkClient.active | status.clientPlayerID == 1))
        {
            Player1Panel.transform.localScale = new Vector3(0, 0, 0);
            Player1Stat = 1;
            panelName = "player1";
            Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
        }
        if (ButtonNumber.Equals(new Vector2(1, -3))) { Player1Name.color = new Color(1, 0.3490196f, 0.1058824f, 1); Pencils[1].SetActive(true); }
        else { Player1Name.color = Color.white; }


        if ((enter && ButtonNumber.Equals(new Vector2(2, 1))) | buttonName == "party")
        {
            if (partyEditorStat == 2)
            {
                FindObjectOfType<Methods>().SetServerListnening(false);
                FindObjectOfType<Party>().LoadRandomGame();
                Destroy(this);
            }
            if (partyEditorStat == 1)
            {
                if (FindObjectOfType<Party>().selectedGames.All(box => !box))
                {
                    FindObjectOfType<Notificaton>(BindingsImage).Notification(Color.red, "Select at least one Game", 2f);
                }
                else
                {
                    LeanTween.value(PartyGameButtons, PartyGameButtons.GetComponent<CanvasGroup>().alpha, 0, animationSpeed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
                    {
                        PartyGameButtons.GetComponent<CanvasGroup>().alpha = val;
                    });

                    LeanTween.value(PCS.Games, PCS.Games.GetComponent<CanvasGroup>().alpha, 0, animationSpeed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
                    {
                        PCS.Games.GetComponent<CanvasGroup>().alpha = val;
                    });

                    LeanTween.value(PartyFieldButtons, PartyFieldButtons.GetComponent<CanvasGroup>().alpha, 1, animationSpeed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
                    {
                        PartyFieldButtons.GetComponent<CanvasGroup>().alpha = val;
                    });

                    PartyButton.text = "Continue";
                    PartyText.text = "Configure Fields";

                    ButtonNumber = new Vector2(2, 1);

                    PartyGameButtons.GetComponent<CanvasGroup>().blocksRaycasts = false;
                    PCS.Games.GetComponent<CanvasGroup>().blocksRaycasts = false;
                    PartyFieldButtons.GetComponent<CanvasGroup>().blocksRaycasts = true;

                    partyEditorStat = 2;
                }
            }
            if (partyEditorStat == -1)
            {
                LeanTween.value(MainPanel, MainPanel.GetComponent<CanvasGroup>().alpha, 0, animationSpeed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
                {
                    MainPanel.GetComponent<CanvasGroup>().alpha = val;
                });

                LeanTween.value(PartyGameButtons, PartyGameButtons.GetComponent<CanvasGroup>().alpha, 1, animationSpeed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
                {
                    PartyGameButtons.GetComponent<CanvasGroup>().alpha = val;
                });

                for (int i = 0; i < TickBoxes.Length; i++)
                {
                    
                    TickBoxes[i]?.SetActive(true);
                    SelectTickBox(i);
                }

                PartyButton.text = "Continue";
                PartyText.text = "Select Games";

                ButtonNumber = new Vector2(2, 1);

                PartyGameButtons.GetComponent<CanvasGroup>().blocksRaycasts = true;
                MainPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;

                partyEditorStat = 1;
                status.isParty = true;
                FindObjectOfType<Party>().isParty = true;
                if (NetworkServer.active)
                {
                    GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().SetPartyState(true);
                }
            }
        }
    }

    private void ChangeFieldNumber(bool negative)
    {
        int fieldsold = int.Parse(Count.GetComponent<TMP_Text>().text);
        int fields = fieldsold;
        int summand = negative ? -3 : 3;

        fields += summand;
        fields = Mathf.Clamp(fields, 6, 30);

        Count.GetComponent<TMP_Text>().text = fields.ToString();
        FindObjectOfType<Party>(negative).fieldCount = fields;

        LeanTween.cancel(Count);
        if (fieldsold == fields)
        {
            LeanTween.value(Count, Color.white, Color.red, animationSpeed).setEaseInOutQuad().setIgnoreTimeScale(true).setLoopPingPong(1).setOnUpdate((Color col) =>
            {
                Count.GetComponent<TMP_Text>().color = col;
            });
        }
        else
        {
            LeanTween.value(Count, Color.white, CloseGold, animationSpeed).setEaseInOutQuad().setIgnoreTimeScale(true).setLoopPingPong(1).setOnUpdate((Color col) =>
            {
                Count.GetComponent<TMP_Text>().color = col;
            });
        }
    }

    private void ChangePanel(GameObject Panel, GameObject TargetPos, ref int state, ref Vector2 buttonNumber)
    {
        Vector2 target = Vector2.zero;
        if (TargetPos != null)
        {
            target = TargetPos.GetComponent<RectTransform>().position;
        }

        if (state == 1)
        {
            Panel.SetActive(true);
            LeanTween.value(MenuPanel, 1, 0, animationSpeed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
            {
                MenuPanel.GetComponent<CanvasGroup>().alpha = val;
            });
            LeanTween.scale(Panel.GetComponent<RectTransform>(), new Vector3(1, 1, 1), animationSpeed).setEaseInOutQuad();
            Panel.GetComponent<RectTransform>().position = target;
            LeanTween.move(Panel.GetComponent<RectTransform>(), Vector2.zero, animationSpeed).setEaseInOutQuad();
            state = 2;
        }
        if (state == 3)
        {
            LeanTween.value(MenuPanel, 0, 1, animationSpeed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
            {
                MenuPanel.GetComponent<CanvasGroup>().alpha = val;
            });
            LeanTween.scale(Panel, Vector3.zero, animationSpeed).setEaseInOutQuad();
            LeanTween.move(Panel, target, animationSpeed).setEaseInOutQuad();

            panelName = "menu";
            buttonNumber = Vector2.zero;
            state = 0;
        }
    }

    private void ChangeSelectedVector()
    {
        if (ButtonNumber != ButtonNumberBefore)
        {
            UpdateButtonState();

            ChangeSelection.Play();

            SetButtonToSelected(ExitDescriptionImage, ExitImage, false, 0, new Vector2(4, 1));
            SetButtonToSelected(FakeDescriptionImage, PartyImage, false, 0, new Vector2(2, 1), false, new Vector2(5.45f, 5.45f), new Vector2(5.45f * 1.1f, 5.45f * 1.1f), true, PartyColorImage);
            SetButtonToSelected(SettingsDescriptionImage, SettingsImage, false, 0, new Vector2(3, 1));
            SetButtonToSelected(BindingsDescriptionImage, BindingsImage, false, 0, new Vector2(1, 1));
            SetButtonToSelected(StartGameDescriptionImage, StartGameImage, false, 0, new Vector2(0, 0), false, new Vector2(1.5f, 1.5f), new Vector2(1.5f * 1.1f, 1.5f * 1.1f), true);
            SetButtonToSelected(ResetScoreDescriptionImage, ResetScoreImage, false, 0, new Vector2(0, 1));
            SetButtonToSelected(SelectAllDescriptionImage, SelectAllImage, false, 0, new Vector2(1, 1));
            SetButtonToSelected(DeselectAllDescriptionImage, DeselectAllImage, false, 0, new Vector2(3, 1));
            SetButtonToSelected(FakeDescriptionImage, ArrowRight, false, 0, new Vector2(3, 0), false, new Vector2(3, 3), new Vector2(3 * 1.1f, 3 * 1.1f), true, null, Orange, Color.white);
            SetButtonToSelected(FakeDescriptionImage, ArrowLeft, false, 0, new Vector2(2, 0), false, new Vector2(3, 3), new Vector2(3 * 1.1f, 3 * 1.1f), true, null, Orange, Color.white);
            SetButtonToSelected(FakeDescriptionImage, SpecialFieldsBox, false, 0, new Vector2(2, -1), false, new Vector2(1.2f, 1.2f), new Vector2(1.2f * 1.1f, 1.2f * 1.1f), true, FakeDescriptionImage);

            if (ButtonNumberBefore.Equals(new Vector2(0, -3))) { Pencils[0].SetActive(false); }
            if (ButtonNumberBefore.Equals(new Vector2(1, -3))) { Pencils[1].SetActive(false); }
            if (ButtonNumberBefore.Equals(new Vector2(2, -3))) { Pencils[2].SetActive(false); }
            if (ButtonNumberBefore.Equals(new Vector2(3, -3))) { Pencils[3].SetActive(false); }

            foreach (var (s, index) in GameNames.Select((value, i) => (value, i)))
            {
                Vector2 pos2 = Positions[index];
                pos2 = new Vector2(pos2.x + 1, pos2.y);
                bool rightPos = ButtonNumber.Equals(Positions[index]) | ButtonNumber.Equals(pos2);
                if (rightPos)
                {
                    LeanTween.value(gameObject, Sb.value, ScrollbarValue[index], 0.2f).setOnUpdate((float val) =>
                    {
                        Sb.value = val;
                    });
                }
            }
        }
        ButtonNumberBefore = ButtonNumber;



        if (input.up && (input.up != upB) && ButtonNumber.y < 1)
        {
            if (networkWait && ButtonNumber.y == maxYPos)
            {
                if (StartGameImage.activeSelf) { ButtonNumber = Vector2.zero; }
                else { ButtonNumber.y = 1; }
            }
            else
            {
                ButtonNumber.y += 1;

                if (partyEditorStat != -1 && ButtonNumber.y == 1)
                {
                    ButtonNumber.x = 2;
                }
            }
        }

        if (input.down && (input.down != downB) && ButtonNumber.y > maxYPos)
        {
            if (networkWait && (ButtonNumber.y == 1 | ButtonNumber.y == 0))
            {
                if (ButtonNumber.y == 0)
                {
                    ButtonNumber.y = maxYPos;
                }
                if (ButtonNumber.y == 1)
                {
                    if (!StartGameImage.activeSelf) { ButtonNumber.y = maxYPos; }
                    if (StartGameImage.activeSelf) { ButtonNumber = Vector2.zero; }
                }
            }
            else
            {
                if (partyEditorStat == -1 || (partyEditorStat == 1 && ButtonNumber.y > maxYPos + 1) || (partyEditorStat == 2 && ButtonNumber.y > -1))
                {
                    ButtonNumber.y -= 1;
                }
            }
            if (ButtonNumber.x == 4) { ButtonNumber.x = 3; }

            if (ButtonNumber.y == maxYPos)
            {
                if (NetworkClient.active) { ButtonNumber.x = status.clientPlayerID; }
                ButtonNumber.x = 0;
            }

            if (partyEditorStat == 2 && ButtonNumber.y == -1)
            {
                ButtonNumber.x = 2;
            }
        }

        bool isNetworkAndPlayer = (NetworkClient.active | status.playerNumber == 1) && ButtonNumber.y == maxYPos;
        if (!isNetworkAndPlayer)
        {
            if (input.right && (input.right != rightB) && ButtonNumber.x < 4)
            {
                if (ButtonNumber.x == 0 && ButtonNumber.y < 1 && ButtonNumber.y > maxYPos && partyEditorStat != 2) { ButtonNumber.x = 3; }
                else
                {
                    if (ButtonNumber.x < 3 || ButtonNumber.y == 1)
                    {
                        if (partyEditorStat == -1 || (partyEditorStat == 1 && ButtonNumber.x < 3) || (partyEditorStat == 2 && ButtonNumber.x < 3 && ButtonNumber.y == 0))
                        {
                            if (ButtonNumber.y > maxYPos || ButtonNumber.x < 1)
                            {
                                ButtonNumber.x += 1;
                            }
                        }
                    }
                }
            }


            if (input.left && (input.left != leftB) && ButtonNumber.x > 0)
            {
                if (ButtonNumber.y < 1 && ButtonNumber.x == 3 && ButtonNumber.y > maxYPos && partyEditorStat != 2) { ButtonNumber.x = 0; }
                else
                {
                    if (partyEditorStat == -1 || (partyEditorStat == 1 && ButtonNumber.x > 1) || (partyEditorStat == 2 && ButtonNumber.x > 2 && ButtonNumber.y == 0))
                    {
                        if (ButtonNumber.y > maxYPos || ButtonNumber.x > 0)
                        {
                            ButtonNumber.x -= 1;
                        }
                    }
                }
            }
        }

        upB = input.up;
        downB = input.down;
        leftB = input.left;
        rightB = input.right;
    }

    public void SetButtonToSelected(GameObject DescriptionImage, GameObject Image, bool isStartGameButton, int index,
        Vector2? buttonNumber = null, bool isStartDeselector = false, Vector2 customStartVector = new Vector2(), Vector2 customTargetVector = new Vector2(), bool customSize = false, GameObject sepperateColorImage = null, Color customColorSelected = default(Color), Color customColorUnselected = default(Color))
    {
        if (!isStartGameButton)
        {
            if (ButtonNumber.Equals(buttonNumber))
            {
                Image.GetComponent<UniversalButton>().active[1] = true;

                Vector2 target;
                if (customSize)
                {
                    target = customTargetVector;
                }
                else { target = new Vector2(1.1f, 1.1f); }
                LeanTween.scale(Image.GetComponent<RectTransform>(), target, animationSpeed);

                GameObject ColorImage;
                if (sepperateColorImage == null)
                {
                    ColorImage = Image;
                }
                else
                {
                    ColorImage = sepperateColorImage;
                }
                Color imageColor = customColorSelected == default(Color) ? ButtonSelected : customColorSelected;
                ColorImage.GetComponent<Image>().color = imageColor;
                DescriptionImage.GetComponent<Image>().color = ButtonDescriptionSelected;
            }
            if (ButtonNumberBefore.Equals(buttonNumber))
            {
                Image.GetComponent<UniversalButton>().active[1] = false;

                Vector2 target = new();
                if (customSize)
                {
                    target = customStartVector;
                }
                else { target = new Vector2(1, 1); }
                if (Image.GetComponent<UniversalButton>().active.All(active => active == false)) { LeanTween.scale(Image.GetComponent<RectTransform>(), target, animationSpeed); }

                GameObject ColorImage;
                if (sepperateColorImage == null)
                {
                    ColorImage = Image;
                }
                else
                {
                    ColorImage = sepperateColorImage;
                }

                Color imageColor = customColorUnselected == default(Color) ? ButtonUnSelected : customColorUnselected;
                ColorImage.GetComponent<Image>().color = imageColor;
                DescriptionImage.GetComponent<Image>().color = ButtonDescriptionUnSelected;
            }
        }
        else
        {
            UniversalButton UB = Thumbnails[index].transform.parent.GetComponent<UniversalButton>();
            UB.active[1] = true;
            SetColor(DescriptionImage.GetComponent<Image>(), "#bcbcbc");
            Image.GetComponent<Image>().color = new Color(Orange.r, Orange.g, Orange.b, Image.GetComponent<Image>().color.a);
            if (tweeningState[index] == 0)
            {
                LeanTween.scale(Image.transform.parent.gameObject, new Vector3(1.05f, 1.05f, 1), 0.1f);
                tweeningState[index] = 1;
            }
            if (activeGames[index] == false)
            {
                GameObject ThumbNail = Images[index].transform.parent.GetChild(0).gameObject;
                LeanTween.alpha(ThumbNail.GetComponent<RectTransform>(), 1, 0);
            }
            else
            {
                GameObject ThumbNail = Images[index].transform.parent.GetChild(0).gameObject;
                LeanTween.alpha(ThumbNail.GetComponent<RectTransform>(), 0.5f, animationSpeed);
            }
        }
        if (isStartDeselector)
        {
            Image.GetComponent<UniversalButton>().active[1] = false;

            Vector2 target = new();
            if (customSize)
            {
                target = customStartVector;
            }
            else { target = new Vector2(1, 1); }
            LeanTween.scale(Image.GetComponent<RectTransform>(), target, 0);

            GameObject ColorImage;
            if (sepperateColorImage == null)
            {
                ColorImage = Image;
            }
            else
            {
                ColorImage = sepperateColorImage;
            }

            Color imageColor = customColorUnselected == default(Color) ? ButtonUnSelected : customColorUnselected;
            ColorImage.GetComponent<Image>().color = imageColor;
            DescriptionImage.GetComponent<Image>().color = ButtonDescriptionUnSelected;
        }
    }

    public void SetButtonToUnSelected(GameObject DescriptionImage, GameObject Image, bool isStartGameButton, int index)
    {
        if (isStartGameButton)
        {
            UniversalButton UB = Thumbnails[index].transform.parent.GetComponent<UniversalButton>();
            UB.active[1] = false;
            SetColor(DescriptionImage.GetComponent<Image>(), "#797980");

            if (tweeningState[index] == 1)
            {
                if (UB.active.All(active => active == false)) { LeanTween.scale(Image.transform.parent.gameObject, new Vector3(1f, 1f, 1), 0.1f); Image.GetComponent<Image>().color = new Color(0, 0, 0); }
                tweeningState[index] = 0;
            }
            if (activeGames[index] == false)
            {
                GameObject ThumbNail = Images[index].transform.parent.GetChild(0).gameObject;
                LeanTween.alpha(ThumbNail.GetComponent<RectTransform>(), 0.5f, animationSpeed);
            }
            else
            {
                GameObject ThumbNail = Images[index].transform.parent.GetChild(0).gameObject;
                LeanTween.alpha(ThumbNail.GetComponent<RectTransform>(), 1, 0);
            }
        }
    }

    public void SetColor(Image image, string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        image.color = new Color(c.r, c.g, c.b, image.color.a);
    }

    public void SetCloseColor(Image CloseColorImage, bool setSelected)
    {
        print("setclose color " + setSelected + CloseColorImage.name);
        if (setSelected)
        {
            CloseColorImage.transform.parent.GetComponentInChildren<UniversalButton>().active[1] = true;
            LeanTween.color(CloseColorImage.gameObject.GetComponent<RectTransform>(), CloseGold, animationSpeed);
        }
        else
        {
            CloseColorImage.transform.parent.GetComponentInChildren<UniversalButton>().active[1] = false;
            if (CloseColorImage.transform.parent.GetComponentInChildren<UniversalButton>().active.All(active => active == false))
            {
                LeanTween.color(CloseColorImage.gameObject.GetComponent<RectTransform>(), CloseBlue, animationSpeed);
            }
        }
    }
}
