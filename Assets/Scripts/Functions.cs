using Mirror;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Functions : MonoBehaviour
{
    [Header("Custom Variables")]
    public Vector3 FireWorkcorner1;
    public Vector3 FireWorkcorner2;
    public UniversalRenderPipelineAsset RPA; //this is optional
    public float shadowDistance; //this is optional
    public bool spawnFireworkAfterScore;
    public bool splitScreen;
    public bool dontRestartAfterScore;
    public bool hideNameText;
    public bool useTeams;
    public string winingCondition;
    public GameObject FireworkPos; //this is optional

    //Variables:
    bool UIisOpen;
    bool scoredAnyScore;
    bool countdownStarted;
    [Header("Variables")]
    public bool countdownFinished;
    public bool scoredPoint;
    public bool scoredScore;
    public bool movementLock;
    public int pointWinner;
    public int scoreWinner;
    float animSpeed = 0.25f;
    float elapsed = 0f;
    float timeLeftSinceUIClose;
    float delayF;
    float timeSincePoint;
    float timeSinceLastInstantiatedScore;
    float bufferTime = 1f;
    int instantiatedScores;
    public int[] scores = new int[4];
    int selectedButton = 0;
    int selectedButtonBefore = 0;
    public int sceneLoadingState = 0;

    InputS input;
    public Pause PauseS;
    public IdDataApply[] IDAs;
    Status status;
    Party party;


    TextMeshProUGUI[] Scores;
    [Header("UI")]

    public GameObject playButton;
    public GameObject playButtonDescription;
    public GameObject MenuButton;
    public GameObject MenuButtonDescription;
    public GameObject WiningConditionGO;
    public GameObject[] PlayerNames;
    public GameObject[] CharacterModels;
    public GameObject[] CharacterRotModels;
    public TMP_Text[] PlayerPoints;
    public GameObject[] PlayerUIs;
    public GameObject[] Players;
    GameObject WaitForHostText;

    GameObject WinPlayerPsGOClone;
    GameObject WinPlayerPsGO;

    public ParticleSystem ScorePs;

    RectTransform[] ScoreAnimators = new RectTransform[2];

    public string buttonName;

    [Header("Audio")]
    public AudioSource[] Music;
    public AudioSource Firework;
    public AudioSource CSound;
    public AudioSource GOSound;
    public AudioSource ChangeSelection;
    public AudioSource Win;

    static bool isPlaying;
    static int currentNumber;
    static float delayMusicScwitch = 5f;
    float volumeDown = 0.5f;

    private void Awake()
    {
        movementLock = true;

        if (GameObject.Find("@DontDestroyOnLoadObjects") == null)
        {
            Object obj = Instantiate(Resources.Load("@DontDestroyOnLoadObjects"));
            obj.name = "@DontDestroyOnLoadObjects";
        }

        Declaration();
    }

    private void Update()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.isLoaded)
        {
            sceneLoadingState = 0;
        }

        if (scene.isLoaded)
        {
            switch (sceneLoadingState)
            {
                case 0:
                    {
                        if (NetworkClient.active)
                        {
                            if (GameObject.Find("NetworkPlayer0").GetComponent<Sync>().allClientsReady)
                            { Setup(); }
                        }
                        else { Setup(); }

                        break;
                    }
                case 1:
                    {
                        UpdateReady();
                        break;
                    }
            }
        }
    }

    void UpdateReady()
    {
        //if (PauseS.isPause && countdownEnded) { Music[currentNumber].Pause(); }
        //if (PauseS.startMusic && countdownEnded) { Music[currentNumber].Play(); PauseS.startMusic = false; }

        if (!PauseS.isPause)
        {
            //Point ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            if (scoredPoint)
            {
                buttonName = status.buttonName;

                PlayFirework();

                FindObjectOfType<Methods>().SetPointsColorOrder(PlayerPoints, true);
                Music[currentNumber].volume = volumeDown * (PlayerPrefs.GetFloat("MusicVolume") * PlayerPrefs.GetFloat("GeneralVolume"));
                movementLock = true;

                input.menu = true;

                if (selectedButton != selectedButtonBefore)
                {
                    ChangeSelection.Play();

                    SetButton(playButtonDescription, playButton, 0);
                    SetButton(MenuButtonDescription, MenuButton, 1);
                }
                selectedButtonBefore = selectedButton;

                if (input.right && selectedButton != 1)
                {
                    selectedButton = 1;
                }

                if (input.left && selectedButton != 0)
                {
                    selectedButton = 0;
                }

                if (status.clientPlayerID < 1 && !status.isParty) { CheckIfButtonPressed(); }

                if (!status.isParty && timeSinceLastInstantiatedScore > 0.5f && timeSincePoint > animSpeed * 3)
                {
                    for (int i = 0; i < status.playerNumber; i++)
                    {
                        if (scores[i] > instantiatedScores)
                        {
                            GameObject go = Instantiate(ScoreAnimators[i].gameObject, ScoreAnimators[i].parent);
                            if (status.playerNumber == 1)
                            {
                                go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60);
                            }
                            go.AddComponent<ScoreAnimation>();
                            go.GetComponent<ScoreAnimation>().ID = i;
                            go.GetComponent<TMP_Text>().color = FindObjectOfType<Methods>().GetPlayerColor(i);
                            go.SetActive(true);
                        }
                    }

                    timeSinceLastInstantiatedScore = 0;
                    instantiatedScores++;
                }

                volumeDown -= Time.deltaTime / 4f;
                timeSinceLastInstantiatedScore += Time.deltaTime;
                timeSincePoint += Time.deltaTime;
            }

            //Score ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            if (scoredScore)
            {
                if (delayF < 0f)
                {
                    if (dontRestartAfterScore)
                    {
                        scoredScore = false;
                    }
                    else
                    {
                        ResetGame();
                    }
                }

                delayF -= Time.deltaTime;
            }

            if (scoredAnyScore)
            {
                for (int i = 0; i < Scores.Length; i++)
                {
                    if (!LeanTween.isTweening(Scores[i].gameObject.GetComponent<RectTransform>()))
                    {
                        LeanTween.scale(Scores[i].gameObject.GetComponent<RectTransform>(), new Vector2(1, 1), .2f).setEaseInOutQuad();
                    }
                }
            }

            if (!countdownStarted)
            {
                Countdown();
            }
            MusicMethod();
        }
        if (PauseS.isPause) { movementLock = true; }

        if (!scoredPoint) { timeLeftSinceUIClose += -Time.deltaTime; }

        if (timeLeftSinceUIClose < 0 && PlayerUIs[0].activeSelf && !PauseS.isPause && !UIisOpen)
        {
            foreach (GameObject go in CharacterModels)
            {
                if (go != null) go.SetActive(false);
            }
            foreach (TMP_Text text in PlayerPoints)
            {
                if (text != null) text.gameObject.SetActive(false);
            }
            foreach (GameObject go in PlayerUIs)
            {
                go.SetActive(false);
            }
        }
        bufferTime += -Time.deltaTime;
    }

    private void CheckIfButtonPressed()
    {
        if ((input.enter && selectedButton == 1) | buttonName == "MenuButton")
        {
            FindObjectOfType<Methods>().LoadMenu();
        }

        if ((input.enter && selectedButton == 0) | buttonName == "PlayButton")
        {
            PlayAgain();
        }
    }

    private void Declaration()
    {
        status = FindObjectOfType<Status>();
        party = FindObjectOfType<Party>();
        input = FindObjectOfType<InputS>();

        Scores = new TextMeshProUGUI[status.playerNumber];
        ScoreAnimators = new RectTransform[status.playerNumber];
        PlayerNames = new GameObject[2];
        CharacterModels = new GameObject[status.playerNumber];
        CharacterRotModels = new GameObject[status.playerNumber];
        PlayerPoints = new TMP_Text[status.playerNumber];
        PlayerUIs = new GameObject[2];
        IDAs = new IdDataApply[status.playerNumber];
        Players = new GameObject[status.playerNumber];

        for (int i = 0; i < status.playerNumber; i++)
        {
            Players[i] = GameObject.Find("%Player" + i);
            CharacterModels[i] = GameObject.Find("Character3D" + i);
            CharacterRotModels[i] = GameObject.Find("RotateCharacter" + i);
            IDAs[i] = Players[i].transform.GetChild(0).GetChild(1).GetComponent<IdDataApply>();

            int original = i;
            if (status.playerNumber == 2 && i == 1) { i = 2; }
            if (status.playerNumber == 2 && i == 0) { i = 1; }
            Scores[original] = GameObject.Find("Score" + i).GetComponent<TextMeshProUGUI>();
            ScoreAnimators[original] = GameObject.Find("Score" + i + "Animation").GetComponent<RectTransform>();

            i = original;
            if (status.playerNumber == 2 && i == 1) { i = 2; }
            PlayerPoints[original] = GameObject.Find("Points" + i).GetComponent<TMP_Text>();
            i = original;
        }
        PlayerUIs[0] = GameObject.Find("PlayerUIL");
        PlayerUIs[1] = GameObject.Find("PlayerUIR");
        PlayerNames[0] = GameObject.Find("Name" + 0);
        PlayerNames[1] = GameObject.Find("Name" + 1);
        WiningConditionGO = GameObject.Find("WinningCondition").transform.GetChild(0).gameObject;
        WaitForHostText = GameObject.Find("WaitForHostText").transform.GetChild(0).gameObject;
    }


    public virtual void Setup()
    {
        scoredPoint = false;
        scoredScore = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;

        foreach (GameObject go in PlayerUIs)
        {
            go.SetActive(false);

            foreach (GameObject obj in PlayerUIs)
            {
                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, obj.GetComponent<RectTransform>().anchoredPosition.y);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            GameObject.Find("Score" + i + "Animation").SetActive(false);
        }

        if (hideNameText)
        {
            foreach (GameObject go in PlayerNames)
            {
                go.GetComponent<TMP_Text>().alpha = 0;
            }
        }

        if (status.isParty)
        {
            foreach (TMP_Text text in PlayerPoints)
            {
                text.gameObject.SetActive(false);
            }
        }

        PlayerUIs[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(-800, PlayerUIs[0].GetComponent<RectTransform>().anchoredPosition.y);
        PlayerUIs[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(800, PlayerUIs[1].GetComponent<RectTransform>().anchoredPosition.y);

        playButton.SetActive(false);
        MenuButton.SetActive(false);
        playButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(-900, playButton.GetComponent<RectTransform>().anchoredPosition.y, 0);
        MenuButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(900, MenuButton.GetComponent<RectTransform>().anchoredPosition.y, 0);
        Scores[0].gameObject.transform.parent.GetComponentInParent<RectTransform>().localScale = new Vector2(1, 1); //Scoreboard

        if (SceneManager.GetActiveScene().name.Contains("Tennis")) { Physics.gravity = new Vector3(0, -100, 0); } //Has this an impact on Particle systems with gravity modifyer
        else { Physics.gravity = new Vector3(0, -9.81f, 0); }

        for (int i = 0; i < Scores.Length; i++)
        {
            Scores[i].text = "0";
            scores[i] = 0;
        }

        WiningConditionGO.GetComponent<TMP_Text>().text = winingCondition;

        for (int i = 0; i < status.playerNumber; i++)  //Disable Crown since its dont destroy
        {
            GameObject Crown = FindObjectOfType<PlayerCountManager>().Players[i].transform.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(2).gameObject;
            Crown.SetActive(false);
        }

        sceneLoadingState = 1;
        status.enteredMenu = true;
    }

    private void Countdown()
    {
        if (NetworkClient.active && !GameObject.Find("NetworkPlayer" + status.clientPlayerID).GetComponent<Sync>())
        {
            status.sync.SetSetupReady(status.clientPlayerID);
            return;
        }

        FindObjectOfType<LoadingScreen>().DisableLoadingScreen();
        FindObjectOfType<Countdown>().StartCountdown();

        //print("CountdownStarted " + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second + ":" + System.DateTime.Now.Millisecond);
        countdownStarted = true;
    }

    public void SetButton(GameObject DescriptionImage, GameObject Image, int selectedButton, bool deselect = false)
    {
        if (selectedButtonBefore == selectedButton | deselect)
        {
            Image.GetComponent<UniversalButton>().active[1] = false;
            if (Image.GetComponent<UniversalButton>().active.All(active => active == false)) { LeanTween.scale(Image.GetComponent<RectTransform>(), new Vector2(1f, 1f), animSpeed); }
            ColorUtility.TryParseHtmlString("#8A8AFF", out Color c);
            ColorUtility.TryParseHtmlString("#797980", out Color c2);
            Image.GetComponent<Image>().color = c;
            DescriptionImage.GetComponent<Image>().color = c2;
        }

        if (selectedButton == this.selectedButton)
        {
            Image.GetComponent<UniversalButton>().active[1] = true;
            LeanTween.scale(Image.GetComponent<RectTransform>(), new Vector2(1.1f, 1.1f), animSpeed);
            ColorUtility.TryParseHtmlString("#c3c3ed", out Color c);
            ColorUtility.TryParseHtmlString("#bcbcbc", out Color c2);
            Image.GetComponent<Image>().color = c;
            DescriptionImage.GetComponent<Image>().color = c2;
        }
    }

    private void MusicMethod()
    {
        for (int i = 0; i < Music.Length; i++)
        {
            AudioSource audio = Music[i];
            if (audio.isPlaying)
            {
                isPlaying = true;
                currentNumber = i;
            }
        }


        if (Music[currentNumber].isPlaying == false) { isPlaying = false; }

        if (!scoredPoint && !isPlaying)
        {
            if (delayMusicScwitch < 0f)
            {
                int i = Random.Range(0, Music.Length);
                Music[i].Play();
                delayMusicScwitch = 5f;
            }
            delayMusicScwitch -= Time.deltaTime;
        }
    }

    public int GetTeammateID(int ID)
    {
        int teammateID = -1;
        if (useTeams)
        {
            if ((status.playerNumber == 3 && ID != 2) | status.playerNumber == 4)
            {
                int[] table = { 1, 0, 3, 2 };
                teammateID = table[ID];
            }
        }

        ID = teammateID;
        return ID;
    }

    public void SetUIScreen(bool isPause = false, bool isClosing = false)
    {
        LeanTween.cancel(Scores[0].gameObject.transform.parent.GetComponent<RectTransform>());
        LeanTween.cancel(GameObject.Find("Background").GetComponent<RectTransform>());
        LeanTween.cancel(PlayerNames[0].GetComponent<RectTransform>());
        LeanTween.cancel(PlayerNames[1].GetComponent<RectTransform>());
        LeanTween.cancel(PlayerUIs[0].GetComponent<RectTransform>());
        LeanTween.cancel(PlayerUIs[1].GetComponent<RectTransform>());
        LeanTween.cancel(WaitForHostText.GetComponent<RectTransform>());

        float speed = 1;

        if (isClosing) // only called by Pause
        {
            speed = animSpeed * 1.5f;

            LeanTween.scale(Scores[0].gameObject.transform.parent.GetComponentInParent<RectTransform>(), new Vector2(1, 1), speed).setEaseInOutQuad().setIgnoreTimeScale(true);
            LeanTween.alpha(GameObject.Find("Background").GetComponent<RectTransform>(), 0f, speed).setIgnoreTimeScale(true).setIgnoreTimeScale(true);
            int x;
            if (status.playerNumber > 2) { x = 180; }
            else { x = 110; }
            LeanTween.move(PlayerNames[0].GetComponent<RectTransform>(), new Vector2(-x, -75), speed).setEaseInOutQuad().setIgnoreTimeScale(true);
            LeanTween.move(PlayerNames[1].GetComponent<RectTransform>(), new Vector2(x, -75), speed).setEaseInOutQuad().setIgnoreTimeScale(true);
            foreach (GameObject go in PlayerNames)
            {
                LeanTween.value(go, go.GetComponent<TMP_Text>().fontSizeMax, 100, speed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
                {
                    go.GetComponent<TMP_Text>().fontSizeMax = val;
                });
            }
            LeanTween.moveX(PlayerUIs[0].GetComponent<RectTransform>(), -1000, speed).setEaseInOutQuad().setIgnoreTimeScale(true);
            LeanTween.moveX(PlayerUIs[1].GetComponent<RectTransform>(), 1000, speed).setEaseInOutQuad().setIgnoreTimeScale(true);

            if (NetworkClient.active && status.clientPlayerID != 0)
            {
                LeanTween.moveY(WaitForHostText.GetComponent<RectTransform>(), -100, speed).setEaseInOutQuad().setIgnoreTimeScale(true);
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            timeLeftSinceUIClose = speed;
            UIisOpen = false;
        }
        else
        {
            if (isPause)
            {
                speed = animSpeed * 1.5f;

                if (NetworkClient.active && status.clientPlayerID != 0)
                {
                    WaitForHostText.GetComponent<TMP_Text>().text = "Host paused the game";
                    LeanTween.moveY(WaitForHostText.GetComponent<RectTransform>(), 100, speed).setEaseInOutQuad().setIgnoreTimeScale(true); ;
                }
            }
            else
            {
                speed = animSpeed * 3f;

                if (!status.isParty)
                {
                    if (status.clientPlayerID > 0)
                    {
                        WaitForHostText.GetComponent<TMP_Text>().text = "Wait for the host";
                        LeanTween.moveY(WaitForHostText.GetComponent<RectTransform>(), 100, speed).setEaseInOutQuad().setIgnoreTimeScale(true);
                    }
                    if (status.clientPlayerID < 1)
                    {
                        LeanTween.moveX(playButton.GetComponent<RectTransform>(), 0, speed).setEaseInOutQuad();
                        LeanTween.moveX(MenuButton.GetComponent<RectTransform>(), 0, speed).setEaseInOutQuad();

                        playButton.SetActive(true);
                        MenuButton.SetActive(true);
                    }
                }

                Win.PlayDelayed(0.1f);
            }

            FindObjectOfType<Methods>().SetPointsColorOrder(PlayerPoints);

            foreach (GameObject go in CharacterModels)
            {
                if (go != null) go.SetActive(true);
            }
            if (!status.isParty)
            {
                foreach (TMP_Text text in PlayerPoints)
                {
                    if (text != null) text.gameObject.SetActive(true);
                }
            }

            LeanTween.cancel(Scores[0].gameObject.transform.parent.GetComponent<RectTransform>());
            LeanTween.scale(Scores[0].gameObject.transform.parent.GetComponentInParent<RectTransform>(), new Vector2(1.5f, 1.5f), speed).setEaseInOutQuad().setIgnoreTimeScale(true);  //scoreboard
            LeanTween.cancel(GameObject.Find("Background").GetComponent<RectTransform>());
            LeanTween.alpha(GameObject.Find("Background").GetComponent<RectTransform>(), 0.7f, speed).setIgnoreTimeScale(true);
            foreach (GameObject go in PlayerUIs)
            {
                if (status.playerNumber == 1)
                {
                    go.GetComponent<RectTransform>().anchorMin = new Vector2(.5f, go.GetComponent<RectTransform>().anchorMin.y);
                    go.GetComponent<RectTransform>().anchorMax = new Vector2(.5f, go.GetComponent<RectTransform>().anchorMax.y);
                }
                LeanTween.moveX(go.GetComponent<RectTransform>(), 0, speed).setIgnoreTimeScale(true).setEaseInOutQuad();
            }

            foreach (GameObject go in PlayerUIs)
            {
                go.SetActive(true);
            }
            foreach (GameObject go in PlayerNames)
            {
                if (status.playerNumber == 1)
                {
                    if (go.name == "Name1") { continue; }
                    float x = Screen.width / 2;

                    LeanTween.value(go, go.GetComponent<RectTransform>().position.x, x, speed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
                    {
                        go.GetComponent<RectTransform>().position = new Vector2(val, go.GetComponent<RectTransform>().position.y);
                    });
                    LeanTween.moveY(go.GetComponent<RectTransform>(), -260, speed).setEaseInOutQuad().setIgnoreTimeScale(true);
                }
                else
                {
                    LeanTween.move(go.GetComponent<RectTransform>(), new Vector2(0, -260), speed).setEaseInOutQuad().setIgnoreTimeScale(true);
                }

                LeanTween.value(go, go.GetComponent<TMP_Text>().fontSizeMax, 75, speed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
                {
                    go.GetComponent<TMP_Text>().fontSizeMax = val;
                });
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SetButton(playButtonDescription, playButton, 0, true);
            SetButton(MenuButtonDescription, MenuButton, 1, true);

            UIisOpen = true;
        }
    }

    public void OnCoundownFinished()
    {
        movementLock = false;
        countdownFinished = true;

        if (status.isParty && status.clientPlayerID < 1)
        {
            for (int i = 0; i < status.playerNumber; i++)
            {
                if (party.startWithPowerUp[i])
                {
                    if (NetworkClient.active) { status.sync.SetPowerUpPlayerServer(i, -1); }
                    else
                    {
                        Players[i].GetComponent<Movement>().OnPowerUp();
                    }
                    party.startWithPowerUp[i] = false;
                }
            }
        }
    }
    private void PlayFirework()
    {
        WinPlayerPsGO = GameObject.Find("Particle System Win");

        if (Firework.isPlaying == false) { Firework.Play(); }

        if (elapsed > 0.3f)
        {
            int id;
            Vector3 randPos = new(Random.Range(FireWorkcorner1.x, FireWorkcorner2.x), Random.Range(FireWorkcorner1.y, FireWorkcorner2.y), Random.Range(FireWorkcorner1.z, FireWorkcorner2.z));
            WinPlayerPsGOClone = Instantiate(WinPlayerPsGO, randPos, Quaternion.identity);
            if (GetTeammateID(pointWinner) != -1)
            {
                if (Random.Range(0, 2) == 0)
                {
                    id = GetTeammateID(pointWinner);
                }
                else { id = pointWinner; }
            }
            else
            {
                id = pointWinner;
            }
            FindObjectOfType<Methods>().SetPSColorFromPlayerColor(WinPlayerPsGOClone.GetComponent<ParticleSystem>(), id, true);
            elapsed = 0;
        }
        elapsed += Time.deltaTime;
    }

    public virtual void AddScore(float delay, int maxScores, int ID, bool isCalledByServer = false, bool isTeammate = false, int scoreCount = 1)
    {
        if (!isTeammate)
        {
            if (bufferTime > 0f) { return; }
            if (NetworkClient.active && status.clientPlayerID != 0 && !isCalledByServer) { Debug.LogError("Score can only be added by Server; Use Sync.AddScoreOnServer"); return; }

            if (NetworkClient.active && !isCalledByServer) { status.sync.AddScore(delay, maxScores, ID, true); }
        }
        //1. player scores score; 2. everybody regocnice it; 3. server sends all other clients addscore command; 4.if (useTeams == true) then call addscore on local brain again but this time without useless stuff for a "second" goal

        if ((!scoredPoint && !scoredScore) | isTeammate)
        {
            scores[ID] += 1;

            for (int i = 0; i < Scores.Length; i++)
            {
                Scores[i].text = scores[i].ToString();
            }

            if (!isTeammate)
            {
                scoreWinner = ID;

                if (spawnFireworkAfterScore)
                {
                    ScorePs = GameObject.Find("Particle System Score").GetComponent<ParticleSystem>();
                    if (FireworkPos != null) { ScorePs.gameObject.transform.position = FireworkPos.transform.position; }

                    FindObjectOfType<Methods>().SetPSColorFromPlayerColor(ScorePs, ID);
                }
            }

            if (maxScores == scores[ID])
            {
                if (!isTeammate)
                {
                    SetUIScreen();

                    print(ID + " won the game!" + scores[ID]);
                    GameObject Crown = FindObjectOfType<PlayerCountManager>().Players[ID].transform.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(2).gameObject;
                    Crown.SetActive(true);

                    scoredPoint = true;
                    pointWinner = ID;
                }

                if (!status.isParty)
                {
                    for (int i = 0; i < status.playerNumber; i++)
                    {
                        if (NetworkClient.active)
                        {
                            status.points[i] += scores[i];
                        }
                        if (status.playerNumber == 1)
                        {
                            PlayerPrefs.SetInt("SinglePlayerPoint" + i, PlayerPrefs.GetInt("SinglePlayerPoint" + i) + scores[i]);
                        }
                        else if (!NetworkClient.active)
                        {
                            PlayerPrefs.SetInt("Point" + i, PlayerPrefs.GetInt("Point" + i) + scores[i]);
                        }
                    }
                }
                else
                {
                    FindObjectOfType<Party>().InvokeLoadPartyScene(scores);
                }
            }
            else
            {
                scoredScore = true;
                scoredAnyScore = true;
                delayF = delay;

                if (!scoredPoint)
                {
                    LeanTween.scale(Scores[ID].gameObject.GetComponent<RectTransform>(), Scores[ID].gameObject.GetComponent<RectTransform>().localScale * 1.2f, 0.1f).setEaseInOutQuad();
                }
            }

            if (!isTeammate && GetTeammateID(ID) != -1)
            {
                //print("Add Score for team mate with id: " + GetTeammateID(ID));
                AddScore(delay, maxScores, GetTeammateID(ID), false, true);
            }
            Debug.Log("Player " + ID + " scored | New Score of this Player is: " + scores[ID] + "| max score are: " + maxScores);

            if (scoreCount > 1)
            {
                for (int i = 1; i < scoreCount; i++)
                {
                    AddScore(delay, maxScores, ID);
                }
            }
        }
    }

    private void ResetGame()
    {
        foreach (Movement move in FindObjectsOfType<Movement>())
        {
            move.OnReset();
        }

        scoredScore = false;
    }

    public virtual void PlayAgain()
    {
        FindObjectOfType<LoadingScreen>().SetLoadingScreen(status.gameIndex);

        Scene scene = SceneManager.GetActiveScene();
        Time.timeScale = 1;
        if (NetworkClient.active)
        {
            FindObjectOfType<Methods>().DeleteAllNetworkPlayers();
            Sync sync = GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>();
            sync.LoadScene(scene.buildIndex, true, status.gameIndex);
        }
        else { SceneManager.LoadScene(scene.buildIndex); }
    }
}