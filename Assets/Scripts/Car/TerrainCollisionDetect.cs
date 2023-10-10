using UnityEngine;

public class TerrainCollisionDetect : MonoBehaviour
{
    public bool upSideDown;

    private void OnTriggerEnter(Collider other)
    {
        //print("Trigger: " + other.gameObject);
        if (other.gameObject.name == "Terrain")
        {
            upSideDown = true;
        }
    }
}
