using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    public bool duplicate;

    private void Start()
    {
        if (!duplicate && GameObject.FindGameObjectsWithTag(this.gameObject.tag).Length > 1)
        {
            print(this.gameObject + " was destroyed, because there is already a same object in the scene");
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }
}
