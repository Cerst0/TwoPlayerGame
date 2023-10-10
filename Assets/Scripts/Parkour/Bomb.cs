using Mirror;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public int BombID;
    public float delay;
    public float delayRed;
    public float duration;

    public float delayR;
    public float delayRedR;
    public float durationR;

    public float force;
    public float frequence;
    public float gravity;
    public bool randomZ;
    public bool randomX;
    public bool randomT;
    public float delayMax;
    public bool useZone;
    float gradientTime;
    public int state;
    bool resumeSound;
    bool isVisible;
    Vector3[] PlayerForces = new Vector3[4];

    public GameObject Model;
    public Gradient g;
    public ParticleSystem ps;
    public AudioSource asExplosion;

    Rigidbody rb;
    Material m;
    Pause pause;
    Status status;
    Functions functions;

    private void Start()
    {
        pause = GameObject.FindGameObjectWithTag("Brain").GetComponent<Pause>();
        status = FindObjectOfType<Status>();
        functions = FindObjectOfType<Functions>();
        rb = gameObject.GetComponent<Rigidbody>();
        GetComponent<CapsuleCollider>().material.bounciness = 0.5f;
        m = Model.GetComponent<Renderer>().material;
        isVisible = false;
        GetComponent<CapsuleCollider>().enabled = true;
        transform.position = new Vector3(transform.position.x, 20, transform.position.z);
        Model.transform.localScale = Vector3.zero;

        Randomise();

        delayR = delay;
        delayRedR = delayRed;
        durationR = duration;

        Reset();
    }

    private void Randomise()
    {
        if (randomX)
        {
            transform.position = new Vector3(Random.Range(-6.7f, 5.2f), transform.position.y, transform.position.z);
        }

        if (randomZ)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, Random.Range(50f, 230f));
        }

        if (randomT)
        {
            delay = Random.Range(0f, delayMax);
        }
    }

    private void Update()
    {
        LayerMask mask = LayerMask.GetMask("Bomb");
        if (status.clientPlayerID == 0 | status.clientPlayerID == -1)
        {
            if (!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out RaycastHit hit, Mathf.Infinity, mask) && state == 1 && randomZ && useZone)
            {
                transform.position = new Vector3(Random.Range(-6.7f, 5.2f), gameObject.transform.position.y, Random.Range(50f, 230f));
                //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down), Color.yellow);
            }
        }

        if ((NetworkClient.active && status.clientPlayerID == 0) | !NetworkClient.active)
        {
            if (state == 3 && duration < 0f)
            {
                state = 4;
            }
        }

        if (state == 0)
        {
            if (status.clientPlayerID == 0 | status.clientPlayerID == -1)
            {
                Invoke(nameof(Spawn), delay);
            }
            state = 1;
        }

        if (state == 3)
        {
            duration += -Time.deltaTime;
            gradientTime += Time.deltaTime * frequence;
            if (gradientTime > 1f) { gradientTime = 0f; }
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", g.Evaluate(gradientTime));
        }

        if (state == 4)
        {
            if (status.clientPlayerID == 0 | status.clientPlayerID == -1)
            {
                Explode();
            }
        }

        if (pause.isPause && !resumeSound) { asExplosion.Pause(); resumeSound = true; }
        if (!pause.isPause && resumeSound) { asExplosion.Play(); resumeSound = false; }
    }

    private void Spawn()
    {
        if (status.clientPlayerID == 0)
        {
            GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().BombInvoke("Spawn", BombID, transform.position);
        }

        Reset();
        state = 2;
        if (!isVisible) { LeanTween.scale(Model, Vector3.one * 15, 0.25f).setEaseOutCubic(); isVisible = true; }

        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezePositionX;
        rb.constraints = RigidbodyConstraints.FreezePositionZ;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        Invoke(nameof(SetState3), delayRed);
    }

    private void Explode()
    {
        if (status.clientPlayerID == 0)
        {
            GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().BombInvoke("Explode", BombID, transform.position);
        }

        ps.Play();
        asExplosion.Play();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        Model.transform.localScale = Vector3.zero;

        float[] distances = new float[status.playerNumber];

        for (int i = 0; i < status.playerNumber; i++)
        {
            distances[i] = Vector3.Distance(functions.Players[i].transform.position, transform.position);

            if (distances[i] < 20f)
            {
                Vector3 direction = -(transform.position - functions.Players[i].transform.position).normalized;
                Vector3 vector = direction;
                vector *= force;
                vector *= 20 - distances[i];
                PlayerForces[i] = vector;
                //print("bomb explosion hit Player" + i + " vector = " + vector + "distance: " + distances[i]);
            }
        }

        Invoke(nameof(Reset), 2);
        state = 5;
    }

    private void FixedUpdate()
    {
        if (isVisible)
        {
            rb.AddForce(new Vector3(0, gravity, 0));
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < status.playerNumber; i++)
        {
            if (PlayerForces[i] != Vector3.zero)
            {
                functions.Players[i].GetComponent<PlayerMovementParkour>().explosion = PlayerForces[i];
                PlayerForces[i] = Vector3.zero;
            }
        }
    }

    private void Reset()
    {
        delay = delayR;
        delayRed = delayRedR;
        isVisible = false;
        duration = durationR;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        if (NetworkClient.active && status.clientPlayerID == 0)
        {
            state = 0;
        }
        transform.position = new Vector3(transform.position.x, 20, transform.position.z);
        m.SetColor("_EmissionColor", Color.black);
        gameObject.GetComponent<CapsuleCollider>().enabled = true;
        ps.Stop();

        if (state == 5)
        {
            Randomise();

            state = 0;
        }
    }

    public void SetState3()
    {
        state = 3;
    }
}
