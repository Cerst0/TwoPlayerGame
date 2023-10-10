using Mirror;
using TMPro;
using UnityEngine;

public class Viking : NetworkBehaviour
{
    [SyncVar] public string id;
    public bool isViking;
    public float speed;
    public float dis;
    public Vector3 hitVec;
    public float attackStrenght;
    private float startAttackStrenght;

    public GameObject Ball;
    public Light Light;
    public ParticleSystem psFire;
    public TextMeshProUGUI text;
    private Animator am;
    private Vector3 BallStart;

    private float attackCoolDown = 0.1f;
    public string team;
    public int teamInt;
    public string oppositeTeam;
    private string HitName;
    private float HitDis;
    public float health;
    private float startSpeed;
    private float deathCoolDown = 0.1f;
    private bool walk;
    private Vector3 startVel;
    private float currentTime;
    private bool stand;
    public bool power;
    private bool attack;
    private bool isAttackingCastle;
    private string oppositeVikingID;

    private Functions functions;
    private Status status;

    private void Start()
    {
        functions = FindObjectOfType<Functions>();
        status = FindObjectOfType<Status>();

        text.SetText(string.Empty);
        if (gameObject.name.Contains("Red"))
        {
            team = "Red";
            oppositeTeam = "Blue";
        }
        else
        {
            team = "Blue";
            oppositeTeam = "Red";
        }
        am = GetComponent<Animator>();

        if (!isViking)
        {
            Light.enabled = false;
            Ball.SetActive(false);
            BallStart = Ball.transform.localPosition;
        }
        startSpeed = speed;
        startAttackStrenght = attackStrenght;

        SetVikingColor();

        gameObject.SetActive(true);
    }

    private void SetVikingColor()
    {
        SkinnedMeshRenderer SMR = transform.Find("Viking").GetComponent<SkinnedMeshRenderer>();

        SMR.materials[0].color = FindObjectOfType<Methods>().GetPlayerColor(teamInt);
        SMR.materials[4].color = FindObjectOfType<Methods>().GetPlayerColor(teamInt);
    }

    private void Update()
    {
        if (power)
        {
            speed = startSpeed * 1.5f;
            attackStrenght = startAttackStrenght * 1.5f;
        }
        else
        {
            speed = startSpeed;
            attackStrenght = startAttackStrenght;
        }

        if ((team == "Red" && transform.position.x < -18.5 && isViking) | (team == "Blue" && transform.position.x > 18.5 && isViking) | (team == "Red" && transform.position.x < -3.5 && !isViking) | (team == "Blue" && transform.position.x > 3.5 && !isViking))
        {
            attack = true;
            walk = false;
            stand = false;
            isAttackingCastle = true;
        }
        else { isAttackingCastle = false; }

        if (Physics.Raycast(transform.position, hitVec, out RaycastHit hit))
        {
            HitName = hit.collider.gameObject.name;
            HitDis = hit.distance;

            bool containViking = HitName.Contains("Viking");
            bool distanceIsGreater = (hit.distance > dis && isViking) | (hit.distance > 8 && !isViking);
            if (((containViking && distanceIsGreater) | !containViking) && !isAttackingCastle)
            {
                attack = false;
                walk = true;
                stand = false;
            }
            if (hit.collider.gameObject.name.Contains("Viking" + oppositeTeam) && hit.distance < dis)
            {
                attack = true;
                walk = false;
                stand = false;

                oppositeVikingID = hit.collider.gameObject.GetComponent<Viking>().id;
            }
            else { oppositeVikingID = null; }
            if (hit.collider.gameObject.name.Contains("Viking" + team) && ((hit.distance < dis && isViking) | (hit.distance < 8 && !isViking)))
            {
                attack = false;
                walk = false;
                stand = true;
            }
            Debug.DrawLine(transform.position, hit.point);
            //print(name + hit.collider.name + containViking + distanceIsGreater + "   " + attack + walk + stand);
        }
        else
        {
            attack = false;
            walk = true;
            stand = false;
        }

        if (walk)
        {
            am.SetBool("attack", false);
            am.enabled = true;
            transform.position = new Vector3(transform.position.x + (speed * Time.deltaTime), transform.position.y, transform.position.z);
        }
        if (stand)
        {
            if (isViking) { am.enabled = false; }
            else { am.SetBool("attack", true); }
        }
        if (attack)
        {
            if (isViking)
            {
                AnimatorStateInfo animState = am.GetCurrentAnimatorStateInfo(0);
                currentTime = animState.normalizedTime % 1;

                if (currentTime > 0.95f && attackCoolDown < 0f && status.clientPlayerID < 1)
                {
                    if (isAttackingCastle)
                    {
                        HitCastle();
                    }
                    else
                    {
                        if (oppositeVikingID != null)
                        {
                            if (NetworkClient.active)
                            {
                                status.sync.VikingHurt(attackStrenght, oppositeVikingID);
                                print("hurt" + oppositeVikingID);
                            }
                            else
                            {
                                FindVikingByID(oppositeVikingID).OnHurt(attackStrenght);
                            }
                        }
                    }
                    attackCoolDown = 0.1f;
                }
            }
            else
            {
                if (currentTime == 0)
                {
                    GetComponent<AudioSource>().Play();
                    Ball.GetComponent<SphereCollider>().isTrigger = true;
                    Ball.transform.localPosition = BallStart;
                    Ball.SetActive(true);
                    Light.enabled = true;
                    psFire.Play();
                    Ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    LeanTween.value(gameObject, 100, 0, 0.55f).setOnUpdate((float val) =>
                    {
                        Light.intensity = val;
                    });
                    LeanTween.value(gameObject, 4, 0, 0.55f).setOnUpdate((float val) =>
                    {
                        Light.range = val;
                    });
                    if (isAttackingCastle)
                    {
                        Ball.GetComponent<Rigidbody>().AddForce(new Vector3(hitVec.x * 100, 50, 0));
                    }
                    else
                    {
                        float x;
                        float y;
                        if (HitDis < 10)
                        {
                            x = hitVec.x * 100;
                            y = 50 * ((HitDis - 5) / 15);
                        }
                        else
                        {
                            if (HitName.Contains("Cannon") && hit.collider.gameObject.GetComponent<Viking>().isAttackingCastle)
                            {
                                x = hitVec.x * 500 * ((HitDis - 7.5f) / 15);
                                y = 50 * ((HitDis - 5) / 15);
                            }
                            else
                            {
                                x = hitVec.x * 150 * ((HitDis - 7.5f) / 15);
                                y = 40 * ((HitDis - 5) / 15);
                            }
                        }
                        Ball.GetComponent<Rigidbody>().AddForce(new Vector3(x, y, 0));
                    }
                }

                currentTime += Time.deltaTime * 0.5f;
                if (currentTime < 0.1f)
                {
                    startVel = Ball.GetComponent<Rigidbody>().velocity;
                }
                if (Mathf.Abs(Ball.GetComponent<Rigidbody>().velocity.x) > 15f)
                {
                    Ball.GetComponent<Rigidbody>().velocity = new Vector3(startVel.x * (1 - currentTime), Ball.GetComponent<Rigidbody>().velocity.y, Ball.GetComponent<Rigidbody>().velocity.z);
                }

                if (currentTime > 1f)
                {
                    currentTime = 0;
                }
            }

            attackCoolDown += -Time.deltaTime;
            am.enabled = true;
            am.SetBool("attack", true);
        }
        else
        {
            if (!isViking) { currentTime = 0f; Ball.SetActive(false); }
        }

        if (!isViking)
        {
            if (!walk) { transform.Find("Cannon").gameObject.GetComponent<Animator>().enabled = false; }
            else { transform.Find("Cannon").gameObject.GetComponent<Animator>().enabled = true; }
        }

        if (health <= 0)
        {
            if (deathCoolDown <= 0f)
            {
                Destroy(gameObject);
            }
            deathCoolDown += -Time.deltaTime;
        }
    }

    public void HitCastle()
    {
        int index = 0;
        if (team == "Blue") { index = 1; }
        if (status.playerNumber > 2 && team == "Blue") { index = 2; }

        if (NetworkClient.active)
        {
            status.sync.VikingHitCastle(attackStrenght, index);
        }
        else
        {
            functions.Players[index].GetComponent<MovementTD>().ReceiveHit(Mathf.RoundToInt(-attackStrenght));
        }
    }

    public void ShowText(int damage)
    {
        text.alpha = 1;
        text.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(text.gameObject.GetComponent<RectTransform>().anchoredPosition.x, -537);
        text.SetText("-" + damage.ToString());
        LeanTween.value(gameObject, 1, 0, 0.5f).setOnUpdate((float val) =>
        {
            text.alpha = val;
        });
        LeanTween.moveY(text.gameObject.GetComponent<RectTransform>(), -535, 0.5f);
    }

    public void OnHurt(float attackStrenght)
    {
        print(name + "got hurt; attackstrenght: " + attackStrenght);

        ShowText(Mathf.RoundToInt(attackStrenght));
        if (status.clientPlayerID < 1) health += -attackStrenght;
        print(health);
    }

    public static Viking FindVikingByID(string id)
    {
        Viking[] vikings = FindObjectsOfType<Viking>();
        foreach (Viking vik in vikings)
        {
            if (vik.id == id)
            {
                return vik;
            }
        }
        return null;
    }
}