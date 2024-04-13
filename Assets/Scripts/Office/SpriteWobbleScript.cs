using UnityEngine;

public class SpriteWobbleScript : MonoBehaviour
{
    float t;
    float angle;
    Vector3 baseScale;
    // Start is called before the first frame update
    void Start()
    {
        baseScale = transform.localScale;
    }
    // Update is called once per frame
    void Update()
    {
        if (t > 0)
        {
            t -= Time.deltaTime;
            angle += 960 * Time.deltaTime;
            transform.localScale = baseScale + t * 0.05f * Mathf.Sin(angle * Mathf.Deg2Rad) * new Vector3(-1, 1, 0);
            if (t <= 0) { angle = 0; }
        }
    }
    public void PlayAnim() { t = 2; }
}