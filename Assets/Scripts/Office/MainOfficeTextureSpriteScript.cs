using UnityEngine;

public class MainOfficeTextureSpriteScript : MonoBehaviour, IMainOfficeTexturable
{
    SpriteRenderer _r;
    SpriteRenderer r
    {
        get
        {
            if (!_r) { _r = GetComponent<SpriteRenderer>(); }
            return _r;
        }
    }
    [SerializeField] string filename;
    public void LoadTextures(string _folderName)
    {
        Sprite s = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/{filename}");
        if (!s && filename == "Ceiling") { s = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/Floor"); }
        r.sprite = s;
    }
}