using Mirror;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Custom")]
    [Range(1, 100)]
    public float SpawnChange;//default per second
    public bool perMinute;
    public int activeTime;
    public Vector2 SpawnCorner1;
    public Vector2 SpawnCorner2;
    public AnimationCurve DissapearDeltaTime;
    public Gradient PowerUpBarColor;

    [Header("Other")]
    public int PowerUpIndex = -1;
    float disappearTime;
    bool isVisible;
    bool isDisappearing;
    bool isDummy;

    GameObject obj;
    ParticleSystem Ps;
    Functions functions;
    Status status;

    private void Start()
    {
        functions = FindObjectOfType<Functions>();
        status = FindObjectOfType<Status>();
        obj = transform.GetChild(0).gameObject;
        Ps = transform.GetChild(1).GetComponent<ParticleSystem>();
        Despawn();

        obj.transform.localPosition = new Vector2(0, -0.1f);
        LeanTween.moveLocalY(obj, 0.1f, .5f).setLoopPingPong().setEaseOutQuad();
        if (status.clientPlayerID > 0) { isDummy = true; }
    }

    private void FixedUpdate()// called every 0.2seconds
    {
        if (!isDummy)
        {
            RandomSpawn();
        }
    }

    private void Update()
    {
        obj.transform.Rotate(Vector3.up * Time.deltaTime * 75);

        if (isDisappearing)
        {
            disappearTime += -Time.deltaTime;
        }
    }

    private void RandomSpawn()
    {
        float range;
        float max = 100 * 100;
        range = SpawnChange * 100;
        range /= 50;
        if (perMinute)
        {
            max *= 60;
        }

        if (!isVisible && !functions.movementLock && Random.Range(0, max) < range)
        {
            print("spawn powerup with index: " + PowerUpIndex + "; max: " + range);

            if (NetworkClient.active) { GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().SpawnPowerUp(PowerUpIndex); }
            else { Spawn(); }
        }
    }

    public void Spawn()
    {
        Vector3 RandomPos = new Vector3(Random.Range(SpawnCorner1.x, SpawnCorner2.x), transform.position.y, (Random.Range(SpawnCorner1.y, SpawnCorner2.y)));
        transform.position = RandomPos;

        obj.transform.localScale = Vector3.zero;
        LeanTween.scale(obj, new Vector3(50, 50, 50), .25f).setEaseOutQuad();
        obj.SetActive(true);

        float r = .3f;
        ParticleSystem.ShapeModule ps = Ps.shape;
        ps.radius = r;
        Ps.Play();

        Invoke("Disappear", activeTime);

        disappearTime = 3;
        isVisible = true;
    }

    private void Despawn()
    {
        obj.SetActive(false);
        Ps.Stop();

        isDisappearing = false;
        isVisible = false;
    }

    private void Disappear()
    {
        isDisappearing = true;
        obj.SetActive(!obj.activeSelf);

        if (disappearTime > 0)
        {
            float f = DissapearDeltaTime.Evaluate(1 - disappearTime);
            Invoke("Disappear", f);
        }
        else
        {
            Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print(other.name);
        PlayerUsePowerUp(other);
    }

    public void PlayerUsePowerUp(Collider other, int IDFromServer = -1)
    {
        if (IDFromServer == -1)
        {
            if (other.tag == "Player" && !other.GetComponent<Movement>().isDummy && isVisible)
            {
                if (NetworkClient.active) { GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().SetPowerUpPlayerServer(other.GetComponent<Movement>().playerID, PowerUpIndex); }
                else { other.GetComponent<Movement>().OnPowerUp(); }

                CancelInvoke();
                Despawn();
            }
        }
        else
        {
            functions.Players[IDFromServer].GetComponent<Movement>().OnPowerUp();

            CancelInvoke();
            Despawn();
        }
    }
}
