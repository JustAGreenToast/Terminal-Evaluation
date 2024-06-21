using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SettingsMenuScript : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] Slider masterVol;
    [SerializeField] Slider musicVol;
    [SerializeField] Slider sfxVol;
    [Space]
    [Header("Video Settings")]
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Toggle postProcessToggle;
    [SerializeField] Toggle extendedUIToggle;
    [Space]
    [Header("Gameplay Settings")]
    [SerializeField] TMP_Dropdown guardianAngelDropdown;
    [SerializeField] Sprite[] ashleySprites;
    [SerializeField] Image ashleyIcon;
    [SerializeField] TMP_Dropdown consoleThemeDropdown;
    [SerializeField] Image[] consoleThemeIcons;
    [SerializeField] Toggle doorBodyguardToggle;
    [SerializeField] Sprite[] miriamSprites;
    [SerializeField] Image miriamIcon;
    [SerializeField] Toggle barcodeSaveToggle;
    [SerializeField] Sprite[] barcodeSprites;
    [SerializeField] Image barcodeIcon;
    [SerializeField] Toggle doorStallToggle;
    [SerializeField] Sprite[] midnightSprites;
    [SerializeField] Image midnightIcon;
    [SerializeField] TMP_Dropdown examDifficultyDropdown;
    [SerializeField] TMP_Dropdown mainOfficeTextureSetDropdown;
    [SerializeField] Toggle harderFinalExercisesToggle;
    [Space]
    [Header("Music Panel")]
    [SerializeField] MainMenuScript mainMenu;
    [SerializeField] GameObject musicPanel;
    [SerializeField] GameObject noSongsPanel;
    [SerializeField] Transform songSlotsParent;
    [SerializeField] GameObject songSlotPrefab;
    [SerializeField] GameObject songDeletedPanel;
    Resolution[] resolutions;
    [SerializeField] TMP_Dropdown resolutionsDropdown;
    // Start is called before the first frame update
    void Start()
    {
        resolutions = SettingsManager.settings.GetAvailableResolutions();
        int selectedRes = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            resolutionsDropdown.options.Add(new TMP_Dropdown.OptionData(resolutions[i].ToString()));
            if (SettingsManager.settings.resolution.Equals(resolutions[i])) { selectedRes = i; }
        }
        masterVol.value = SettingsManager.settings.masterVol;
        musicVol.value = SettingsManager.settings.musicVol;
        sfxVol.value = SettingsManager.settings.sfxVol;
        SetResulotion(selectedRes);
        fullscreenToggle.isOn = SettingsManager.settings.fullscreen;
        postProcessToggle.isOn = SettingsManager.settings.postProcess;
        extendedUIToggle.isOn = SettingsManager.settings.extenededUI;
        harderFinalExercisesToggle.isOn = SettingsManager.settings.finalFiveEnabled;
        SetGuardianAngelType((int)SettingsManager.settings.selectedGuardianAngel);
        SetConsoleTheme((int)SettingsManager.settings.selectedConsoleTheme);
        SetDoorBodyguardFlag(SettingsManager.settings.miriamDoor);
        SetBarcodeSaveFlag(SettingsManager.settings.barcodeSave);
        SetDoorStallFlag(SettingsManager.settings.midnightDoor);
        SetExamDifficulty((int)SettingsManager.settings.examDifficulty);
        SetMainOfficeTextureSet(SettingsManager.settings.mainOfficeTextureSet);
        for (int i = 0; i < VirtualRAM.loadedSongs.Count; i++)
        {
            GameObject songSlot = Instantiate(songSlotPrefab, songSlotsParent);
            songSlot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = VirtualRAM.loadedSongs[i].name;
            int n = i;
            songSlot.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => PlaySong(n, false));
            songSlot.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => PlaySong(-1, false));
            songSlot.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => DeleteSongFile(n));
        }
    }
    public void SetMasterVolume(float _val) { SettingsManager.settings.SetMasterVolume(_val); }
    public void SetMusicVolume(float _val) { SettingsManager.settings.SetMusicVolume(_val); }
    public void SetSFXVolume(float _val) { SettingsManager.settings.SetSFXVolume(_val); }
    public void SetResulotion(int _i)
    {
        resolutionDropdown.value = _i;
        SettingsManager.settings.SetResolution(resolutions[_i]);
    }
    public void SetFullscreenFlag(bool _val) { SettingsManager.settings.SetFullscreenFlag(_val); }
    public void SetPostProcessFlag(bool _val) { SettingsManager.settings.SetPostProcessFlag(_val); }
    public void SetExtendedUIFlag(bool _val) { SettingsManager.settings.SetExtendedUIFlag(_val); }
    public void SetGuardianAngelType(int _angelType)
    {
        guardianAngelDropdown.value = _angelType;
        SettingsManager.settings.SelectGuardianAngel((SettingsManager.Settings.GuardianAngels)_angelType);
        ashleyIcon.sprite = ashleySprites[_angelType > 0 ? 1 : 0];
        ashleyIcon.color = _angelType > 0 ? Color.white : new Color(0.2f, 0.2f, 0.2f);
    }
    public void SetConsoleTheme(int _consoleTheme)
    {
        consoleThemeDropdown.value = _consoleTheme;
        SettingsManager.settings.SelectConsoleTheme((SettingsManager.Settings.ConsoleThemes)_consoleTheme);
        for (int i = 0; i < consoleThemeIcons.Length; i++) { consoleThemeIcons[i].color = i == _consoleTheme ? Color.white : new Color(0.2f, 0.2f, 0.2f); }
    }
    public void SetDoorBodyguardFlag(bool _val)
    {
        doorBodyguardToggle.isOn = _val;
        SettingsManager.settings.SetDoorBodyguardFlag(_val);
        miriamIcon.sprite = miriamSprites[_val ? 1 : 0];
        miriamIcon.color = _val ? Color.white : new Color(0.2f, 0.2f, 0.2f);
    }
    public void SetBarcodeSaveFlag(bool _val)
    {
        barcodeSaveToggle.isOn = _val;
        SettingsManager.settings.SetBarcodeSaveFlag(_val);
        barcodeIcon.sprite = barcodeSprites[_val ? 1 : 0];
        barcodeIcon.color = _val ? Color.white : new Color(0.2f, 0.2f, 0.2f);
    }
    public void SetDoorStallFlag(bool _val)
    {
        doorStallToggle.isOn = _val;
        SettingsManager.settings.SetDoorStallFlag(_val);
        midnightIcon.sprite = midnightSprites[_val ? 1 : 0];
        midnightIcon.color = _val ? Color.white : new Color(0.2f, 0.2f, 0.2f);
    }
    public void SetExamDifficulty(int _examDifficulty)
    {
        examDifficultyDropdown.value = _examDifficulty;
        SettingsManager.settings.SelectExamDifficulty((VirtualRAM.ExamData.DifficultyBumps)_examDifficulty);
    }
    public void SetMainOfficeTextureSet(int _textureSet)
    {
        mainOfficeTextureSetDropdown.value = _textureSet;
        SettingsManager.settings.SetMainOfficeTextureSet(_textureSet);
    }
    public void SetFinalFiveFlag(bool _val) { SettingsManager.settings.SetFinalFiveFlag(_val); }
    public void OpenMusicPanel() { if (VirtualRAM.loadedSongs.Count > 0) { musicPanel.gameObject.SetActive(true); } else { noSongsPanel.gameObject.SetActive(true); } }
    public void OpenCustomMusicFolder() { Application.OpenURL(Uri.EscapeUriString("file:///" + Path.Combine(Application.streamingAssetsPath, "music"))); }
    public void PlaySong(int _n) { PlaySong(_n - 1, true); }
    public void PlaySong(int _n, bool _fromDropdown)
    {
        mainMenu.PlaySong(_n);
        for (int i = 0; i < songSlotsParent.childCount; i++)
        {
            bool isStopped = _fromDropdown || i != _n;
            songSlotsParent.GetChild(i).GetChild(1).gameObject.SetActive(isStopped);
            songSlotsParent.GetChild(i).GetChild(2).gameObject.SetActive(!isStopped);
        }
    }
    public void DeleteSongFile(int _n)
    {
        if (songSlotsParent.GetChild(_n).GetChild(2).gameObject.activeInHierarchy) { PlaySong(-1, false); }
        songSlotsParent.GetChild(_n).gameObject.SetActive(false);
        File.Delete(Path.Combine(Application.persistentDataPath, "customSongs", VirtualRAM.loadedSongs[_n].name + ".mp3"));
        songDeletedPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"'{VirtualRAM.loadedSongs[_n].name}' has been deleted successfully!\n\nChanges will be applied after restarting the game.";
        songDeletedPanel.gameObject.SetActive(true);
    }
}