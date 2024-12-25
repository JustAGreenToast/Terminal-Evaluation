using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] Image overlay;
    [SerializeField] RawImage bgImage;
    [SerializeField] TextMeshProUGUI titleLabel;
    [SerializeField] RectTransform[] mainButtons;
    [SerializeField] GameObject freeplayPanel;
    [SerializeField] Transform[] freeplayButtons;
    [SerializeField] GameObject musicPanel;
    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource soundPlayer;
    const float bgScrollSpeed = 0.2f;
    bool skipMainMenuAnim { get { return Input.anyKeyDown; } }
    Sprite[] rankCards;
    // Start is called before the first frame update
    void Start()
    {
        rankCards = Resources.LoadAll<Sprite>("Sprites/Rank Cards");
        if (SaveManager.saveData.clearedExams >= 5)
        {
            Sprite[] rankIcons = Resources.LoadAll<Sprite>("Sprites/Rank Icons");
            for (int i = 0; i < 5; i++)
            {
                SaveManager.SaveData.ExamData clearData = SaveManager.saveData.clearData[i];
                freeplayButtons[i].GetChild(1).GetComponent<TextMeshProUGUI>().text = TimeUtils.SecondsToTimerString(clearData.bestTime);
                freeplayButtons[i].GetChild(2).GetComponent<Image>().sprite = rankIcons[clearData.bestRank];
            }
            if (SaveManager.saveData.allPRanks)
            {
                SaveManager.SaveData.ExamData clearData = SaveManager.saveData.clearData[5];
                if (clearData.examCleared)
                {

                    freeplayButtons[5].GetChild(1).GetComponent<TextMeshProUGUI>().text = TimeUtils.SecondsToTimerString(clearData.bestTime);
                    freeplayButtons[5].GetChild(2).GetComponent<Image>().sprite = rankIcons[clearData.bestRank];
                }
                else
                {
                    freeplayButtons[5].GetChild(1).GetComponent<TextMeshProUGUI>().text = "--:--.---";
                    freeplayButtons[5].GetChild(2).GetComponent<Image>().enabled = false;
                }
            }
            else
            {
                freeplayButtons[3].localPosition = new Vector2(-250, -200);
                freeplayButtons[4].localPosition = new Vector2(250, -200);
                freeplayButtons[5].gameObject.SetActive(false);
            }
        }
        UpdateMusicVolume();
        UpdateSoundVolume();
        StartCoroutine(MainAnim());
    }
    // Update is called once per frame
    void Update()
    {
        bgImage.uvRect = new Rect(Time.time * bgScrollSpeed, Time.time * bgScrollSpeed, 16, 16);
        if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Z))
        {
            SettingsManager.settings.SetBarcodeAltFlag(!SettingsManager.settings.barcodeAlt);
            SecretFlagToggled(SettingsManager.settings.barcodeAlt);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            SettingsManager.settings.SetDeluxeStatusPanelFlag(!SettingsManager.settings.deluxeStatusPanel);
            SecretFlagToggled(SettingsManager.settings.deluxeStatusPanel);
        }
        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            SettingsManager.settings.SetMidnightAggressiveKnockFlag(!SettingsManager.settings.midnightAggressiveKnock);
            SecretFlagToggled(SettingsManager.settings.midnightAggressiveKnock);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            SettingsManager.settings.SetTetrisCartridgeFlag(!SettingsManager.settings.tetrisCartridge);
            SecretFlagToggled(SettingsManager.settings.tetrisCartridge);
        }
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.C))
        {
            SettingsManager.settings.SetQuadCoreCartridgeFlag(!SettingsManager.settings.quadCoreCartridge);
            SecretFlagToggled(SettingsManager.settings.quadCoreCartridge);
        }
        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.P))
        {
            SettingsManager.settings.SetJesterPearlieFlag(!SettingsManager.settings.jesterPearlie);
            SecretFlagToggled(SettingsManager.settings.jesterPearlie);
        }
        for (int i = 0; i < 6; i++)
        {
            GameObject rankIcon = freeplayButtons[i].GetChild(2).gameObject;
            GameObject rankCard = freeplayButtons[i].GetChild(3).gameObject;
            if (SettingsManager.settings.mainOfficeTextureSet == 14 && SaveManager.saveData.clearData[i].bestRank == 5)
            {
                rankCard.GetComponent<Image>().sprite = rankCards[(int)SettingsManager.settings.selectedGuardianAngel];
                rankIcon.SetActive(false);
                rankCard.SetActive(true);
            }
            else
            {
                rankIcon.SetActive(true);
                rankCard.SetActive(false);
            }
        }
    }
    IEnumerator MainAnim()
    {
        PlaySong(VirtualRAM.titleScreenSong);
        float t = 0;
        while (t < 0.75f)
        {
            t += Time.deltaTime;
            if (skipMainMenuAnim) { break; }
            yield return null;
        }
        // Fade In
        t = 0;
        while (t < 1)
        {
            t += 2.5f * Time.deltaTime;
            overlay.color = Color.Lerp(Color.black, Color.clear, t);
            if (skipMainMenuAnim) { break; }
            yield return null;
        }
        overlay.enabled = false;
        t = 0;
        while (t < 0.75f)
        {
            t += Time.deltaTime;
            if (skipMainMenuAnim) { break; }
            yield return null;
        }
        // Title
        string titleText = "Terminal Evaluation";
        for (int i = 0; i < titleText.Length; i++)
        {
            titleLabel.text = "> " + titleText.Substring(0, i + 1);
            t = 0;
            while (t < 0.05f)
            {
                t += Time.deltaTime;
                if (skipMainMenuAnim) { break; }
                yield return null;
            }
        }
        t = 0;
        while (t < 0.75f)
        {
            t += Time.deltaTime;
            if (skipMainMenuAnim) { break; }
            yield return null;
        }
        // Main Buttons
        float y = -25;
        for (int i = 0; i < 4; i++)
        {
            if (i == 1 && SaveManager.saveData.clearedExams < 5) { continue; }
            StartCoroutine(ButtonFadeIn(mainButtons[i], new Vector2(-500, y)));
            y -= 125f;
            t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                if (skipMainMenuAnim) { break; }
                yield return null;
            }
        }
        // Help Button
        StartCoroutine(ButtonFadeIn(mainButtons[4], new Vector2(750, 300)));
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            if (skipMainMenuAnim) { break; }
            yield return null;
        }
        t = 0;
        while (t < 1)
        {
            t += 1.5f * Time.deltaTime;
            bgImage.color = new Color(1, 1, 1, t);
            if (skipMainMenuAnim) { break; }
            yield return null;
        }
        bgImage.color = Color.white;
        if (skipMainMenuAnim)
        {
            overlay.color = Color.white;
            overlay.enabled = true;
            t = 0;
            while (t < 1)
            {
                t += 2.5f * Time.deltaTime;
                overlay.color = Color.Lerp(Color.white, Color.clear, t);
                yield return null;
            }
            overlay.enabled = false;
        }
    }
    IEnumerator ButtonFadeIn(RectTransform _button, Vector2 _targetPos)
    {
        Vector2 startPos = _button.localPosition;
        float t = 0;
        while (t < 1)
        {
            t += 1.5f * Time.deltaTime;
            _button.localPosition = new Vector2(Mathf.Lerp(startPos.x, _targetPos.x, Mathf.Sin(Mathf.Clamp01(t) * 90 * Mathf.Deg2Rad)), _targetPos.y);
            if (skipMainMenuAnim) { break; }
            yield return null;
        }
        _button.localPosition = _targetPos;
    }
    public void MainGameSelected() { if (SaveManager.saveData.clearedExams < 5) { SelectExamPreset(SaveManager.saveData.clearedExams); } else { freeplayPanel.SetActive(true); } }
    public void SelectExamPreset(int _examIndex)
    {
        VirtualRAM.examData.LoadPreset((VirtualRAM.ExamData.Presets)_examIndex);
        if (VirtualRAM.loadedSongs.Count > 0) { musicPanel.SetActive(true); } else { StartExam(); }
    }
    public void SelectLap1Song(int _index) { VirtualRAM.lap1Song = _index - 1; }
    public void SelectLap2Song(int _index) { VirtualRAM.lap2Song = _index - 1; }
    public void SelectTitleScreenSong(int _index) { VirtualRAM.titleScreenSong = _index - 1; }
    public void PlaySong(int _n)
    {
        if (_n < 0) { musicPlayer.Stop(); }
        else
        {
            musicPlayer.clip = VirtualRAM.loadedSongs[_n];
            musicPlayer.Play();
        }
    }
    public void UpdateMusicVolume() { musicPlayer.volume = SettingsManager.settings.masterVol * SettingsManager.settings.musicVol; }
    public void UpdateSoundVolume() { soundPlayer.volume = SettingsManager.settings.masterVol * SettingsManager.settings.sfxVol; }
    public void StartExam()
    {
        VirtualRAM.ResetTournamentData();
        SceneManager.LoadScene(2);
    }
    public void Quit() { Application.Quit(); }
    void SecretFlagToggled(bool _enabled)
    {
        soundPlayer.clip = Resources.Load<AudioClip>($"SFX/secret_{(_enabled ? "on" : "off")}");
        soundPlayer.Play();
    }
}