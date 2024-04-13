using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuadCoreScript : ConsoleWindow
{
    [SerializeField] RectTransform bars;
    [SerializeField] RectTransform comboIndicators;
    const int roundsPerSet = 1;
    int roundsLeft;
    float timeScale { get { return Mathf.Clamp(Mathf.Lerp(0.75f, 0.25f, aiLevel), 0.25f, 0.4f); } }
    float updateDelay { get { return 1 * timeScale; } }
    float updateTimer;
    List<int> barPos = new List<int>();
    bool flipDir;
    protected override void OnStart()
    {
        DrawScreen();
    }
    protected override void OnUpdate()
    {
        if (roundsLeft == 0) { return; }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (barPos[0] == 4) { barPos.RemoveAt(0); } else { SpawnBars(); }
            DrawScreen();
        }
        if (barPos.Count == 0)
        {
            roundsLeft--;
            if (roundsLeft % roundsPerSet == 0) { RoundSetCleared(); }
            if (roundsLeft > 0) { SpawnBars(); }
            DrawScreen();
        }
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0)
        {
            for (int i = 0; i < barPos.Count; i++)
            {
                barPos[i]++;
                if (barPos[i] > 4)
                {
                    SpawnBars();
                    break;
                }
            }
            DrawScreen();
            updateTimer = updateDelay;
        }
    }
    void DrawScreen()
    {
        for (int i = 0; i < bars.childCount; i++) { bars.GetChild(i).GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f); }
        for (int i = 0; i < barPos.Count; i++)
        {
            if (barPos[i] < 0) { break; }
            bars.GetChild(flipDir ? 8 - barPos[i] : barPos[i]).GetComponent<Image>().color = Color.white;
        }
        for (int i = 0; i < 4; i++) { comboIndicators.GetChild(i).GetComponent<Image>().color = roundsLeft > 0 && 4 - i > barPos.Count ? Color.white : new Color(0.25f, 0.25f, 0.25f); }
    }
    public override void AddRounds()
    {
        if (roundsLeft == 0)
        {
            updateTimer = updateDelay;
            SpawnBars();
        }
        roundsLeft += roundsPerSet;
    }
    void SpawnBars()
    {
        barPos.Clear();
        int[] pattern = GetBarPattern();
        for (int i = 0; i < pattern.Length; i++) { barPos.Add(-(pattern[i] + 1)); }
        flipDir = Random.value > 0.5f;
    }
    int[] GetBarPattern()
    {
        switch (Random.Range(0, 4))
        {
            case 0: return new int[4] { 0, 2, 4, 6 };
            case 1: return new int[4] { 0, 1, 3, 4 };
            case 2: return new int[4] { 0, 1, 2, 3 };
            default:
                int[] pattern = new int[4];
                int pos = 0;
                for (int i = 0; i < 4; i++)
                {
                    pattern[i] = pos;
                    pos++;
                    for (int j = 0; j < 2 && Random.value > 0.5f; j++) { pos++; }
                }
                return pattern;
        }
    }
    public override void OnPullUp()
    {
        DrawScreen();
    }
}