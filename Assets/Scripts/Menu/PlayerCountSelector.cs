using Mirror;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCountSelector : MonoBehaviour
{
    [Header("HostServerL")]
    public GameObject HostServerDescriptionImageL;
    public GameObject HostServerImageL;

    [Header("JoinServerL")]
    public GameObject JoinServerDescriptionImageL;
    public GameObject JoinServerImageL;

    [Header("HostServer")]
    public GameObject HostServerDescriptionImage;
    public GameObject HostServerImage;

    [Header("JoinServer")]
    public GameObject JoinServerDescriptionImage;
    public GameObject JoinServerImage;

    [Header("TwoPlayers")]
    public TMP_Text TwoPlayersText;

    [Header("OnePlayers")]
    public TMP_Text OnePlayersText;

    public GameObject Panel;
    public CanvasGroup StartPanel;
    public GameObject RetryHint;
    public GameObject Menu;
    public GameObject Games;

    public TMP_Text MenuText;
    string currentText;
    string waitingDots;
    string ip;

    public int connectionStat;
    public Vector2 buttonNumber;
    Vector2 buttonNumberB;
    public AudioSource ChangeSelection;
    public Color targetTextColor;
    public float animSpeed = 0.2f;
    InputS InputScript;
    Status status;
    public Mirror.Discovery.NetworkDiscovery networkDiscovery;

    bool rightB;
    bool leftB;
    bool downB;
    bool upB;

    private void Awake()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        status = GameObject.FindGameObjectWithTag("Status").GetComponent<Status>();

        RetryHint.SetActive(false);
        MenuText.gameObject.SetActive(false);
        Menu.GetComponent<CanvasGroup>().alpha = 0;

        if (!status.enteredMenu)
        {
            gameObject.GetComponent<Menu>().enabled = false;
            GetComponent<Menu>().MenuPanel.SetActive(false);
        }
        else
        {
            SkipPlayerCountSelection();
        }

        Invoke("searchForMessage", .5f);
    }

    public void SkipPlayerCountSelection()
    {
        GetComponent<Menu>().MenuPanel.GetComponent<CanvasGroup>().alpha = 1;
        Menu.SetActive(true);
        Panel.SetActive(false);
        gameObject.GetComponent<Menu>().PlayerCountSelectorStat = 0;
        connectionStat = -1;

        if (NetworkClient.active && NetworkManager.singleton.numPlayers < 2) { connectionStat = 1; }
    }

    // Start is called before the first frame update
    private void Start()
    {
        InputScript = FindObjectOfType<InputS>();

        SetButton(HostServerDescriptionImageL, HostServerImageL, new Vector2(1, -1), true);
        SetButton(JoinServerDescriptionImageL, JoinServerImageL, new Vector2(0, -1), true);
        SetButton(JoinServerDescriptionImage, JoinServerImage, new Vector2(2, -1), true);
        SetButton(HostServerDescriptionImage, HostServerImage, new Vector2(3, -1), true);
        SetButton(HostServerDescriptionImage, TwoPlayersText.gameObject.transform.parent.gameObject, new Vector2(1, 0), false, TwoPlayersText, true);
        SetButton(HostServerDescriptionImage, OnePlayersText.gameObject.transform.parent.gameObject, Vector2.zero, false, OnePlayersText, true);
    }

    // Update is called once per frame
    void Update()
    {
        ChangeSelectedVector();
        CheckConnectionStat();

        if (connectionStat == 0)
        {
            if ((InputScript.enter && buttonNumber.Equals(new Vector2(1, 0))) | status.buttonName == "2Players")
            {
                connectionStat = -1;
                Menu.SetActive(true);
                GetComponent<Menu>().enabled = true;
                GetComponent<Menu>().PlayerCountSelectorStat = 3;
            }

            if ((InputScript.enter && buttonNumber.Equals(Vector2.zero)) | status.buttonName == "1Players")
            {
                status.playerNumber = 1;
                connectionStat = -1;
                Menu.SetActive(true);
                GetComponent<Menu>().enabled = true;
                GetComponent<Menu>().PlayerCountSelectorStat = 3;
            }

            if ((InputScript.enter && buttonNumber.Equals(new Vector2(1, -1))) | status.buttonName == "HostServerL")
            {
                ip = GetLocalIPAddress();
                if (!NetworkClient.active) { NetworkManager.singleton.StartHost(); NetworkManager.singleton.networkAddress = ip; }
                networkDiscovery.AdvertiseServer();

                GetComponent<Menu>().PlayerCountSelectorStat = 3;
                connectionStat = 1;
            }

            if ((InputScript.enter && buttonNumber.Equals(new Vector2(0, -1))) | status.buttonName == "JoinServerL")
            {
                print("start client");

                status.enteredMenu = true;

                networkDiscovery.StartDiscovery();

                LeanTween.value(StartPanel.gameObject, 1, 0, 0.2f).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
                {
                    StartPanel.alpha = val;
                });
                currentText = MenuText.text;
                currentText = currentText.Insert(MenuText.text.IndexOf("server") + 6, "dots");

                connectionStat = 1;
            }
        }
    }

    public void SetButton(GameObject DescriptionImage, GameObject Image, Vector2 selectedButton, bool deselect = false, TMP_Text localText = null, bool doNotDeselect = false)
    {
        if (selectedButton.Equals(buttonNumber))
        {
            Image.GetComponent<UniversalButton>().active[1] = true;
            if (localText != null)
            {
                LeanTween.scale(localText.transform.parent.GetComponent<RectTransform>(), new Vector2(2.6f, 2.6f), animSpeed).setIgnoreTimeScale(true);

                LeanTween.value(gameObject, localText.color, targetTextColor, 0.2f).setIgnoreTimeScale(true).setOnUpdate((Color c) =>
                {
                    localText.color = c;
                });
            }
            if (localText == null)
            {
                LeanTween.scale(Image.GetComponent<RectTransform>(), new Vector2(1.7f * 1.1f, 1.7f * 1.1f), animSpeed).setIgnoreTimeScale(true);
                ColorUtility.TryParseHtmlString("#c3c3ed", out Color c);
                ColorUtility.TryParseHtmlString("#bcbcbc", out Color c2);
                Image.GetComponent<Image>().color = c;
                DescriptionImage.GetComponent<Image>().color = c2;
            }
        }
        if (selectedButton.Equals(buttonNumberB) | deselect && !doNotDeselect)
        {
            Image.GetComponent<UniversalButton>().active[1] = false;
            if (localText != null)
            {
                if (Image.GetComponent<UniversalButton>().active.All(active => active == false) | deselect)
                {
                    LeanTween.scale(localText.transform.parent.GetComponent<RectTransform>(), new Vector2(2.5f, 2.5f), animSpeed).setIgnoreTimeScale(true);

                    LeanTween.value(gameObject, localText.color, Color.white, 0.2f).setIgnoreTimeScale(true).setOnUpdate((Color c) =>
                    {
                        localText.color = c;
                    });
                }
            }
            if (localText == null)
            {
                if (Image.GetComponent<UniversalButton>().active.All(active => active == false) | deselect) { LeanTween.scale(Image.GetComponent<RectTransform>(), new Vector2(1.7f, 1.7f), animSpeed).setIgnoreTimeScale(true); }

                ColorUtility.TryParseHtmlString("#8A8AFF", out Color c);
                ColorUtility.TryParseHtmlString("#797980", out Color c2);
                Image.GetComponent<Image>().color = c;
                DescriptionImage.GetComponent<Image>().color = c2;
            }
        }
    }

    public static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    private void CheckConnectionStat()
    {
        if (connectionStat == 1)
        {
            MenuText.gameObject.SetActive(true);
            Games.SetActive(false);
            GetComponent<Menu>().ButtonNumber = new Vector2(0, 1);
            GetComponent<Menu>().networkWait = true;
            GetComponent<Menu>().enabled = true;
            GetComponent<Menu>().PlayerCountSelectorStat = 3;
            status.playerNumber = 1;

            if (NetworkServer.active)
            {
                connectionStat = 3;
            }
            else
            {
                Invoke("SetRetryHint", 10);
                connectionStat = 2;
            }
        }

        if (connectionStat == 2)
        {
            if (NetworkClient.isConnected)
            {
                LeanTween.cancel(RetryHint.transform.GetChild(1).GetComponent<RectTransform>());
                CancelInvoke("SetRetryHint");
                RetryHint.SetActive(false);
                connectionStat = 3;
            }

            if (!LeanTween.isTweening(MenuText.gameObject))
            {
                LeanTween.value(MenuText.gameObject, -.5f, 3.5f, 3f).setIgnoreTimeScale(true).setOnUpdate((float val) =>
                {
                    if (connectionStat != 2) { return; }
                    int dotsCount = Mathf.RoundToInt(val);
                    if (dotsCount == 0) { waitingDots = ""; }
                    if (dotsCount == 1) { waitingDots = "."; }
                    if (dotsCount == 2) { waitingDots = ".."; }
                    if (dotsCount == 3) { waitingDots = "..."; }
                    MenuText.text = currentText.Replace("dots", waitingDots);
                });
            }
        }

        if (connectionStat == 3)
        {
            string text = "";
            if (status.clientPlayerID == 0)
            {
                text = "<color=#fff>Waiting for Players<br>Players: </color>" + status.playerNumber;
            }
            else
            {
                text = "<color=#fff>Waiting for Host<br>Players: </color>" + status.playerNumber;
            }
            MenuText.text = text;
        }

        if (connectionStat == 4)
        {
            Games.SetActive(true);
            MenuText.gameObject.SetActive(false);
            if (GetComponent<Menu>() != null)
            {
                GetComponent<Menu>().networkWait = false;
                GetComponent<Menu>().PartyImage.SetActive(true);
            }
        }
    }

    public void OnDiscoverdServer(Mirror.Discovery.ServerResponse response)
    {
        print("connecting to server...");
        networkDiscovery.StopDiscovery();
        NetworkManager.singleton.StartClient(response.uri);
    }

    private void ChangeSelectedVector()
    {
        if (InputScript.right && !rightB && buttonNumber.x < 3)
        {
            if (buttonNumber.y == 0 && buttonNumber.x < 1) { buttonNumber.x += 1; }
            else if (buttonNumber.y == -1) { buttonNumber.x += 1; }
        }

        if (InputScript.left && !leftB && buttonNumber.x > 0)
        {
            buttonNumber.x += -1;
        }

        if (InputScript.down && !downB && buttonNumber.y > -1)
        {
            if (buttonNumber.x == 1) { buttonNumber.x = 2; }
            buttonNumber.y += -1;
        }

        if (InputScript.up && !upB && buttonNumber.y < 0)
        {
            if (buttonNumber.x > 1) { buttonNumber.x = 1; }
            else { buttonNumber.x = 0; }
            buttonNumber.y += 1;
        }

        if (!buttonNumber.Equals(buttonNumberB))
        {
            ChangeSelection.Play();

            SetButton(HostServerDescriptionImageL, HostServerImageL, new Vector2(1, -1));
            SetButton(JoinServerDescriptionImageL, JoinServerImageL, new Vector2(0, -1));
            SetButton(JoinServerDescriptionImage, JoinServerImage, new Vector2(2, -1));
            SetButton(HostServerDescriptionImage, HostServerImage, new Vector2(3, -1));
            SetButton(HostServerDescriptionImage, TwoPlayersText.gameObject.transform.parent.gameObject, new Vector2(1, 0), false, TwoPlayersText);
            SetButton(HostServerDescriptionImage, OnePlayersText.gameObject.transform.parent.gameObject, Vector2.zero, false, OnePlayersText);
        }

        buttonNumberB = buttonNumber;
        rightB = InputScript.right;
        leftB = InputScript.left;
        downB = InputScript.down;
        upB = InputScript.up;
    }

    public void SetRetryHint()
    {
        RetryHint.SetActive(true);
        LeanTween.moveY(RetryHint.transform.GetChild(1).GetComponent<RectTransform>(), 300, .5f).setLoopPingPong().setEaseInOutQuad();
    }

    private void searchForMessage()
    {
        try
        {
            GameObject go = GameObject.FindGameObjectWithTag("SelfDisconnect");
            FindObjectOfType<Notificaton>().Notification(Color.red, go.name + ", you were disconnected", 4f);
            Destroy(go);
        }
        catch { }
    }
}
