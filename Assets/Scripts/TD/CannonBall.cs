using Mirror;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    string team;
    ParticleSystem DustPS;
    float Cooldown = 0.1f;
    GameObject LastViking;
    Viking viking;

    Functions functions;
    Status status;

    private void Start()
    {
        functions = FindObjectOfType<Functions>();
        status = FindObjectOfType<Status>();

        viking = transform.parent.GetComponent<Viking>();
        team = viking.team;
        DustPS = GameObject.Find("DustPS" + viking.teamInt).GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (Cooldown < 0)
        {
            LastViking.GetComponent<BoxCollider>().isTrigger = true;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.name.Contains(team) && other.gameObject.name != "Ball")
        {
            //print("hitColl: " + other.gameObject.name);

            gameObject.GetComponent<SphereCollider>().isTrigger = false;

            if (other.gameObject.name.Contains("Viking"))
            {
                other.gameObject.GetComponent<BoxCollider>().isTrigger = false;
                LastViking = other.gameObject;
                Cooldown = 0.1f;
            }

            if (other.gameObject.name.Contains("Viking" + viking.oppositeTeam) && status.clientPlayerID < 1)
            {
                string oppositeVikingID = other.GetComponent<Viking>().id;
                if (NetworkClient.active)
                {
                    status.sync.VikingHurt(viking.attackStrenght, oppositeVikingID);
                    print("hurt" + oppositeVikingID);
                }
                else
                {
                    FindVikingByID(oppositeVikingID).OnHurt(viking.attackStrenght);
                }
            }

            if (other.gameObject.name == "Gate" && status.clientPlayerID < 1)
            {
                Vector3 pos = new(DustPS.transform.position.x, transform.position.y, transform.position.z);
                DustPS.transform.position = pos;

                if (status.clientPlayerID == 0)
                {
                    status.sync.SetGameObjectPos(DustPS.name, pos);
                    FindObjectOfType<Methods>().PlayNetworkedPs(DustPS.name, false, Color.clear, null, true);
                }

                viking.HitCastle();
            }
        }
    }

    public static Viking FindVikingByID(string id)
    {
        return Viking.FindVikingByID(id);
    }
}
