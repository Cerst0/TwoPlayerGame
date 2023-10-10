using UnityEngine;

public class WheelRot : MonoBehaviour
{

    public float rotationSpeed = 99.0f;
    public bool reverse = false;
    public bool isCar;
    public bool isLeft;
    public CarController CC;

    void Update()
    {
        //if (isCar)
        //{
        //    rotationSpeed = Mathf.Abs(CC.rotationSpeed) * 10; 

        //    if (CC.rotationSpeed > 0)
        //    {
        //        if (isLeft) reverse = true;
        //        else { reverse = false; }
        //    }
        //    else
        //    {
        //        if (isLeft) reverse = false;
        //        else { reverse = true; }
        //    }
        //}

        //if (reverse)
        //{
        //    transform.Rotate(new Vector3(-1f, 0f, 0f) * Time.deltaTime * rotationSpeed);
        //}
        //else
        //{
        //    transform.Rotate(new Vector3(1f, 0f, 0f) * Time.deltaTime * rotationSpeed);
        //}
    }

    private void LateUpdate()
    {

    }
}
