using UnityEngine;

public class Checker : MonoBehaviour
{

    public bool[] hasPlayerReachedCheckpoint = new bool[4];

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            int id = other.GetComponent<Movement>().playerID;
            hasPlayerReachedCheckpoint[id] = true;
        }
    }
}
