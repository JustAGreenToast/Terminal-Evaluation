using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SFXSubtitlesUIScript : MonoBehaviour
{
    class Subtitle
    {
        public string text { get; private set; }
        public bool leftSpeaker { get; private set; }
        public bool rightSpeaker { get; private set; }
        public float t;
        public Subtitle(string _text, bool _leftSpeaker, bool _rightSpeaker)
        {
            text = _text;
            leftSpeaker = _leftSpeaker;
            rightSpeaker = _rightSpeaker;
            t = 7.5f;
        }
        public bool IsRepeat(Subtitle _other) { return text == _other.text && leftSpeaker == _other.leftSpeaker && rightSpeaker == _other.rightSpeaker; }
    }
    RectTransform panel;
    List<Subtitle> subtitles;
    float alphaFade;
    CanvasGroup groupAlpha;
    // Start is called before the first frame update
    void Start()
    {
        panel = GetComponent<RectTransform>();
        subtitles = new List<Subtitle>();
        groupAlpha = GetComponent<CanvasGroup>();
    }
    // Update is called once per frame
    void Update()
    {
        if (alphaFade < 1)
        {
            alphaFade += Time.deltaTime;
            groupAlpha.alpha = alphaFade;
        }
        for (int i = subtitles.Count - 1; i >= 0; i--)
        {
            subtitles[i].t -= 2.5f * Time.deltaTime;
            if (subtitles[i].t <= 0) { subtitles.RemoveAt(i); }
        }
        for (int i = 0; i < subtitles.Count; i++)
        {
            transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text = subtitles[i].text;
            transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, Mathf.Clamp01(subtitles[i].t));
            transform.GetChild(i).GetChild(1).GetComponent<Image>().color = subtitles[i].leftSpeaker ? Color.white : new Color(0.2f, 0.2f, 0.2f);
            transform.GetChild(i).GetChild(2).GetComponent<Image>().color = subtitles[i].rightSpeaker ? Color.white : new Color(0.2f, 0.2f, 0.2f);
            transform.GetChild(i).gameObject.SetActive(true);
        }
        for (int i = subtitles.Count; i < transform.childCount; i++) { transform.GetChild(i).gameObject.SetActive(false); }
        panel.sizeDelta = new Vector2(576, 48 * (subtitles.Count + 1));
    }
    public void AddSubtitle(string _text, bool _leftSpeaker, bool _rightSpeaker)
    {
        subtitles.Insert(0, new Subtitle(_text, _leftSpeaker, _rightSpeaker));
        for (int i = 1; i < subtitles.Count; i++) { if (subtitles[i].IsRepeat(subtitles[0])) { subtitles.RemoveAt(i); } }
        while (subtitles.Count > 8) { subtitles.RemoveAt(8); }
    }
}