using System.Collections.Generic;
using UnityEngine;

public static class VirtualRAM
{
    public static ExerciseSet exercises;
    public static List<AudioClip> loadedSongs;
    public class ExamData
    {
        public enum Presets { Exam1, Exam2, Exam3, Exam4, Exam5, FinalExam, AllStar };
        public int examIndex { get; private set; }
        public int minExercises { get; private set; }
        public int totalExercises { get; private set; }
        public enum DifficultyBumps { Auto, Original, Quick, Advanced, Casual };
        public DifficultyBumps progression { get; private set; }
        public int[] aiLevels { get; private set; }
        public bool tiredMidnight { get; private set; }
        public enum WindowObstacle { None, Balloons, Blinders };
        public WindowObstacle windowObstacle { get; private set; }
        public bool endless { get; private set; }
        public bool rollBank { get; private set; }
        public void LoadPreset(Presets _preset)
        {
            DifficultyBumps defaultProgression = SettingsManager.settings.examDifficulty;
            if (defaultProgression == DifficultyBumps.Auto && SaveManager.saveData.clearedExams < 5) { defaultProgression = DifficultyBumps.Casual; }
            switch (_preset)
            {
                case Presets.Exam1:
                    examIndex = 0;
                    progression = defaultProgression != DifficultyBumps.Auto ? defaultProgression : DifficultyBumps.Original;
                    minExercises = new int[4] { 1, 1, 1, 1 }[(int)progression - 1];
                    totalExercises = new int[4] { 3, 2, 2, 1 }[(int)progression - 1];
                    aiLevels = new int[12];
                    aiLevels[(int)EnemyScript.EnemyTypes.Chelsea] = 1;
                    aiLevels[(int)EnemyScript.EnemyTypes.Cupcake] = 1;
                    aiLevels[(int)EnemyScript.EnemyTypes.Midnight] = 1;
                    tiredMidnight = false;
                    windowObstacle = WindowObstacle.None;
                    break;
                case Presets.Exam2:
                    examIndex = 1;
                    progression = defaultProgression != DifficultyBumps.Auto ? defaultProgression : DifficultyBumps.Original;
                    minExercises = new int[4] { 3, 2, 2, 2 }[(int)progression - 1];
                    totalExercises = new int[4] { 5, 3, 4, 2 }[(int)progression - 1];
                    aiLevels = new int[12];
                    aiLevels[(int)EnemyScript.EnemyTypes.Chelsea] = 2;
                    aiLevels[(int)EnemyScript.EnemyTypes.Cupcake] = 2;
                    aiLevels[(int)EnemyScript.EnemyTypes.Midnight] = 2;
                    aiLevels[(int)EnemyScript.EnemyTypes.Melissa] = 1;
                    aiLevels[(int)EnemyScript.EnemyTypes.Carla] = 1;
                    tiredMidnight = false;
                    windowObstacle = WindowObstacle.None;
                    break;
                case Presets.Exam3:
                    examIndex = 2;
                    progression = defaultProgression != DifficultyBumps.Auto ? defaultProgression : DifficultyBumps.Original;
                    minExercises = new int[4] { 5, 4, 4, 3 }[(int)progression - 1];
                    totalExercises = new int[4] { 7, 5, 6, 3 }[(int)progression - 1];
                    aiLevels = new int[12];
                    aiLevels[(int)EnemyScript.EnemyTypes.Chelsea] = 3;
                    aiLevels[(int)EnemyScript.EnemyTypes.Cupcake] = 3;
                    aiLevels[(int)EnemyScript.EnemyTypes.Midnight] = 3;
                    aiLevels[(int)EnemyScript.EnemyTypes.Melissa] = 2;
                    aiLevels[(int)EnemyScript.EnemyTypes.Barcode] = 1;
                    aiLevels[(int)EnemyScript.EnemyTypes.Carla] = 2;
                    aiLevels[(int)EnemyScript.EnemyTypes.Aqua] = 1;
                    tiredMidnight = false;
                    windowObstacle = WindowObstacle.None;
                    break;
                case Presets.Exam4:
                    examIndex = 3;
                    progression = defaultProgression != DifficultyBumps.Auto ? defaultProgression : DifficultyBumps.Advanced;
                    minExercises = new int[4] { 7, 6, 7, 4 }[(int)progression - 1];
                    totalExercises = new int[4] { 10, 8, 9, 4 }[(int)progression - 1];
                    aiLevels = new int[12];
                    aiLevels[(int)EnemyScript.EnemyTypes.Chelsea] = 5;
                    aiLevels[(int)EnemyScript.EnemyTypes.Cupcake] = 5;
                    aiLevels[(int)EnemyScript.EnemyTypes.Midnight] = 2;
                    aiLevels[(int)EnemyScript.EnemyTypes.Melissa] = 4;
                    aiLevels[(int)EnemyScript.EnemyTypes.Barcode] = 3;
                    aiLevels[(int)EnemyScript.EnemyTypes.Carla] = 3;
                    aiLevels[(int)EnemyScript.EnemyTypes.Aqua] = 3;
                    aiLevels[(int)EnemyScript.EnemyTypes.H41] = 1;
                    tiredMidnight = true;
                    windowObstacle = WindowObstacle.None;
                    break;
                case Presets.Exam5:
                    examIndex = 4;
                    progression = defaultProgression != DifficultyBumps.Auto ? defaultProgression : DifficultyBumps.Advanced;
                    minExercises = new int[4] { 10, 8, 10, 5 }[(int)progression - 1];
                    totalExercises = new int[4] { 15, 10, 13, 5 }[(int)progression - 1];
                    aiLevels = new int[12];
                    aiLevels[(int)EnemyScript.EnemyTypes.Chelsea] = 7;
                    aiLevels[(int)EnemyScript.EnemyTypes.Cupcake] = 7;
                    aiLevels[(int)EnemyScript.EnemyTypes.Midnight] = 4;
                    aiLevels[(int)EnemyScript.EnemyTypes.Melissa] = 5;
                    aiLevels[(int)EnemyScript.EnemyTypes.Barcode] = 5;
                    aiLevels[(int)EnemyScript.EnemyTypes.Carla] = 4;
                    aiLevels[(int)EnemyScript.EnemyTypes.Aqua] = 4;
                    aiLevels[(int)EnemyScript.EnemyTypes.H41] = 3;
                    tiredMidnight = true;
                    windowObstacle = WindowObstacle.None;
                    break;
                case Presets.AllStar:
                    examIndex = 6;
                    progression = defaultProgression != DifficultyBumps.Auto ? defaultProgression : DifficultyBumps.Quick;
                    minExercises = new int[4] { 15, 10, 13, 5 }[(int)progression - 1];
                    totalExercises = minExercises;
                    aiLevels = new int[12];
                    for (int i = 0; i < 12; i++) { aiLevels[i] = 10; }
                    tiredMidnight = true;
                    windowObstacle = WindowObstacle.None;
                    break;
                default:
                    Debug.Log($"Error: case {_preset} not defined!");
                    break;
            }
        }
        public void RegularLastDance(int _exercises, DifficultyBumps _progression, int[] _aiLevels, bool _tiredMidnight, WindowObstacle _windowObstacle, bool _endlessMode)
        {
            examIndex = 8;
            minExercises = _exercises;
            totalExercises = _exercises;
            progression = _progression;
            aiLevels = _aiLevels;
            tiredMidnight = _tiredMidnight;
            windowObstacle = _windowObstacle;
            endless = _endlessMode;
        }
        public void FinalFiveLastDance(int[] _aiLevels, bool _tiredMidnight, WindowObstacle _windowObstacle, bool _endlessMode)
        {
            examIndex = 9;
            minExercises = 5;
            totalExercises = 5;
            progression = DifficultyBumps.Original;
            aiLevels = _aiLevels;
            tiredMidnight = _tiredMidnight;
            windowObstacle = _windowObstacle;
            endless = _endlessMode;
        }
        public void SingleDifficultyLastDance(int _difficulty, int[] _aiLevels, bool _tiredMidnight, WindowObstacle _windowObstacle, bool _endlessMode)
        {
            examIndex = 10;
            minExercises = _difficulty;
            totalExercises = _difficulty;
            progression = DifficultyBumps.Original;
            aiLevels = _aiLevels;
            tiredMidnight = _tiredMidnight;
            windowObstacle = _windowObstacle;
            endless = _endlessMode;
        }
        public void RollLastDance(int _difficulty, int[] _aiLevels, bool _tiredMidnight, WindowObstacle _windowObstacle, bool _rollBank)
        {
            examIndex = 11;
            minExercises = _difficulty;
            totalExercises = _difficulty * 2;
            aiLevels = _aiLevels;
            tiredMidnight = _tiredMidnight;
            windowObstacle = _windowObstacle;
            endless = true;
            rollBank = _rollBank;
        }
        public void AllStarLastDance(bool _tiredMidnight, WindowObstacle _windowObstacle, bool _endlessMode)
        {
            examIndex = 12;
            minExercises = 10;
            totalExercises = 10;
            progression = DifficultyBumps.Quick;
            aiLevels = new int[12];
            tiredMidnight = _tiredMidnight;
            windowObstacle = _windowObstacle;
            endless = _endlessMode;
        }
    }
    public static ExamData examData { get; private set; } = new ExamData();
    public enum SongRegisters { TitleScreen, Lap1, Lap2, LastDance };
    public static int[] songIndices = new int[4] { -1, -1, -1, -1 };
    public static int titleScreenSong
    {
        get { return songIndices[(int)SongRegisters.TitleScreen]; }
        set { songIndices[(int)SongRegisters.TitleScreen] = value; }
    }
    public static int lap1Song
    {
        get { return songIndices[(int)SongRegisters.Lap1]; }
        set { songIndices[(int)SongRegisters.Lap1] = value; }
    }
    public static int lap2Song
    {
        get { return songIndices[(int)SongRegisters.Lap2]; }
        set { songIndices[(int)SongRegisters.Lap2] = value; }
    }
    public static int lastDanceSong
    {
        get { return songIndices[(int)SongRegisters.LastDance]; }
        set { songIndices[(int)SongRegisters.LastDance] = value; }
    }
    public static int lastDanceScore = -1;
    public static string failMessage;
    public static float clearTime;
    public static int clearRank;
}