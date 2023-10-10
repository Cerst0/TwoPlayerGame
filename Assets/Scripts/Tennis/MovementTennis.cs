using Mirror;
using UnityEngine;

public class MovementTennis : Movement
{
    public bool slowMotion;
    bool slowMotionB;
    bool inside = true;
    bool hitWasPredicted;
    bool hitBall;

    int random = -1;
    int predictions;
    int wait;

    float delayServe = 0.833f;
    float delay = 0.833f * .5f;

    public Vector3 getPos;
    public Vector3 servePos;
    public Vector3 serveRot;
    public Vector3 getRot;
    Vector3 baseForce = new(12500, 7500, 0);
    Vector2 ballDirection;
    Vector3 ballDirectionB;

    public GameObject target;
    public GameObject BallArea;

    public TrajectoryPrediction tp;
    public TennisBall TB;
    MovementTennis opponentMovement;

    public PlayerState state;


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        opponentMovement = GameObject.Find("%Player" + (1 - playerID)).GetComponent<MovementTennis>();
        tp = FindObjectOfType<TrajectoryPrediction>();

        ServerOrWait();
    }

    public void ServerOrWait(int randomPlayer = -1)
    {
        if (playerID == 0 | NetworkClient.active)
        {
            if (randomPlayer == -1)
            {
                random = Random.Range(0, 2);
                if (functions.scoreWinner != -1)
                {
                    random = functions.scoreWinner;
                }
            }
            else { random = randomPlayer; }

            if (NetworkClient.active)
            {
                if (playerID != 0) { return; }
                status.sync.ServeOrWaitNetwork(random);
            }

            opponentMovement.GetComponent<MovementTennis>().random = random;

            GameObject ServingPlayer = GameObject.Find("%Player" + random);
            GameObject WaitingPlayer = GameObject.Find("%Player" + (1 - random));

            MovementTennis ServingPlayerMovement = ServingPlayer.GetComponent<MovementTennis>();
            MovementTennis WaitingPlayerMovement = WaitingPlayer.GetComponent<MovementTennis>();

            ServingPlayer.transform.SetPositionAndRotation(new(ServingPlayerMovement.servePos.x, ServingPlayer.transform.position.y, ServingPlayerMovement.servePos.z), Quaternion.Euler(ServingPlayerMovement.serveRot));
            ServingPlayerMovement.state = PlayerState.serving;
            FindObjectOfType<Methods>().SetHint(true, "Serve", new int[] { 0 }, ServingPlayerMovement.playerID);
            ServingPlayerMovement.freezeMH = true;

            WaitingPlayer.transform.SetPositionAndRotation(new(WaitingPlayerMovement.getPos.x, WaitingPlayer.transform.position.y, WaitingPlayerMovement.getPos.z), Quaternion.Euler(WaitingPlayerMovement.getRot));
            WaitingPlayerMovement.state = PlayerState.waitingForServe;
            WaitingPlayerMovement.freezeMH = true;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!isDummy)
        {
            switch (state)
            {
                case PlayerState.serving:
                    {
                        bool predict = (functions.movementLock == false && !hitBall && move != Vector3.zero);
                        if (predictions < 10 || predict)
                        {
                            PredictServe();
                            predictions++;
                        }

                        if (mV > 0.25f && !functions.movementLock)
                        {
                            TB.Serve(playerID, false);
                            am.SetBool("isBallUp", true);
                            state = PlayerState.perfomingServe;
                        }

                        break;
                    }

                case PlayerState.waitingForServe:
                    {
                        if (TB.state == TennisBall.BallState.InAir)
                        {
                            state = PlayerState.ballInTheAir;
                        }
                        break;
                    }

                case PlayerState.perfomingServe:
                    {
                        am.SetBool("isWalking", false);
                        delayServe += -Time.deltaTime;
                        tp.DisableUI();

                        break;
                    }

                case PlayerState.ballInTheAir:
                    {
                        freezeMH = false;
                        freezeMV = false;

                        if (NetworkClient.active && TB.state == TennisBall.BallState.InAir)
                        {
                            tp.DisableUI();
                        }

                        break;
                    }

                case PlayerState.hitball:
                    {
                        freezeMH = true;
                        freezeMV = true;

                        delay += -Time.deltaTime;
                        am.SetBool("isWalking", false);
                        break;
                    }
            }

            if (delayServe < 0 && state == PlayerState.perfomingServe)
            {
                LeanTween.rotateY(gameObject, serveRot.y + 10, 0.5f);
                FindObjectOfType<Methods>().SetHint(false, "Serve", null, playerID);
                state = PlayerState.ballInTheAir;
                am.SetBool("isBallUp", false);
            }

            //-----------------------------------

            if (BallArea.GetComponent<Collider>().bounds.Contains(TB.transform.position) && TB.state == TennisBall.BallState.InAir && !inside && state == PlayerState.ballInTheAir)
            {
                inside = true;
                TB.target = target.transform.position;
                TB.hitTeam = playerID;
                hitBall = true;
                TB.state = TennisBall.BallState.CalculateHit;
                if (NetworkClient.active)
                {
                    status.sync.BallCalculateHit(target.transform.position, playerID);
                }
                am.SetBool("isBall", true);
                state = PlayerState.hitball;
                print("gotBall" + playerID + state);
            }

            if (!BallArea.GetComponent<Collider>().bounds.Contains(TB.transform.position))
            {
                inside = false;
            }

            if (delay < 0.25f && delay > 0.15f && !slowMotion)
            {
                slowMotion = true;
                ballDirection = Vector2.zero;
            }
            if (delay < 0.15f && slowMotion) { slowMotion = false; hitWasPredicted = false; wait = 0; }

            if (!pause.freeze && slowMotionB && !slowMotion)
            {
                if (NetworkClient.active) { status.sync.SetTimeScale(1f); }
                Time.timeScale = 1f;
            }

            if (slowMotion)
            {
                if (Time.timeScale == 1)
                {
                    if (NetworkClient.active) { status.sync.SetTimeScale(0.05f); }
                    Time.timeScale = 0.05f;
                }

                if (wait == 0)
                {
                    wait++;
                }
                else
                {
                    float mV2;
                    if (ballDirection.y < 0 && mV < 0) { mV2 = mV * 1.5f; }
                    else { mV2 = mV; }
                    ballDirection += (new Vector2(mH, mV2 * 0.5f) * Time.deltaTime);
                }

                TB.ballDirection = ballDirection;
                Vector3 ballDirectionPrediction = ballDirection;
                if (!ballDirection.Equals(ballDirectionB) | !hitWasPredicted)
                {
                    if (playerID == 0)
                    {
                        ballDirectionPrediction = new(ballDirectionPrediction.y * 200000 * TB.BallSpeed, baseForce.y, -ballDirectionPrediction.x * 150000 * TB.BallSpeed);
                        ballDirectionPrediction.x += baseForce.x * TB.BallSpeed;
                    }
                    else
                    {
                        ballDirectionPrediction = new Vector3(-ballDirectionPrediction.y * 200000 * TB.BallSpeed, baseForce.y, ballDirectionPrediction.x * 150000 * TB.BallSpeed);
                        ballDirectionPrediction.x += -baseForce.x * TB.BallSpeed;
                    }
                    tp.Predict(target.transform.position, ballDirectionPrediction, playerID);
                    hitWasPredicted = true;
                }

                ballDirectionB = ballDirection;
            }

            if (delay < 0.1 && TB.state == TennisBall.BallState.Hit)
            {
                TB.state = TennisBall.BallState.PerformHit;
                if (NetworkClient.active) { status.sync.BallSetState(TB.state); }
            }

            if (delay < 0)
            {
                hitBall = false;
                am.SetBool("isBall", false);
                state = PlayerState.ballInTheAir;
                tp.DisableUI();
                delay = 0.833f * .5f;
            }
            slowMotionB = slowMotion;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void OnReset()
    {
        ServerOrWait();
        TB.SetUp();

        functions.ScorePs.Clear();
        functions.ScorePs.Stop();

        random = -1;
        predictions = 0;
        delayServe = 0.833f;
    }

    private void PredictServe()
    {
        Vector3 force = new(12500 * TB.BallSpeed, 7500, 5000 * TB.BallSpeed);
        Vector3 ballPos = new(transform.position.x + 1, transform.position.y + 7.2f, transform.position.z - 5);

        if (playerID == 1)
        {
            force.x = -force.x;
            force.z = -force.z;
            ballPos.x += 2;
            ballPos.z += 10;
        }

        TB.gameObject.transform.position = ballPos;
        tp.Predict(TB.transform.position, force, playerID);
    }

    // private void PowerUp()
    //{
    //if (PowerUpScript.powerActive)
    //{
    //    //if (PowerUpScript.powerPlayerID == playerID)
    //    //{
    //    //    playerSpeed = playerSpeedStart * (1 + powerStrenght);
    //    //}
    //    //else
    //    //{
    //    //    playerSpeed = playerSpeedStart * (1 - powerStrenght);
    //    //}
    //}

    //if (!PowerUpScript.powerActive)
    //{
    //    baseForce = new Vector3(12500, 7500, 0);
    //    playerSpeed = playerSpeedStart;
    //}
    // }

    public enum PlayerState
    {
        serving,
        waitingForServe,
        ballInTheAir,
        hitball,
        perfomingServe
    }
}
