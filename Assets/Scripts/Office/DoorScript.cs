using UnityEngine;

public class DoorScript : MonoBehaviour, IMainOfficeTexturable
{
    float angle;
    float targetAngle;
    float angleSpeed;
    Sprite[] sprites;
    SpriteRenderer _r;
    SpriteRenderer r
    {
        get
        {
            if (!_r) { _r = GetComponent<SpriteRenderer>(); }
            return _r;
        }
    }
    const int holdFrameBuffer = 5;
    int frameCooldown;
    public bool isClosed { get; private set; }
    // Update is called once per frame
    void Update()
    {
        if (angle != targetAngle)
        {
            angle = Mathf.MoveTowards(angle, targetAngle, angleSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(-Vector3.up * angle);
            UpdateSprite();
        }
        if (frameCooldown > 0)
        {
            frameCooldown--;
            if (frameCooldown == 0) { isClosed = false; }
        }
    }
    public void LoadTextures(string _folderName)
    {
        sprites = Resources.LoadAll<Sprite>($"Sprites/Main Office Textures/{_folderName}/Door");
        UpdateSprite();
    }
    public void Rotate(float _targetAngle, float _v)
    {
        targetAngle = _targetAngle;
        angleSpeed = _v;
    }
    void UpdateSprite()
    {
        r.sprite = sprites[angle > 90 ? 1 : 0];
        r.flipX = angle > 90;
    }
    public void RefreshClickHold()
    {
        isClosed = true;
        frameCooldown = holdFrameBuffer;
    }
}