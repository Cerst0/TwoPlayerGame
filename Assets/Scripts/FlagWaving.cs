using UnityEngine;

public class FlagWaving : MonoBehaviour
{
    public GameObject armL;
    Vector3 posB;

    private void Start()
    {
        posB.z = 310;
    }

    private void Update()
    {
        if (armL.transform.localEulerAngles.z > 311 && posB.z < 311)
        {
            posB = armL.transform.localEulerAngles;
            GetComponent<Animator>().SetBool("start", true);
        }
        else
        {
            GetComponent<Animator>().SetBool("start", false);
        }
        posB = armL.transform.localEulerAngles;
    }
}


