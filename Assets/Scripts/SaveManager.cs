using System.IO;
using UnityEngine;

public static class SaveManager
{
    public class SaveData
    {
        readonly static string path = Path.Combine(Application.persistentDataPath, "save.json");
        public class ExamData
        {
            public bool examCleared { get { return bestRank != -1 && bestTime != -1; } }
            public int bestRank { get; private set; } = -1;
            public float bestTime { get; private set; } = -1;
            public ExamData()
            {
                bestRank = -1;
                bestTime = -1;
            }
            public ExamData(SaveDataJSON.ExamDataJSON _examData)
            {
                bestRank = _examData.bestRank;
                bestTime = _examData.bestTime;
            }
            public bool UpdateClearData(int _rank, float _t)
            {
                bool dataUpdated = false;
                if (bestRank == -1 || _rank > bestRank)
                {
                    bestRank = _rank;
                    dataUpdated = true;
                }
                if (bestTime == -1 || _t < bestTime)
                {
                    bestTime = _t > 5999.999f ? 5999.999f : _t;
                    dataUpdated = true;
                }
                return dataUpdated;
            }
        }
        public ExamData[] clearData { get; private set; } = new ExamData[6];
        public int clearedExams { get { for (int i = 0; i < clearData.Length; i++) { if (!clearData[i].examCleared) { return i; } } return clearData.Length; } }
        public bool allSRanks { get { for (int i = 0; i < 5; i++) { if (clearData[i].bestRank < 4) { return false; } } return true; } }
        public bool allPRanks { get { for (int i = 0; i < 5; i++) { if (clearData[i].bestRank < 5) { return false; } } return true; } }
        [System.Serializable]
        public class SaveDataJSON
        {
            [System.Serializable]
            public class ExamDataJSON
            {
                public int bestRank = -1;
                public float bestTime = -1;
                public ExamDataJSON()
                {
                    bestRank = -1;
                    bestTime = -1;
                }
                public ExamDataJSON(ExamData _examData)
                {
                    bestRank = _examData.bestRank;
                    bestTime = _examData.bestTime;
                }
            }
            public ExamDataJSON[] clearData = new ExamDataJSON[6];
            public SaveDataJSON(SaveData _saveData)
            {
                clearData = new ExamDataJSON[6];
                for (int i = 0; i < 6; i++) { clearData[i] = new ExamDataJSON(_saveData.clearData[i]); }
            }
            public void Save() { using (StreamWriter sw = new StreamWriter(path)) { sw.Write(JsonUtility.ToJson(this)); } }
            public static SaveDataJSON Load() { using (StreamReader sr = new StreamReader(path)) { return JsonUtility.FromJson<SaveDataJSON>(sr.ReadToEnd()); } }
        }
        public SaveData()
        {
            clearData = new ExamData[6];
            for (int i = 0; i < 6; i++) { clearData[i] = new ExamData(); }
        }
        public SaveData(SaveDataJSON _json)
        {
            clearData = new ExamData[6];
            for (int i = 0; i < 6; i++) { clearData[i] = new ExamData(_json.clearData[i]); }
        }
        public static SaveData Load()
        {
            if (!File.Exists(path)) { new SaveData().Save(); }
            return new SaveData(SaveDataJSON.Load());
        }
        public void Save() { new SaveDataJSON(this).Save(); }
    }
    public static SaveData saveData { get; private set; } = new SaveData();
    public static void Load() { saveData = SaveData.Load(); }
}