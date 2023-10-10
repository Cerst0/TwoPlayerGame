using UnityEngine;

public class SyncPre : MonoBehaviour
{
    public Color[] colors;
    public string playerName;
    public int meshNumber;

    bool ran;

    private void Update()
    {
        if (!ran)
        {
            IdDataApply IDA = GameObject.FindGameObjectWithTag("IDA0").GetComponent<IdDataApply>();

            //Colors
            colors = new Color[20];

            SkinnedMeshRenderer skr = IDA.GetComponent<SkinnedMeshRenderer>();
            for (int i = 0; i <= skr.materials.Length - 1; i++)
            {
                colors[i] = skr.materials[i].color;
            }

            //Name
            playerName = IDA.GetComponent<IdDataApply>().playerName;

            //MeshNumber
            meshNumber = IDA.GetComponent<IdDataApply>().meshNumber;

            // -----------
            ran = true;
        }
    }
}
