using UnityEngine;
using UnityEngine.UI;

public class GameConsoleImageScript : MonoBehaviour
{
    [System.Serializable]
    public struct ImgData
    {
        public Sprite img;
        public Color color;
        public Vector2 pos;
    };
    public ImgData[] data = new ImgData[4];
    private void Awake()
    {
        int index = (int)SettingsManager.settings.selectedConsoleTheme;
        GetComponent<Image>().sprite = data[index].img;
        GetComponent<Image>().color = data[index].color;
        GetComponent<RectTransform>().localPosition = data[index].pos;
        GetComponent<RectTransform>().sizeDelta = new Vector2(data[index].img.rect.width, data[index].img.rect.height);
    }
}