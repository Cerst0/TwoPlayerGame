using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    [Header("Custom Variables")]

    public int playerID;
    [Tooltip("0 - deactivated | 1 - copyCamRot | 2 - lookAtCamera")]
    public int allignNameRotationWithCam;
    public bool rotate;
    public InvertOptions invert;
    public InvertOptions2 invert2;
    public bool freezeMH;
    public bool freezeMV;
    public bool dontMoveController; //if set to true the Player Movement has to be applied by another script
    public bool syncPositionAtFixedUpdate;
    public bool showPlayerInFPVCamera; //if set to true the Player is visible in its own camera
    public bool dontAnimateCharacter;
    public bool disableCollisionWithOtherPlayers;
    public float playerSpeed;
    public float movementSmoothness;
    public float gravityValue;
    public float powerStrenght;
    public float powerTime;

    [Header("Player Start Position")]
    public Vector3 SinglePlayerStartPos;
    public Vector3 SinglePlayerStartRot;

    public Vector3 CenterPos;
    public string axisName;
    public bool oppositeOrder; //e.g.: false = (Player0.x = 0; Player1.x = 2); true = (Player0.x = 2; Player1.x = 0)
    public float spacing;
    public bool isSpacingProportional;

    [Header("Other Variables")]

    protected float mH = 0;
    protected float mV = 0;
    float playerSpeedStart;
    float rotateBufferAfterCountDown;

    Vector3 offset;
    [Header("Other Variables")]
    public Vector3 firstPos;
    public Quaternion firstRot;
    protected Vector3 move;
    protected Vector2 input;
    protected Vector3 velocity;
    private Vector3 gravityVelocity;
    protected Vector3 additionalMoveVector; //e.g explosion force

    protected float PowerUpTime;

    private bool groundedPlayer;
    bool isAnyPowerUpActive;
    bool isMoving;
    bool isUsingPowerUps;
    bool networkWasSetUp;
    public bool isDummy;
    public bool[] syncAmBools;
    public int team;
    protected bool isTeamLeader;
    protected bool isSingleTeam;

    protected Animator am;
    CharacterController controller;
    Camera cam;
    ParticleSystem PowerUpPs;
    GameObject PowerUpBar;
    Image PowerUpBarProgress;
    Gradient PowerUpBarColor;
    GameObject PowerUpUIPs;
    GameObject PlayerName;
    GameObject playerBody;

    protected Functions functions;
    protected InputS InputScript;
    protected Notificaton n;
    protected Methods methods;
    protected Pause pause;
    protected Status status;
    protected IdDataApply IDA;
    protected Movement teamMate;

    protected virtual void Awake()
    {
        GetChildRecursive(gameObject);
    }

    protected virtual void Start()
    {
        //CheckDestroy();
        //if (n == null) { DeclarationSetup(); } // if DeclarationSetup didn't run yet
        //return;
        CheckDestroy();
        DeclarationSetup();
    }

    protected virtual void Update()
    {
        Network();

        if (!isDummy)
        {
            MovementPart();
            if (!dontAnimateCharacter) { Animations(); }
        }
        if (isUsingPowerUps) CheckPowerUps();

        if (allignNameRotationWithCam == 1)
        {
            PlayerName.transform.rotation = functions.transform.rotation;
        }
        if (allignNameRotationWithCam == 2)
        {
            PlayerName.transform.LookAt(functions.transform);
            PlayerName.transform.Rotate(new Vector3(0, 180, 0), Space.Self);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (syncPositionAtFixedUpdate && NetworkClient.active && networkWasSetUp) { SyncPos(); }
    }

    protected void Network()
    {
        if (NetworkClient.active | NetworkServer.active)
        {
            if (transform.parent == null)
            {
                FirstNetworkSetUp();
            }
            else
            {
                transform.parent.GetComponent<NetworkTransform>().enabled = true;
                transform.parent.GetComponent<NetworkTransform>().interpolatePosition = !functions.movementLock;
            }

            if (isDummy)
            {
                for (int i = 0; i < syncAmBools.Length; i++)
                {
                    am.SetBool(am.GetParameter(i).name, syncAmBools[i]);
                }
            }
            else
            {
                for (int i = 0; i < syncAmBools.Length; i++)
                {
                    syncAmBools[i] = am.GetBool(am.GetParameter(i).name);
                }
                transform.parent.GetComponent<Sync>().SyncAnimator(syncAmBools);

                if (rotate && isMoving && rotateBufferAfterCountDown < 0)
                {
                    transform.parent.GetComponent<Sync>().Rotate(transform.GetChild(0).transform.localRotation);
                }
            }

            if (!syncPositionAtFixedUpdate) { SyncPos(); }
        }
    }

    private void SyncPos()
    {
        transform.parent.position = transform.position;
        transform.localPosition = Vector3.zero;
    }

    private void FirstNetworkSetUp()
    {
        GameObject playerInstance = GameObject.Find("NetworkPlayer" + playerID);

        try { playerInstance.transform.GetChild(0); Destroy(gameObject); }
        catch { }

        playerInstance.GetComponent<NetworkTransform>().enabled = false;

        transform.parent = playerInstance.transform;

        SyncPos();

        if (isDummy)
        {
            Destroy(transform.GetChild(1).GetComponent<Camera>().gameObject);
        }
        else
        {
            if (functions.splitScreen)
            {
                SplitScreenToSingleScreen();
            }
        }

        transform.parent.GetComponent<Sync>().movement = this;
        syncAmBools = new bool[am.parameterCount];

        networkWasSetUp = true;
        print("Made " + name + "a networked Object; isDumm? " + isDummy);
    }

    protected void GetChildRecursive(GameObject obj)
    {
        if (null == obj)
            return;

        gameObject.layer = LayerMask.NameToLayer("Player" + playerID);

        foreach (Transform child in obj.transform)
        {
            if (null == child)
                continue;
            //child.gameobject contains the current child you can do whatever you want like add it to an array
            child.name += playerID;
            child.gameObject.layer = LayerMask.NameToLayer("Player" + playerID);
            GetChildRecursive(child.gameObject);
        }
    }

    //Add Score/Point (Condition)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal") && status.clientPlayerID < 1)
        {
            functions.AddScore(1, 2, playerID);
        }
    }

    private void DeclarationSetup()
    {
        status = GameObject.FindGameObjectWithTag("Status").GetComponent<Status>();

        if (playerID >= status.playerNumber) { return; }

        n = FindObjectOfType<Notificaton>();
        methods = FindObjectOfType<Methods>();
        functions = FindObjectOfType<Functions>();
        InputScript = FindObjectOfType<InputS>();
        pause = FindObjectOfType<Pause>();
        controller = GetComponent<CharacterController>();
        playerSpeedStart = playerSpeed;
        am = transform.GetComponentInChildren<Animator>();
        cam = GetComponentInChildren<Camera>();
        IDA = transform.GetChild(0).GetComponentInChildren<IdDataApply>();
        playerBody = transform.GetChild(0).gameObject;
        PlayerName = GameObject.Find("NameWorldSpace" + playerID);
        PowerUpPs = transform.Find("PowerUpPs" + playerID).GetComponent<ParticleSystem>();

        if (functions.useTeams && status.playerNumber < 3)
        {
            transform.position = SinglePlayerStartPos;
            playerBody.transform.rotation = Quaternion.Euler(SinglePlayerStartRot);
        }

        if (NetworkClient.active && status.clientPlayerID != playerID) { isDummy = true; }

        if (dontAnimateCharacter) { am.enabled = false; }

        SetUpPowerUp();

        if (axisName != "") // Player lineup with spacing
        {
            PlayerLineUp();
        }

        if (!NetworkClient.active && functions.splitScreen)
        {
            if (playerID == 0)
            {
                cam.rect = new Rect(0, 0, 0.5f, 1);
            }
            if (playerID == 1)
            {
                cam.rect = new Rect(0.5f, 0, 0.5f, 1);
            }
        }

        if (!functions.splitScreen)
        {
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.GetComponent<Camera>().enabled = false;
        }
        else if (status.playerNumber == 1)
        {
            SplitScreenToSingleScreen();
        }

        if (functions.useTeams) // Team stuff ----
        {
            if (status.playerNumber == 2) { team = playerID; isTeamLeader = true; isSingleTeam = true; }
            if (status.playerNumber > 2)
            {
                int[] table = { 0, 0, 1, 1 };
                team = table[playerID];
                if (playerID == 0 | playerID == 2)
                {
                    isTeamLeader = true;
                }

                if (status.playerNumber == 3 && playerID == 2) { isSingleTeam = true; }

                if (!isSingleTeam)
                {
                    int[] table2 = { 1, 0, 3, 2 };
                    teamMate = functions.Players[table2[playerID]].GetComponent<Movement>();
                }
            }
        }
        else { isTeamLeader = true; team = playerID; }

        for (int i = 0; i < status.playerNumber; i++)
        {
            if (i != playerID)
            {
                Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player" + playerID), LayerMask.NameToLayer("Player" + i), disableCollisionWithOtherPlayers);
                print("Player" + playerID + " and Player" + i + " are now " + (disableCollisionWithOtherPlayers ? "disabled" : "enabled") + " to collide");
            }
        }

        if (functions.splitScreen && !showPlayerInFPVCamera && cam != null) { cam.cullingMask &= ~(1 << LayerMask.NameToLayer("Player" + playerID)); }

        rotateBufferAfterCountDown = .2f;
        firstPos = transform.position;
        firstRot = playerBody.transform.rotation;

        IDA.IDDataApply();

        print(name + " Done Setup");
    }

    private void SetUpPowerUp()
    {
        if (GameObject.FindGameObjectWithTag("PowerUpStrength") != null) { isUsingPowerUps = true; }
        if (isUsingPowerUps)
        {
            int index = playerID;
            if (status.playerNumber == 2) { index *= 2; }
            PowerUpBar = GameObject.Find("PowerUpUI" + index);
            PowerUpBarProgress = PowerUpBar.transform.GetChild(0).GetChild(1).GetComponent<Image>();
            PowerUpBarColor = FindObjectOfType<PowerUp>().PowerUpBarColor;
            PowerUpUIPs = PowerUpBar.transform.GetChild(1).gameObject;

            if (!isDummy)
            {
                PowerUpBar.transform.GetChild(0).GetChild(2).GetComponent<Image>().color = methods.GetPlayerColor(playerID);
            }
            PowerUpBar.transform.GetChild(0).GetChild(3).GetComponent<TMP_Text>().text = methods.GetPlayerName(playerID);
            PowerUpBar.transform.GetChild(0).GetChild(3).GetComponent<TMP_Text>().color = methods.GetPlayerColor(playerID);
        }

        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("PowerUpStrength");

        if (playerID == 0 && isUsingPowerUps) // deactivate PowerUpBars of unused Players & more
        {
            bool[] b = new bool[4];
            for (int i = 0; i < status.playerNumber; i++)
            {
                if (status.playerNumber == 2 && i == 1) { b[2] = true; }
                else { b[i] = true; }
            }

            for (int i = 0; i < 4; i++)
            {
                GameObject.Find("PowerUpUI" + i).SetActive(b[i]);
            }

            for (int i = 0; i < gameObjects.Length; i++)
            {
                gameObjects[i].GetComponent<PowerUp>().PowerUpIndex = i;
                gameObjects[i].name = "PowerUp" + i;
            }
        }
    }

    private void PlayerLineUp()
    {
        if (isSpacingProportional)
        {
            spacing = (spacing * 2) / status.playerNumber;
        }

        Vector3 pos0 = transform.position;

        int negativeOrPositiveDirection = -1;
        if (oppositeOrder) { negativeOrPositiveDirection = +1; }

        if (axisName == "z")
        {
            pos0.z = CenterPos.z;
            pos0.z = (spacing / 2) + ((status.playerNumber * .5f - 1) * spacing);
        }
        if (axisName == "x")
        {
            pos0.x = CenterPos.x;
            pos0.x = CenterPos.x + negativeOrPositiveDirection * ((spacing / 2) + ((status.playerNumber * .5f - 1) * spacing));
        }

        Vector3 pos = transform.position;
        int index = playerID;
        if (oppositeOrder)
        {
            index = -playerID;
        }
        if (axisName == "z")
        {
            pos.z = pos0.z + (spacing * index);
        }
        if (axisName == "x")
        {
            pos.x = pos0.x + (spacing * index);
        }
        transform.position = pos;
    }

    private void CheckDestroy()
    {
        status = GameObject.FindGameObjectWithTag("Status").GetComponent<Status>();
        if (status.playerNumber < playerID + 1 | SceneManager.GetActiveScene().name == "Menu")
        {
            //print(name + " was destroyed");
            Destroy(gameObject);
            return;
        }
    }

    protected void Animations()
    {
        if (!status.countdown) //Rotation of Player
        {
            if (rotate && isMoving && rotateBufferAfterCountDown < 0)
            {
                Quaternion quart = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z));
                playerBody.transform.rotation = quart;
            }
            else { rotateBufferAfterCountDown += -Time.deltaTime; }
        }

        if (functions.scoredPoint)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (functions.pointWinner == playerID | functions.GetTeammateID(playerID) == playerID)
            {
                am.SetBool("isWalking", false);
                am.SetBool("isWinning", true);

                if (functions.splitScreen)
                {
                    GameObject Middle = gameObject.transform.Find("Character" + playerID).transform.Find("Armature" + playerID).transform.Find("Middle" + playerID).gameObject;
                    if (offset.Equals(Vector3.zero))
                    {
                        offset = Middle.transform.position - cam.gameObject.transform.position;
                    }
                    cam.gameObject.transform.position = -offset + Middle.transform.position;
                }
            }
        }
        else
        {
            am.SetBool("isWinning", false);

            if (isMoving && !functions.movementLock)
            {
                am.SetBool("isWalking", true);
            }

            if (!isMoving | status.countdown)
            {
                am.SetBool("isWalking", false);
            }
        }
    }

    protected void MovementPart()
    {
        if (!dontMoveController) { groundedPlayer = controller.isGrounded; }

        isMoving = new Vector2(velocity.x, velocity.z).magnitude > 1f;

        if (groundedPlayer && gravityVelocity.y < 0)
        {
            gravityVelocity.y = 0f;
        }

        if (NetworkClient.active)
        {
            mH = InputScript.MHBlue;
            mV = InputScript.MVBlue;
        }
        else
        {
            if (playerID == 0)
            {
                mH = InputScript.MHBlue;
                mV = InputScript.MVBlue;
            }
            else
            {
                mH = InputScript.MHRed;
                mV = InputScript.MVRed;
            }
        }

        InvertInput();

        input = Vector3.Normalize(input);
        input *= playerSpeed;


        if (freezeMH) { input.x = 0; }
        if (freezeMV) { input.y = 0; }

        gravityVelocity.y += gravityValue * Time.deltaTime;

        if (functions.movementLock)
        {
            move = Vector3.Lerp(new Vector3(velocity.x, 0, velocity.z), Vector3.zero, movementSmoothness);
        }
        else
        {
            move = Vector3.Lerp(new Vector3(velocity.x, 0, velocity.z), new Vector3(input.x, 0, input.y), movementSmoothness);
        }
        move += additionalMoveVector;
        move *= Time.deltaTime;

        if (!dontMoveController)
        {
            controller.Move(move + gravityVelocity);
            velocity = controller.velocity;
        }
    }

    private void InvertInput()
    {
        float invertedMH = mH;
        float invertedMV = mV;

        switch (invert)
        {
            case InvertOptions.InvertXY:
                {
                    invertedMH = -mH;
                    invertedMV = -mV;
                    break;
                }

            case InvertOptions.InvertX:
                {
                    invertedMH = -mH;
                    break;
                }

            case InvertOptions.InvertY:
                {
                    invertedMV = -mV;
                    break;
                }

            case InvertOptions.None:
                {
                    input = new Vector2(mH, mV);
                    break;
                }
        }

        switch (invert2)
        {
            case InvertOptions2.SwapXWithY:
                {
                    (invertedMV, invertedMH) = (invertedMH, invertedMV);
                    break;
                }
        }

        input = new Vector2(invertedMH, invertedMV);
    }

    void SplitScreenToSingleScreen()
    {
        print("EndSplitScreen");
        if (GameObject.Find("SplitScreenBorder") != null)
        {
            Destroy(GameObject.Find("SplitScreenBorder"));
        }
        GetComponentInChildren<Camera>().rect = new Rect(0, 0, 1, 1);
    }
    protected void CheckPowerUps()
    {
        if (PowerUpTime < 0) //PowerUp was deactivated
        {
            if (isAnyPowerUpActive)
            {
                PowerUpPs.Stop();
                PowerUpUIPs.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
                playerSpeed = playerSpeedStart;
                LeanTween.cancel(PowerUpBar.transform.GetChild(0).GetComponent<RectTransform>());
                LeanTween.moveY(PowerUpBar.transform.GetChild(0).GetComponent<RectTransform>(), 60, 0.2f);

                isAnyPowerUpActive = false;
            }
        }
        else //PowerUp is active
        {
            float perCent = PowerUpTime / powerTime;
            PowerUpBarProgress.fillAmount = perCent;
            PowerUpBarProgress.transform.GetChild(0).GetComponent<Image>().color = PowerUpBarColor.Evaluate(perCent);
            perCent = ((perCent * 100) * 8) - 35;
            PowerUpBar.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(perCent, PowerUpBar.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition.y);
            var main = PowerUpUIPs.transform.GetChild(0).GetComponent<ParticleSystem>().main;
            main.startColor = PowerUpBarProgress.transform.GetChild(0).GetComponent<Image>().color;

            isAnyPowerUpActive = true;
        }

        if (LeanTween.isTweening(PowerUpBar.transform.GetChild(0).GetComponent<RectTransform>())) //Updates the PowerUpLayoutGroup
        {
            PowerUpBar.transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = false;
            PowerUpBar.transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = true;
        }

        PowerUpTime += -Time.deltaTime;
    }

    public virtual void OnPowerUp()
    {
        print(name + "activated PowerUp");
        PowerUpUIPs.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        transform.Find("PsHit" + playerID).GetComponent<ParticleSystem>().Play();
        if (!PowerUpPs.isPlaying)
        {
            FindObjectOfType<Methods>().SetPSColorFromPlayerColor(PowerUpPs, playerID);
        }

        if (!isAnyPowerUpActive)
        {
            LeanTween.cancel(PowerUpBar.transform.GetChild(0).GetComponent<RectTransform>());
            LeanTween.moveY(PowerUpBar.transform.GetChild(0).GetComponent<RectTransform>(), 0, 0.2f);
        }

        float x = PowerUpBar.transform.GetChild(0).GetChild(3).GetComponent<TMP_Text>().bounds.extents.x + 25;
        RectTransform LightningSymbol0 = PowerUpBar.transform.GetChild(0).GetChild(4).GetComponent<RectTransform>();
        RectTransform LightningSymbol1 = PowerUpBar.transform.GetChild(0).GetChild(5).GetComponent<RectTransform>();
        LightningSymbol0.anchoredPosition = new Vector2(x, LightningSymbol0.anchoredPosition.y);
        LightningSymbol1.anchoredPosition = new Vector2(-x, LightningSymbol1.anchoredPosition.y);

        playerSpeed = playerSpeedStart * (powerStrenght); //overite it for example to something that also reduce speed of not powered Players

        PowerUpTime = powerTime;

        //print(name + " activated PowerUp");
    }

    public virtual void OnReset()
    {
        if (NetworkClient.active)
        {
            transform.localPosition = Vector3.zero;
            transform.parent.transform.position = firstPos;
        }
        else
        {
            transform.position = firstPos;
        }
        playerBody.transform.rotation = firstRot;
    }

    public enum InvertOptions
    {
        None,
        InvertX,
        InvertY,
        InvertXY,
    }

    public enum InvertOptions2
    {
        None,
        SwapXWithY,
    }
}