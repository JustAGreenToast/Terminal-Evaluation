using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class MainTerminalScript : MonitorWindow
{
    [SerializeField] GameManagerScript manager;
    [SerializeField] MonitorScript monitor;
    [SerializeField] TextMeshProUGUI shellOutput;
    [SerializeField] TMP_InputField commandInput;
    [SerializeField] DeluxeStatusPanelScript deluxeStatusPanel;
    [SerializeField] ExerciseSelectPanelScript exerciseSelectPanel;
    [SerializeField] TMP_InputField vimEditor;
    LinkedList<Exercise> exercises;
    public Exercise currentExercise { get { return VirtualRAM.isInTournamentMode ? VirtualRAM.tournamentData.exercise : exercises.Count > 0 ? exercises.First.Value : null; } }
    public string currentExerciseFolderName { get { return (VirtualRAM.isInTournamentMode && VirtualRAM.tournamentData.reverseEngineer) || VirtualRAM.examData.examIndex == 13 ? $"ex_{(solvedExercises + 1).ToString().PadLeft(2, '0')}" : currentExercise.folderName; } }
    public string currentExerciseFileName { get { return currentExerciseFolderName + ".c"; } }
    public int solvedExercises { get; private set; }
    int exercisesLeft;
    public string localScript { get; private set; }
    Dictionary<string, string> gitScripts;
    string currentGitScript
    {
        get { return gitScripts.ContainsKey(currentExerciseFolderName) ? gitScripts[currentExerciseFolderName] : null; }
        set
        {
            if (!gitScripts.ContainsKey(currentExerciseFolderName))
            {
                gitScripts.Add(currentExerciseFolderName, value);
                if (deluxeStatusPanelEnabled) { deluxeStatusPanel.GitScriptAdded(); }
            }
            else { gitScripts[currentExerciseFolderName] = value; }
        }
    }
    enum Status { None, ExamStarted, FolderCreated, FileCreated, Evaluating, Lap2Question, ExamFinished };
    Status currentStatus;
    Task<MoulinetteTerminal.Status> evalTask;
    List<string> cmdHistory;
    int cmdHistoryIndex;
    bool deluxeStatusPanelEnabled { get { return SettingsManager.settings.deluxeStatusPanel || VirtualRAM.isInTournamentMode || VirtualRAM.examData.examIndex == 13; } }
    protected override void OnStart()
    {
        PickExercises();
        solvedExercises = 0;
        exercisesLeft = VirtualRAM.examData.examIndex < 8 || VirtualRAM.examData.examIndex == 11 ? VirtualRAM.examData.minExercises : exercises.Count;
        if (deluxeStatusPanelEnabled) { deluxeStatusPanel.InitRoadmap(exercises, exercisesLeft); }
        gitScripts = new Dictionary<string, string>();
        cmdHistory = new List<string>() { "" };
        cmdHistoryIndex = 0;
    }
    private void OnDisable() { MoulinetteTerminal.OnQuit(); }
    private void OnApplicationQuit() { MoulinetteTerminal.OnQuit(); }
    protected override void OnUpdate()
    {
        if (EventSystem.current.currentSelectedGameObject == commandInput.gameObject)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && cmdHistoryIndex > 0)
            {
                if (cmdHistoryIndex == cmdHistory.Count - 1) { cmdHistory[cmdHistoryIndex] = commandInput.text; }
                cmdHistoryIndex--;
                commandInput.text = cmdHistory[cmdHistoryIndex];
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && cmdHistoryIndex < cmdHistory.Count - 1)
            {
                cmdHistoryIndex++;
                commandInput.text = cmdHistory[cmdHistoryIndex];
            }
        }
        if (currentStatus == Status.Evaluating)
        {
            if (evalTask.IsCompleted)
            {
                if (evalTask.Result == MoulinetteTerminal.Status.OK)
                {
                    exercises.RemoveFirst();
                    vimEditor.text = "";
                    localScript = "";
                    solvedExercises++;
                    exercisesLeft--;
                    manager.ExercisePassed();
                    if (deluxeStatusPanelEnabled) { deluxeStatusPanel.ExerciseCleared(); }
                    if (exercises.Count == 0)
                    {
                        currentStatus = Status.ExamFinished;
                        SetOutput("Result: <color=#00ff00>Success!</color>\n\n\nCongratulations, you have done all available exercises! You may type 'quit' to finish your session.");
                    }
                    else if (exercisesLeft == 0)
                    {
                        currentStatus = Status.Lap2Question;
                        StringBuilder sb = new StringBuilder();
                        if (VirtualRAM.examData.examIndex == 11)
                        {
                            sb.AppendLine("<color=#ff00ff>Up for Round 2?</color> [ Y / N ]");
                            sb.AppendLine();
                            sb.AppendLine(manager.rollLap2BonusTime > 0 ? $"Extra Time:<color=#ff00ff> {TimeUtils.SecondsToTimerString(manager.rollLap2BonusTime)}</color>" : "<color=#ffff00>Warning: no extra time will be given!</color>");
                        }
                        else
                        {
                            sb.AppendLine("Congratulations, you passed the exam! :D");
                            sb.AppendLine();
                            sb.AppendLine("<color=#ffff00>Would you like to continue?</color> [ Y / N ]");
                            sb.AppendLine();
                            sb.AppendLine("<color=#ffff00>Warning:</color> if you continue, you won't be able to leave until you finish all bonus exercises!");
                            sb.AppendLine();
                            sb.AppendLine($"<color=#ff00ff>Bonus exercises:</color> {exercises.Count}");
                        }
                        SetOutput(sb.ToString());
                    }
                    else
                    {
                        currentStatus = Status.ExamStarted;
                        SetOutput("Result: <color=#00ff00>Success!</color>\n\n\nCongratulations, you did it! You may type 'status' to read your next assignment.");
                        manager.ExerciseStarted();
                    }
                }
                else
                {
                    currentStatus = Status.FileCreated;
                    SetOutput($"Result: <color=#ff0000>Failure</color>\n\n\nReason: {evalTask.Result}\n\nTry typing 'viewtrace' to see the last saved error log.");
                    manager.ExerciseFailed();
                    if (deluxeStatusPanelEnabled) { deluxeStatusPanel.ExerciseFailed(); }
                }
            }
        }
    }
    void PickExercises()
    {
        ExerciseSet set = VirtualRAM.exercises.Copy();
        List<Exercise> exerciseBag = new List<Exercise>();
        foreach (Exercise ex in set.exercises) { exerciseBag.Add(ex.Copy()); }
        exercises = new LinkedList<Exercise>();
        int picksLeft = VirtualRAM.examData.totalExercises;
        List<int> difficulties = new List<int>();
        switch (VirtualRAM.examData.examIndex)
        {
            // Final Five
            case 9:
                difficulties.AddRange(new int[5] { 4, 4, 4, 4, 4 });
                break;
            // Single Difficulty
            case 10:
                {
                    int[] exerciseCount = new int[6];
                    picksLeft = 0;
                    foreach (Exercise ex in exerciseBag) { exerciseCount[ex.difficulty]++; }
                    for (int i = 0; i < 6; i++)
                    {
                        if (VirtualRAM.examData.minExercises == 6 || VirtualRAM.examData.minExercises == i)
                        {
                            picksLeft += exerciseCount[i];
                            for (int j = 0; j < exerciseCount[i]; j++) { difficulties.Add(i); }
                        }
                    }
                }
                break;
            // Roll Exam
            case 11:
                for (int n = 0; n < 2; n++) { for (int i = 0; i < VirtualRAM.examData.minExercises; i++) { difficulties.Add(i); } }
                break;
            // Nanoshell
            case 14:
            case 15:
            case 16:
                exerciseBag.Clear();
                exerciseBag.Add(VirtualRAM.specialExercises.exercises[0].Copy());
                difficulties.Add(0);
                break;
            default:
                switch (VirtualRAM.examData.progression)
                {
                    case VirtualRAM.ExamData.DifficultyBumps.Quick:
                        difficulties.AddRange(new int[10] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4 });
                        break;
                    case VirtualRAM.ExamData.DifficultyBumps.Advanced:
                        difficulties.AddRange(new int[13] { 0, 0, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4 });
                        break;
                    case VirtualRAM.ExamData.DifficultyBumps.Casual:
                        difficulties.AddRange(new int[5] { 0, 1, 2, 3, 4 });
                        break;
                    default:
                        difficulties.AddRange(new int[15] { 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4 });
                        break;
                }
                break;
        }
        while (picksLeft > 0)
        {
            List<int> indexBag = new List<int>();
            for (int i = 0; i < exerciseBag.Count; i++)
            {
                int currentDifficulty = difficulties[0] == 4 && SettingsManager.settings.finalFiveEnabled && VirtualRAM.examData.examIndex != 10 ? 5 : difficulties[0];
                if (exerciseBag[i].difficulty == currentDifficulty) { indexBag.Add(i); }
            }
            int n = indexBag[Random.Range(0, indexBag.Count)];
            exerciseBag[n].ReplaceWildcards();
            exercises.AddLast(exerciseBag[n]);
            exerciseBag.RemoveAt(n);
            picksLeft--;
            difficulties.RemoveAt(0);
        }
    }
    public void TryRunCommand(string _cmd) { RunCommand(_cmd, false); }
    public void RunCommand(string _cmd) { RunCommand(_cmd, true); }
    void RunCommand(string _cmd, bool _force)
    {
        if (!isOpen || currentStatus == Status.Evaluating || !(_force || Input.GetKeyDown(KeyCode.Return))) { return; }
        _cmd = _cmd.Trim();
        string[] args = _cmd.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 0) { return; }
        if (currentStatus == Status.Lap2Question)
        {
            if (args.Length > 1 || (args[0].ToLower() != "y" && args[0].ToLower() != "n")) { SetOutput("Invalid input, please try again.\n\n\nY: Continue Exam (You will need to finish the rest of the exam before leaving!)\n\nN: Quit Exam"); }
            else if (args[0].ToLower() == "n") { QuitExam(); }
            else { StartLap2(); }
            return;
        }
        if (!_force && (cmdHistory.Count == 0 || cmdHistory[cmdHistory.Count - 1] != _cmd))
        {
            cmdHistory.Add(_cmd);
            cmdHistoryIndex = cmdHistory.Count - 1;
        }
        bool reselectCommandLine = true;
        switch (args[0].ToLower())
        {
            // Display All Possible Commands
            case "help":
                // Extra Arguments Check
                if (args.Length > 1) { SetOutput("Error: Extra arguments were given."); }
                else { SetOutput("help: Lists all commands.\n\nstartexam: Starts the exam.\n\nstatus: Displays your current exam status.\n\nmkcd: Creates the specified folder and sets it as the working directory.\n\nvim: Creates and opens the specified file in the text editor.\n\ngit status: Displays your current repository's status.\n\ngit update: Adds, commits and pushes all changes to the repository.\n\ngit restore: Reverts all local changes that have not been pushed yet.\n\ngit ls: Lists all uploaded content.\n\ngit load: Copies the specified file's content to your clipboard.\n\n" + (VirtualRAM.examData.examIndex >= 2 ? "git grade: Runs 'git update' and 'grademe'.\n\n" : "") + "grademe: Evaluates your current exercise.\n\n" + (VirtualRAM.examData.examIndex >= 2 ? "halconfig: Opens the Server Configuration GUI.\n\n" : "") + "quit: Closes your current session and finishes the exam."); }
                break;
            // Start Exam
            case "startexam":
                if (manager.IsAnyServerDown()) { SetOutput("Error: Check your Internet connection!"); }
                // Extra Arguments Check
                else if (args.Length > 1) { SetOutput("Error: Extra arguments were given."); }
                // Exam Already Started Check
                else if (currentStatus != Status.None) { SetOutput("Error: The exam has already been started!"); }
                // Start Exam
                else
                {
                    currentStatus = Status.ExamStarted;
                    manager.ExamStarted();
                    manager.ExerciseStarted();
                    if (deluxeStatusPanelEnabled) { deluxeStatusPanel.ExerciseStarted(); }
                    RunCommand("status", true);
                }
                break;
            // Check Exam Status
            case "status":
                if (manager.IsAnyServerDown()) { SetOutput("Error: Check your Internet connection!"); }
                // Extra Arguments Check
                else if (args.Length > 1) { SetOutput("Error: Extra arguments were given."); }
                // Exam Not Started Check
                else if (currentStatus == Status.None) { SetOutput("You need to start the exam first!"); }
                // Exam Finished Check
                else if (currentStatus == Status.ExamFinished) { SetOutput("You've already finished the exam!"); }
                // Display Status
                else { if (deluxeStatusPanelEnabled) { shellOutput.text = ""; deluxeStatusPanel.Show(); } else { SetOutput(GetStatusOutput()); } }
                break;
            // Create And Go To Folder
            case "mkcd":
                // No File Name
                if (args.Length == 1) { SetOutput("Error: No folder name was given."); }
                // Multiple Arguments
                else if (args.Length > 2) { SetOutput("Error: More than one argument was given."); }
                // Exam Started Check
                else if (currentStatus == Status.None) { SetOutput("You should start the exam first."); }
                // Exam Finished Check
                else if (currentStatus == Status.ExamFinished) { SetOutput("You've already finished the exam!"); }
                // Folder Name Check
                else if (args[1] != currentExerciseFolderName) { SetOutput("Are you sure that's the correct folder name?"); }
                // Folder Exists Check
                else if (currentStatus != Status.ExamStarted) { SetOutput("Error: This folder already exists!"); }
                // Create And Open File
                else
                {
                    currentStatus = Status.FolderCreated;
                    SetOutput("Directory created successfully.");
                }
                break;
            // Open Text Editor
            case "vim":
            case "vi":
            case "nano":
                // No File Name
                if (args.Length == 1) { SetOutput("Error: No file name was given."); }
                // Multiple Arguments
                else if (args.Length > 2) { SetOutput("Error: More than one argument was given."); }
                // Exam Started Check
                else if (currentStatus == Status.None) { SetOutput("You should start the exam first."); }
                // Exam Finished Check
                else if (currentStatus == Status.ExamFinished) { SetOutput("You've already finished the exam!"); }
                // File Name Check
                else if (args[1] != currentExerciseFileName) { SetOutput("Are you sure that's the correct file name?"); }
                // Folder Check (Disregard If File Already Created)
                else if (currentStatus != Status.FolderCreated && currentStatus != Status.FileCreated) { SetOutput("You should create the folder for the file before this."); }
                // Create And Open File
                else
                {
                    currentStatus = Status.FileCreated;
                    SetOutput("File created successfully.");
                    monitor.PullUp(MonitorScript.Windows.Vim);
                }
                break;
            // Create Directory + Create File + Open Text Editor
            case "mcv":
                // No File Name
                if (args.Length == 1) { SetOutput("Error: No file/directory name was given."); }
                // Multiple Arguments
                else if (args.Length > 2) { SetOutput("Error: More than one argument was given."); }
                // Exam Started Check
                else if (currentStatus == Status.None) { SetOutput("You should start the exam first."); }
                // Exam Finished Check
                else if (currentStatus == Status.ExamFinished) { SetOutput("You've already finished the exam!"); }
                // Name Check
                else if (args[1] != currentExerciseFolderName && args[1] != currentExerciseFileName) { SetOutput("Are you sure that's the correct name?"); }
                // Create File And Directory
                else
                {
                    currentStatus = Status.FileCreated;
                    SetOutput("File and directory created successfully.");
                    monitor.PullUp(MonitorScript.Windows.Vim);
                }
                break;
            // Git Commands
            case "git":
                // No File Name
                if (args.Length == 1) { SetOutput("Error: No secondary command was given."); }
                // Extra Arguments Check
                else if (args.Length > 2 && args[1] != "load") { SetOutput("Error: More than two arguments were given."); }
                else if (args.Length > 3 && args[1] == "load") { SetOutput("Error: More than three arguments were given."); }
                // Exam Started Check
                else if (currentStatus == Status.None) { SetOutput("You should start the exam first."); }
                // Exam Finished Check
                else if (currentStatus == Status.ExamFinished) { SetOutput("You've already finished the exam!"); }
                // Run Git Command
                else
                {
                    switch (args[1].ToLower())
                    {
                        case "status":
                            if (manager.IsAnyServerDown()) { SetOutput("Error: Unable to access remote repository! Check your Internet connection."); }
                            else
                            {
                                if (localScript == currentGitScript) { SetOutput("Your git repository is up to date."); }
                                else { SetOutput("Your git repository does not match your local repository."); }
                            }
                            break;
                        case "update":
                            if (manager.IsAnyServerDown()) { SetOutput("Error: Unable to access remote repository! Check your Internet connection."); }
                            else
                            {
                                currentGitScript = localScript;
                                SetOutput("Your git repository has been updated successfully!");
                            }
                            break;
                        case "restore":
                            if (manager.IsAnyServerDown()) { SetOutput("Error: Unable to access remote repository! Check your Internet connection."); }
                            else
                            {
                                localScript = currentGitScript;
                                SetOutput("Your local repository has been restored successfully!");
                            }
                            break;
                        case "ls":
                            if (manager.IsAnyServerDown())
                            {
                                SetOutput("Error: Unable to access remote repository! Check your Internet connection.");
                                return;
                            }
                            if (gitScripts.Count > 0)
                            {
                                string s = "This repository contains:\n";
                                foreach (string key in gitScripts.Keys) { s += $"\n- {key}"; }
                                SetOutput(s);
                            }
                            else { SetOutput("This repository is empty."); }
                            break;
                        case "load":
                            if (manager.IsAnyServerDown())
                            {
                                SetOutput("Error: Unable to access remote repository! Check your Internet connection.");
                                return;
                            }
                            if (args.Length < 3)
                            {
                                SetOutput("Error: No directory/file was specified.");
                                return;
                            }
                            if (args.Length > 3)
                            {
                                SetOutput("Error: Extra arguments were given.");
                                return;
                            }
                            if (args[2].EndsWith(".c")) { args[2] = args[2].Substring(0, args[2].Length - 2); }
                            if (gitScripts.ContainsKey(args[2]))
                            {
                                GUIUtility.systemCopyBuffer = gitScripts[args[2]];
                                SetOutput($"'{args[2]}.c' loaded to clipboard successfully!");
                            }
                            else { SetOutput("Error: File/Directory not found in repository."); }
                            break;
                        // Git Update + Grademe
                        case "grade":
                            if (manager.IsAnyServerDown()) { SetOutput("Error: Unable to access remote repository! Check your Internet connection."); }
                            else
                            {
                                currentGitScript = localScript;
                                RunCommand("grademe", true);
                            }
                            break;
                        default:
                            SetOutput("Error: Git command not recognized.\n\nValid git commands:\ngit status\ngit update\ngit restore\ngit ls\ngit load\ngit grade");
                            break;
                    }
                }
                break;
            // Hand In Current Exercise
            case "grademe":
                if (manager.IsAnyServerDown()) { SetOutput("Error: Check your Internet connection!"); }
                // Extra Arguments Check
                else if (args.Length > 1) { SetOutput("Error: Extra arguments were given."); }
                // Exam Started Check
                else if (currentStatus == Status.None) { SetOutput("You should start the exam first."); }
                // Exam Finished Check
                else if (currentStatus == Status.ExamFinished) { SetOutput("You've already finished the exam!"); }
                // File / Folder Created Check
                else if (currentStatus != Status.FileCreated) { SetOutput($"You shoud at least create the {(currentStatus == Status.FolderCreated ? "file" : "folder")} first..."); }
                // Connection Check
                else if (manager.IsAnyServerDown()) { SetOutput("Error: Check your Internet connection!"); }
                // Evaluate Current Exercise
                else
                {
                    evalTask = new Task<MoulinetteTerminal.Status>(() => MoulinetteTerminal.Evaluate(currentGitScript, currentExercise));
                    evalTask.Start();
                    SetOutput("Evaluating, please wait...");
                    currentStatus = Status.Evaluating;
                }
                break;
            // Git Grade
            case "gg":
                // Extra Arguments Check
                if (args.Length > 1) { SetOutput("Error: Extra arguments were given."); }
                // Run Command
                RunCommand("git grade", true);
                break;
            // View Last Trace
            case "viewtrace":
                // Exam Started Check
                if (currentStatus == Status.None) { SetOutput("You should start the exam first."); }
                // Exam Finished Check
                else if (currentStatus == Status.ExamFinished) { SetOutput("You've already finished the exam!"); }
                else { SetOutput(MoulinetteTerminal.lastTrace); }
                break;
            // Server Configuration GUI
            case "halconfig":
                if (SaveManager.saveData.clearedExams == 0)
                {
                    SceneManager.LoadScene(5);
                    return;
                }
                monitor.PullDown();
                monitor.PullUp(MonitorScript.Windows.ServerGUI);
                reselectCommandLine = false;
                break;
            // Stop Exam
            case "quit":
                // Force Quit
                if (args.Length > 1)
                {
                    if (args.Length > 2) { SetOutput("Error: More than one argument was given."); }
                    else if (args[1].ToLower() == "-f") { manager.ExitExam(); }
                    else if (args[1].ToLower() == "-r") { manager.ReloadExam(); }
                    else { SetOutput($"Error: Flag '{args[1]}' not recognized.\n\nValid flags:\n-f: Force Quit (Exit Exam)\n-r: Reset Quit (Reload Exam)"); }
                }
                // Regular Quit
                else if (exercisesLeft > 0) { SetOutput($"You need to complete at least {exercisesLeft} exercises before you can leave!\n\nIf you -really- want to leave, use 'quit -f'."); }
                else { QuitExam(); }
                break;
            // Tee-Hee! :3
            case "./microhell":
            case "./nanoshell":
#if PLATFORM_STANDALONE_LINUX
                if (args.Length > 1)
                {
                    if (args.Length > 2) { SetOutput("Error: More than one argument was given."); }
                    else if (args[1].ToLower() == "-p")
                    {
                        VirtualRAM.examData.NanoshellLastDance(1);
                        manager.ReloadExam();
                    }
                    else if (args[1].ToLower() == "-h")
                    {
                        VirtualRAM.examData.NanoshellLastDance(2);
                        manager.ReloadExam();
                    }
                    else { SetOutput($"Error: Flag '{args[1]}' not recognized.\n\nValid flags:\n-p: Practice Run (Disables All Enemies)\n-h: Hard Mode (Regular Enemies Stay Enabled)"); }
                }
                else
                {
                    VirtualRAM.examData.NanoshellLastDance(0);
                    manager.ReloadExam();
                }
#else
                SetOutput("Sorry, that exam is currently Linux-only :(");
#endif
                break;
            // Invalid Command
            default:
                SetOutput("Command not found.\n\nType 'help' to list all commands.");
                break;
        }
        if (!_force && reselectCommandLine) { SelectCommandLine(); }
    }
    void QuitExam()
    {
        currentStatus = Status.ExamFinished;
        SetOutput("Your current session has been finished. You may leave now.");
        manager.ExamPassed();
    }
    void StartLap2()
    {
        exercisesLeft = exercises.Count;
        currentStatus = Status.ExamStarted;
        manager.Lap2Started();
        if (deluxeStatusPanelEnabled) { deluxeStatusPanel.Lap2Started(); }
        RunCommand("status", true);
    }
    string GetStatusOutput()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Solved exercises: {solvedExercises}");
        sb.AppendLine($"<color=#ff00ff>Exercises left: {exercisesLeft}</color>\n");
        Exercise ex = currentExercise;
        sb.AppendLine($"<color=#ffff00>Folder: {ex.folderName}\nFile: {ex.fileName}</color>\n");
        sb.Append("Allowed functions: ");
        string[] allowedFunctions = ex.allowedFunctions;
        if (allowedFunctions.Length == 0) { sb.AppendLine("(None)"); }
        else if (allowedFunctions.Length == 1) { sb.AppendLine($"<color=#00ff00>{allowedFunctions[0]}</color>"); }
        else
        {
            sb.AppendLine();
            foreach (string f in allowedFunctions) { sb.AppendLine($"- <color=#00ff00>{f}</color>"); }
        }
        sb.AppendLine();
        string[] functionPrototypes = ex.functionPrototypes;
        sb.Append($"Function prototype{(functionPrototypes.Length == 1 ? "" : 's')}: ");
        if (functionPrototypes.Length == 0) { sb.AppendLine("(None)"); }
        else
        {
            sb.AppendLine();
            foreach (string p in functionPrototypes) { sb.AppendLine($"<color=#00ffff>{p}</color>"); }
        }
        sb.AppendLine();
        sb.AppendLine($"Subject:\n<color=#00ffff>{ex.subject}</color>");
        return sb.ToString();
    }
    void SetOutput(string _output = "")
    {
        if (deluxeStatusPanelEnabled) { deluxeStatusPanel.Hide(); }
        shellOutput.text = _output;
    }
    public void UpdateLocalScript(string _scr)
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { return; }
        localScript = _scr;
    }
    void SelectCommandLine()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(commandInput.gameObject);
    }
    void DeselectCommandLine() { if (EventSystem.current.currentSelectedGameObject == commandInput.gameObject) { EventSystem.current.SetSelectedGameObject(null); } }
    public override void OnPullUp() { SelectCommandLine(); }
    public override void OnPullDown() { DeselectCommandLine(); }
}