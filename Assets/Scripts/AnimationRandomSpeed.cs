using UnityEngine;

public class AnimationRandomSpeed : MonoBehaviour
{
    float speed;
    Animator am;
    float time;

    private void Start()
    {
        speed = Random.Range(.9f, 1.1f);
        am = GetComponent<Animator>();
        am.speed = speed;
    }

    private void Update()
    {
        time = am.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
}
