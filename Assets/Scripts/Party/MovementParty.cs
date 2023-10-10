using Mirror;
using Mono.Cecil.Cil;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MovementParty : MonoBehaviour
{
    [Header("Custom Variables")]

    public int playerID;
    public float playerSpeed;
    public float movementSmoothness;

    public Gradient Double;
    public Gradient PowerUp;
    [ColorUsage(false, true)] public Color[] PillarColors;
    public TMP_Text DiceNumberUI;
    public GameObject Crown;

    [Header("Player Start Position")]
    public Vector3 CenterPos;
    public string axisName;
    public bool oppositeOrder; //e.g.: false = (Player0.x = 0; Player1.x = 2); true = (Player0.x = 2; Player1.x = 0)
    public float spacing;
    public bool isSpacingProportional;

    [Header("Other Variables")]
    public Vector3 newMoveVector;
    public Vector3 firstPos;
    public Quaternion firstRot;
    protected Vector3 move;
    protected Vector3 velocity;
    private Vector3 gravityVelocity;

    private bool groundedPlayer;
    bool isMoving;
    bool finishedMove;
    public bool moveTilPillar;
    public bool isDummy;
    int startField;
    public int targetField;
    float diceRolls;
    float diceRollsUI;
    float moveStartDelay;

    public State state;

    Animator am;
    CharacterController controller;
    Camera cam;
    ParticleSystem PowerUpPs;
    ParticleSystem SpecialFieldUpPs;
    GameObject PlayerName;
    GameObject playerBody;
    GameObject Pillar;

    Notificaton n;
    Methods methods;
    Status status;
    IdDataApply IDA;
    Party party;

    protected virtual void Awake()
    {
        if (GameObject.Find("@DontDestroyOnLoadObjects") == null)
        {
            UnityEngine.Object obj = Instantiate(Resources.Load("@DontDestroyOnLoadObjects"));
            obj.name = "@DontDestroyOnLoadObjects";
        }

        GetChildRecursive(gameObject);
    }

    protected virtual void Start()
    {
        CheckDestroy();
        DeclarationSetup();
    }

    protected virtual void Update()
    {
        if (!isDummy) { MovementPart(); }
        Animations();

        PlayerName.transform.LookAt(cam.transform);
        PlayerName.transform.Rotate(new Vector3(0, 180, 0), Space.Self);
    }

    protected void GetChildRecursive(GameObject obj)
    {
        if (obj is null)
            return;

        gameObject.layer = LayerMask.NameToLayer("Player" + playerID);

        foreach (Transform child in obj.transform)
        {
            if (child is null || child.name == "Armature")
                continue;
            child.name += playerID;
            child.gameObject.layer = LayerMask.NameToLayer("Player" + playerID);
            GetChildRecursive(child.gameObject);
        }
    }

    private void DeclarationSetup()
    {
        status = GameObject.FindGameObjectWithTag("Status").GetComponent<Status>();

        if (playerID >= status.playerNumber) { return; }

        n = FindObjectOfType<Notificaton>();
        methods = FindObjectOfType<Methods>();
        controller = GetComponent<CharacterController>();
        am = transform.GetComponentInChildren<Animator>();
        IDA = transform.GetChild(0).GetComponentInChildren<IdDataApply>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        playerBody = transform.GetChild(0).gameObject;
        PlayerName = GameObject.Find("NameWorldSpace" + playerID);
        Pillar = GameObject.Find("Pillar" + playerID);
        party = FindObjectOfType<Party>();
        PowerUpPs = transform.Find("PowerUpPs" + playerID).GetComponent<ParticleSystem>();
        SpecialFieldUpPs = transform.Find("SpecialFieldPs" + playerID).GetComponent<ParticleSystem>();


        isDummy = NetworkClient.active && !NetworkServer.active;

        if (axisName != "") // Player lineup with spacing
        {
            PlayerLineUp();
        }

        if (isDummy)
        {
            GetComponent<CharacterController>().enabled = false;
        }

        startField = GetCurrentField();
        targetField = party.scores[playerID] + startField;

        firstPos = transform.position;
        firstRot = playerBody.transform.rotation;

        IDA.IDDataApply();

        print(name + " Done Setup");
    }

    private void PlayerLineUp()
    {
        if (isSpacingProportional)
        {
            spacing = (spacing * 2) / status.playerNumber;
            if (status.playerNumber < 4) { spacing -= (4 - status.playerNumber); }
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

        TPToStartField(party.previousSessionFields[playerID]);
        party.SetPillar();
    }

    private void TPToStartField(int field)
    {
        Vector3 startPos = transform.position;
        startPos.z = -party.fieldCount * 9 - 8.9f;
        startPos.z += field * 9;
        transform.position = startPos;
        //print(startPos + " " + playerID + "field: " + field);
    }

    public enum State
    {
        idle,
        walk,
        finishedMove,
        winMove,
        winFreeze
    }

    private void CheckDestroy()
    {
        status = FindObjectOfType<Status>();
        if (status.playerNumber < playerID + 1 || SceneManager.GetActiveScene().name == "Menu")
        {
            print(name + " was destroyed");
            Destroy(gameObject);
            return;
        }
    }

    protected void Animations()
    {
        if (!NetworkClient.active || NetworkServer.active)
        {
            am.SetBool("isWinning", false);

            if (isMoving)
            {
                am.SetBool("isWalking", true);
            }
            else
            {
                am.SetBool("isWalking", false);
            }
        }
    }

    protected void MovementPart()
    {
        groundedPlayer = controller.isGrounded;

        isMoving = new Vector2(velocity.x, velocity.z).magnitude > 1f;

        if (groundedPlayer && gravityVelocity.y < 0)
        {
            gravityVelocity.y = 0f;
        }
        gravityVelocity.y += -9.81f * Time.deltaTime;

        switch (state)
        {
            case State.walk:
                {
                    if (playerID > 0) //give time for camera to focus on player
                    {
                        float camDifference = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);

                        if (camDifference > 1)
                        {
                            break;
                        }
                        else if (moveStartDelay < 1 && party.scores[playerID] == 0)
                        {
                            moveStartDelay += Time.deltaTime;
                            break;
                        }
                    }

                    if (Mathf.Abs(GetTargetFieldDistance()) < .5f) // reached target field
                    {
                        //print(name + " reached target field");
                        newMoveVector.z = 0; //when standing on special field and waiting for the "animation" to finish;
                        if (targetField > 0) //check if player has moved at least one field so he isnt on the start area anymore
                        {
                            switch (GameObject.Find("Field" + (targetField - 1)).GetComponent<Field>().type)
                            {
                                case Field.Type.doublePoints:
                                    {
                                        party.doublePointsPlayers[playerID] = true;

                                        methods.PlayNetworkedPs(PowerUpPs.name, false, Color.clear, Double);
                                        methods.PlayNetworkedPs(SpecialFieldUpPs.name, false, Color.clear, Double);

                                        if (NetworkServer.active)
                                        {
                                            status.sync.syncUIParty(3, -1, null, null, playerID, 0, true, true);
                                        }
                                        party.SetPowerUpUI(playerID, 0, true, true);

                                        EndMove();
                                        break;
                                    }
                                case Field.Type.dice:
                                    {
                                        int random = UnityEngine.Random.Range(-2, 3);
                                        Color col = (random < 0) ? Color.red : (random == 0) ? Color.white : (random > 0) ? Color.green : Color.magenta;

                                        if (diceRolls > 3)
                                        {
                                            SetDiceRollUIText(random.ToString(), col);

                                            party.targetFields[playerID] += random;
                                            if (NetworkServer.active) //server server
                                            {
                                                status.sync.syncUIParty(2, 0, party.targetFields, null, -1, -1, false, false);
                                            }
                                            if (NetworkServer.active || !NetworkClient.active) { party.SetFieldUI(party.targetFields, null); } //server or no lan
                                            methods.PlayNetworkedPs(SpecialFieldUpPs.name, true, col);

                                            diceRollsUI = 0;
                                            diceRolls = 0;

                                            if (random == 0)
                                            {
                                                EndMove();
                                            }
                                            else
                                            {
                                                targetField += random;
                                                targetField = Mathf.Clamp(targetField, 0, 999);
                                            }
                                        }
                                        else
                                        {
                                            if (diceRollsUI > 0.1f)
                                            {
                                                SetDiceRollUIText(random.ToString(), col);

                                                diceRollsUI = 0;
                                            }
                                            diceRollsUI += Time.deltaTime;
                                            diceRolls += Time.deltaTime;
                                        }

                                        break;
                                    }
                                case Field.Type.powerUp:
                                    {
                                        methods.PlayNetworkedPs(PowerUpPs.name, false, Color.clear, PowerUp);
                                        methods.PlayNetworkedPs(SpecialFieldUpPs.name, false, Color.clear, PowerUp);

                                        if (NetworkServer.active)
                                        {
                                            status.sync.syncUIParty(3, -1, null, null, playerID, 1, true, true);
                                        }
                                        party.SetPowerUpUI(playerID, 1, true, true);

                                        party.startWithPowerUp[playerID] = true;

                                        EndMove();
                                        break;
                                    }
                                default:
                                    {
                                        EndMove();
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            EndMove();
                        }
                    }
                    else
                    {
                        if (targetField > party.fieldCount) // Win
                        {
                            state = State.winMove;
                        }
                        else // Continue Move
                        {
                            //print(name + "continue move");

                            float speed = playerSpeed * (GetTargetFieldDistance() / 5);
                            speed = Mathf.Clamp(speed, -playerSpeed, playerSpeed);
                            if (Mathf.Abs(speed) < playerSpeed / 10)
                            {
                                speed = 0;
                            }
                            newMoveVector.z = speed;
                        }
                    }
                    break;
                }

            case State.winMove:
                {
                    CheckIfArrivedAtPillar();

                    break;
                }

            case State.winFreeze:
                {
                    newMoveVector = Vector3.zero;
                    break;
                }
        }

        if (finishedMove && moveTilPillar)
        {
            CheckIfArrivedAtPillar();
        }

        move = Vector3.Lerp(new Vector3(velocity.x, 0, velocity.z), newMoveVector, movementSmoothness);
        move *= Time.deltaTime;

        controller.Move(move + gravityVelocity);
        velocity = controller.velocity;
    }

    private void SetDiceRollUIText(string random, Color col)
    {
        random = random.Replace("-", string.Empty);

        DiceNumberUI.text = random;
        DiceNumberUI.color = col;

        if (NetworkClient.active)
        {
            status.sync.SetDiceRollNumUI(random, col);
        }
    }

    private void CheckIfArrivedAtPillar()
    {
        if (transform.position.z > party.Pillars[playerID].transform.position.z) // reached Pillar
        {
            WhenArrivedAtPillar();
        }
        else
        {
            newMoveVector.z = playerSpeed;
        }
    }

    private void WhenArrivedAtPillar()
    {
        bool lastArrivedPlayer = (playerID + 1) == status.playerNumber;
        LeanTween.rotateY(Pillar, Pillar.transform.rotation.y + 180, 3f);
        LeanTween.rotateY(gameObject, transform.rotation.y - 180, 3f);

        state = State.winFreeze;
        moveTilPillar = false;

        for (int i = 0; i < party.PlayersMovements.Length; i++)
        {
            party.PlayersMovements[i].UpdatePillar();

            if (lastArrivedPlayer)
            {
                if (party.GetPlace(i) == 0)
                {
                    party.PlayersMovements[i].PutOnCrown();
                }
            }
        }
    }

    public void PutOnCrown()
    {
        if (!Crown.activeSelf)
        {
            Crown.SetActive(true);

            if (NetworkServer.active)
            {
                status.sync.PutOnCrownOnClients(playerID);
            }
        }
    }

    private void EndMove()
    {
        SetDiceRollUIText(string.Empty, Color.clear); //reset DiceUI
        if (moveTilPillar)
        {
            state = State.winMove;
        }
        else
        {
            state = State.finishedMove;
            finishedMove = true;
            newMoveVector.z = 0;
        }
    }

    public int GetCurrentField()
    {
        int field = Mathf.RoundToInt((transform.position.z - (transform.position.z % 9)) / 9);
        field = Mathf.Abs(field);
        field = party.fieldCount - field;
        print(name + " field: " + field);
        return field;
    }

    public float GetTargetFieldDistance()
    {
        float distance;
        if (targetField <= 0 || targetField > party.fieldCount)
        {
            if (targetField <= 0)
            {
                distance = (party.Fields[0].transform.position.z - 9) - transform.position.z;
            }
            else
            {
                distance = party.Pillars[0].transform.position.z - transform.position.z;
            }
        }
        else
        {
            distance = party.Fields[targetField - 1].transform.position.z - transform.position.z;
        }
        //print(name + distance);
        return distance;
    }

    public void UpdatePillar()
    {
        if (state != State.winFreeze) return;
        int place = party.GetPlace(playerID);
        float height = -2.2f * place + 4.6f;

        Color col = PillarColors[place];
        Pillar.GetComponent<MeshRenderer>().material.SetColor("_GapColor", col);
        if (NetworkClient.active)
        {
            status.sync.SetPillarMaterial(playerID, col);
        }

        bool differentHeight = Pillar.transform.position.y != height ? true : false;

        if (differentHeight)
        {
            LeanTween.cancel(Pillar);
            LeanTween.moveLocalY(Pillar, height, 3f);

            //PS
            ParticleSystem PillarPS = GameObject.Find("Pillar" + playerID + "PS").GetComponent<ParticleSystem>();
            PillarPS.gameObject.transform.position = new Vector3(Pillar.transform.position.x, PillarPS.transform.position.y, PillarPS.transform.position.z);
            PillarPS.Clear();
            PillarPS.Play();
            methods.PlayNetworkedPs(PillarPS.name, false, Color.clear, null, true);
        }
    }
}