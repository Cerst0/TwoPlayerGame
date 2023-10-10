using UnityEngine;

public class TennisTrigger : MonoBehaviour
{
    public TennisBall TB;
    public bool court;
    public int teamSide;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            TB.trigger = true;
            if (court)
            {
                TB.teamSide = teamSide;
            }
            else
            {
                TB.ballOut = true;
            }

            print("Tennis Ball Triggered" + teamSide + court + TB.ballOut);
        }
    }
}
