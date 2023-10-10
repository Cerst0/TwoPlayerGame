using Mirror;
using UnityEngine;

public class MSMovement : Movement
{
    public Material[] GoalM;
    float yPosB;

    public GameObject Ball;
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        yPosB = transform.position.y;
        base.Start();

        if (isTeamLeader)
        {
            GoalM[team].color = IDA.PlayerColor;
        }
        if (!NetworkClient.active)
        {
            Ball.SetActive(true);
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void Update()
    {
        base.Update();
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.y = yPosB;
        transform.position = pos;
        yPosB = transform.position.y;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Ball" && !isDummy)
        {
            if (NetworkClient.active) { GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().ServerKick(gameObject.transform.position, velocity); print("Client apply serverkick"); }
            else { collision.collider.gameObject.GetComponent<KickBall>().Kick(gameObject.transform.position, velocity); }
        }
    }

    public override void OnReset()
    {
        base.OnReset();

        if (playerID == 0)
        {
            Ball.transform.position = new Vector3(0, 2.2f, 0);
            Ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    public override void OnPowerUp()
    {
        base.OnPowerUp();
    }
}
