using Mirror;
using UnityEngine;

public class PlayerMovementParkour : Movement
{
    public Vector3 explosion;
    public float explosionSmoothness;
    public Material[] Arch;
    public int LevelNum;
    bool resumeSound;
    bool reachedCheckpoint;
    int ownLayer;

    Checkpoint checkpoint;
    AudioSource Check;

    protected override void Awake()
    {
        if (playerID == 0)
        {
            int levelID = Random.Range(0, 4);

            FindObjectOfType<HighScore>().levelID = levelID;

            if (!NetworkClient.active)
            {
                foreach (GameObject level in GameObject.FindGameObjectsWithTag("Level"))
                {
                    if (!level.name.Contains(levelID.ToString()))
                    {
                        Destroy(level);
                    }
                }
            }
            if (NetworkClient.active)
            {
                if (GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<NetworkIdentity>().playerID == 0) //status ins't spawned yet
                {
                    GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().InstantiateLevel(levelID);
                }
            }
        }

        base.Awake();
    }

    protected override void Start()
    {
        transform.position = new Vector3(transform.position.x, 5.75f, transform.position.z);
        base.Start();

        ownLayer = gameObject.layer;

        Physics.IgnoreLayerCollision(12, ownLayer, false);

        if (isTeamLeader)
        {
            Arch[team].color = FindObjectOfType<Methods>().GetPlayerColor(team);
            if (status.playerNumber == 1)
            {
                foreach (Material mat in Arch)
                {
                    mat.color = FindObjectOfType<Methods>().GetPlayerColor(team);
                }
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void Update()
    {
        explosion = Vector3.Lerp(explosion, Vector3.zero, explosionSmoothness);
        additionalMoveVector = explosion;

        if (FindObjectOfType<Checkpoint>() != null)
        {
            checkpoint = FindObjectOfType<Checkpoint>();
            Check = GameObject.Find("CheckSound").GetComponent<AudioSource>();
        }

        base.Update();

        if (transform.position.y < 5.35f)
        {
            Physics.IgnoreLayerCollision(12, ownLayer, true);
        }

        if (transform.position.y < -10f)
        {
            print(name + " respawn at checkpoint " + transform.position);
            velocity = Vector3.zero;
            explosion = Vector3.zero;
            Physics.IgnoreLayerCollision(12, ownLayer, false);
            OnReset();
        }

        if (pause.isPause && !resumeSound && Check.isPlaying) { Check.Pause(); resumeSound = true; }
        if (!pause.isPause && resumeSound) { Check.Play(); resumeSound = false; }
    }

    public void OnCheckpoint()
    {
        if (reachedCheckpoint == true) { return; }
        reachedCheckpoint = true;
        Check.Play();
        firstPos = transform.position;

        n.NotificationWith2Colors(new Color(1, 0.67f, 0.086f, 0), 2f, functions.IDAs[playerID].playerName, "reached Checkpoint", methods.GetPlayerColor(playerID));

        checkpoint.SetCheckpoint(playerID);
    }

    public override void OnReset()
    {
        base.OnReset();
    }

    public override void OnPowerUp()
    {
        base.OnPowerUp();
    }
}
