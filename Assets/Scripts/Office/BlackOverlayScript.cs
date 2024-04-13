using UnityEngine;

public class BlackOverlayScript : MonoBehaviour
{
    float alpha;
    float targetAlpha;
    [SerializeField] bool enableFlickering;
    // Start is called before the first frame update
    void Start()
    {
        Activate();
    }
    // Update is called once per frame
    void Update()
    {
        if (alpha != targetAlpha)
        {
            alpha = Mathf.MoveTowards(alpha, targetAlpha, 4 * Time.deltaTime);
            GetComponent<SpriteRenderer>().color = Color.black * Mathf.Clamp01(alpha);
            if (alpha == targetAlpha && enableFlickering) { targetAlpha = targetAlpha > 0 ? 0: Random.value > 0.9f ? Random.Range(0.75f, 1.5f) : Random.Range(0.1f, 0.6f); }
        }
    }
    public void Activate()
    {
        alpha = 1.75f;
        targetAlpha = 0;
        GetComponent<SpriteRenderer>().color = Color.black;
    }
}