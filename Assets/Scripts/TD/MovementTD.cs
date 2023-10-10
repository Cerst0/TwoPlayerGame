using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovementTD : Movement
{
    public float health;

    float CDViking;
    float CDVikingCannon;
    public Image[] CooldownVikingImage;
    public TextMeshProUGUI[] CDVikingText;
    public Image[] CooldownVikingCannonImage;
    public TextMeshProUGUI[] CDVikingCannonText;
    public Image[] CannonKey;
    public Image[] VikingKey;
    public Image[] HealthBar;
    public TextMeshProUGUI[] HealthBarText;
    public Gradient HealthBarColor;
    public Gradient HealthBarHitGradient;

    public GameObject[] Viking;
    public GameObject[] VikingCannon;
    public Gradient VikingG;
    public Gradient VikingGI;
    public Gradient CannonG;

    public Material[] CastleFlagMaterial;

    public AudioSource Spawn;

    ControllVikingType vikingType;

    bool bb;
    bool cc;
    float d;
    float e;

    int currentVikingID;

    public GameObject[] Gate;
    float GateProgress;
    public Mesh step1;
    public Mesh step2;
    public Mesh step3;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        if (isSingleTeam)
        {
            vikingType = ControllVikingType.Both;
        }
        else
        {
            if (playerID == 0 || playerID == 2)
            {
                vikingType = ControllVikingType.Viking;
            }
            else
            {
                vikingType = ControllVikingType.VikingCannon;
            }
        }

        CannonKey[team].enabled = false;
        VikingKey[team].enabled = false;

        CDViking = 3;
        CooldownVikingImage[team].fillAmount = 1;

        CDVikingCannon = 5;
        CooldownVikingCannonImage[team].fillAmount = 1;

        HealthBarText[team].alpha = 0;

        if (isTeamLeader)
        {
            CastleFlagMaterial[team].color = IDA.PlayerColor;
        }

        GameObject HelpLine = GameObject.Find("HelpLineCanvas" + playerID).transform.GetChild(0).gameObject;

        if (team == 1)
        {
            HelpLine.GetComponent<RectTransform>().anchoredPosition = new Vector2(-362.8f, -243.7f);
            HelpLine.GetComponent<RectTransform>().rotation = Quaternion.Euler(90, 0, 0);
        }

        HelpLine.GetComponent<Image>().color = IDA.PlayerColor;
        HelpLine.SetActive(true);

        if (isDummy && (teamMate == null || teamMate.isDummy))
        {
            CooldownVikingImage[team].transform.parent.gameObject.SetActive(false);
            CooldownVikingCannonImage[team].transform.parent.gameObject.SetActive(false);
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void Update()
    {
        base.Update();

        UpdateHealthBar();


        //Check Cooldowns----------------------------------
        if (!isDummy)
        {
            UpdateCooldowns();
        }


        //Spawning--------------------------------------------
        if (!functions.movementLock)
        {
            if (mH < -0.5f)
            {
                if (team == 1)
                {
                    SpawnViking();
                }
                else
                {
                    SpawnVikingCannon();
                }
            }

            if (mH > 0.5f)
            {
                if (team == 0)
                {
                    SpawnViking();
                }
                else
                {
                    SpawnVikingCannon();
                }
            }
        }
    }

    private void UpdateCooldowns()
    {
        if (vikingType == ControllVikingType.Viking || vikingType == ControllVikingType.Both)
        {
            CDViking += -Time.deltaTime;
            CooldownVikingImage[team].fillAmount = CDViking / 4;

            if (CDViking < 0f)
            {
                if (!status.useGamepads)
                {
                    VikingKey[team].enabled = true;
                }

                if (!bb)
                {
                    LeanTween.value(0, 1, .5f).setOnUpdate((float val) =>
                    {
                        CooldownVikingImage[team].gameObject.transform.parent.gameObject.GetComponent<Image>().color = VikingGI.Evaluate(val);
                    });
                    bb = true;
                }
                CDVikingText[team].SetText(" ");
            }
            else
            {
                string text = Mathf.RoundToInt(CDViking + 0.5f).ToString();
                CDVikingText[team].SetText(text);
                CDVikingText[team].text = text;
            }
        }
        if (vikingType == ControllVikingType.VikingCannon || vikingType == ControllVikingType.Both)
        {
            CDVikingCannon += -Time.deltaTime;
            CooldownVikingCannonImage[team].fillAmount = CDVikingCannon / 10;

            if (CDVikingCannon < 0f)
            {
                if (!status.useGamepads)
                {
                    CannonKey[team].enabled = true;
                }

                if (!cc)
                {
                    LeanTween.value(0, 1, .5f).setOnUpdate((float val) =>
                    {
                        CooldownVikingCannonImage[team].gameObject.transform.parent.gameObject.GetComponent<Image>().color = VikingGI.Evaluate(val);
                    });
                    cc = true;
                }
                CDVikingCannonText[team].SetText("");
            }
            else
            {
                string text = Mathf.RoundToInt(CDVikingCannon + 0.5f).ToString();
                CDVikingCannonText[team].SetText(text);
            }
        }
    }

    private void UpdateHealthBar()
    {
        if (!isTeamLeader) { return; }

        d += Time.deltaTime;
        e += Time.deltaTime;

        if (d < 0)
        {
            CooldownVikingCannonImage[team].color = CannonG.Evaluate(Mathf.Abs(d) / 0.5f);
        }
        if (e < 0)
        {
            float r = Mathf.Lerp(HealthBarHitGradient.Evaluate(Mathf.Abs(d) / 0.5f).r, HealthBarColor.Evaluate(health / 200).r, 0.5f);
            float g = Mathf.Lerp(HealthBarHitGradient.Evaluate(Mathf.Abs(d) / 0.5f).g, HealthBarColor.Evaluate(health / 200).g, 0.5f);
            float b = Mathf.Lerp(HealthBarHitGradient.Evaluate(Mathf.Abs(d) / 0.5f).b, HealthBarColor.Evaluate(health / 200).b, 0.5f);
            HealthBar[team].color = new Color(r, g, b);

            if (health <= 0)
            {
                HealthBarText[team].SetText("");
                HealthBar[team].fillAmount = health / 200;
            }
        }
        else
        {
            HealthBar[team].color = HealthBarColor.Evaluate(health / 200);
            HealthBar[team].transform.parent.GetComponent<Image>().fillAmount = health / 200;
        }

        if (health <= 0)
        {
            Dead();

            if (GateProgress == 0)
            {
                LeanTween.moveLocalY(Gate[team], 0.885762f, 5);
            }
            GateProgress += Time.deltaTime;

            if (GateProgress > 2.375f && GateProgress < 3.75f)
            {
                Gate[team].GetComponent<MeshFilter>().mesh = step1;
            }
            if (GateProgress > 3.75f && GateProgress < 5f)
            {
                Gate[team].GetComponent<MeshFilter>().mesh = step2;
            }
        }
    }

    private void SpawnViking()
    {
        if (CDViking > 0 | vikingType == ControllVikingType.VikingCannon) { return; }
        CDViking = 4f;

        VikingKey[team].enabled = false;
        Spawn.Play();
        bb = false;
        LeanTween.value(0, 1, .5f).setOnUpdate((float val) =>
        {
            CooldownVikingImage[team].color = VikingG.Evaluate(val);
        });

        InstantiateEntity(Viking[team]);
    }

    private void SpawnVikingCannon()
    {
        if (CDVikingCannon > 0 | vikingType == ControllVikingType.Viking) { return; }
        CDVikingCannon = 10f;

        CannonKey[team].enabled = false;
        Spawn.Play();
        cc = false;
        d = -0.5f;

        InstantiateEntity(VikingCannon[team]);
    }

    private void InstantiateEntity(GameObject Entity)
    {
        Vector3 pos = Entity.transform.position;
        pos.z = transform.position.z;
        Entity.transform.position = pos;

        string id = "PlayerID:" + playerID + "VikingID:" + currentVikingID;
        currentVikingID++;
        Entity.GetComponent<Viking>().id = id;


        if (NetworkClient.active)
        {
            GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().Spawn(Entity.name, pos, new string[] { id }, 0);
        }
        else
        {
            GameObject instantiated = Instantiate(Entity);
        }
    }

    private void Dead()
    {
        if (!isTeamLeader || functions.scoredPoint) { return; }
        FindObjectOfType<Methods>().SetScoreOnAllOpponents(team, 3);
    }

    public void ReceiveHit(int damage)
    {
        health += damage;

        e = -.1f;
        HealthBarText[team].alpha = 1;
        HealthBarText[team].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(HealthBarText[team].gameObject.GetComponent<RectTransform>().anchoredPosition.x, 15);
        HealthBarText[team].SetText(damage.ToString());

        LeanTween.cancel(HealthBarText[team].gameObject);
        LeanTween.value(HealthBarText[team].gameObject, 1, 0, .5f).setOnUpdate((float val) =>
        {
            HealthBarText[team].alpha = val;
        });
        LeanTween.moveY(HealthBarText[team].gameObject.GetComponent<RectTransform>(), 10, .5f);
    }

    enum ControllVikingType
    {
        Viking,
        VikingCannon,
        Both
    }
}
