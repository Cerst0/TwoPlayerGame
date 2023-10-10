using Mirror;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyBindings : MonoBehaviour
{
    InputS input;
    Menu menu;
    Status status;
    PlayerInputManager manager;
    Notificaton n;

    public AudioSource ChangeSelection;
    public float animationSpeed;

    public Vector2 ButtonNumber;
    Vector2 ButtonNumberBefore;

    public int currenPlayerIndex;

    string buttonNameB;
    int count;
    int pairingStep;
    bool bindingsFor1Player;
    bool upB;
    bool downB;
    bool leftB;
    bool rightB;
    bool enter;
    bool isOpen;
    bool isOpenB;
    public bool isPairing;
    [HideInInspector] public bool listen;
    [HideInInspector] public string BName;

    public Texture2D cursorNormal;

    [Header("Buttons")]
    public GameObject CloseImage;
    public GameObject upBImage;
    public GameObject downBImage;
    public GameObject rightBImage;
    public GameObject leftBImage;
    public GameObject upRImage;
    public GameObject downRImage;
    public GameObject rightRImage;
    public GameObject leftRImage;
    public GameObject enter1Image;
    public GameObject enter2Image;
    public GameObject esc1Image;
    public GameObject esc2Image;
    public GameObject ResetImage;
    public GameObject ResetDescriptionImage;
    public TextMeshProUGUI ResetDescriptionText;
    public GameObject PairImage;
    public Sprite KeyboardMaterial;
    public Sprite GamepadMaterial;
    public Image PairButtonImage;
    public GameObject PairDescriptionImage;
    public GameObject PairingPanel;
    public GameObject CrossLine;
    public Text Player0BindingsText;
    public Text Player1BindingsText;

    [Header("Panel")]
    public Image PanelCloseColor;


    private void Start()
    {
        status = FindObjectOfType<Status>();
        input = FindObjectOfType<InputS>();
        n = FindObjectOfType<Notificaton>();
        menu = FindObjectOfType<Menu>();
        manager = FindObjectOfType<PlayerInputManager>();

        PairingPanel.SetActive(false);
    }

    private void Update()
    {
        bindingsFor1Player = NetworkClient.active | status.playerNumber == 1;

        if (isOpen && !isOpenB)
        {
            SetButton(ResetImage, ResetDescriptionImage, new Vector2(0, -6), true);
            SetButton(PairImage, PairDescriptionImage, new Vector2(1, -6), true);

            OnlineOrLocalCheck();
        }
        isOpenB = isOpen;

        if (isOpen) { enter = input.enter; }
        if (menu.BindingsStat == 2)
        {
            isOpen = true;
        }

        if (isOpen && !listen && !isPairing) { ChangeSelectedVector(); KeyPressed(); }
        if (isPairing)
        {
            print("is pairing");
            manager.EnableJoining();

            switch (pairingStep)
            {
                case 0:
                    {
                        PairingPanel.SetActive(true);
                        Cursor.lockState = CursorLockMode.Locked;
                        Color c = FindObjectOfType<Methods>().GetPlayerColor(0);
                        c.a = 0;
                        PairingPanel.GetComponent<Image>().color = c;
                        LeanTween.alpha(PairingPanel.GetComponent<RectTransform>(), 0.75f, animationSpeed);
                        PairingPanel.transform.GetChild(1).GetComponent<TMP_Text>().text = GameDataManager.GetGameData().PlayerDatas[0].name;


                        pairingStep = 1;
                        break;
                    }

                case 1:
                    {
                        if (currenPlayerIndex == 1)
                        {
                            pairingStep = 2;
                            Color c = FindObjectOfType<Methods>().GetPlayerColor(1);
                            c.a = .75f;
                            LeanTween.color(PairingPanel.GetComponent<RectTransform>(), c, animationSpeed);
                            PairingPanel.transform.GetChild(1).GetComponent<TMP_Text>().text = GameDataManager.GetGameData().PlayerDatas[1].name;
                        }
                        break;
                    }

                case 2:
                    {
                        if (currenPlayerIndex == 2)
                        {
                            manager.DisableJoining();
                            status.inputViewingType = 1;
                            PairingPanel.SetActive(false);
                            Cursor.lockState = CursorLockMode.None;
                            Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
                            LeanTween.alpha(PairingPanel.GetComponent<RectTransform>(), 0f, animationSpeed);
                            n.Notification(new Color(0, 1, 0, 0), "successfully paired", 1f);
                            pairingStep = 0;
                            isPairing = false;
                        }
                        break;
                    }
            }
        }
    }

    private void OnlineOrLocalCheck()
    {
        if (!bindingsFor1Player)
        {
            IdDataApply IDA0 = GameObject.FindGameObjectWithTag("IDA0").GetComponent<IdDataApply>();
            IdDataApply IDA1 = GameObject.FindGameObjectWithTag("IDA1").GetComponent<IdDataApply>();

            Player0BindingsText.text = IDA0.playerName;
            Player1BindingsText.text = IDA1.playerName;

            Player0BindingsText.color = IDA0.PlayerColor;
            Player1BindingsText.color = IDA1.PlayerColor;

            PairingPanel.GetComponent<Image>().color = new Color(IDA0.PlayerColor.r, IDA0.PlayerColor.g, IDA0.PlayerColor.b, PairingPanel.GetComponent<Image>().color.a);

            CrossLine.SetActive(true);
            PairImage.SetActive(true);
            Player1BindingsText.transform.parent.gameObject.SetActive(true);

            Player0BindingsText.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(650.0938f, 136);
            ResetImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(-137, -356);
        }
        else
        {
            CrossLine.SetActive(false);
            PairImage.SetActive(false);
            Player1BindingsText.transform.parent.gameObject.SetActive(false);

            Player0BindingsText.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(964, 136);
            ResetImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -356);
        }
    }

    private void KeyPressed()
    {

        if ((enter && ButtonNumber.x == 2) | status.buttonName == "CloseBindings" | input.esc)
        {
            enter = false;
            isOpen = false;
            menu.BindingsStat = 3;
            Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
            ButtonNumber = Vector2.zero;
        }

        if ((enter && ButtonNumber.Equals(new Vector2(0, 0))) | status.buttonName == "upB")
        {
            listen = true;
            BName = "upBlue";
            print("upB");
        }
        if (ButtonNumber.Equals(new Vector2(0, 0))) { SetButtonToSelectedBinding(upBImage); }
        else { SetButtonToUnSelectedBinding(upBImage); }


        if ((enter && ButtonNumber.Equals(new Vector2(0, -1))) | status.buttonName == "downB")
        {
            listen = true;
            BName = "downBlue";
            print("downB");
        }
        if (ButtonNumber.Equals(new Vector2(0, -1))) { SetButtonToSelectedBinding(downBImage); }
        else { SetButtonToUnSelectedBinding(downBImage); }


        if ((enter && ButtonNumber.Equals(new Vector2(0, -2))) | status.buttonName == "rightB")
        {
            listen = true;
            BName = "rightBlue";
            print("rightB");
        }
        if (ButtonNumber.Equals(new Vector2(0, -2))) { SetButtonToSelectedBinding(rightBImage); }
        else { SetButtonToUnSelectedBinding(rightBImage); }


        if ((enter && ButtonNumber.Equals(new Vector2(0, -3))) | status.buttonName == "leftB")
        {
            listen = true;
            BName = "leftBlue";
            print("leftB");
        }
        if (ButtonNumber.Equals(new Vector2(0, -3))) { SetButtonToSelectedBinding(leftBImage); }
        else { SetButtonToUnSelectedBinding(leftBImage); }


        if ((enter && ButtonNumber.Equals(new Vector2(1, 0))) | status.buttonName == "upR")
        {
            listen = true;
            BName = "upRed";
            print("upR");
        }
        if (ButtonNumber.Equals(new Vector2(1, 0))) { SetButtonToSelectedBinding(upRImage); }
        else { SetButtonToUnSelectedBinding(upRImage); }


        if ((enter && ButtonNumber.Equals(new Vector2(1, -1))) | status.buttonName == "downR")
        {
            listen = true;
            BName = "downRed";
            print("downR");
        }
        if (ButtonNumber.Equals(new Vector2(1, -1))) { SetButtonToSelectedBinding(downRImage); }
        else { SetButtonToUnSelectedBinding(downRImage); }


        if ((enter && ButtonNumber.Equals(new Vector2(1, -2))) | status.buttonName == "rightR")
        {
            listen = true;
            BName = "rightRed";
            print("rightR");
        }
        if (ButtonNumber.Equals(new Vector2(1, -2))) { SetButtonToSelectedBinding(rightRImage); }
        else { SetButtonToUnSelectedBinding(rightRImage); }


        if ((enter && ButtonNumber.Equals(new Vector2(1, -3))) | status.buttonName == "leftR")
        {
            listen = true;
            BName = "leftRed";
            print("leftR");
        }
        if (ButtonNumber.Equals(new Vector2(1, -3))) { SetButtonToSelectedBinding(leftRImage); }
        else { SetButtonToUnSelectedBinding(leftRImage); }


        if ((enter && ButtonNumber.Equals(new Vector2(0, -4))) | status.buttonName == "enter1")
        {
            listen = true;
            BName = "enter1";
            print("leftR");
        }
        if (ButtonNumber.Equals(new Vector2(0, -4))) { SetButtonToSelectedBinding(enter1Image); }
        else { SetButtonToUnSelectedBinding(enter1Image); }


        if ((enter && ButtonNumber.Equals(new Vector2(1, -4))) | status.buttonName == "enter2")
        {
            listen = true;
            BName = "enter2";
            print("leftR");
        }
        if (ButtonNumber.Equals(new Vector2(1, -4))) { SetButtonToSelectedBinding(enter2Image); }
        else { SetButtonToUnSelectedBinding(enter2Image); }

        if ((enter && ButtonNumber.Equals(new Vector2(0, -5))) | status.buttonName == "esc1")
        {
            listen = true;
            BName = "esc1";
            print("leftR");
        }
        if (ButtonNumber.Equals(new Vector2(0, -5))) { SetButtonToSelectedBinding(esc1Image); }
        else { SetButtonToUnSelectedBinding(esc1Image); }

        if ((enter && ButtonNumber.Equals(new Vector2(1, -5))) | status.buttonName == "esc2")
        {
            listen = true;
            BName = "esc2";
            print("leftR");
        }
        if (ButtonNumber.Equals(new Vector2(1, -5))) { SetButtonToSelectedBinding(esc2Image); }
        else { SetButtonToUnSelectedBinding(esc2Image); }

        if ((enter && ButtonNumber.Equals(new Vector2(0, -6))) | status.buttonName == "ResetBindings")
        {
            input.ResetBindings();
        }

        if ((enter && ButtonNumber.Equals(new Vector2(1, -6))) | (status.buttonName == "PairController" && (buttonNameB != status.buttonName)))
        {
            if (status.inputViewingType == 0)
            {
                if (input.twoGamepads) { isPairing = true; PairButtonImage.sprite = KeyboardMaterial; ResetDescriptionText.SetText("Other Bindings"); count = 1; }
                else { n.Notification(new Color(1, 0.8f, 0, 0), "You need at least 2 controllers for pairing", 3f); }
            }
            if (status.inputViewingType == 2) { status.inputViewingType = 1; PairButtonImage.sprite = KeyboardMaterial; count = 1; }
            if (status.inputViewingType == 1 && count != 1) { status.inputViewingType = 2; PairButtonImage.sprite = GamepadMaterial; }
            count = 0;
        }

        if (status.inputViewingType == 2) { PairButtonImage.sprite = GamepadMaterial; }
        if (status.inputViewingType == 1) { PairButtonImage.sprite = KeyboardMaterial; ResetDescriptionText.SetText("Other Bindings"); }
    }

    private void LateUpdate()
    {
        buttonNameB = status.buttonName;
    }

    private void ChangeSelectedVector()
    {
        if (ButtonNumber != ButtonNumberBefore)
        {
            ChangeSelection.Play();

            SetButton(ResetImage, ResetDescriptionImage, new Vector2(0, -6));
            SetButton(PairImage, PairDescriptionImage, new Vector2(1, -6));

            if (ButtonNumber.x == 2 && ButtonNumberBefore.x != 2) { menu.SetCloseColor(PanelCloseColor, true); }
            if (ButtonNumberBefore.x == 2) { menu.SetCloseColor(PanelCloseColor, false); }
        }
        ButtonNumberBefore = ButtonNumber;


        if (input.up && (input.up != upB) && ButtonNumber.y < 0)
        {
            if (ButtonNumber.x != 2)
            {
                ButtonNumber.y += 1;
            }
            if (bindingsFor1Player && ButtonNumber.y == -3)
            {
                ButtonNumber.x = 0;
            }
        }

        if (input.down && (input.down != downB) && ButtonNumber.y > -6)
        {
            if (ButtonNumber.x != 2)
            {
                ButtonNumber.y -= 1;
                if (ButtonNumber.y == -6)
                {
                    ButtonNumber.x = 0;
                }
            }
        }


        if (input.right && (input.right != rightB) && ButtonNumber.x < 2)
        {
            ButtonNumber.x += 1;
            if (bindingsFor1Player && ButtonNumber.y > -4)
            {
                ButtonNumber.x = 2;
            }
        }

        if (input.left && (input.left != leftB) && ButtonNumber.x > 0)
        {
            ButtonNumber.x -= 1;
            if (bindingsFor1Player && ButtonNumber.y > -4)
            {
                ButtonNumber.x = 0;
            }
        }


        upB = input.up;
        downB = input.down;
        leftB = input.left;
        rightB = input.right;
    }


    public void SetButtonToSelectedBinding(GameObject Image)
    {
        if (!listen)
        {
            Image.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
    }

    public void SetButton(GameObject Image, GameObject DescriptionImage, Vector2 Pos, bool isStartDeselector = false)
    {
        if (ButtonNumber == Pos)
        {
            Image.GetComponent<UniversalButton>().active[1] = true;
            LeanTween.scale(Image.GetComponent<RectTransform>(), Image.GetComponent<UniversalButton>().selectedSize, .25f);
            Image.GetComponent<Image>().color = menu.ButtonSelected;
            DescriptionImage.GetComponent<Image>().color = menu.ButtonDescriptionSelected;
        }
        if (ButtonNumberBefore == Pos | isStartDeselector)
        {
            Image.GetComponent<UniversalButton>().active[1] = false;
            if (Image.GetComponent<UniversalButton>().active.All(active => active == false))
            {
                LeanTween.scale(Image.GetComponent<RectTransform>(), Image.GetComponent<UniversalButton>().startSize, .25f);

            }
            Image.GetComponent<Image>().color = menu.ButtonUnSelected;
            DescriptionImage.GetComponent<Image>().color = menu.ButtonDescriptionUnSelected;
        }
    }

    public void SetButtonToUnSelectedBinding(GameObject Image)
    {
        Image.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1);
    }
}
