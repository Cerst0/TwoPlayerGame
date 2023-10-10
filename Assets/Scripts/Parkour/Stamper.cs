using UnityEngine;

public class Stamper : MonoBehaviour
{
    public float speed;
    float maxXPosition;
    public float pushStreght;
    public float delay;
    public bool leftSide;
    float startXPosition;
    int way = 0;

    Status status;

    void Start()
    {
        status = FindObjectOfType<Status>();

        if (status.clientPlayerID > 0)
        {
            Destroy(this);
        }

        startXPosition = transform.position.x;

        speed *= 4;

        if (leftSide) { maxXPosition = -8; }
        if (!leftSide) { maxXPosition = 6.52398f; }
    }

    void Update()
    {
        if (transform.position.x > maxXPosition && leftSide) { way = 1; }
        if (transform.position.x < startXPosition && leftSide) { way = 0; }

        if (transform.position.x < maxXPosition && !leftSide) { way = 1; }
        if (transform.position.x > startXPosition && !leftSide) { way = 0; }

        if (way == 0 && delay < 0f) { transform.position += transform.right * speed * Time.deltaTime; }
        if (way == 1) { transform.position += -transform.right * speed * Time.deltaTime; }

        delay -= Time.deltaTime;
    }
}
