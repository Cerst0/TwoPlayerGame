using UnityEngine;

public class ScorekeeperParkour : MonoBehaviour
{
    Functions functions;
    Status status;

    private void Start()
    {
        functions = FindObjectOfType<Functions>();
        status = FindObjectOfType<Status>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && status.clientPlayerID < 1)
        {
            int id = other.GetComponent<Movement>().playerID;
            functions.AddScore(2, 2, id);
        }
    }
}
