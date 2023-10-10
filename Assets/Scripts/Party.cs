using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Party : MonoBehaviour
{
    Status status;
    Methods methods;
    PartyCamera cam;

    public bool isParty;
    public bool[] selectedGames = new bool[100];
    public bool[] doublePointsPlayers = new bool[4];
    public bool[] startWithPowerUp = new bool[4];
    public int fieldCount = 15;
    public bool specialFields = true;
    public int[] scores = new int[4];
    public int[] targetFields = new int[4];
    public int[] previousSessionFields;

    public bool doneSetup;
    bool spawnedMenuButton;
    bool spawnedFields;
    bool gameSceneInvoked;
    public bool[] clientsSetUp;
    public int currentPlayerMoving;

    public MovementParty[] PlayersMovements;

    GameObject MenuButtonCube;
    bool menuButtonSelected;

    public GameObject Field;
    public GameObject[] Fields;
    public GameObject[] Pillars = new GameObject[4];
    public UI ui;

    private void Start()
    {
        status = FindObjectOfType<Status>();
        methods = FindObjectOfType<Methods>();
    }

    private void OnLevelWasLoaded(int level)
    {
        if (FindObjectOfType<Methods>().IsSceneParty())
        {
            doneSetup = false;
            gameSceneInvoked = false;
            spawnedFields = false;
            currentPlayerMoving = 0;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Cursor.SetCursor(Resources.Load<Texture2D>("cursorNormal"), Vector2.zero, CursorMode.Auto);
        }
    }

    private void SpawnFields()
    {
        if ((NetworkServer.active && status.sync.allClientsReady) || !NetworkClient.active)
        {
            Fields = new GameObject[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                GameObject go = Instantiate(Field, new Vector3(0, 1.65f, -(fieldCount - i) * 9), Quaternion.Euler(270, 90, 0));
                go.name = "Field" + i;
                if (specialFields)
                {
                    go.GetComponent<Field>().AssignFieldType();
                }

                if (NetworkServer.active)
                {
                    NetworkServer.Spawn(go);
                }

                Fields[i] = go;
            }
            spawnedFields = true;
        }
    }

    private void Update()
    {
        if (FindObjectOfType<Methods>().IsSceneParty())
        {
            if (!spawnedFields)
            {
                SpawnFields();
            }
            if (!doneSetup)
            {
                Setup();
            }
            else if (!NetworkClient.active || NetworkServer.active)
            {
                bool isNotClient = !NetworkClient.active || NetworkServer.active;
                bool isNotAbovePlayerCount = currentPlayerMoving < status.playerNumber;
                bool camFinishedStartPan = cam.currentState != PartyCamera.State.startOverview && cam.currentState != PartyCamera.State.startCamPan;

                if (isNotClient && isNotAbovePlayerCount && camFinishedStartPan)
                {
                    if (PlayersMovements[currentPlayerMoving].state == MovementParty.State.idle) // start of turn
                    {
                        if (NetworkServer.active)
                        {
                            status.sync.syncUIParty(0, currentPlayerMoving, null, null, -1, -1, false, false);
                        }
                        StartOfTurnAnimations(currentPlayerMoving);
                    }

                    if (PlayersMovements[currentPlayerMoving].state == MovementParty.State.finishedMove || PlayersMovements[currentPlayerMoving].state == MovementParty.State.winFreeze) // end of turn
                    {
                        EndOfTurn();
                    }
                }
            }
            if (spawnedMenuButton)
            {
                MenuButtonCheckPointer();
            }
        }
    }

    public void StartOfTurnAnimations(int currentPlayer)
    {
        int index = currentPlayer;
        LeanTween.value(ui.Names[index].gameObject, Color.white, methods.GetPlayerColor(index), .2f).setEaseInOutQuad().setOnUpdate((Color c) =>
        {
            ui.Names[index].color = c;
        });
        LeanTween.scale(ui.Names[currentPlayer].GetComponent<RectTransform>(), new Vector2(1.1f, 1.1f), 0.2f);

        PlayersMovements[currentPlayer].state = MovementParty.State.walk;
    }

    private void EndOfTurn()
    {
        if (NetworkServer.active)
        {
            status.sync.syncUIParty(1, currentPlayerMoving, null, null, -1, -1, false, false);
        }
        EndOfTurnAnimations(currentPlayerMoving);

        if (PlayersMovements[currentPlayerMoving].state == MovementParty.State.winFreeze)//at least one player has won
        {
            foreach (MovementParty mp in PlayersMovements)
            {
                mp.moveTilPillar = true;
            }
        }

        currentPlayerMoving++;
        if (currentPlayerMoving >= status.playerNumber && PlayersMovements.All(x => x.state == MovementParty.State.finishedMove))
        {
            for (int i = 0; i < status.playerNumber; i++)
            {
                previousSessionFields[i] = PlayersMovements[i].targetField;
            }
            InvokeLoadRandomGame();
        }
    }

    public void EndOfTurnAnimations(int currentPlayer)
    {
        LeanTween.cancel(ui.Names[currentPlayer].gameObject);
        int index = currentPlayer;
        LeanTween.value(ui.Names[index].gameObject, ui.Names[index].color, Color.white, .2f).setEaseInOutQuad().setOnUpdate((Color c) =>
        {
            ui.Names[index].color = c;
        });
        LeanTween.scale(ui.Names[currentPlayer].GetComponent<RectTransform>(), new Vector2(.9f, .9f), 0.2f);
    }

    private void Setup()
    {
        cam = FindObjectOfType<PartyCamera>();

        if ((NetworkClient.active && GameObject.Find("NetworkPlayer0").GetComponent<Sync>().allClientsReady) || !NetworkClient.active)
        {
            if (NetworkServer.active && !AllClientsSetUp()) { return; }

            GetPlayer();
            GetPillar();
            GetSetUI();

            MenuButtonCube = GameObject.Find("MenuButtonCube");

            if(status.playerNumber > 2)
            {
                GameObject.Find("WoodenBridge").transform.LeanScaleX(3, 0);
            }


            FindObjectOfType<LoadingScreen>().DisableLoadingScreen();
            doneSetup = true;
        }
    }

    bool AllClientsSetUp()
    {
        if (clientsSetUp.Length == 0) { clientsSetUp = new bool[status.playerNumber]; }

        status.sync.CheckIfPartyIsSetUp();

        return clientsSetUp.All(active => active);
    }

    private void GetSetUI()
    {
        ui = new UI
        {
            PlayerUIs = new GameObject[4],
            Names = new TMP_Text[status.playerNumber],
            Fields = new TMP_Text[status.playerNumber],
            FieldAnimators = new GameObject[status.playerNumber],
            PowerUps = new GameObject[status.playerNumber]
        };

        for (int i = 0; i < 4; i++)
        {
            ui.PlayerUIs[i] = GameObject.Find("PlayerUICanvas").transform.GetChild(0).GetChild(i).gameObject;
            if (i >= status.playerNumber)
            {
                ui.PlayerUIs[i].SetActive(false);
            }
        }

        for (int i = 0; i < status.playerNumber; i++)
        {
            ui.Names[i] = ui.PlayerUIs[i].transform.GetChild(0).GetComponent<TMP_Text>();
            ui.Names[i].text = methods.GetPlayerName(i);

            ui.Fields[i] = ui.PlayerUIs[i].transform.GetChild(1).transform.GetChild(0).GetComponent<TMP_Text>();
            targetFields[i] = scores[i] + PlayersMovements[i].GetCurrentField();

            ui.FieldAnimators[i] = ui.PlayerUIs[i].transform.GetChild(2).gameObject;

            ui.PowerUps[i] = ui.PlayerUIs[i].transform.GetChild(1).transform.GetChild(1).gameObject;
        }

        if (NetworkServer.active) //server server
        {
            status.sync.syncUIParty(2, 0, targetFields, previousSessionFields, -1, -1, false, false);
        }
        if (NetworkServer.active || !NetworkClient.active) { SetFieldUI(targetFields, previousSessionFields); } //server or no lan
    }

    public void SetFieldUI(int[] fields, int[] oldFields)
    {
        for (int i = 0; i < status.playerNumber; i++)
        {
            ui.Fields[i].text = fields[i].ToString();
            ui.Fields[i].color = methods.GetPointsOrder(fields)[i];
        }

        if (oldFields != null) // is not dice roll sondern start of turn
        {
            for (int i = 0; i < status.playerNumber; i++)
            {
                StartCoroutine(FieldAnimation(i, fields[i] - oldFields[i]));
            }
        }
    }

    IEnumerator FieldAnimation(int id, int newFields)
    {
        for (int i = 0; i < newFields; i++)
        {
            GameObject go = Instantiate(ui.FieldAnimators[id]);
            go.transform.SetParent(ui.FieldAnimators[id].transform.parent);
            go.AddComponent<ScoreAnimation>();
            go.GetComponent<ScoreAnimation>().ID = id;
            go.GetComponent<ScoreAnimation>().isParty = true;
            go.GetComponent<TMP_Text>().color = methods.GetPlayerColor(id);
            go.GetComponent<RectTransform>().anchoredPosition = ui.FieldAnimators[id].GetComponent<RectTransform>().anchoredPosition;
            go.SetActive(true);

            yield return new WaitForSeconds(.25f);
        }
    }

    private void GetPlayer()
    {
        PlayersMovements = new MovementParty[status.playerNumber];
        for (int i = 0; i < status.playerNumber; i++)
        {
            GameObject Player = FindObjectOfType<Methods>().FindInactiveObjectByName("%Player" + i);
            PlayersMovements[i] = Player.GetComponent<MovementParty>();
        }
    }

    public void SetPillar()
    {
        GetPlayer();
        GetPillar();

        for (int i = 0; i < status.playerNumber; i++)
        {
            Vector3 pos = Pillars[i].transform.position;
            pos.x = PlayersMovements[i].transform.position.x;
            Pillars[i].transform.position = pos;
            print("Pillar" + i + " was moved to " + pos);
        }
    }

    private void GetPillar()
    {
        for (int i = 0; i < 4; i++)
        {
            Pillars[i] = FindObjectOfType<Methods>().FindInactiveObjectByName("Pillar" + i);
            if (i >= status.playerNumber)
            {
                Destroy(Pillars[i]);
            }
        }
    }

    public void InvokeSpawnMenuButton()
    {
        Invoke(nameof(SpawnMenuButton), 7f);
    }

    private void SpawnMenuButton()
    {
        GameObject MenuButton = GameObject.Find("MenuButton");
        ParticleSystem PS = GameObject.Find("MenuButtonPSDust").GetComponent<ParticleSystem>();

        PS.Play();
        LeanTween.moveLocalY(MenuButton, 1.45f, 1.2f).setEaseOutElastic();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        spawnedMenuButton = true;
    }

    void MenuButtonCheckPointer()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out hit) && hit.transform.name == "MenuButtonCube")
        {
            if (!menuButtonSelected)
            {
                SetMenuButtonText(new Color(1, 0.6431373f, 0), -0.45f);
                Cursor.SetCursor(Resources.Load<Texture2D>("cursorClick"), Vector2.zero, CursorMode.Auto);
                menuButtonSelected = true;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                CancelParty();
            }
        }
        else
        {
            if (menuButtonSelected)
            {
                SetMenuButtonText(new Color(.95f, .95f, .95f), -.35f);
                Cursor.SetCursor(Resources.Load<Texture2D>("cursorNormal"), Vector2.zero, CursorMode.Auto);
            }
            menuButtonSelected = false;
        }

        if (FindObjectOfType<InputS>().enter)
        {
            CancelParty();
        }
    }

    private void SetMenuButtonText(Color col, float y)
    {
        GameObject MenuButtonText = MenuButtonCube.transform.parent.GetChild(1).gameObject;

        LeanTween.value(MenuButtonText, MenuButtonText.GetComponent<MeshRenderer>().material.color, col, 0.25f).setEaseOutCubic().setOnUpdate((Color color) =>
        {
            MenuButtonText.GetComponent<MeshRenderer>().material.color = color;
        });

        LeanTween.value(MenuButtonText, MenuButtonText.transform.localPosition.y, y, 0.25f).setEaseOutCubic().setOnUpdate((float y) =>
        {
            Vector3 newPos = MenuButtonText.transform.localPosition;
            newPos.y = y;
            MenuButtonText.transform.localPosition = newPos;
        });
    }

    public void InvokeLoadRandomGame()
    {
        if (status.clientPlayerID > 0 || gameSceneInvoked) { return; }
        Invoke(nameof(LoadRandomGame), 2f);
        gameSceneInvoked = true;
    }

    public void LoadRandomGame()
    {
        int index = -1;
        while (index == -1)
        {
            index = UnityEngine.Random.Range(0, 100);
            if (!selectedGames[index])
            {
                index = -1;
            }
        }

        status.gameIndex = index;
        FindObjectOfType<LoadingScreen>().SetLoadingScreen(index);

        if (NetworkClient.active)
        {
            GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().LoadScene(index + 2, true, index);
        }
        else { SceneManager.LoadSceneAsync(index + 2); }
    }

    public void InvokeLoadPartyScene(int[] scores)
    {
        if (NetworkClient.active && !NetworkServer.active) { return; }
        this.scores = scores;
        for (int i = 0; i < status.playerNumber; i++)
        {
            if (doublePointsPlayers[i])
            {
                this.scores[i] *= 2;
                doublePointsPlayers[i] = false;
            }
        }

        //this.scores = new int[] { 31, 31, 0, 0 };
        Invoke(nameof(LoadPartyScene), 3f);
    }

    void LoadPartyScene()
    {
        status.gameIndex = -1;
        FindObjectOfType<LoadingScreen>().SetLoadingScreen(-2);

        if (NetworkClient.active)
        {
            GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().LoadScene(1, true, -1);
            FindObjectOfType<Methods>().DeleteAllNetworkPlayers();
        }
        else { SceneManager.LoadSceneAsync(1); }
    }

    public void CancelParty(bool alsoLoadMenu = true)
    {
        methods.LoadMenu();

        Debug.Log("Party was canceled");
    }

    public void ResetParty()
    {
        isParty = false;
        selectedGames = new bool[100];
        doublePointsPlayers = new bool[4];
        startWithPowerUp = new bool[4];
        previousSessionFields = new int[4];
        doneSetup = false;
        spawnedFields = false;
        clientsSetUp = new bool[0];
        gameSceneInvoked = false;
        currentPlayerMoving = 0;
        spawnedMenuButton = false;
    }

    public int GetPlace(int playerID)
    {
        int targetField = PlayersMovements[playerID].targetField;
        int[] sorted = new int[4];
        for (int i = 0; i < status.playerNumber; i++)
        {
            sorted[i] = PlayersMovements[i].targetField;
        }

        sorted = sorted.Distinct().ToArray();
        System.Array.Sort(sorted);
        System.Array.Reverse(sorted);
        return System.Array.IndexOf(sorted, targetField);
    }

    public void SetPowerUpUI(int playerID, int index, bool active, bool doActivate)
    {
        GameObject powerUp = ui.PowerUps[playerID].transform.parent.GetChild(index + 1).gameObject;

        if (doActivate)
        {
            powerUp.SetActive(active);
        }

        Vector2 startSize = powerUp.GetComponent<RectTransform>().localScale;
        powerUp.GetComponent<RectTransform>().localScale = Vector3.zero;
        LeanTween.scale(powerUp, startSize, .5f).setEaseOutBack().setOvershoot(2f);
    }
}

public class UI
{
    public GameObject[] PlayerUIs;
    public TMP_Text[] Names;
    public TMP_Text[] Fields;
    public GameObject[] PowerUps;
    public GameObject[] FieldAnimators;
}
