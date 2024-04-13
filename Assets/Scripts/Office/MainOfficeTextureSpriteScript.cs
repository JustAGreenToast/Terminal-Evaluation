using UnityEngine;

public class MainOfficeTextureSpriteScript : MonoBehaviour
{
    [SerializeField] string filename;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Sprites/Main Office Textures/{SettingsManager.settings.mainOfficeTextureSet}/{filename}");
    }
}