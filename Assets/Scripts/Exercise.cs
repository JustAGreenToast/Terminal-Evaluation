using System;
using System.IO;
using UnityEngine;

public class ExerciseSet
{
    readonly string path = Path.Combine(Application.streamingAssetsPath, "exercises.json");
    public Exercise[] exercises { get; private set; }
    public void Load()
    {
        ExerciseSetJSON setJSON = JsonUtility.FromJson<ExerciseSetJSON>(File.ReadAllText(path));
        exercises = new Exercise[setJSON.exercises.Length];
        for (int i = 0; i < exercises.Length; i++) { exercises[i] = new Exercise(setJSON.exercises[i]); }
    }
    public void CopyExercises(Exercise[] _ex)
    {
        exercises = new Exercise[_ex.Length];
        Array.Copy(_ex, exercises, exercises.Length);
    }
    public ExerciseSet Copy()
    {
        ExerciseSet copy = new ExerciseSet();
        copy.CopyExercises(exercises);
        return copy;
    }
    [System.Serializable]
    public class ExerciseSetJSON
    {
        public Exercise.ExerciseJSON[] exercises;
        public ExerciseSetJSON(ExerciseSet _set)
        {
            exercises = new Exercise.ExerciseJSON[_set.exercises.Length];
            for (int i = 0; i < exercises.Length; i++) { exercises[i] = new Exercise.ExerciseJSON(_set.exercises[i]); }
        }
    }
}

public class Exercise
{
    public int difficulty { get; private set; }
    public string folderName { get; private set; }
    public string fileName { get { return folderName + ".c"; } }
    public string subject { get; private set; }
    public string[] allowedFunctions { get; private set; }
    public string[] functionPrototypes { get; private set; }
    public string[] extraScripts { get; private set; }
    public TestCase[] testCases { get; private set; }
    public string testScript { get; private set; }
    public class TestCase
    {
        public string args { get; private set; }
        public string output { get; private set; }
        public TestCase(ExerciseJSON.TestCaseJSON _testCase)
        {
            args = _testCase.args;
            output = _testCase.output;
        }
        TestCase(TestCase _testCase)
        {
            args = _testCase.args;
            output = _testCase.output;
        }
        public TestCase Copy() { return new TestCase(this); }
        public void ReplaceWildcard(string code, string val)
        {
            args = args.Replace(code, val);
            output = output.Replace(code, val);
        }
    }
    [System.Serializable]
    public class ExerciseJSON
    {
        public int difficulty;
        public string folderName;
        public string subject;
        public string[] allowedFunctions;
        public string[] functionPrototypes;
        public string[] extraScripts;
        public TestCaseJSON[] testCases;
        public string testScript;
        [System.Serializable]
        public class TestCaseJSON
        {
            public string args;
            public string output;
            public TestCaseJSON() { }
            public TestCaseJSON(TestCase _testCase)
            {
                args = _testCase.args;
                output = _testCase.output;
            }
        }
        public ExerciseJSON() { }
        public ExerciseJSON(Exercise _ex)
        {
            difficulty = _ex.difficulty;
            folderName = _ex.folderName;
            subject = _ex.subject;
            allowedFunctions = _ex.allowedFunctions;
            functionPrototypes = _ex.functionPrototypes;
            extraScripts = _ex.extraScripts;
            testCases = new TestCaseJSON[_ex.testCases.Length];
            for (int i = 0; i < testCases.Length; i++) { testCases[i] = new TestCaseJSON(_ex.testCases[i]); }
            testScript = _ex.testScript;
        }
    }
    public Exercise(ExerciseJSON _ex)
    {
        difficulty = _ex.difficulty;
        folderName = _ex.folderName;
        subject = _ex.subject;
        allowedFunctions = _ex.allowedFunctions;
        functionPrototypes = _ex.functionPrototypes;
        extraScripts = _ex.extraScripts;
        testCases = new TestCase[_ex.testCases.Length];
        for (int i = 0; i < testCases.Length; i++) { testCases[i] = new TestCase(_ex.testCases[i]); }
        testScript = _ex.testScript;
    }
    Exercise(Exercise _ex)
    {
        difficulty = _ex.difficulty;
        folderName = _ex.folderName;
        subject = _ex.subject;
        allowedFunctions = _ex.allowedFunctions;
        functionPrototypes = _ex.functionPrototypes;
        extraScripts = _ex.extraScripts;
        testCases = new TestCase[_ex.testCases.Length];
        for (int i = 0; i < testCases.Length; i++) { testCases[i] = _ex.testCases[i].Copy(); }
        testScript = _ex.testScript;
    }
    public Exercise Copy() { return new Exercise(this); }
    public void ReplaceWildcards()
    {
        ReplaceWildcard("<char_wildcard>", ((char)UnityEngine.Random.Range('a', 'z' + 1)).ToString());
        ReplaceWildcard("<num_wildcard>", (UnityEngine.Random.value > 0.8f ? 42 : UnityEngine.Random.value > 0.25f ? UnityEngine.Random.Range(0, 10) : UnityEngine.Random.Range(0, 4097)).ToString());
        ReplaceWildcard("<digit_wildcard>", UnityEngine.Random.Range(0, 10).ToString());
    }
    void ReplaceWildcard(string code, string val)
    {
        folderName = folderName.Replace(code, val);
        subject = subject.Replace(code, val);
        for (int i = 0; i < extraScripts.Length; i++) { extraScripts[i] = extraScripts[i].Replace(code, val); }
        for (int i = 0; i < functionPrototypes.Length; i++) { functionPrototypes[i] = functionPrototypes[i].Replace(code, val); }
        for (int i = 0; i < testCases.Length; i++) { testCases[i].ReplaceWildcard(code, val); }
        testScript = testScript.Replace(code, val);
    }
    public bool Verify() { return MoulinetteTerminal.Evaluate(testScript, this) == MoulinetteTerminal.Status.OK; }
}