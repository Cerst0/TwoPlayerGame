using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public Checker[] checker;
    Notificaton n;
    public AudioSource Check;
    public Functions functions;
    Status status;

    private void Update()
    {
        n = GameObject.FindGameObjectWithTag("Notification").GetComponent<Notificaton>();
        status = FindObjectOfType<Status>();
        checker = FindObjectsOfType<Checker>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            int id = other.GetComponent<Movement>().playerID;

            bool finishedTrack = true;

            foreach (Checker c in checker)
            {
                if (!c.hasPlayerReachedCheckpoint[id])
                {
                    finishedTrack = false;
                }
            }

            if (!finishedTrack && Time.timeSinceLevelLoad > 10)
            {
                n.NotificationWith2Colors(new Color(1, 0.5f, 0, 0), 2f, functions.IDAs[id].playerName, ", Please complete the track", FindObjectOfType<Methods>().GetPlayerColor(id));
            }
            if (finishedTrack)
            {
                if (status.clientPlayerID < 1) { functions.AddScore(0, 2, id); }
                else { status.sync.AddScore(0, 2, id); print(status.sync.name); }

                foreach (Checker c in checker)
                {
                    c.hasPlayerReachedCheckpoint[id] = false;
                }
            }
        }
    }
}
