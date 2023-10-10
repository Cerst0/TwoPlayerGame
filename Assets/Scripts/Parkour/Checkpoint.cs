using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public ParticleSystem psL;
    public ParticleSystem psR;

    public SkinnedMeshRenderer flagB;
    public SkinnedMeshRenderer flagR;

    Functions functions;
    Color noTeam = new Color(1, 0.8f, .47f);

    bool isFirstPlayer = true;

    private void Start()
    {
        flagB.material.color = noTeam;
        flagR.material.color = noTeam;
        functions = GameObject.FindGameObjectWithTag("Brain").GetComponent<Functions>();
    }

    public void SetCheckpoint(int index)
    {
        functions.AddScore(0, 2, index);
        FindObjectOfType<Methods>().SetPSColorFromPlayerColor(psL, index);
        FindObjectOfType<Methods>().SetPSColorFromPlayerColor(psR, index);

        if (isFirstPlayer)
        {
            Color color = FindObjectOfType<Methods>().GetPlayerColor(index);
            flagB.material.color = color;
            flagR.material.color = color;
            isFirstPlayer = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerMovementParkour>().OnCheckpoint();
        }
    }
}
