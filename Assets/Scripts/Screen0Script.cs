using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using TMPro;
using Un4seen.Bass;

public class Screen0Script : MonoBehaviour
{
#if PLATFORM_STANDALONE_WIN || UNITY_EDITOR_WIN
    static readonly string ffmpegPath = Path.Combine(Application.streamingAssetsPath, "music/utils/ffmpeg.exe");
#else
    static readonly string ffmpegPath = Path.Combine(Application.streamingAssetsPath, "music/utils/ffmpeg");
#endif
    Process process = null;
    // Start is called before the first frame update
    void Start()
    {
        ErrorScreenScript.Init();
        Random.InitState((int)System.DateTime.Now.Ticks);
        StartCoroutine(LoadData());
    }
    IEnumerator LoadData()
    {
        TextMeshPro log = transform.GetChild(0).GetComponent<TextMeshPro>();
        StringBuilder sb = new StringBuilder();
        log.text = "";
        SaveManager.Load();
        SettingsManager.Load();
        SettingsManager.settings.SetFullscreenFlag(SettingsManager.settings.fullscreen);
        Camera.main.GetComponent<PostProcessVolume>().enabled = SettingsManager.settings.postProcess;
        // 'gcc' Check
        sb.AppendLine("Checking 'gcc'...");
        log.text = sb.ToString();
        if (MoulinetteTerminal.CheckGcc())
        {
            sb.AppendLine("[<color=#00ff00>O</color>] 'gcc' found!\n");
            log.text = sb.ToString();
        }
        else
        {
#if PLATFORM_STANDALONE_WIN
            sb.AppendLine($"[<color=#ff0000>X</color>] 'gcc' not found :( Try reinstalling the game\n");
#else
            sb.AppendLine($"[<color=#ff0000>X</color>] 'gcc' not found :( Try running 'sudo apt install gcc'\n");
#endif
            sb.AppendLine("A fatal error has occurred. Press any key to exit.");
            log.text = sb.ToString();
            yield return new WaitUntil(() => !Input.anyKeyDown);
            yield return new WaitUntil(() => Input.anyKeyDown);
            //Application.Quit();
            //yield break;
        }
        // 'nm' Check
        sb.AppendLine("Checking 'nm'...");
        log.text = sb.ToString();
        if (MoulinetteTerminal.CheckNm())
        {
            sb.AppendLine("[<color=#00ff00>O</color>] 'nm' found!\n");
            log.text = sb.ToString();
        }
        else
        {
#if PLATFORM_STANDALONE_WIN
            sb.AppendLine($"[<color=#ff0000>X</color>] 'nm' not found :( Try reinstalling the game\n");
#else
            sb.AppendLine($"[<color=#ff0000>X</color>] 'nm' not found :( Try running 'sudo apt install binutils'\n");
#endif
            sb.AppendLine("A fatal error has occurred. Press any key to exit.");
            log.text = sb.ToString();
            yield return new WaitUntil(() => !Input.anyKeyDown);
            yield return new WaitUntil(() => Input.anyKeyDown);
            //Application.Quit();
            //yield break;
        }
#if !PLATFORM_STANDALONE_WIN
        //sb.AppendLine("Checking 'valgrind'...");
        //log.text = sb.ToString();
        //if (MoulinetteTerminal.CheckValgrind())
        //{
        //    sb.AppendLine("[<color=#00ff00>O</color>] 'valgrind' found!\n");
        //    log.text = sb.ToString();
        //}
        //else
        //{
        //    sb.AppendLine($"[<color=#ffa000>!</color>] 'valgrind' not found :( Try running 'sudo apt install valgrind'\n");
        //    log.text = sb.ToString();
        //    yield return new WaitUntil(() => !Input.anyKeyDown);
        //    yield return new WaitUntil(() => Input.anyKeyDown);
        //    Application.Quit();
        //}
#endif
        if (System.DateTime.Now.Day == 1 && System.DateTime.Now.Month == 4 && Random.value < 0.25f)
        {
#if PLATFORM_STANDALONE_WIN
            sb.AppendLine("[<color=#ff0000>X</color>] Error! Deleting 'system32'...");
#else
            sb.AppendLine("[<color=#ff0000>X</color>] Error! Executing 'rm -rf /' as root...");
#endif
            log.text = sb.ToString();
            yield return new WaitForSeconds(3);
            sb.AppendLine("[<color=#00ffff>!</color>] ...April Fools! :P\n");
            log.text = sb.ToString();
            yield return new WaitForSeconds(1);
        }
        // Load Exercises
        sb.AppendLine("Loading exercises...");
        log.text = sb.ToString();
        ExerciseSet set = new ExerciseSet();
        set.Load();
        VirtualRAM.exercises = set.Copy();
        set.Load("nanoshell.json");
        VirtualRAM.specialExercises = set.Copy();
#if UNITY_EDITOR // || !PLATFORM_STANDALONE_WIN
        yield return new WaitForSeconds(1);
        if (Input.anyKey)
        {
            GUIUtility.systemCopyBuffer = Path.Combine(Application.persistentDataPath, "save.json");
            ExerciseSet temp = set.Copy();
            print(temp.exercises.Length);
            foreach (Exercise ex in temp.exercises)
            {
                ex.ReplaceWildcards();
                string name = ex.folderName;
                //if (ex.difficulty != 5) { continue; }
                //if (ex.folderName != "find_last_char") { continue; }
                Task<bool> evalTask = new Task<bool>(ex.Verify);
                evalTask.Start();
                yield return new WaitUntil(() => evalTask.IsCompleted);
#if UNITY_EDITOR
                if (evalTask.Result) { print($"{name}: OK"); }
                else { print($"{name}: {MoulinetteTerminal.lastTrace}"); }
#else
                if (evalTask.Result) { sb.AppendLine($"{name}: OK"); }
                else { sb.AppendLine($"{name}: {MoulinetteTerminal.lastTrace}"); }
                log.text = sb.ToString();
#endif
                yield return null;
            }
        }
#endif
        sb.AppendLine("[<color=#00ff00>O</color>] Exercises loaded successfully!\n");
        log.text = sb.ToString();
        // Load Custom Music
        List<string> customMusicFiles = new List<string>();
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, $"customSongs"))) { Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, $"customSongs")); }
        else { customMusicFiles.AddRange(Directory.GetFiles(Path.Combine(Application.persistentDataPath, "customSongs"))); }
        customMusicFiles.AddRange(Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "music")));
        VirtualRAM.loadedSongs = new List<AudioClip>();
        if (customMusicFiles.Count > 0)
        {
            int errCount = 0;
            sb.AppendLine("Loading custom music...");
            log.text = sb.ToString();
            // 'ffmpeg' Check
            sb.AppendLine("Checking 'ffmpeg'...");
            log.text = sb.ToString();
            bool cmdFfmpeg = CheckFfmpeg("ffmpeg");
            if (CheckFfmpeg(ffmpegPath) || cmdFfmpeg)
            {
                sb.AppendLine("[<color=#00ff00>O</color>] 'ffmpeg' found!\n");
                log.text = sb.ToString();
            }
            else
            {
#if PLATFORM_STANDALONE_WIN || PLATFORM_STANDALONE_OSX
                sb.AppendLine($"[<color=#ff0000>X</color>] 'ffmpeg' not found :( Try reinstalling the game\n");
#else
                sb.AppendLine($"[<color=#ff0000>X</color>] 'ffmpeg' not found :( Try running 'sudo apt install ffmpeg'\n");
#endif
                customMusicFiles.Clear();
                errCount++;
            }
            List<string> loadedSongFiles = new List<string>();
            foreach (string path in customMusicFiles)
            {
                List<string> validExtensions = new List<string>() { ".mp3", ".ogg", ".wav" };
                if (!validExtensions.Contains(Path.GetExtension(path)))
                {
                    sb.AppendLine($"[<color=#ffff00>!</color>] Could not load '{Path.GetFileName(path)}': Only MP3, OGG and WAV files are supported.\n");
                    log.text = sb.ToString();
                    errCount++;
                    continue;
                }
                string songName = Path.GetFileNameWithoutExtension(path);
                if (loadedSongFiles.Contains(songName))
                {
                    sb.AppendLine($"[<color=#ffff00>!</color>] A song with the name '{songName}' has already been loaded!\n");
                    log.text = sb.ToString();
                    errCount++;
                    continue;
                }
                sb.AppendLine($"Loading '{songName}'...");
                log.text = sb.ToString();
                AudioClip clip = null;
                string newPath = Path.Combine(Application.persistentDataPath, $"customSongs/{songName}.mp3");
                bool newFile = Path.GetFullPath(path) != Path.GetFullPath(newPath);
                if (newFile)
                {
                    process = new Process();
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = cmdFfmpeg ? "ffmpeg" : ffmpegPath,
                        Arguments = $"-i \"{path}\" \"{newPath}\"",
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    process.Start();
                    Stopwatch sw = Stopwatch.StartNew();
                    while (sw.Elapsed.Seconds < 30 && !process.HasExited) { yield return null; }
                    bool hasExited = process.HasExited;
                    if (!hasExited) { process.Kill(); }
                    process.Close();
                    process.Dispose();
                    process = null;
                    if (!hasExited)
                    {
                        sb.AppendLine($"[<color=#ffff00>!</color>] Error while loading '{songName}': loading time exceeded 30 seconds, timeout exception triggered.\n");
                        log.text = sb.ToString();
                        errCount++;
                        continue;
                    }
                }
                BassNet.Registration(BassNetData.mail, BassNetData.key);
                AudioImporter importer = GetComponent<BassImporter>();
                importer.Import(newPath);
                while (!importer.isInitialized && !importer.isError) { yield return null; }
                if (importer.isError)
                {
                    sb.AppendLine($"[<color=#ff0000>X</color>] Error while loading '{songName}': {importer.error}\n");
                    log.text = sb.ToString();
                    errCount++;
                    continue;
                }
                while (!importer.isDone) { yield return null; }
                clip = importer.audioClip;
                if (clip != null)
                {
                    clip.name = songName;
                    VirtualRAM.loadedSongs.Add(clip);
                    loadedSongFiles.Add(songName);
                    sb.AppendLine($"[<color=#00ff00>O</color>] '{clip.name}' loaded successfully!");
                    log.text = sb.ToString();
#if UNITY_EDITOR
                    if (!Input.anyKey && path == customMusicFiles[0]) { break; }
#else
                    if (newFile) { File.Delete(path); }
#endif
                }
                sb.AppendLine();
                log.text = sb.ToString();
                yield return null;
            }
            if (errCount > 0)
            {
                sb.AppendLine($"[<color=#ffa000>?</color>] {errCount} {(errCount == 1 ? "error has" : "errors have")} occurred while loading custom music! Press Esc to quit, press any other key to continue.");
                log.text = sb.ToString();
                yield return new WaitUntil(() => !Input.anyKeyDown);
                yield return new WaitUntil(() => Input.anyKeyDown);
                if (Input.GetKeyDown(KeyCode.Escape)) { Application.Quit(); }
            }
        }
        SceneManager.LoadScene(1);
    }
    private void OnApplicationQuit()
    {
        if (process != null)
        {
            if (!process.HasExited) { process.Kill(); }
            process.Close();
            process.Dispose();
        }
    }
    bool CheckFfmpeg(string _path)
    {
        MoulinetteTerminal.TerminalOutput result;
        process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = _path,
            Arguments = "-version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        try { process.Start(); } catch { return false; }
        result = new MoulinetteTerminal.TerminalOutput(process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
        process.WaitForExit();
        process.Close();
        process.Dispose();
        process = null;
#if PLATFORM_STANDALONE_OSX
        return result.output.Length > 0;
#endif
        return result.output.Length > 0 && result.error.Length == 0;
    }
}