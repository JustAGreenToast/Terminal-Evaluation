using UnityEngine;

public class CameraControllerScript : MonoBehaviour
{
    float angle;
    const float maxAngle = 5;
    float angleSpeed;
    float targetAngleSpeed;
    float lockAngle;
    bool locked;
    bool isTurnedAround;
    float turnDir;
    float turnLerp;
    float targetTurnLerp { get { return isTurnedAround ? 1 : 0; } }
    const float turnSpeed = 7.5f;
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        if (turnLerp != targetTurnLerp)
        {
            turnLerp = Mathf.MoveTowards(turnLerp, targetTurnLerp, turnSpeed * Time.deltaTime);
            if (turnDir == 0) { turnDir = Random.value > 0.5f ? -1 : 1; }
        }
        if (locked)
        {
            angle = Mathf.MoveTowards(angle, lockAngle, 0.1f);
            transform.rotation = Quaternion.Euler(Vector3.up * (angle + 180 * turnDir * turnLerp));
            return;
        }
        float xPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward).x * Mathf.Lerp(1, -1, turnLerp);
        xPos = Mathf.Clamp(Mathf.Abs(xPos) - 0.25f, 0, 5) * Mathf.Sign(xPos);
        targetAngleSpeed = xPos * 6 * maxAngle;
        if (Mathf.Sign(targetAngleSpeed) == Mathf.Sign(angle)) { targetAngleSpeed *= 1 - (Mathf.Abs(angle) / maxAngle); }
        angleSpeed = Mathf.MoveTowards(angleSpeed, targetAngleSpeed, 240 * Time.deltaTime);
        angle += angleSpeed * Time.deltaTime;
        angle = Mathf.Clamp(angle, -maxAngle, maxAngle);
        transform.rotation = Quaternion.Euler(Vector3.up * (angle + 180 * turnDir * turnLerp));
    }
    public void SetTurnedFlag(bool _isTurned)
    {
        isTurnedAround = _isTurned;
        if (turnLerp == 0 || turnLerp == 1) { turnDir = Random.value > 0.5f ? -1 : 1; }
    }
    public void Lock(bool _customLockAngle, float _lockAngle = 0)
    {
        locked = true;
        lockAngle = _customLockAngle ? _lockAngle : angle;
        isTurnedAround = Mathf.Abs(lockAngle) > 90;
        if (isTurnedAround) { lockAngle -= 180; }
    }
    public void Unlock() { locked = false; }
}