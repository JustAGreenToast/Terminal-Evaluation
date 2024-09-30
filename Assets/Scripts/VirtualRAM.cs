using System.Collections.Generic;
using UnityEngine;

public static class VirtualRAM
{
    public static ExerciseSet exercises;
    public static ExerciseSet specialExercises;
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
        public void ReverseEngineerLastDance(int _exercises, DifficultyBumps _progression, int[] _aiLevels, bool _tiredMidnight, WindowObstacle _windowObstacle, bool _endlessMode)
        {
            examIndex = 13;
            minExercises = _exercises;
            totalExercises = _exercises;
            progression = _progression;
            aiLevels = _aiLevels;
            tiredMidnight = _tiredMidnight;
            windowObstacle = _windowObstacle;
            endless = _endlessMode;
        }
        public void NanoshellLastDance(int _flagData)
        {
            examIndex = 14 + _flagData; // 15 (14 + 1) = Practice, 16 (14 + 2) = Hard
            minExercises = 1;
            totalExercises = 1;
            progression = DifficultyBumps.Auto;
            aiLevels = examData.aiLevels;
            tiredMidnight = examData.tiredMidnight;
            windowObstacle = examData.windowObstacle;
            endless = true;
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
    public class TournamentData
    {
        public bool hardMode { get; private set; }
        uint seed;
        public string seedHex
        {
            get
            {
                string hexChars = "0123456789ABCDEF";
                string s = "";
                for (int i = 7; i >= 0; i--) { s += hexChars[(int)((seed >> (4 * i)) & 0xF)]; }
                return s;
            }
        }
        Exercise[][] exercises;
        int exerciseIndex;
        public Exercise exercise { get { return exerciseIndex == -1 || exerciseIndex >= exercises[roundIndex].Length ? null : exercises[roundIndex][exerciseIndex]; } }
        public bool reverseEngineer { get { return roundIndex == 1 || (roundIndex == 2 && hardMode); } }
        public bool[] round3SelectedExercises { get; private set; }
        float exerciseStartTime;
        int exerciseStreak;
        bool perfectRound;
        public float roundStartTime { get; private set; }
        public float roundMaxTime { get; private set; }
        int[] roundScores;
        int roundIndex;
        public int round1Score { get { return roundScores[0]; } }
        public int round2Score { get { return roundScores[1]; } }
        public int round3Score { get { return roundScores[2]; } }
        public int currentRoundScore { get { return roundScores[roundIndex]; } private set { roundScores[roundIndex] = value; } }
        public TournamentData(uint _seed)
        {
            seed = _seed;
            Random.InitState((int)seed);
            round3SelectedExercises = new bool[15];
            PickExercises();
        }
        void PickExercises()
        {
            exercises = new Exercise[3][];
            PickRound1Exercises();
            PickRound2Exercises();
            PickRound3Exercises();
            Debug.Log(exercises.Length);
            for (int i = 0; i < exercises.Length; i++)
            {
                Debug.Log(exercises[i].Length);
                for (int j = 0; j < exercises[i].Length; j++) { Debug.Log(exercises[i][j].folderName); }
            }
        }
        void PickRound1Exercises()
        {
            ExerciseSet set = VirtualRAM.exercises.Copy();
            List<Exercise> exerciseBag = new List<Exercise>();
            foreach (Exercise ex in set.exercises) { exerciseBag.Add(ex.Copy()); }
            int[] difficulties = new int[5] { 0, 1, 2, 3, 4 };
            exercises[0] = new Exercise[difficulties.Length];
            for (int i = 0; i < difficulties.Length; i++)
            {
                List<int> indexBag = new List<int>();
                for (int j = 0; j < exerciseBag.Count; j++) { if (exerciseBag[j].difficulty == difficulties[i]) { indexBag.Add(j); } }
                int n = indexBag[Random.Range(0, indexBag.Count)];
                exerciseBag[n].ReplaceWildcards();
                exercises[0][i] = exerciseBag[n];
                exerciseBag.RemoveAt(n);
            }
        }
        void PickRound2Exercises()
        {
            ExerciseSet set = VirtualRAM.exercises.Copy();
            List<Exercise> exerciseBag = new List<Exercise>();
            foreach (Exercise ex in set.exercises) { exerciseBag.Add(ex.Copy()); }
            int[] difficulties = new int[10] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4 };
            exercises[1] = new Exercise[difficulties.Length];
            for (int i = 0; i < difficulties.Length; i++)
            {
                List<int> indexBag = new List<int>();
                for (int j = 0; j < exerciseBag.Count; j++) { if (exerciseBag[j].difficulty == difficulties[i]) { indexBag.Add(j); } }
                int n = indexBag[Random.Range(0, indexBag.Count)];
                exerciseBag[n].ReplaceWildcards();
                exercises[1][i] = exerciseBag[n];
                exerciseBag.RemoveAt(n);
            }
        }
        void PickRound3Exercises()
        {
            ExerciseSet set = VirtualRAM.exercises.Copy();
            List<Exercise> exerciseBag = new List<Exercise>();
            foreach (Exercise ex in set.exercises) { exerciseBag.Add(ex.Copy()); }
            int[] difficulties = new int[15] { 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4 };
            exercises[2] = new Exercise[difficulties.Length];
            for (int i = 0; i < difficulties.Length; i++)
            {
                List<int> indexBag = new List<int>();
                for (int j = 0; j < exerciseBag.Count; j++) { if (exerciseBag[j].difficulty == difficulties[i]) { indexBag.Add(j); } }
                int n = indexBag[Random.Range(0, indexBag.Count)];
                exerciseBag[n].ReplaceWildcards();
                exercises[2][i] = exerciseBag[n];
                exerciseBag.RemoveAt(n);
            }
        }
        public void ExerciseFailed()
        {
            exerciseStreak = 0;
            perfectRound = false;
        }
        public void ExerciseCleared()
        {
            float score = (exercise.difficulty + 1) * 100;
            score += 50 * exerciseStreak;
            float maxBonusTime = new float[5] { 60, 90, 150, 210, 300 }[exercise.difficulty];
            //if (hardMode) { maxBonusTime *= 0.75f; }
            score *= Mathf.Lerp(Mathf.InverseLerp(maxBonusTime, maxBonusTime * (hardMode ? 1.5f : 2.5f), Time.time - exerciseStartTime), 1.5f, 1);
            if (perfectRound) { score *= hardMode ? 1.5f : 2; }
            currentRoundScore += Mathf.CeilToInt(score * 0.1f) * 10;
            exerciseStreak++;
            exerciseIndex++;
        }
        public void ExerciseStarted() { exerciseStartTime = Time.time; }
        public void ExerciseSelected(int _n) { if (roundIndex == 2) { exerciseIndex = _n; } }
    }
    public static TournamentData tournamentData { get; private set; }
    public static bool isInTournamentMode { get { return tournamentData != null; } }
    public static void StartNewTournament(string _hexSeed)
    {
        uint seed = 0;
        string hexChars = "0123456789ABCDEF";
        uint scale = 1;
        for (int i = _hexSeed.Length - 1; i >= 0; i--)
        {
            seed += (uint)hexChars.IndexOf(_hexSeed[i]) * scale;
            if (i > 0) { scale *= 16; }
        }
        StartNewTournament(seed);
    }
    public static void StartNewTournament(uint _seed)
    {
        tournamentData = new TournamentData(_seed);
        Debug.Log(tournamentData.seedHex);
    }
    public static void ResetTournamentData() { tournamentData = null; }
}