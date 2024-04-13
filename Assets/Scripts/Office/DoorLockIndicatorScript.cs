using UnityEngine;

public class DoorLockIndicatorScript : MonoBehaviour
{
    [SerializeField] DoorScript door;
    SpriteRenderer r;
    [SerializeField] Sprite[] lockSprites;
    int hoverBuffer;
    const int hoverFrames = 5;
    float alpha;
    const float alphaSpeed = 4;
    float angle;
    const float angleSpeed = 90;
    // Start is called before the first frame update
    void Start()
    {
        if (VirtualRAM.examData.examIndex < 2) { gameObject.SetActive(false); }
        r = GetComponent<SpriteRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        r.sprite = lockSprites[door.isClosed ? 1 : 0];
        if (hoverBuffer > 0) { hoverBuffer--; }
        alpha = Mathf.MoveTowards(alpha, hoverBuffer > 0 ? 1 : 0, alphaSpeed * Time.deltaTime);
        r.color = new Color(1, 1, 1, alpha);
        angle += angleSpeed * Time.deltaTime;
        while (angle >= 360) { angle -= 360; }
        transform.position = new Vector3(0, 0.5f + 0.02f * Mathf.Sin(angle * Mathf.Deg2Rad), 3.4f);
    }
    public void OnHover() { hoverBuffer = hoverFrames; }
}