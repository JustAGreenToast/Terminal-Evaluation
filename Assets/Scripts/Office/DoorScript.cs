using UnityEngine;

public class DoorScript : MonoBehaviour
{
    float angle;
    float targetAngle;
    float angleSpeed;
    Sprite[] sprites;
    SpriteRenderer r;
    const int holdFrameBuffer = 5;
    int frameCooldown;
    public bool isClosed { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<SpriteRenderer>();
        sprites = Resources.LoadAll<Sprite>($"Sprites/Main Office Textures/{SettingsManager.settings.mainOfficeTextureSet}/Door");
        r.sprite = sprites[0];
    }
    // Update is called once per frame
    void Update()
    {
        if (angle != targetAngle)
        {
            angle = Mathf.MoveTowards(angle, targetAngle, angleSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(-Vector3.up * angle);
            r.sprite = sprites[angle > 90 ? 1 : 0];
        }
        if (frameCooldown > 0)
        {
            frameCooldown--;
            if (frameCooldown == 0) { isClosed = false; }
        }
    }
    public void Rotate(float _targetAngle, float _v)
    {
        targetAngle = _targetAngle;
        angleSpeed = _v;
    }
    public void RefreshClickHold()
    {
        isClosed = true;
        frameCooldown = holdFrameBuffer;
    }
}