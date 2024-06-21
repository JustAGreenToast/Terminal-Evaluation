using UnityEngine;

public class WindowBalloonsScript : MonoBehaviour, IMainOfficeTexturable
{
    // Start is called before the first frame update
    void Start()
    {
        if (VirtualRAM.examData.windowObstacle == VirtualRAM.ExamData.WindowObstacle.Balloons)
        {
            int sign = Random.value > 0.5f ? 1 : -1;
            transform.GetChild(0).localPosition = Vector3.right * 3.5f * sign;
            transform.GetChild(1).localPosition = Vector3.right * 3.5f * -sign;
        }
        else { gameObject.SetActive(false); }
    }

    public void LoadTextures(string _folderName)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>($"Sprites/Main Office Textures/{_folderName}/WindowBalloons");
        if (sprites == null || sprites.Length == 0) { sprites = Resources.LoadAll<Sprite>($"Sprites/Main Office Textures/WindowBalloons_Default"); }
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sprites[0];
        transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = sprites[1];
    }
}