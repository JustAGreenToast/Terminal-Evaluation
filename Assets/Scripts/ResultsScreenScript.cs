using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultsScreenScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerLabel;
    [SerializeField] Image rankIcon;
    [SerializeField] Image rankCard;
    [SerializeField] Image overlay;
    // Start is called before the first frame update
    void Start()
    {
        timerLabel.text = TimeUtils.SecondsToTimerString(VirtualRAM.clearTime);
        timerLabel.enabled = false;
        rankIcon.sprite = Resources.LoadAll<Sprite>("Sprites/Rank Icons")[VirtualRAM.clearRank];
        rankIcon.enabled = false;
        rankCard.sprite = Resources.LoadAll<Sprite>("Sprites/Rank Cards")[(int)SettingsManager.settings.selectedGuardianAngel];
        rankCard.enabled = false;
        StartCoroutine(DisplayResults());
    }
    IEnumerator DisplayResults()
    {
        AudioSource sfx = GetComponent<AudioSource>();
        sfx.volume = SettingsManager.settings.masterVol * SettingsManager.settings.sfxVol;
        yield return new WaitForSeconds(1);
        float t = 0;
        while (t < 1)
        {
            t += 2.5f * Time.deltaTime;
            overlay.color = Color.Lerp(Color.black, Color.clear, t);
            yield return null;
        }
        overlay.enabled = false;
        yield return new WaitForSeconds(0.5f);
        timerLabel.enabled = true;
        sfx.Play();
        yield return new WaitForSeconds(0.5f);
        if (VirtualRAM.clearRank > 4 && SettingsManager.settings.mainOfficeTextureSet == 14) { rankCard.enabled = true; } else { rankIcon.enabled = true; }
        sfx.Play();
        yield return new WaitForSeconds(0.5f);
        transform.GetChild(6).gameObject.SetActive(true);
        transform.GetChild(7).gameObject.SetActive(true);
        sfx.Play();
    }
    public void Continue()
    {
        if (VirtualRAM.examData.examIndex < 4)
        {
            VirtualRAM.examData.LoadPreset((VirtualRAM.ExamData.Presets)(VirtualRAM.examData.examIndex + 1));
            SceneManager.LoadScene(2);
        }
        else { Quit(); }
    }
    public void Quit() { SceneManager.LoadScene(1); }
}