using Mirror;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public bool isPause = false;
    float animSpeed = 0.25f;
    public bool startMusic;
    public bool freeze;

    InputS input;
    Status status;
    Functions functions;

    int ButtonNumber;
    int buttonNumberBefore;
    bool esc;
    bool rightB;
    bool leftB;
    int openPause;
    int GC;
    bool closePause;
    float delay;
    string buttonName;

    public GameObject CountDown;
    public GameObject ScoreBoard;
    public AudioSource ChangeSelection;

    public Sprite none;

    public GameObject ResumeButton;
    public GameObject ResumeButtonDescription;

    public GameObject RestartButton;
    public GameObject RestartButtonDescription;

    public GameObject MenuButton;
    public GameObject MenuButtonDescription;

    private void Start()
    {
        functions = FindObjectOfType<Functions>();
        status = FindObjectOfType<Status>();
        input = FindObjectOfType<InputS>();

        ResumeButton.SetActive(false);
        MenuButton.SetActive(false);
        RestartButton.SetActive(false);

        SetButton(ResumeButtonDescription, ResumeButton, 0, true);
        SetButton(RestartButtonDescription, RestartButton, 1, true);
        SetButton(MenuButtonDescription, MenuButton, 2, true);

        if (status.isParty)
        {
            MenuButtonDescription.transform.GetChild(0).GetComponent<TMP_Text>().text = "CancelParty";
        }
    }

    private void Update()
    {
        if (isPause) { esc = input.esc; }

        if (input.esc && !isPause && !status.scoredPoint)
        {
            OpenPause();
        }

        if (esc && isPause)
        {
            ClosePause();
        }

        if (openPause == 1)
        {
            if ((status.clientPlayerID == 0 && NetworkClient.active) | !NetworkClient.active)
            {
                ResumeButton.SetActive(true);
                MenuButton.SetActive(true);
                if (!status.isParty) { RestartButton.SetActive(true); }

                if (status.isParty)
                {
                    LeanTween.moveX(ResumeButton.GetComponent<RectTransform>(), 0, animSpeed).setEaseInOutQuad().setIgnoreTimeScale(true);
                }
                else
                {
                    LeanTween.moveX(ResumeButton.GetComponent<RectTransform>(), -238, animSpeed).setEaseInOutQuad().setIgnoreTimeScale(true);
                }
                LeanTween.moveX(RestartButton.GetComponent<RectTransform>(), 238, animSpeed).setEaseInOutQuad().setIgnoreTimeScale(true);
                LeanTween.moveX(MenuButton.GetComponent<RectTransform>(), 0, animSpeed).setEaseInOutQuad().setIgnoreTimeScale(true);
            }
            openPause = 3;
            delay = animSpeed;
        }

        if (closePause)
        {
            LeanTween.moveX(RestartButton.GetComponent<RectTransform>(), -800, animSpeed).setEaseInOutQuad().setIgnoreTimeScale(true);
            LeanTween.moveX(ResumeButton.GetComponent<RectTransform>(), -800, animSpeed).setEaseInOutQuad().setIgnoreTimeScale(true);
            LeanTween.moveX(MenuButton.GetComponent<RectTransform>(), 800, animSpeed).setEaseInOutQuad().setIgnoreTimeScale(true);
            startMusic = true;
            closePause = false;
            freeze = false;
        }

        if (isPause && status.clientPlayerID < 1)
        {
            KeyPressed();
            delay += -Time.deltaTime;
            if (delay < 0f) { delay = 10000000f; }
        }

        if (MenuButton.GetComponent<RectTransform>().anchoredPosition.x == 700f && !isPause && MenuButton.activeSelf == true)
        {
            ResumeButton.SetActive(false);
            MenuButton.SetActive(false);
            RestartButton.SetActive(false);
        }
    }

    public void ClosePause()
    {
        print("Exit Pause");
        if (NetworkClient.active && status.clientPlayerID == 0) { GameObject.Find("NetworkPlayer0").GetComponent<Sync>().ClosePauseOnServer(); }

        // Resume the game
        Time.timeScale = 1;
        isPause = false;
        closePause = true;
        openPause = 0;
        GC = 2;

        // Reset UI
        functions.SetUIScreen(true, true);
        CountDown.GetComponent<Canvas>().sortingOrder = 130;

        // Reset input directions
        input.up = false;
        input.down = false;
        input.left = false;
        input.right = false;
        esc = false;
        input.menu = false;

        if (!functions.scoredPoint && !status.countdown)
        {
            functions.movementLock = false;
        }
    }

    public void OpenPause(bool isCalledBySync = false)
    {
        if (NetworkClient.active && !isCalledBySync)
        {
            if (status.clientPlayerID != 0) { return; }
            GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().OpenPauseOnServer();
        }

        print("Pause");

        // Pause the game
        Time.timeScale = 0;
        isPause = true;
        freeze = true;

        // Update UI
        ResumeButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(-935, ResumeButton.GetComponent<RectTransform>().anchoredPosition.y, 0);
        RestartButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(-678, RestartButton.GetComponent<RectTransform>().anchoredPosition.y, 0);
        MenuButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(700, MenuButton.GetComponent<RectTransform>().anchoredPosition.y, 0);

        functions.SetUIScreen(true);
        CountDown.GetComponent<Canvas>().sortingOrder = 45;

        // Handle button number and input
        ButtonNumber = 0;
        input.menu = true;
        GC = 1;

        if (openPause == 0) { openPause = 1; }
    }

    private void LateUpdate()
    {
        if (isPause && status.clientPlayerID < 1) { ChangeSelectedVector(); }
    }

    public void ChangeSelectedVector()
    {
        if (input.right && !rightB && ButtonNumber < 2)
        {
            ButtonNumber += 1;
            if (status.isParty && ButtonNumber == 1) { ButtonNumber = 2; }
        }

        if (input.left && !leftB && ButtonNumber > 0)
        {
            ButtonNumber += -1;
            if (status.isParty && ButtonNumber == 1) { ButtonNumber = 0; }
        }

        if (ButtonNumber != buttonNumberBefore)
        {
            ChangeSelection.Play();

            SetButton(ResumeButtonDescription, ResumeButton, 0);
            SetButton(RestartButtonDescription, RestartButton, 1);
            SetButton(MenuButtonDescription, MenuButton, 2);
        }

        buttonNumberBefore = ButtonNumber;
        rightB = input.right;
        leftB = input.left;
    }

    public void KeyPressed()
    {
        buttonName = status.buttonName;

        if ((input.enter && ButtonNumber == 0) | buttonName == "ResumeButton")
        {
            ClosePause();
        }

        if ((input.enter && ButtonNumber == 1) | buttonName == "RestartButton")
        {
            functions.PlayAgain();
        }

        if ((input.enter && ButtonNumber == 2) | buttonName == "MenuButton")
        {
            FindObjectOfType<Methods>().LoadMenu();
        }
    }

    public void SetButton(GameObject DescriptionImage, GameObject Image, int selectedButton, bool deselect = false)
    {
        if (Image.GetComponent<UniversalButton>().enabled == false) { Image.GetComponent<UniversalButton>().enabled = true; }

        if (buttonNumberBefore == selectedButton | deselect)
        {
            Image.GetComponent<UniversalButton>().active[1] = false;
            if (Image.GetComponent<UniversalButton>().active.All(active => active == false)) { LeanTween.scale(Image.GetComponent<RectTransform>(), new Vector2(1f, 1f), animSpeed).setIgnoreTimeScale(true); }
            ColorUtility.TryParseHtmlString("#8A8AFF", out Color c);
            ColorUtility.TryParseHtmlString("#797980", out Color c2);
            Image.GetComponent<Image>().color = c;
            DescriptionImage.GetComponent<Image>().color = c2;
        }
        if (selectedButton == ButtonNumber)
        {
            Image.GetComponent<UniversalButton>().active[1] = true;
            LeanTween.scale(Image.GetComponent<RectTransform>(), new Vector2(1.05f, 1.05f), animSpeed).setIgnoreTimeScale(true);
            ColorUtility.TryParseHtmlString("#c3c3ed", out Color c);
            ColorUtility.TryParseHtmlString("#bcbcbc", out Color c2);
            Image.GetComponent<Image>().color = c;
            DescriptionImage.GetComponent<Image>().color = c2;
        }
    }

    void OnGUI()
    {
        if (GC == 2)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        GC = 0;
    }
}

