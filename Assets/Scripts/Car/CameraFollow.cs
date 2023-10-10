using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject anchor;
    public Status status;
    public Vector3 Offset;
    public float rotXOffset;
    public float smoothPosition;

    private void Start()
    {
        //transform.rotation = Quaternion.Euler(anchor.transform.rotation.eulerAngles.x + rotXOffset, anchor.transform.rotation.eulerAngles.y, anchor.transform.rotation.eulerAngles.z);
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(anchor.transform.position + Offset, transform.position, smoothPosition);
        if (!status.countdown) { smoothPosition = 0; }
        transform.rotation = anchor.transform.rotation;
    }

}
