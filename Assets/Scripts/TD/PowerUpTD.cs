using UnityEngine;

public class PowerUpTD : MonoBehaviour
{
    public int maxRandomFactor;
    public Vector2 SpawnCorner1;
    public Vector2 SpawnCorner2;
    float y;

    public GameObject powerUp;
    public GameObject powerUpG;
    Status status;


    AudioSource powerUpSound;

    public bool powerActive = false;
    bool isVisible = false;
    float countdownPower = 0f;
    float countdownPowerBefore = 3f;
    float countdownBefore = 3f;
    float countdown = 0f;
    string psState = "off";
    string psPowerState = "off";
    ParticleSystem psPlayer;
    Viking viking;

    // Start is called before the first frame update
    void Start()
    {
        powerUpSound = GetComponent<AudioSource>();
        status = GameObject.FindGameObjectWithTag("Status").GetComponent<Status>();
        powerUp.SetActive(false);
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<SphereCollider>().enabled = false;
        y = powerUpG.transform.position.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Random.Range(1, maxRandomFactor) == 1 && !powerActive && !isVisible && !status.scoredPoint && !status.scoredScore && !status.movementLock)
        {
            powerUpG.transform.position = new Vector3(Random.Range(SpawnCorner1.x, SpawnCorner2.x), y, Random.Range(SpawnCorner1.y, SpawnCorner2.y));
            LeanTween.moveY(powerUpG, y + 0.75f, 0.75f).setLoopPingPong();
            print("power");
            powerUp.SetActive(true);
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<SphereCollider>().enabled = true;
            countdown = 10;
            isVisible = true;
        }
    }

    private void Update()
    {
        if (powerActive)
        {
            countdownPower -= Time.deltaTime;

            if (countdownPower < 2f && countdownPowerBefore > countdownPower + 0.15f && psPowerState == "on")
            {
                psPlayer.Stop();
                countdownPowerBefore = countdownPower;
                psPowerState = "off";
            }
            if (countdownPower < 2f && countdownPowerBefore > countdownPower + 0.15f && psPowerState == "off")
            {
                psPlayer.Play();
                countdownPowerBefore = countdownPower;
                psPowerState = "on";
            }
        }
        else
        {
            countdownPowerBefore = 3f;
        }

        if (isVisible)
        {
            countdown -= Time.deltaTime;

            if (countdown < 3f && countdownBefore > (countdown + 0.15f) && psState == "on" && !powerActive)
            {
                countdownBefore = countdown;
                psState = "off";
                GetComponent<MeshRenderer>().enabled = false;
            }
            if (countdown < 3f && countdownBefore > (countdown + 0.15f) && psState == "off" && !powerActive)
            {
                countdownBefore = countdown;
                psState = "on";
                GetComponent<MeshRenderer>().enabled = true;
            }
        }
        else
        {
            LeanTween.pause(powerUpG);
            countdownBefore = 3f;
        }

        if (countdownPower < 0 && powerActive)
        {
            isVisible = false;
            powerUp.SetActive(false);
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<SphereCollider>().enabled = false;

            psPlayer.Stop();
            powerActive = false;
            viking.power = false;
        }

        if (countdown < 0 && isVisible)
        {
            isVisible = false;
            powerUp.SetActive(false);
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<SphereCollider>().enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.name);
        if (other.gameObject.name.Contains("Viking"))
        {
            viking = other.gameObject.GetComponent<Viking>();
            viking.power = true;
            psPlayer = other.gameObject.transform.Find("PS").GetComponent<ParticleSystem>();
            powerUpSound.Play();
            powerActive = true;
            countdownPower = 10f;
            powerUp.SetActive(false);
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<SphereCollider>().enabled = false;
            psPlayer.Play();
        }
    }
}
