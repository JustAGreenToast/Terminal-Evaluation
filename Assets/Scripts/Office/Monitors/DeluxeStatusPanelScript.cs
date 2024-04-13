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
                slotObj.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{n}. ???";
                for (int i = 1; i < 5; i++) { slotObj.GetChild(i).gameObject.SetActive(false); }
            }
            else
            {
                color = endTime == -1 ? Color.yellow : Color.cyan;
                for (int i = 1; i < 5; i++) { slotObj.GetChild(i).gameObject.SetActive(true); }
                slotObj.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{n}. {exercise.folderName}";
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
        public void ExerciseStarted()
        {
            startTime = Time.time;
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
    [SerializeField] GameObject roadmapSlotPrefab;
    [SerializeField] GameObject gitSlotPrefab;
    // Start is called before the first frame update
    void Start()
    {
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
            roadmapSlots[n] = new RoadmapSlot(n + 1, ex, Instantiate(roadmapSlotPrefab, contentParents[0].transform.GetChild(0).GetChild(0)));
            n++;
        }
        for (int i = _minExercises; i < _exercises.Count; i++) { contentParents[0].transform.GetChild(0).GetChild(0).GetChild(i).gameObject.SetActive(false); }
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
        for (int i = 0; i < contentParents[0].transform.GetChild(0).GetChild(0).childCount; i++) { if (contentParents[0].transform.GetChild(0).GetChild(0).GetChild(i).gameObject.activeSelf) { slotCount++; } }
        contentParents[0].GetComponent<ScrollRect>().normalizedPosition = slotCount > 1 ? new Vector2(0, Mathf.InverseLerp(0, slotCount - 1, roadmapIndex)) : Vector2.up * 0.5f;
    }
    public void ExerciseStarted()
    {
        roadmapSlots[roadmapIndex].ExerciseStarted();
        StringBuilder sb = new StringBuilder();
        // Subject
        sb.AppendLine($"<color=#00ffff>{roadmapSlots[roadmapIndex].exercise.folderName}</color>");
        sb.AppendLine();
        sb.AppendLine();
        sb.Append(roadmapSlots[roadmapIndex].exercise.subject);
        contentParents[1].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = sb.ToString();
        sb.Clear();
        // Extra Data
        sb.Append($"<color=#00ff00>Function Prototypes:</color>");
        if (roadmapSlots[roadmapIndex].exercise.functionPrototypes.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine();
            foreach (string f in roadmapSlots[roadmapIndex].exercise.functionPrototypes) { sb.AppendLine($"- {f}"); }
        }
        else { sb.AppendLine(" (None)"); }
        sb.AppendLine();
        sb.AppendLine();
        sb.Append($"<color=#00ff00>Allowed Functions:</color>");
        if (roadmapSlots[roadmapIndex].exercise.allowedFunctions.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine();
            foreach (string f in roadmapSlots[roadmapIndex].exercise.allowedFunctions) { sb.AppendLine($"- {f}"); }
        }
        else { sb.AppendLine(" (None)"); }
        contentParents[2].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = sb.ToString();
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
        GameObject gitSlot = Instantiate(gitSlotPrefab, contentParents[3].transform.GetChild(0).GetChild(0));
        gitSlot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = roadmapSlots[roadmapIndex].exercise.folderName;
        int i = roadmapIndex;
        gitSlot.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => LoadExercise(i));
    }
    public void Lap2Started() { for (int i = roadmapIndex; i < roadmapSlots.Length; i++) { contentParents[0].transform.GetChild(0).GetChild(0).GetChild(i).gameObject.SetActive(true); } }
    public void LoadExercise(int _i) { mainTerminal.RunCommand($"git load {roadmapSlots[_i].exercise.fileName}"); }
}