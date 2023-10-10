using UnityEngine;

public class anchorTransform : MonoBehaviour
{
    public GameObject anchor;
    public float smoothT;
    public float smoothR;
    Vector3 b;

    private void Update()
    {
        transform.position = Vector3.Lerp(anchor.transform.position, transform.position, smoothT);
        transform.eulerAngles = Vector3.Lerp(anchor.transform.eulerAngles, b, smoothR);
    }
    private void LateUpdate()
    {
        b = transform.eulerAngles;
    }
}
