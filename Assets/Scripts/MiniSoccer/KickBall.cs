using UnityEngine;

public class KickBall : MonoBehaviour
{
    public float kickForce;
    public float maxVelocity;
    public float rotationSpeed;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 dir = new Vector3(rb.velocity.z, 0, -rb.velocity.x);
        dir *= rotationSpeed;
        transform.RotateAround(transform.position, dir, rb.velocity.magnitude);
    }

    private void Update()
    {
        GetComponent<MeshRenderer>().enabled = !FindObjectOfType<Functions>().scoredScore;
    }

    public void Kick(Vector3 pos, Vector3 vel)
    {
        vel.y = 0;

        Vector3 direction = (pos - transform.position).normalized;

        Vector3 force = -direction;
        force *= kickForce;
        force *= vel.magnitude;

        force = Vector3.ClampMagnitude(force, maxVelocity);

        //if(force.magnitude == 0) { rb.velocity = Vector3.zero; }

        rb.AddForce(force, ForceMode.Impulse);

        GetComponent<AudioSource>().Play();

        print("kicked Ball and addForce with magnetude of: " + force.magnitude + " direction: " + direction + " vel.mag: " + vel.magnitude);
    }
}
