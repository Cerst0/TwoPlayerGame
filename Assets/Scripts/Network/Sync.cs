using Mirror;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sync : NetworkBehaviour
{
    NetworkManager manager;
    public Movement movement;
    Status status;
    Color[] colors;
    bool overrided;
    bool firstIDA;
    bool appliedData;
    float IDATime;
    float collectStartDelay = .5f;

    bool[] readyClients = new bool[4];
    bool readyB;
    bool[] doneSetups = new bool[4];

    public int playerID;

    readonly public SyncList<Color> currentMeshColors = new();
    [SyncVar] public string playerName;
    [SyncVar] public int playersConnected;
    [SyncVar] public int meshNumber;
    [SyncVar] public bool allClientsReady;

    private void Awake()
    {
        status = GameObject.FindGameObjectWithTag("Status").GetComponent<Status>();
    }

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();

        playerID = GetComponent<NetworkIdentity>().playerID;
        name = "NetworkPlayer" + playerID;

        //name = "NetworkPlayer" + playerIndex.ToString();
        if (isLocalPlayer)
        {
            gameObject.tag = "PlayerInstance";
            status.clientPlayerID = playerID;
        }
        else { gameObject.tag = "Player" + playerID; }
        CollectAll(false);
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if (NetworkClient.ready) { SetReady(playerID); }
        }

        //only server is allowed to announce player count since he is the only that can count them
        if (isServer)
        {
            playersConnected = manager.numPlayers;
        }

        try
        {
            if (currentMeshColors[0] != null && !firstIDA) { overrided = true; }
        }
        catch { }

        if (overrided && !appliedData) // first time Data apply on all IDAs
        {
            IdDataApply IDA = GameObject.FindGameObjectWithTag("IDA" + playerID).GetComponent<IdDataApply>();
            print(IDA.gameObject.name);
            IDA.IDDataApply();
            print("new unlocal Player with ID " + playerID + " spawned and the Player assigned his ID-Data");
            overrided = false;
            appliedData = true;
            IDATime = 1;
        }

        if (IDATime > 0) //DataApply in a time span, but only if Ida first(not on start)
        {
            IdDataApply IDA = GameObject.FindGameObjectWithTag("IDA" + playerID).GetComponent<IdDataApply>();
            IDA.IDDataApply();

            IDATime += -Time.deltaTime;
        }


        if (collectStartDelay > 0) //Collects Only on first TIME with idaFirst = false
        {
            collectStartDelay += -Time.deltaTime;
        }
    }

    public void CollectAll(bool IDAfirst = true)
    {
        if (!isLocalPlayer) { return; }

        int index = 0;
        if (IDAfirst)
        {
            index = playerID;
        }

        IdDataApply IDA = GameObject.FindGameObjectWithTag("IDA" + index).GetComponent<IdDataApply>();
        colors = new Color[20];

        if (IDAfirst) //If so, applies first Data and then apply it; don't run on start
        {
            IDA.IDDataApply();

            //Colors
            SkinnedMeshRenderer skr = IDA.GetComponent<SkinnedMeshRenderer>();
            for (int i = 0; i <= skr.materials.Length - 1; i++)
            {
                colors[i] = skr.materials[i].color;
            }

            //Name
            playerName = IDA.GetComponent<IdDataApply>().playerName;

            //MeshNumber
            meshNumber = IDA.GetComponent<IdDataApply>().meshNumber;
        }
        else
        {
            SyncPre SP = FindObjectOfType<SyncPre>();

            colors = SP.colors;
            playerName = SP.playerName;
            meshNumber = SP.meshNumber;
        }

        //---------------
        firstIDA = IDAfirst;
        print("Local Sync collected all PlayerData from IDA: " + IDA.name + playerName + playerID);
        Override(colors, playerName, meshNumber, firstIDA);
    }

    [Command]
    private void Override(Color[] colors, string playerName, int meshNumber, bool IDA)
    {
        currentMeshColors.Clear();
        foreach (Color c in colors)
        {
            currentMeshColors.Add(c);
        }

        this.playerName = playerName;
        this.meshNumber = meshNumber;

        if (IDA)
        {
            PerformIDDataApply();
        }
    }

    [ClientRpc]
    private void PerformIDDataApply()
    {
        print("IDA Perform on Client");
        IDATime = 1f;
    }

    [Command]
    public void LoadScene(int sceneIndex, bool setGameEnterstoZero, int gameIndex)
    {
        if (playerID == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                doneSetups[i] = false;
                readyClients[i] = false;
            }
            allClientsReady = false;
        }

        if (setGameEnterstoZero)
        {
            SceneChangeSetupClient(gameIndex);
        }

        string path = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
        string sceneName = path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);

        //print("Server load scene: " + sceneName + " with index: " + sceneIndex + " and gameIndex: " + gameIndex);
        manager.ServerChangeScene(sceneName);
    }

    [ClientRpc]
    private void SceneChangeSetupClient(int gameIndex)
    {
        if (status.clientPlayerID == 0) { return; }

        Destroy(GameObject.FindGameObjectWithTag("Brain"));
        Destroy(GameObject.FindGameObjectWithTag("Canvas"));

        Movement[] movements = FindObjectsOfType<Movement>();
        foreach (Movement m in movements)
        {
            Destroy(m.gameObject);
        }

        FindObjectOfType<LoadingScreen>().SetLoadingScreen(status.gameIndex);
    }

    [Command]
    public void Spawn(string resourceName, Vector3 Position, string[] additionalID, int additionalIDTarget)
    {
        GameObject go = Resources.Load<GameObject>(resourceName);
        go.transform.position = Position;

        if (additionalID != null)
        {
            if (additionalIDTarget == 0) { go.GetComponent<Viking>().id = additionalID[0]; }
        }

        GameObject instantiatedObj = Instantiate(go);
        NetworkServer.Spawn(instantiatedObj);

        //print("Object spawned on Server with additional Id : " + additionalID);
    }

    [Command]
    public void SyncAnimator(bool[] syncAmBools)
    {
        SyncAnimatorClient(syncAmBools);
    }

    [ClientRpc]
    private void SyncAnimatorClient(bool[] syncAmBools)
    {
        if (isLocalPlayer | movement == null) { return; }
        movement.syncAmBools = syncAmBools;
    }

    [Command]
    public void Rotate(Quaternion localRotation)
    {
        RotateClient(localRotation);
    }

    [ClientRpc]
    private void RotateClient(Quaternion localRotation)
    {
        if (isLocalPlayer) { return; }

        Transform Character = transform.GetChild(0).GetChild(0);
        Character.localRotation = Quaternion.Lerp(localRotation, Character.localRotation, .9f);
    }

    [Command]
    public void Print(string message)
    {
        print(message);
    }

    [Command]
    public void SetSetupReady(int index)
    {
        Sync sync = GameObject.Find("NetworkPlayer0").GetComponent<Sync>();
        sync.doneSetups[index] = true;
        sync.CheckReady();
    }

    [Command]
    public void OpenPauseOnServer()
    {
        foreach (Sync sync in FindObjectsOfType<Sync>())
        {
            sync.OpenPauseOnClient();
        }
    }

    [ClientRpc]
    private void OpenPauseOnClient()
    {
        if (status.clientPlayerID == 0 | !isLocalPlayer) { return; }
        FindObjectOfType<Pause>().OpenPause(true);
    }

    [Command]
    public void ClosePauseOnServer()
    {
        foreach (Sync sync in FindObjectsOfType<Sync>())
        {
            sync.ClosePauseOnClient();
        }
    }

    [ClientRpc]
    private void ClosePauseOnClient()
    {
        if (status.clientPlayerID == 0 | !isLocalPlayer) { return; }
        FindObjectOfType<Pause>().ClosePause();
    }

    public void AddScore(float delay, int maxScores, int ID, bool isCalledByFunctions = false, int scoreCount = 1)
    {
        AddScoreOnServer(delay, maxScores, ID, isCalledByFunctions, scoreCount);
    }

    [Command]
    private void AddScoreOnServer(float delay, int maxScores, int ID, bool isCalledByFunctions, int scoreCount)
    {
        AddScoreOnClient(delay, maxScores, ID, isCalledByFunctions, scoreCount);
    }

    [ClientRpc]
    private void AddScoreOnClient(float delay, int maxScores, int ID, bool isCalledByFunctions, int scoreCount)
    {
        if (status.clientPlayerID == 0 && isCalledByFunctions) { return; }
        FindObjectOfType<Functions>().AddScore(delay, maxScores, ID, true, false, scoreCount);
    }

    [Command]
    private void SetReady(int index)
    {
        //print("client " + index + " is ready");
        Sync sync = GameObject.Find("NetworkPlayer0").GetComponent<Sync>();
        sync.readyClients[index] = true;
        sync.CheckReady();
    }

    [ClientRpc]
    public void SetCountdown(int index)
    {
        FindObjectOfType<Countdown>().UpdateCountdown(index);
    }

    [Command]
    public void PlayPsOnClients(string psName, bool setStartColor, Color startColor)
    {
        foreach (Sync sync in FindObjectsOfType<Sync>())
        {
            sync.PlayPsOnClientsClient(psName, setStartColor, startColor);
        }
    }

    [ClientRpc]
    public void PlayPsOnClientsClient(string psName, bool setStartColor, Color startColor)
    {
        ParticleSystem ps = GameObject.Find(psName).GetComponent<ParticleSystem>();
        var main = ps.main;

        if (setStartColor)
        {
            main.startColor = startColor;
        }
        ps.Clear();
        ps.Play();
    }

    [Command]
    public void SetGameObjectPos(string GameObjectName, Vector3 pos)
    {
        SetGameObjectPosClient(GameObjectName, pos);
    }

    [ClientRpc]
    private void SetGameObjectPosClient(string GameObjectName, Vector3 pos)
    {
        GameObject go = GameObject.Find(GameObjectName);
        go.transform.position = pos;
    }

    [Command]
    public void NotificatonWith2Colors(Color color, float duration, string playerName, string text, Color playerColor)
    {
        ClientNotificatonWith2Colors(color, duration, playerName, text, playerColor);
    }

    [ClientRpc]
    private void ClientNotificatonWith2Colors(Color color, float duration, string playerName, string text, Color playerColor)
    {
        Notificaton n = FindObjectOfType<Notificaton>();
        n.NotificationWith2Colors(color, duration, playerName, text, playerColor, false);
    }

    [Command]
    public void ServerNetworkNotification(Color color, string name, string message, float duration, int id)
    {
        ClientNotification(color, name, message, duration, id);
    }

    [ClientRpc]
    private void ClientNotification(Color color, string name, string message, float duration, int id)
    {
        Notificaton n = FindObjectOfType<Notificaton>();

        if (id == status.clientPlayerID)
        {
            name += ", you were ";
        }
        else { name += " "; }

        string notifi = name + message;
        n.Notification(color, notifi, duration);
    }

    [Command]
    public void SetPowerUpPlayerServer(int ID, int powerUpIndex)
    {
        SetPowerUpPlayerClient(ID, powerUpIndex);
    }

    [ClientRpc]
    private void SetPowerUpPlayerClient(int ID, int powerUpIndex)
    {
        print(GameObject.Find("PowerUp" + powerUpIndex));
        if (powerUpIndex == -1) //only issued by function in party mode
        {
            FindObjectOfType<Functions>().Players[ID].GetComponent<Movement>().OnPowerUp();
        }
        else
        {
            GameObject.Find("PowerUp" + powerUpIndex).GetComponent<PowerUp>().PlayerUsePowerUp(null, ID);
        }
    }

    [ClientRpc]
    public void UpdatePlayerID(int newID)
    {
        GetComponent<NetworkIdentity>().playerID = newID;
        playerID = newID;

        print(name + " update player id; new ID is: " + playerID + " status id: " + status.clientPlayerID);
        name = "NetworkPlayer" + playerID;

        //name = "NetworkPlayer" + playerIndex.ToString();
        if (isLocalPlayer)
        {
            status.clientPlayerID = playerID;
        }
        else { gameObject.tag = "Player" + playerID; }
    }

    [ClientRpc]
    public void SpawnPowerUp(int PowerUpIndex)
    {
        GameObject.Find("PowerUp" + PowerUpIndex).GetComponent<PowerUp>().Spawn();
    }

    private void CheckReady()
    {
        bool anyFalse = false;
        for (int i = 0; i < manager.numPlayers; i++)
        {
            if (readyClients[i] == false) { anyFalse = true; }
        }
        if (!anyFalse) { allClientsReady = true; }

        //print(manager.numPlayers + " ReadyCheck " + doneSetups[0] + " " + startCountDown + " " + readyClients[0] + " " + allClientsReady);
    }

    [Command]
    public void SetTimeScale(float timeScale)
    {
        SetTimeScaleClient(timeScale);
    }

    [ClientRpc]
    private void SetTimeScaleClient(float timeScale)
    {
        if (isLocalPlayer) { return; }
        Time.timeScale = timeScale;
    }

    [ClientRpc]
    public void SetPartyState(bool isParty)
    {
        status.isParty = true;
        FindObjectOfType<Party>().isParty = true;
    }

    [ClientRpc]
    public void callFunction(string functionName, int scriptType)
    {
        //scryptType: 0 Functions, 1 Methods, 2 Party
        switch (scriptType)
        {
            case 0:
                {
                    FindObjectOfType<Functions>().Invoke(functionName, 0f);
                    break;
                }
            case 1:
                {
                    FindObjectOfType<Methods>().Invoke(functionName, 0f);
                    break;
                }
            case 2:
                {
                    FindObjectOfType<Party>().Invoke(functionName, 0f);
                    break;
                }
        }
    }


    //OTHER GAMES ------------------------------------------------------------------------------------------------------------------------------------------------------------------

    //MS----------------------
    [Command]
    public void ServerKick(Vector3 pos, Vector3 vel)
    {
        print("Server Kick");
        FindObjectOfType<KickBall>().Kick(pos, vel);
    }

    //PARKOUR --------------------
    public void InstantiateLevel(int levelID)
    {
        if (allClientsReady)
        {
            InstantiateLevelClient(levelID);
        }
        else
        {
            StartCoroutine(WaitForReady(levelID));
        }
    }

    IEnumerator WaitForReady(int levelID)
    {
        while (!allClientsReady)
        {
            yield return new WaitForSeconds(0.25f);
        }
        InstantiateLevelClient(levelID);
    }

    [ClientRpc]
    private void InstantiateLevelClient(int levelID)
    {
        foreach (GameObject level in GameObject.FindGameObjectsWithTag("Level"))
        {
            if (!level.name.Contains(levelID.ToString()))
            {
                Destroy(level);
            }
        }
    }

    [ClientRpc]
    public void BombInvoke(string FunctionName, int id, Vector3 pos)
    {
        if (status.clientPlayerID == 0) { return; }
        Bomb[] bombs = FindObjectsOfType<Bomb>();
        foreach (Bomb b in bombs)
        {
            if (b.BombID == id)
            {
                b.transform.position = pos;
                b.Invoke(FunctionName, 0);
            }
        }
    }

    [ClientRpc]
    public void SetLabySide(int labyID, int side)
    {
        if (status.clientPlayerID == 0) { return; }
        GameObject.Find("Laby" + labyID).GetComponent<Laby>().side = side;
    }

    // TD --------------------------------------------

    [ClientRpc]
    public void VikingHurt(float attackStrenght, string VikingID)
    {
        Viking[] vikings = FindObjectsOfType<Viking>();
        foreach (Viking vik in vikings)
        {
            if (vik.id == VikingID)
            {
                vik.OnHurt(attackStrenght);
                return;
            }
        }
        Debug.LogError("Viking with id: " + VikingID + " wasn't found by sync");
    }

    [ClientRpc]
    public void VikingHitCastle(float attackStrenght, int targetCastleTeamID)
    {
        FindObjectOfType<Functions>().Players[targetCastleTeamID].GetComponent<MovementTD>().ReceiveHit(Mathf.RoundToInt(-attackStrenght));
    }

    // CAR ----------------------------------------
    [Command]
    public void SetCarVars(Quaternion rot, Quaternion[] currentWheelRotations, Vector3 PowerBarScale, Vector3 PowerBarPos, int PowerBarMatIndex, bool isSmokePlaying, Color smokeColor, int id)
    {
        SetCarVarsClient(rot, currentWheelRotations, PowerBarScale, PowerBarPos, PowerBarMatIndex, isSmokePlaying, smokeColor, id);
    }

    [ClientRpc]
    private void SetCarVarsClient(Quaternion rot, Quaternion[] currentWheelRotations, Vector3 PowerBarScale, Vector3 PowerBarPos, int PowerBarMatIndex, bool isSmokePlaying, Color smokeColor, int id)
    {
        if (status.functions.Players[id] == null)
        {
            return;
        }

        if (status.clientPlayerID != id && status.functions.Players.Length > 0)
        {
            status.functions.Players[id].transform.rotation = Quaternion.Lerp(rot, status.functions.Players[id].transform.rotation, .95f);

            CarController CC = status.functions.Players[id].GetComponent<CarController>();

            for (int i = 0; i < CC.Wheels.Length; i++)
            {
                CC.Wheels[i].transform.rotation = Quaternion.Lerp(currentWheelRotations[i], CC.Wheels[i].transform.rotation, .8f);
            }

            CC.Power.transform.localScale = Vector3.Lerp(PowerBarScale, CC.Power.transform.localScale, .8f);
            CC.Power.transform.localPosition = Vector3.Lerp(PowerBarPos, CC.Power.transform.localPosition, .8f);
            CC.Power.GetComponent<MeshRenderer>().material = CC.PowerBarMaterials[PowerBarMatIndex];
            CC.smoke2.startColor = Color.Lerp(smokeColor, CC.smoke2.startColor, .8f);

            if (isSmokePlaying) { CC.smoke.Play(); CC.smoke2.Play(); }
            else { CC.smoke.Stop(); CC.smoke2.Stop(); }
        }
    }

    // Tennis ----------------------------------------

    [ClientRpc]
    public void ServeOrWaitNetwork(int randomPlayer)
    {
        if (status.clientPlayerID == 0) { return; }
        GameObject.Find("%Player1").GetComponent<MovementTennis>().ServerOrWait(randomPlayer);
    }

    [Command]
    public void TennisServe(int id)
    {
        TennisServeClient(id);
    }

    [ClientRpc]
    private void TennisServeClient(int id)
    {
        FindObjectOfType<TennisBall>().Serve(id, true);
    }

    [Command]
    public void PredictTennisBall(Vector3 currentPosition, Vector3 force, int id)
    {
        PredictTennisBallClient(currentPosition, force, id);
    }

    [ClientRpc]
    private void PredictTennisBallClient(Vector3 currentPosition, Vector3 force, int id)
    {
        if (isLocalPlayer) { return; }
        FindObjectOfType<TrajectoryPrediction>().Predict(currentPosition, force, id, true);
    }

    [Command]
    public void BallAddForce(Vector3 force)
    {
        BallAddForceClient(force);
    }

    [ClientRpc]
    private void BallAddForceClient(Vector3 force)
    {
        if (!NetworkServer.active) { return; }
        FindObjectOfType<TennisBall>().AddForce(force, true);
    }

    [Command]
    public void BallCalculateHit(Vector3 target, int id)
    {
        BallCalculateHitClient(target, id);
    }

    [ClientRpc]
    private void BallCalculateHitClient(Vector3 target, int id)
    {
        if (isLocalPlayer) { return; }
        FindObjectOfType<TennisBall>().state = TennisBall.BallState.CalculateHit;
        FindObjectOfType<TennisBall>().target = target;
        FindObjectOfType<TennisBall>().hitTeam = id;
    }

    [Command]
    public void BallSetState(TennisBall.BallState state)
    {
        BallSetStateClient(state);
    }

    [ClientRpc]
    private void BallSetStateClient(TennisBall.BallState state)
    {
        if (isLocalPlayer) { return; }
        FindObjectOfType<TennisBall>().state = state;
    }


    //PARTY ----------------------------------------
    [ClientRpc]
    public void SetPillarMaterial(int pillarID, Color GapColor)
    {
        if (status.clientPlayerID == 0) { return; }
        GameObject.Find("Pillar" + pillarID).GetComponent<MeshRenderer>().material.SetColor("_GapColor", GapColor);
    }

    [ClientRpc]
    public void PutOnCrownOnClients(int ID)
    {
        FindObjectOfType<Party>().PlayersMovements[ID].PutOnCrown();
    }

    [ClientRpc]
    public void SetDiceRollNumUI(string text, Color col)
    {
        if (status.clientPlayerID == 0) { return; }
        TMP_Text DiceText = FindObjectOfType<Party>().PlayersMovements[0].DiceNumberUI;
        DiceText.text = text;
        DiceText.color = col;
    }

    [ClientRpc]
    public void syncUIParty(int index, int currentPlayer, int[] fields, int[] oldFields, int playerID, int powerUpIndex, bool activate, bool doActivate)
    {
        if (status.clientPlayerID == 0) { return; }

        if (index == 0)
        {
            FindObjectOfType<Party>().StartOfTurnAnimations(currentPlayer);
        }
        if (index == 1)
        {
            FindObjectOfType<Party>().EndOfTurnAnimations(currentPlayer);
        }
        if (index == 2)
        {
            FindObjectOfType<Party>().SetFieldUI(fields, oldFields);
        }
        if (index == 3)
        {
            FindObjectOfType<Party>().SetPowerUpUI(playerID, powerUpIndex, activate, doActivate);
        }
        //print("synced Party UI" + index + " " + currentPlayer + " " + playerID + " " + powerUpIndex + " " + activate);

    }

    [ClientRpc]
    public void CheckIfPartyIsSetUp()
    {
        print("set up client " + status.clientPlayerID + " " + FindObjectOfType<Party>().doneSetup);
        if (FindObjectOfType<Party>().doneSetup || status.clientPlayerID == 0) { status.sync.setPartySetUp(status.clientPlayerID); }
    }

    [Command]
    private void setPartySetUp(int id)
    {
        if (FindObjectOfType<Party>().clientsSetUp == null) { return; }
        FindObjectOfType<Party>().clientsSetUp[id] = true;
    }
}
