using UnityEngine;

public class PSManager : MonoBehaviour
{
    public bool destroy;
    public bool destroyParent;

    ParticleSystem m_ParticleSystem;
    bool played;

    private void Start()
    {
        m_ParticleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (m_ParticleSystem.isPlaying)
        {
            played = true;
        }
        else if (played && destroy)
        {
            if (destroyParent) { Destroy(transform.parent.gameObject); }
            Destroy(this.gameObject);
        }
    }
}
