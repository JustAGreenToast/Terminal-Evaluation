using UnityEngine;

public class LookAtCamScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 ogRot = transform.eulerAngles;
        transform.forward = Camera.main.transform.forward;
        transform.rotation = Quaternion.Euler(ogRot.x, transform.eulerAngles.y, ogRot.z);
    }
}