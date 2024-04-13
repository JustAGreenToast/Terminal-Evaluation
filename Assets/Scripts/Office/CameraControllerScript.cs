using UnityEngine;

public class CameraControllerScript : MonoBehaviour
{
    float angle;
    const float maxAngle = 5;
    float angleSpeed;
    float targetAngleSpeed;
    float lockAngle;
    bool locked;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (locked)
        {
            angle = Mathf.MoveTowards(angle, lockAngle, 0.1f);
            transform.rotation = Quaternion.Euler(Vector3.up * angle);
            return;
        }
        float xPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward).x;
        xPos = Mathf.Clamp(Mathf.Abs(xPos) - 0.25f, 0, 5) * Mathf.Sign(xPos);
        targetAngleSpeed = xPos * 6 * maxAngle;
        if (Mathf.Sign(targetAngleSpeed) == Mathf.Sign(angle)) { targetAngleSpeed *= 1 - (Mathf.Abs(angle) / maxAngle); }
        angleSpeed = Mathf.MoveTowards(angleSpeed, targetAngleSpeed, 240 * Time.deltaTime);
        angle += angleSpeed * Time.deltaTime;
        angle = Mathf.Clamp(angle, -maxAngle, maxAngle);
        transform.rotation = Quaternion.Euler(Vector3.up * angle);
    }
    public void Lock(bool _customLockAngle, float _lockAngle = 0)
    {
        locked = true;
        lockAngle = _customLockAngle ? _lockAngle : angle;
    }
    public void Unlock() { locked = false; }
}