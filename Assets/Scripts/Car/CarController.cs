using Mirror;
using UnityEngine;

public class CarController : Movement
{
    string debugCurrentDriveState;

    public AxleInfo[] axleInfos; // the information about each individual axle
    public float friction;
    public float SteeringWheelStreinght;
    public float maxSpeed;
    float startMaxSpeed;
    public float massOffset;
    public float breakStreingth;
    public float terrainBreakStreingth;
    public float stiffness;
    float startStiffness;
    public float trottleMultiplyer;
    float starTrottleMultiplyer;
    public float terrainMaxSpeed;
    float startTerrainMaxSpeed;
    public float steeringStrength;
    public float pitch;
    public float wheelRotationSpeed;
    int powerState;
    public float powerChargeDuration;
    float durationB;
    public float powerDuration;
    float durationPowerB;
    public AnimationCurve nonlinearSteering;
    public AnimationCurve overSpeedCurve;
    public AnimationCurve debugCurve;

    float localMV;
    float trottle;
    public float localMH;
    float rpm;
    float carVelocity;
    Vector3 posB;

    public WheelCollider[] WheelColliders;
    public GameObject[] Wheels;
    Quaternion[] currentWheelRotations = new Quaternion[4];

    public AudioSource engine;
    public AudioSource Powerup;

    public ParticleSystem smoke;
    public ParticleSystem smoke2;

    public Material[] PowerBarMaterials;
    int PowerBarMatIndex;

    public Light PowerLight;
    public GameObject Power;

    public TerrainCollisionDetect tcd;
    Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody>();
        PowerLight.intensity = 0;
        durationB = powerChargeDuration;
        durationPowerB = powerDuration;
        //startVol = engine.volume;
        startMaxSpeed = maxSpeed;
        startStiffness = stiffness;
        starTrottleMultiplyer = trottleMultiplyer;
        startTerrainMaxSpeed = terrainMaxSpeed;

        transform.Find("Car" + playerID).GetComponent<MeshRenderer>().material.color = IDA.PlayerColor;

        Wheels = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            Wheels[i] = transform.Find("Wheels" + playerID).transform.GetChild(i).gameObject;
        }

        WheelColliders = new WheelCollider[4];
        for (int i = 0; i < 4; i++)
        {
            WheelColliders[i] = transform.Find("Wheel Colliders" + playerID).transform.GetChild(i).GetComponent<WheelCollider>();
        }

        if (NetworkClient.active)
        {
            if (isDummy)
            {
                GameObject wheelCollider = WheelColliders[0].transform.parent.gameObject;
                Destroy(wheelCollider);
                Destroy(rb);
                Destroy(GetComponent<Collider>());
            }
            //else { FindObjectOfType<Methods>().SetHint(true, "Boost", new int[] { 1, 2 }, -1, new bool[] { true, false }, false, 5); }
        }

        if (playerID == 0)
        {
            FindObjectOfType<Methods>().SetHint(true, "Boost", new int[] { 1, 2 }, -1, new bool[] { true, true, true, true }, false, 5);
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isDummy)
        {
            rpm = 0; // assign rpm
            foreach (AxleInfo axleInfo in axleInfos)
            {
                rpm += (axleInfo.leftWheel.rpm + axleInfo.rightWheel.rpm) / 4;
            }

            carVelocity = Vector3.Distance(transform.position, posB);
            if (rpm < 0) { carVelocity = -carVelocity; }
            posB = transform.position;

            float steering = localMH * steeringStrength * nonlinearSteering.Evaluate(carVelocity);

            foreach (WheelCollider wheelCollider in WheelColliders)
            {
                WheelFrictionCurve wheelFriction = wheelCollider.forwardFriction;
                wheelFriction.stiffness = stiffness;
                wheelCollider.forwardFriction = wheelFriction;
            }

            AdjustTrottle();

            foreach (AxleInfo axleInfo in axleInfos)
            {
                if (axleInfo.steering)
                {
                    axleInfo.leftWheel.steerAngle = steering;
                    axleInfo.rightWheel.steerAngle = steering;
                }
                if (axleInfo.motor)
                {
                    axleInfo.leftWheel.motorTorque = trottle;
                    axleInfo.rightWheel.motorTorque = trottle;
                }
            }

            GetComponent<Rigidbody>().centerOfMass = new Vector3(rb.centerOfMass.x, rb.centerOfMass.y - massOffset, rb.centerOfMass.z);

            if (status.countdown)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                rb.constraints = RigidbodyConstraints.None;
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        localMV = mV; // assign input
        localMH = mH;

        if (Mathf.Abs(localMV) > 0.2)
        {
            if (!smoke.isPlaying) { smoke.Play(); smoke2.Play(); }
        }
        else { smoke.Stop(); smoke2.Stop(); }

        if (tcd.upSideDown && !status.movementLock)
        {
            FindObjectOfType<Methods>().SetScoreOnAllOpponents(playerID, 2);
            FindObjectOfType<Notificaton>().NotificationWith2Colors(Color.red, 3f, IDA.playerName, "crashed", methods.GetPlayerColor(playerID), true);
        }

        if (!isDummy)
        {
            RotateWheels();
            PowerUp();

            if (NetworkClient.active)
            {
                status.sync.SetCarVars(transform.rotation, currentWheelRotations, Power.transform.localScale, Power.transform.localPosition, PowerBarMatIndex, smoke.isPlaying, smoke.startColor, playerID);
            }
        }
    }

    private void AdjustTrottle()
    {
        trottle = localMV;

        debugCurrentDriveState = null;

        if (Mathf.Abs(carVelocity) > maxSpeed | (!IsOnStreet() && Mathf.Abs(carVelocity) > maxSpeed * terrainMaxSpeed))
        {
            SlowCarDown(maxSpeed);
        }
        else
        {
            trottleMultiplyer = starTrottleMultiplyer;
        }

        if ((localMV < 0 && carVelocity > 0) | (localMV > 0 && carVelocity < 0))
        {
            trottle *= breakStreingth;
            debugCurrentDriveState += "brake";
        }

        if (carVelocity > 0 && localMV == 0)
        {
            trottle = -0.25f * friction;
            debugCurrentDriveState += ("roll " + carVelocity + " " + localMV);
        }

        if (carVelocity < 0 && localMV == 0)
        {
            trottle = 0.25f * friction;
            debugCurrentDriveState += ("rollBackwards " + carVelocity + " " + localMV);
        }

        if (Mathf.Abs(carVelocity) < 0.5f && Mathf.Abs(localMV) < 0.01f)
        {
            trottle = 0;
            debugCurrentDriveState += ("stopp " + carVelocity + " " + localMV);
        }

        trottle *= trottleMultiplyer;
    }

    private void SlowCarDown(float maxSpeed)
    {
        debugCurrentDriveState += "over max speed: " + carVelocity + " " + maxSpeed;

        float difference = Mathf.Abs(carVelocity) - maxSpeed;

        trottleMultiplyer = starTrottleMultiplyer * overSpeedCurve.Evaluate(difference * 10);

        //debugCurve.AddKey(Time.timeSinceLevelLoad, trottleMultiplyer);
    }

    private bool IsOnStreet()
    {
        bool isOnStreet = true;
        foreach (WheelCollider wheel in GetComponentsInChildren<WheelCollider>())
        {
            wheel.GetGroundHit(out WheelHit wheelHit);
            if (wheelHit.collider != null)
            {
                if (wheelHit.collider.name != "Road Mesh Holder")
                {
                    isOnStreet = false;
                }
            }
        }
        return isOnStreet;
    }

    private void RotateWheels()
    {
        float z = rpm * 360 * wheelRotationSpeed;
        z /= 60; // to seconds
        z *= Time.deltaTime;

        for (int i = 0; i < Wheels.Length; i++)
        {
            if (Wheels[i].name.Contains("F")) //if its a front wheel(steering)
            {
                Quaternion steeringRot;
                steeringRot = Wheels[i].transform.localRotation;
                steeringRot = Quaternion.Euler(steeringRot.eulerAngles.x,
                    Mathf.Lerp((SteeringWheelStreinght * localMH) + 90,
                    steeringRot.eulerAngles.y, .9f), steeringRot.eulerAngles.z);
                Wheels[i].transform.localRotation = steeringRot;
            }
            Wheels[i].transform.Rotate(new Vector3(0, 0, z));
            currentWheelRotations[i] = Wheels[i].transform.localRotation;
        }
    }

    private void PowerUp()
    {
        if (powerState == 0)
        {
            maxSpeed = startMaxSpeed;
            stiffness = startStiffness;
            terrainMaxSpeed = startTerrainMaxSpeed;

            powerDuration = durationPowerB;
            powerChargeDuration = durationB;
            PowerLight.intensity = 0.02f;
            PowerLight.color = new Color(0.8000001f, 0.6588235f, 0.02745098f, 1);
            Power.transform.localPosition = new Vector3(-0.3526f, 0, 0);
            Power.transform.localScale = new Vector3(0, 100, 100);
            Power.GetComponent<MeshRenderer>().material = PowerBarMaterials[0];
            PowerBarMatIndex = 0;
            LeanTween.moveLocalX(Power, 0, powerChargeDuration);
            LeanTween.scaleX(Power, 100, powerChargeDuration);

            smoke.startColor = Color.white;
            smoke2.startColor = Color.white;

            powerState = 1;
        }
        if (powerState == 1)
        {
            powerChargeDuration += -Time.deltaTime;
            if (powerChargeDuration < 0f)
            {
                powerState = 2;
                PowerLight.intensity = 0.15f;
                Invoke(nameof(SetHint), 10f);
            }
        }
        if (powerState == 2)
        {
            PowerLight.color = PowerBarMaterials[1].color;
            Power.GetComponent<MeshRenderer>().material = PowerBarMaterials[1];
            PowerBarMatIndex = 1;

            if (!LeanTween.isTweening(PowerLight.gameObject))
            {
                if (PowerLight.intensity == 0.15f)
                {
                    LeanTween.value(PowerLight.gameObject, 0.15f, 0f, 1f).setOnUpdate((float val) => { PowerLight.intensity = val; });
                }
                else
                {
                    LeanTween.value(PowerLight.gameObject, 0f, 0.15f, 1f).setOnUpdate((float val) => { PowerLight.intensity = val; });
                }
            }

            if (localMH > 0.7f && localMV < -0.7f)
            {
                PowerLight.intensity = 0.15f;
                maxSpeed = startMaxSpeed * 2f;
                stiffness = startStiffness * 1.5f;
                terrainMaxSpeed = 1;

                smoke.startColor = IDA.PlayerColor;
                smoke2.startColor = IDA.PlayerColor;

                LeanTween.moveLocalX(Power, -0.3526f, powerDuration);
                LeanTween.scaleX(Power, 0, powerDuration);

                CancelInvoke(nameof(SetHint));
                FindObjectOfType<Methods>().SetHint(false, null, null, playerID, null);

                powerState = 3;
            }
        }
        if (powerState == 3)
        {
            powerDuration += -Time.deltaTime;

            if (powerDuration < 0)
            {
                powerState = 0;
            }
        }
    }

    void SetHint()
    {
        FindObjectOfType<Methods>().SetHint(true, "Boost", new int[] { 1, 2 }, playerID, null);
    }
}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
}
