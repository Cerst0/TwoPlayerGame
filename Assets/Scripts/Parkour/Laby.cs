using Mirror;
using UnityEngine;

public class Laby : MonoBehaviour
{
    public int labyID; // levelNum + labyNumb
    public int side = -1;
    public GameObject wayRight;
    public GameObject wayLeft;
    public Collider colL;
    public Collider colR;
    public GameObject arrow;

    GameObject[] Players;
    Functions functions;
    Status status;

    float time;
    bool isOpen;
    bool syncedSite;

    private void Start()
    {
        status = FindObjectOfType<Status>();
        functions = FindObjectOfType<Functions>();

        name = "Laby" + labyID;

        if (status.clientPlayerID < 1)
        {
            side = Random.Range(1, 3);
            if (NetworkServer.active) { GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().SetLabySide(labyID, side); print(" SetLabySideClient(int labyID, int side)"); }
        }
    }

    private void Update()
    {
        if (NetworkServer.active && !syncedSite && status.sync.allClientsReady)
        {
            status.sync.SetLabySide(labyID, side);
            syncedSite = true;
        }


        if (side != -1)
        {
            if (side == 1) { wayLeft.GetComponent<BoxCollider>().enabled = false; }
            if (side == 2) { wayRight.GetComponent<BoxCollider>().enabled = false; }
        }

        if (functions.Players.Length != 0)
        {
            Players = functions.Players;

            foreach (GameObject go in Players)
            {
                if (colL.bounds.Contains(go.transform.position) && side == 1)
                {
                    LeanTween.alpha(wayLeft, 0, 0.5f);
                    isOpen = true;
                }

                if (colR.bounds.Contains(go.transform.position) && side == 2)
                {
                    LeanTween.alpha(wayRight, 0, 0.5f);
                    isOpen = true;
                }
            }
        }

        if (time + 0.5f < Time.time && !isOpen)
        {
            if (arrow.transform.rotation.eulerAngles.y == 270)
            {
                arrow.transform.rotation = Quaternion.Euler(0, 90, 180);
            }
            else { arrow.transform.rotation = Quaternion.Euler(0, 270, 180); }

            time = Time.time;
        }

        if (isOpen)
        {
            if (side == 1) { arrow.transform.rotation = Quaternion.Euler(0, 270, 180); }
            if (side == 2) { arrow.transform.rotation = Quaternion.Euler(0, 90, 180); }
        }
    }
}
