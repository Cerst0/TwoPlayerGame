using UnityEngine;

public class StoneObstacle : MonoBehaviour
{
    Status status;

    public ParticleSystem[] particles;

    public Vector3 pos1;
    public Vector3 pos2;
    Vector3 targetPos;
    public float speed;
    int way = 1;
    public bool isRandom;

    private void Start()
    {
        status = FindObjectOfType<Status>();

        if (status.clientPlayerID > 0)
        {
            Destroy(particles[0].gameObject);
            Destroy(particles[1].gameObject);
            Destroy(this);
        }

        if (isRandom && status.clientPlayerID < 1)
        {
            int i = Random.Range(0, 2);
            if (i == 1) Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (way == 3)
        {
            LeanTween.move(gameObject, pos1, speed);

            particles[0].Stop();
            particles[1].Play();

            way = 4;
        }

        if (way == 1)
        {
            LeanTween.move(gameObject, pos2, speed);

            particles[0].Play();
            particles[1].Stop();

            way = 2;
        }

        if (!LeanTween.isTweening(gameObject))
        {
            if (way == 4)
            {
                way = 1;
            }
            if (way == 2)
            {
                way = 3;
            }
        }
    }
}
