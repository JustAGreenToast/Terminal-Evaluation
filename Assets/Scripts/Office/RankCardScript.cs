using UnityEngine;
using UnityEngine.UI;

public class RankCardScript : MonoBehaviour
{
    float t;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Image>().sprite = Resources.LoadAll<Sprite>("Sprites/Rank Cards")[(int)SettingsManager.settings.selectedGuardianAngel];
    }
    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        transform.localPosition = new Vector2(-850, 4 * Mathf.Sin(t * 120 * Mathf.Deg2Rad));
    }
}