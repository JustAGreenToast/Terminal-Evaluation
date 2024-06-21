using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeluxeStatusPanelScript : MonoBehaviour
{
    [SerializeField] MainTerminalScript mainTerminal;
    [SerializeField] Image[] sectionButtons;
    [SerializeField] Sprite[] sectionIcons;
    [SerializeField] GameObject[] contentParents;
    class RoadmapSlot
    {
        int n;
        public Exercise exercise { get; private set; }
        float startTime;
        float endTime;
        int failCount;
        string overrideName = null;
        Transform slotObj;
        public RoadmapSlot(int _n, Exercise _ex, GameObject _slotObj)
        {
            n = _n;
            exercise = _ex;
            startTime = -1;
            endTime = -1;
            failCount = 0;
            slotObj = _slotObj.transform;
        }
        public void UpdateDisplay()
        {
            Color color;
            if (startTime == -1)
            {
                color = Color.magenta;
#if UNITY_EDITOR
                slotObj.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{n}. {exercise.folderName}";
#else
                slotObj.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{n}. ???";
#endif
                for (int i = 1; i < 5; i++) { slotObj.GetChild(i).gameObject.SetActive(false); }
            }
            else
            {
                color = endTime == -1 ? Color.yellow : Color.cyan;
                for (int i = 1; i < 5; i++) { slotObj.GetChild(i).gameObject.SetActive(true); }
                slotObj.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{n}. {(string.IsNullOrEmpty(overrideName) ? exercise.folderName : overrideName)}";
                slotObj.GetChild(2).GetComponent<TextMeshProUGUI>().text = TimeUtils.SecondsToTimerString((endTime != -1 ? endTime : Time.time) - startTime);
                slotObj.GetChild(4).GetComponent<TextMeshProUGUI>().text = failCount.ToString();
            }
            slotObj.GetComponent<Image>().color = color;
            slotObj.GetChild(0).GetComponent<TextMeshProUGUI>().color = color;
            slotObj.GetChild(1).GetComponent<Image>().color = color;
            slotObj.GetChild(2).GetComponent<TextMeshProUGUI>().color = color;
            slotObj.GetChild(3).GetComponent<Image>().color = color;
            slotObj.GetChild(4).GetComponent<TextMeshProUGUI>().color = color;
        }
        public void ExerciseStarted(string _overrideName = null)
        {
            startTime = Time.time;
            if (!string.IsNullOrEmpty(_overrideName)) { overrideName = _overrideName; }
            UpdateDisplay();
        }
        public void ExerciseFailed()
        {
            failCount++;
            UpdateDisplay();
        }
        public void ExerciseCleared()
        {
            endTime = Time.time;
            UpdateDisplay();
        }
    }
    RoadmapSlot[] roadmapSlots;
    int roadmapIndex;
    Exercise currentExercise { get { return mainTerminal.currentExercise; } }
    [SerializeField] Transform roadmapPanel;
    [SerializeField] GameObject roadmapSlotPrefab;
    [SerializeField] TextMeshProUGUI subjectTextbox;
    [SerializeField] GameObject testCasesPanel;
    bool testCasesPanelEnabled { get { return (VirtualRAM.isInTournamentMode && VirtualRAM.tournamentData.reverseEngineer) || VirtualRAM.examData.examIndex == 13; } }
    //bool testCasesPanelEnabled { get { return true; } }
    [SerializeField] TextMeshProUGUI testCasesPanelTitle;
    [SerializeField] Transform testCasesPanelSlots;
    [SerializeField] GameObject testCasesPanelSlotPrefab;
    [SerializeField] TextMeshProUGUI testCaseTextbox;
    [SerializeField] TextMeshProUGUI dataTextbox;
    [SerializeField] Transform gitPanel;
    [SerializeField] GameObject gitSlotPrefab;
    // Start is called before the first frame update
    void Start()
    {
        testCasesPanel.SetActive(testCasesPanelEnabled);
        subjectTextbox.enabled = !testCasesPanelEnabled;
        SelectSection(1);
    }
    // Update is called once per frame
    void Update()
    {
        if (roadmapIndex < roadmapSlots.Length) { roadmapSlots[roadmapIndex].UpdateDisplay(); }
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    print(contentParents[0].GetComponent<ScrollRect>().normalizedPosition);
        //    print(roadmapIndex);
        //    print((float)roadmapSlots.Length - 1);
        //    print(roadmapIndex / ((float)roadmapSlots.Length - 1));
        //}
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void Show()
    {
        CenterRoadmap();
        gameObject.SetActive(true);
    }
    public void SelectSection(int _section)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i == _section)
            {
                sectionButtons[i].sprite = sectionIcons[i + 4];
                contentParents[i].SetActive(true);
            }
            else
            {
                sectionButtons[i].sprite = sectionIcons[i];
                contentParents[i].SetActive(false);
            }
        }
        if (_section == 0) { CenterRoadmap(); }
    }
    public void InitRoadmap(LinkedList<Exercise> _exercises, int _minExercises)
    {
        roadmapSlots = new RoadmapSlot[_exercises.Count];
        int n = 0;
        foreach (Exercise ex in _exercises)
        {
            roadmapSlots[n] = new RoadmapSlot(n + 1, ex, Instantiate(roadmapSlotPrefab, roadmapPanel));
            n++;
        }
        for (int i = _minExercises; i < _exercises.Count; i++) { roadmapPanel.GetChild(i).gameObject.SetActive(false); }
        UpdateRoadmap();
    }
    void UpdateRoadmap()
    {
        foreach (RoadmapSlot slot in roadmapSlots) { slot.UpdateDisplay(); }
        CenterRoadmap();
    }
    void CenterRoadmap()
    {
        int slotCount = 0;
        for (int i = 0; i < roadmapPanel.childCount; i++) { if (roadmapPanel.GetChild(i).gameObject.activeSelf) { slotCount++; } }
        contentParents[0].GetComponent<ScrollRect>().normalizedPosition = slotCount > 1 ? new Vector2(0, Mathf.InverseLerp(0, slotCount - 1, roadmapIndex)) : Vector2.up * 0.5f;
    }
    public void ExerciseStarted()
    {
        roadmapSlots[roadmapIndex].ExerciseStarted(testCasesPanelEnabled ? mainTerminal.currentExerciseFolderName : null);
        StringBuilder sb = new StringBuilder();
        // Subject
        sb.AppendLine($"<color=#00ffff>{mainTerminal.currentExerciseFolderName}</color>");
        sb.AppendLine();
        sb.AppendLine();
        sb.Append(currentExercise.subject);
        subjectTextbox.GetComponent<TextMeshProUGUI>().text = sb.ToString();
        sb.Clear();
        // Test Cases
        testCasesPanelTitle.text = $"<color=#00ffff>{mainTerminal.currentExerciseFolderName}</color>";
        for (int i = testCasesPanelSlots.childCount; i < currentExercise.testCases.Length; i++)
        {
            GameObject slotObj = Instantiate(testCasesPanelSlotPrefab, testCasesPanelSlots);
            int n = i;
            slotObj.GetComponent<Button>().onClick.AddListener(() => SelectTestCase(n));
            slotObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (n + 1).ToString();
        }
        for (int i = 0; i < testCasesPanelSlots.childCount; i++) { testCasesPanelSlots.GetChild(i).gameObject.SetActive(i < currentExercise.testCases.Length); }
        SelectTestCase(0);
        // Extra Data
        sb.Append($"<color=#00ff00>Function Prototypes:</color>");
        if (currentExercise.functionPrototypes.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine();
            foreach (string f in currentExercise.functionPrototypes) { sb.AppendLine($"- {f}"); }
        }
        else { sb.AppendLine(" (None)"); }
        sb.AppendLine();
        sb.AppendLine();
        sb.Append($"<color=#00ff00>Allowed Functions:</color>");
        if (currentExercise.allowedFunctions.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine();
            foreach (string f in currentExercise.allowedFunctions) { sb.AppendLine($"- {f}"); }
        }
        else { sb.AppendLine(" (None)"); }
        dataTextbox.GetComponent<TextMeshProUGUI>().text = sb.ToString();
    }
    public void ExerciseCleared()
    {
        roadmapSlots[roadmapIndex].ExerciseCleared();
        roadmapIndex++;
        if (roadmapIndex < roadmapSlots.Length) { ExerciseStarted(); }
    }
    public void ExerciseFailed() { roadmapSlots[roadmapIndex].ExerciseFailed(); }
    public void GitScriptAdded()
    {
        GameObject gitSlot = Instantiate(gitSlotPrefab, gitPanel);
        gitSlot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = mainTerminal.currentExerciseFileName;
        int i = roadmapIndex;
        gitSlot.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => LoadExercise(i));
    }
    public void Lap2Started() { for (int i = roadmapIndex; i < roadmapSlots.Length; i++) { roadmapPanel.GetChild(i).gameObject.SetActive(true); } }
    public void LoadExercise(int _i) { mainTerminal.RunCommand($"git load {roadmapSlots[_i].exercise.fileName}"); }
    public void SelectTestCase(int _i)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<color=#ffff00>[IN]</color>    ");
        sb.AppendLine(currentExercise.testCases[_i].args);
        sb.AppendLine();
        sb.Append("<color=#00ffff>[OUT]</color>   ");
        sb.Append(currentExercise.testCases[_i].output);
        testCaseTextbox.text = sb.ToString();
        for (int i = 0; i < currentExercise.testCases.Length; i++)
        {
            Color c = i == _i ? Color.white : Color.cyan;
            testCasesPanelSlots.GetChild(i).GetComponent<Image>().color = c;
            testCasesPanelSlots.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().color = c;
        }
    }
}