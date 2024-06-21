using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LastDanceMenuScript : MonoBehaviour
{
    [SerializeField] MainMenuScript mainMenu;
    [SerializeField] Slider exerciseCountSlider;
    [SerializeField] Transform noSongsPanel;
    VirtualRAM.ExamData.DifficultyBumps examDifficulty
    {
        get
        {
            if (SettingsManager.settings.examDifficulty == VirtualRAM.ExamData.DifficultyBumps.Auto) { return VirtualRAM.ExamData.DifficultyBumps.Advanced; }
            return SettingsManager.settings.examDifficulty;
        }
    }
    int exerciseCount;
    int[] aiLevels = new int[12];
    bool tiredMidnight;
    VirtualRAM.ExamData.WindowObstacle windowObstacle;
    bool endlessMode;
    private void OnEnable()
    {
        int maxVal = new int[4] { 15, 10, 13, 5 }[(int)examDifficulty - 1];
        exerciseCountSlider.maxValue = maxVal;
        if (exerciseCountSlider.value > maxVal) { exerciseCountSlider.value = maxVal; }
        SetExerciseCount(exerciseCountSlider.value);
    }
    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(1).GetChild(9).gameObject.SetActive(SaveManager.saveData.allPRanks);
        transform.GetChild(1).GetChild(10).gameObject.SetActive(SaveManager.saveData.allSRanks);
        UpdateAILevels();
        noSongsPanel.gameObject.SetActive(VirtualRAM.loadedSongs.Count == 0);
    }
    // Update is called once per frame
    void Update()
    {
        // Final Five
        if ((Input.GetKey(KeyCode.Alpha4) || Input.GetKey(KeyCode.Keypad4)) && (Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Keypad2)))
        {
            VirtualRAM.examData.FinalFiveLastDance(aiLevels, tiredMidnight, windowObstacle, endlessMode);
            mainMenu.StartExam();
        }
        // Single Difficulty
        else if (Input.GetKey(KeyCode.S) && GetPressedDigit(7) != -1)
        {
            VirtualRAM.examData.SingleDifficultyLastDance(GetPressedDigit(7) - 1, aiLevels, tiredMidnight, windowObstacle, endlessMode);
            mainMenu.StartExam();
        }
        // Roll
        else if (Input.GetKey(KeyCode.R) && GetPressedDigit(5) != -1)
        {
            VirtualRAM.examData.RollLastDance(GetPressedDigit(5), aiLevels, tiredMidnight, windowObstacle, Input.GetKey(KeyCode.LeftShift));
            mainMenu.StartExam();
        }
        // All-Star Jr
        else if ((Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Keypad1)) && (Input.GetKey(KeyCode.Alpha0) || Input.GetKey(KeyCode.Keypad0)))
        {
            VirtualRAM.examData.AllStarLastDance(tiredMidnight, windowObstacle, endlessMode);
            mainMenu.StartExam();
        }
        // Reverse Engineer
        else if (Input.GetKey(KeyCode.R) && (Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.LeftArrow)))
        {
            VirtualRAM.examData.ReverseEngineerLastDance(exerciseCount, examDifficulty, aiLevels, tiredMidnight, windowObstacle, endlessMode);
            mainMenu.StartExam();
        }
    }
    int GetPressedDigit(int _max)
    {
        for (int i = 1; i <= _max && i < 10; i++) { if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i)) { return i; } }
        return -1;
    }
    public void UpdateAILevels()
    {
        for (int i = 0; i < 12; i++)
        {
            aiLevels[i] = transform.GetChild(1).GetChild(i).gameObject.activeInHierarchy ? (int)transform.GetChild(1).GetChild(i).GetComponentInChildren<Slider>().value : 0;
            transform.GetChild(1).GetChild(i).GetComponentInChildren<Image>().color = aiLevels[i] == 10 ? Color.white : aiLevels[i] == 0 ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.9f, 0.9f, 0.9f);
            transform.GetChild(1).GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = aiLevels[i] == 10 ? "<size=48>*</size>" : aiLevels[i].ToString();
        }
    }
    public void OverrideAILevels(float n) { for (int i = 0; i < 12; i++) { if (transform.GetChild(1).GetChild(i).gameObject.activeInHierarchy) { transform.GetChild(1).GetChild(i).GetComponentInChildren<Slider>().value = n; } } }
    public void SetExerciseCount(float n)
    {
        exerciseCount = (int)n;
        transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"Exercises: {exerciseCount}";
    }
    public void SetTiredMidnightFlag(bool val) { tiredMidnight = val; }
    public void SelectWindowObstacle(int n) { windowObstacle = (VirtualRAM.ExamData.WindowObstacle)n; }
    public void SetEndlessModeFlag(bool val) { endlessMode = val; }
    public void SelectSong(int n) { VirtualRAM.lastDanceSong = n - 1; }
    public void StartExam()
    {
        VirtualRAM.examData.RegularLastDance(exerciseCount, examDifficulty, aiLevels, tiredMidnight, windowObstacle, endlessMode);
        mainMenu.StartExam();
    }
    public void OpenCustomMusicFolder() { Application.OpenURL(Uri.EscapeUriString("file:///" + Path.Combine(Application.streamingAssetsPath, "music"))); }
}