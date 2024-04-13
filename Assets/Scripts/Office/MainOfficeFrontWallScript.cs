using UnityEngine;

public class MainOfficeFrontWallScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Sprites/Main Office Textures/{SettingsManager.settings.mainOfficeTextureSet}/Overlay");
        GetComponent<Renderer>().material.SetTexture("_WallTile", Resources.Load<Texture>($"Sprites/Main Office Textures/{SettingsManager.settings.mainOfficeTextureSet}/WallTile"));
        GetComponent<Renderer>().material.SetTexture("_TileGuide", Resources.Load<Texture>($"Sprites/Main Office Textures/{SettingsManager.settings.mainOfficeTextureSet}/TileAlpha"));
    }
}