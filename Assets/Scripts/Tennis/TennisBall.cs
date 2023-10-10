using Mirror;
using UnityEngine;

public class TennisBall : MonoBehaviour
{
    public BallState state;
    float serveDelay;
    public Rigidbody rb;
    public bool trigger;
    public Vector3 target;
    public Vector3 ballDirection;
    public float BallSpeed;
    float speed;
    public bool ballOut;
    int touches;
    public bool served;
    public int hitTeam;
    public int teamSide;
    public Functions functions;
    public AudioSource Swing;
    public AudioSource Hit;
    public AudioSource Bounce;
    Status status;

    private void Start()
    {
        SetUp();
    }

    public void SetUp()
    {
        status = FindObjectOfType<Status>();

        GetComponent<PowerUpRotation>().enabled = false;
        Terrain.activeTerrain.detailObjectDistance = 1000;
        rb = gameObject.GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeAll;
        gameObject.GetComponent<MeshRenderer>().enabled = false;

        serveDelay = 0.54145f;
        state = BallState.Invisible;
        touches = 0;
        ballOut = false;
        served = false;
    }

    private void Update()
    {
        switch (state)
        {
            case BallState.Serve:
                {
                    gameObject.GetComponent<MeshRenderer>().enabled = true;
                    serveDelay += -Time.deltaTime;

                    if (serveDelay < 0)
                    {
                        served = true;
                        rb.constraints = RigidbodyConstraints.None;

                        Hit.Play();
                        if (hitTeam == 0)
                        {
                            AddForce(new Vector3(12500 * BallSpeed, 7500, 5000 * BallSpeed), false);
                        }
                        else
                        {
                            AddForce(new Vector3(-12500 * BallSpeed, 7500, -5000 * BallSpeed), false);
                        }

                        GetComponent<PowerUpRotation>().enabled = true;
                        print("Served");
                        state = BallState.InAir;
                    }

                    break;
                }

            case BallState.InAir:
                {
                    if (trigger)
                    {
                        print("HitGround" + hitTeam + " " + teamSide + " " + touches + " " + ballOut);
                        rb.velocity = new Vector3(rb.velocity.x * 0.75f, 50, rb.velocity.z * 0.75f);
                        Bounce.Play();

                        if (touches > 0)
                        {
                            functions.AddScore(2, 5, hitTeam);
                        }

                        if ((ballOut | teamSide == hitTeam) && touches == 0)
                        {
                            if (hitTeam == 0)
                            {
                                functions.AddScore(2, 5, 1);
                            }
                            else
                            {
                                functions.AddScore(2, 5, 0);
                            }
                        }

                        touches++;
                        trigger = false;
                    }

                    break;
                }

            case BallState.CalculateHit:
                {
                    speed = 3.5f * Vector3.Distance(transform.position, target);
                    //print(Vector3.Distance(transform.position, target));
                    rb.isKinematic = true;
                    state = BallState.Hit;
                    touches = 0;

                    break;
                }

            case BallState.Hit:
                {
                    transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);

                    break;
                }

            case BallState.PerformHit:
                {
                    Hit.Play();
                    rb.velocity = Vector3.zero;

                    if (hitTeam == 0)
                    {
                        ballDirection = new Vector3(ballDirection.y * 200000 * BallSpeed, 7500, -ballDirection.x * 150000 * BallSpeed);
                        ballDirection.x += 12500 * BallSpeed;
                    }
                    else
                    {
                        ballDirection = new Vector3(-ballDirection.y * 200000 * BallSpeed, 7500, ballDirection.x * 150000 * BallSpeed);
                        ballDirection.x += -12500 * BallSpeed;
                    }

                    state = BallState.InAir;

                    if (NetworkClient.active && status.sync.movement.GetComponent<MovementTennis>().state == MovementTennis.PlayerState.ballInTheAir) { break; } // if not the client, don't do anything
                    AddForce(ballDirection, false);
                    print("Hit" + ballDirection);

                    break;
                }
        }

        if (hitTeam == 0)
        {
            GetComponent<PowerUpRotation>().reverse = false;
        }
        else { GetComponent<PowerUpRotation>().reverse = true; }
    }

    public void Serve(int id, bool calledByServer)
    {
        if (NetworkClient.active && !calledByServer) { status.sync.TennisServe(id); return; }

        Vector3 playerPos = functions.Players[id].transform.position;
        if (id == 1)
        {
            gameObject.transform.position = new Vector3(playerPos.x - 1, playerPos.y + 7f, playerPos.z + 4.8f);
        }
        else
        {
            gameObject.transform.position = new Vector3(playerPos.x + 1, playerPos.y + 7f, playerPos.z - 4.8f);
        }
        hitTeam = id;
        state = TennisBall.BallState.Serve;
        Swing.PlayDelayed(0.4f);
    }

    public void AddForce(Vector3 force, bool calledByServer)
    {
        if (NetworkClient.active)
        {
            if (hitTeam != status.clientPlayerID && !calledByServer) { return; }
            if (!calledByServer) { status.sync.BallAddForce(force); return; }
        }

        rb.isKinematic = false;
        state = BallState.InAir;
        print("AddForce" + force);
        rb.AddForce(force, ForceMode.Impulse);
    }

    public enum BallState
    {
        Invisible,
        Serve,
        CalculateHit,
        Hit,
        PerformHit,
        InAir
    }
}


