using UnityEngine;

public class MainOfficeMemorialPortraitScript : MonoBehaviour
{
    Sprite[] portraits;
    // Start is called before the first frame update
    void Start()
    {
        portraits = Resources.LoadAll<Sprite>("Sprites/MemorialPortraits");
        if (Random.value < 0.1f) { GetComponent<SpriteRenderer>().sprite = portraits[Random.Range(0, 2)]; }
        else if (Random.value < 0.05f) { GetComponent<SpriteRenderer>().sprite = portraits[2]; }
        else { gameObject.SetActive(false); }
    }
}