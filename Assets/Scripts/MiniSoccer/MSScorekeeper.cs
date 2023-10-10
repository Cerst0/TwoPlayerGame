using UnityEngine;

public class MSScorekeeper : MonoBehaviour
{
    public int id;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball") && !FindObjectOfType<Functions>().scoredScore)
        {
            FindObjectOfType<Functions>().AddScore(2, 3, id);
        }
    }
}