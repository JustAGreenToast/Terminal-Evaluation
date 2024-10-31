using UnityEngine;

public class MainOfficeMemorialPortraitScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Sprite[] portraits = Resources.LoadAll<Sprite>("Sprites/MemorialPortraits");
        if (Random.value < 0.1f) { GetComponent<SpriteRenderer>().sprite = portraits[Random.Range(0, 2)]; }
        else if (Random.value < 0.05f) { GetComponent<SpriteRenderer>().sprite = portraits[Random.Range(2, 4)]; }
        else { gameObject.SetActive(false); }
    }
}