using UnityEngine;

public class MainOfficeFrontWallScript : MonoBehaviour, IMainOfficeTexturable
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
    Vector2 size;
    Vector2 scroll;
    Vector2 scrollSpeed;
    Texture[] wallTiles;
    int wallTileCounter;
    float wallTileTimer;
    [SerializeField] GameManagerScript manager;
    public void LoadTextures(string _folderName)
    {
        r.sprite = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/Overlay");
        if (_folderName == "9" || (_folderName.StartsWith("14_") && manager.hasPerfectRank))
        {
            wallTiles = new Texture[4]
            {
                Resources.Load<Texture>($"Sprites/Main Office Textures/{_folderName}/WallTile"),
                Resources.Load<Texture>($"Sprites/Main Office Textures/{_folderName}/WallTile2"),
                Resources.Load<Texture>($"Sprites/Main Office Textures/{_folderName}/WallTile3"),
                Resources.Load<Texture>($"Sprites/Main Office Textures/{_folderName}/WallTile2")
            };
            r.material.SetTexture("_WallTile", wallTiles[0]);
        }
        else
        {
            wallTiles = null;
            r.material.SetTexture("_WallTile", Resources.Load<Texture>($"Sprites/Main Office Textures/{_folderName}/WallTile"));
        }
        r.material.SetTexture("_TileGuide", Resources.Load<Texture>($"Sprites/Main Office Textures/{_folderName}/TileAlpha"));
        size = new Vector2(22, 6);
        scrollSpeed = Vector2.zero;
        switch (_folderName)
        {
            // Synthwave Stardom
            case "9":
                size = new Vector2(11, 3);
                scrollSpeed = new Vector2(Random.Range(-1f, 1f), -1).normalized * Random.Range(0.5f, 0.85f);
                break;
            // Sandy Shores
            case "10":
                size = new Vector2(22, 1);
                break;
            // Splendid Spectrum
            case "12":
                size = new Vector2(11, 3);
                if (Random.value > 0.5f) { scrollSpeed = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Random.Range(0.05f, 0.25f); }
                break;
            // Mischief Mansion
            case "13":
                size = new Vector2(44, 12);
                break;
            // Haunting Hallows
            case "14_0":
            case "14_1":
            case "14_2":
            case "14_3":
            case "14_4":
                size = new Vector2(11, 3);
                if (manager.hasPerfectRank) { scrollSpeed = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * (Random.value < 0.5f ? Random.Range(0.05f, 0.1f) : Random.Range(0.1f, 0.25f)); }
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (wallTiles != null)
        {
            wallTileTimer -= Time.deltaTime;
            if (wallTileTimer < 0)
            {
                wallTileCounter++;
                wallTileCounter %= wallTiles.Length;
                r.material.SetTexture("_WallTile", wallTiles[wallTileCounter]);
                wallTileTimer += wallTileCounter % 2 > 0 ? 0.1f : 0.75f;
            }
        }
        scroll -= scrollSpeed * Time.deltaTime;
        for (int i = 0; i < 2; i++)
        {
            while (scroll[i] < 0) { scroll[i]++; }
            while (scroll[i] >= 1) { scroll[i]--; }
        }
        GetComponent<Renderer>().material.SetVector("_TileSizeOffset", new Vector4(size.x, size.y, scroll.x, scroll.y));
    }
}