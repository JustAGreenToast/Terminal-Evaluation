using UnityEngine;

public class HeatspawnFireScript : MonoBehaviour
{
    [SerializeField] Color[] colors;
    Material[] materials;
    Mesh[] meshes;
    Vector3[] verts;
    bool startHigh;
    bool flippedPalette;
    float t;
    // Start is called before the first frame update
    void Start()
    {
        CreateMaterials();
        CreateVertexData();
        startHigh = Random.value < 0.5f;
        flippedPalette = Random.value < 0.5f;
    }
    // Update is called once per frame
    void Update()
    {
        t -= Time.deltaTime;
        if (t <= 0)
        {
            UpdateVertices();
            if (Random.value < 0.75f) { startHigh = !startHigh; }
            UpdateMaterials();
            flippedPalette = !flippedPalette;
            t = Random.Range(0.05f, 0.08f);
        }
    }
    void CreateMaterials()
    {
        materials = new Material[colors.Length - 1];
        Resolution res = SettingsManager.settings.fullscreen ? Screen.currentResolution : SettingsManager.settings.resolution;
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = new Material(Shader.Find("Unlit/HeatspawnFire"));
            materials[i].SetTexture("_MainTex", Resources.Load<Texture2D>("Sprites/HeatspawnFirePattern"));
            materials[i].SetTextureScale("_MainTex", new Vector2(8, 50));
            materials[i].SetVector("_Resolution", new Vector2(res.width, res.height));
            materials[i].SetColor("_Color1", colors[i]);
            materials[i].SetColor("_Color2", colors[i + 1]);
        }
    }
    void CreateVertexData()
    {
        meshes = new Mesh[transform.childCount];
        for (int i = 0; i < meshes.Length; i++)
        {
            Transform child = transform.GetChild(i);
            meshes[i] = child.GetComponent<MeshFilter>().mesh;
            child.GetComponent<MeshRenderer>().material = materials[i];
        }
        verts = new Vector3[meshes[0].vertexCount];
        for (int i = 0; i < meshes[0].vertexCount; i++) { verts[i] = meshes[0].vertices[i]; }
    }
    void UpdateVertices()
    {
        bool highVert = startHigh;
        for (int i = 1; i < verts.Length; i += 2)
        {
            verts[i].y = highVert ? Random.Range(-0.1f, 0.1f) : Random.Range(0.4f, 0.6f);
            highVert = !highVert;
        }
        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i].vertices = verts;
            meshes[i].RecalculateBounds();
            meshes[i].RecalculateNormals();
        }
    }
    void UpdateMaterials()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            bool colorSwap = i % 2 == 0 == flippedPalette;
            materials[i].SetColor("_Color1", colorSwap ? colors[i + 1] : colors[i]);
            materials[i].SetColor("_Color2", colorSwap ? colors[i] : colors[i + 1]);
        }
    }
}