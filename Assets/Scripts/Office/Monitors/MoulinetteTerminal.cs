using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public static class MoulinetteTerminal
{
    static readonly string evalFolder = Path.Combine(Application.streamingAssetsPath, "eval");
    static readonly string sigHandlerScript = "#include <unistd.h>\n#include <stdlib.h>\n#include <signal.h>\n\nvoid signal_handler(int signal)\n{\n\tswitch(signal)\n\t{\n\t\tcase SIGSEGV:\n\t\t\twrite(2, \"SEGFAULT\", 8);\n\t\t\tbreak;\n\t\tcase SIGFPE:\n\t\t\twrite(2, \"Floating point exception.\", 25);\n\t\t\tbreak;\n\t\tcase SIGABRT:\n\t\t\twrite(2, \"Program aborted.\", 16);\n\t\t\tbreak;\n\t}\n\texit(1);\n}\n\nvoid add_sighandlers()\n{\n\tsignal(SIGSEGV, signal_handler);\n\tsignal(SIGFPE, signal_handler);\n\tsignal(SIGABRT, signal_handler);\n}";
#if PLATFORM_STANDALONE_WIN || UNITY_EDITOR_WIN
    static readonly string gccPath = Path.Combine(evalFolder, "MinGW64/bin/gcc.exe");
    static readonly string nmPath = Path.Combine(evalFolder, "MinGW64/bin/nm.exe");
#else
    static readonly string gccPath = "gcc";
    static readonly string nmPath = "nm";
    static readonly string valgrindPath = "valgrind";
#endif
    static readonly string exePath = Path.Combine(evalFolder, "test.exe");
    static readonly string objPath = Path.Combine(evalFolder, "test.o");
    public enum Status { EmptyFile, ForbiddenFunction, CompileError, CompileWarning, RuntimeError, TimeOut, OK, KO };
    public struct TerminalOutput
    {
        public string output;
        public string error;
        public TerminalOutput(string output, string error)
        {
            this.output = output;
            this.error = error;
        }
    }
    static Process terminal;
    private static string _trace;
    public static string lastTrace { get { return string.IsNullOrWhiteSpace(_trace) ? "No trace available." : $"Last trace:\n\n{_trace}"; } }
    public static bool CheckGcc()
    {
        TerminalOutput result;
        terminal = new Process();
        terminal.StartInfo = new ProcessStartInfo
        {
            FileName = gccPath,
            Arguments = "--version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        try { terminal.Start(); } catch { return false; }
        result = new TerminalOutput(terminal.StandardOutput.ReadToEnd(), terminal.StandardError.ReadToEnd());
        terminal.WaitForExit();
        terminal.Close();
        terminal.Dispose();
        terminal = null;
        return result.output.Length > 0 && result.error.Length == 0;
    }
    public static bool CheckNm()
    {
        TerminalOutput result;
        terminal = new Process();
        terminal.StartInfo = new ProcessStartInfo
        {
            FileName = nmPath,
            Arguments = "--version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        try { terminal.Start(); } catch { return false; }
        result = new TerminalOutput(terminal.StandardOutput.ReadToEnd(), terminal.StandardError.ReadToEnd());
        terminal.WaitForExit();
        terminal.Close();
        terminal.Dispose();
        terminal = null;
        return result.output.Length > 0 && result.error.Length == 0;
    }
#if !PLATFORM_STANDALONE_WIN
    //public static bool CheckValgrind()
    //{
    //    TerminalOutput result;
    //    terminal = new Process();
    //    terminal.StartInfo = new ProcessStartInfo
    //    {
    //        FileName = valgrindPath,
    //        Arguments = "--version",
    //        RedirectStandardOutput = true,
    //        RedirectStandardError = true,
    //        UseShellExecute = false,
    //        CreateNoWindow = true
    //    };
    //    try { terminal.Start(); } catch { return false; }
    //    result = new TerminalOutput(terminal.StandardOutput.ReadToEnd(), terminal.StandardError.ReadToEnd());
    //    terminal.WaitForExit();
    //    terminal.Close();
    //    terminal.Dispose();
    //    terminal = null;
    //    return result.output.Length > 0 && result.error.Length == 0;
    //}
#endif
    public static Status Evaluate(string _script, Exercise _ex)
    {
        // Reset Traceback Data
        _trace = "";
        // Empty File Check
        if (string.IsNullOrEmpty(_script))
        {
            _trace = "<#color=ffff00Error: Git repository is empty or missing critical files!</color>\n\nDid you remember to upload your files to your git repository?";
            return Status.EmptyFile;
        }
        // Try Compiling Script
        Task<TerminalOutput> compTask = Task.Run(() => CompileScript(_script, _ex));
        Stopwatch sw = Stopwatch.StartNew();
        while (!compTask.IsCompleted)
        {
            if (sw.Elapsed.Seconds > 5)
            {
                terminal.Kill();
                terminal.Dispose();
                terminal = null;
                _trace = $"<color=#ffff00>TimeOut</color>\n\nCompilation time exceeded timeout limit.";
                return Status.TimeOut;
            }
            //else { UnityEngine.Debug.Log(sw.Elapsed.ToString()); }
        }
        TerminalOutput terminalResult = compTask.Result;
        if (terminalResult.error != "")
        {
            _trace = $"<color=#ff0000>Error:</color> {terminalResult.error}";
            return Status.CompileError;
        }
        if (terminalResult.output != "")
        {
            _trace = $"<color=#ffff00>Warning:</color> {terminalResult.output}";
            return Status.CompileWarning;
        }
        // Look For Forbidden Functions
        string[] forbiddenFunctions = FindForbiddenFunctions(_script, _ex.allowedFunctions);
        if (forbiddenFunctions.Length > 0)
        {
            _trace = $"<color=#ff0000>Forbidden Function{(forbiddenFunctions.Length > 1 ? "s" : "")}</color>\n";
            foreach (string function in forbiddenFunctions) { _trace += $"\n- {function}"; }
            return Status.ForbiddenFunction;
        }
        // Run Test Cases
        foreach (Exercise.TestCase testCase in _ex.testCases)
        {
            Task<TerminalOutput> evalTask = Task.Run(() => RunFile(testCase.args, _ex.folderName));
            sw = Stopwatch.StartNew();
            string testCaseData = $"Arguments: {(string.IsNullOrWhiteSpace(testCase.args) ? "(None)" : testCase.args)}\nExpected: \"{testCase.output.Replace("\n", "<color=#7f7f7f>\\n</color>")}\"";
            while (!evalTask.IsCompleted)
            {
                if (sw.Elapsed.Seconds > 5)
                {
                    terminal.Kill();
                    terminal.Dispose();
                    terminal = null;
                    _trace = $"<color=#ffff00>TimeOut</color>\n\nExecution time exceeded timeout limit.\n\n{testCaseData}";
                    return Status.TimeOut;
                }
                //else { UnityEngine.Debug.Log(sw.Elapsed.ToString()); }
            }
            if (evalTask.IsCompletedSuccessfully)
            {
                terminalResult = evalTask.Result;
                terminalResult.output = terminalResult.output.Replace("\r\n", "\n").Replace("\r", "\n");
                if (terminalResult.error != "")
                {
                    if (terminalResult.error == "SEGFAULT") { _trace = $"<color=#ffff00>SIGSEGV / SEGFAULT</color>\n\nSegmentation fault detected.\n\n{testCaseData}"; }
                    else { _trace = $"<color=#ff0000>Error:</color> {terminalResult.error}"; }
                    evalTask.Dispose();
                    return Status.RuntimeError;
                }
                else if (terminalResult.output != testCase.output)
                {
                    _trace = $"<color=#ff0000>KO</color>\n\n{testCaseData}\nResult: \"{terminalResult.output.Replace("\n", "<color=#7f7f7f>\\n</color>")}\"";
                    //UnityEngine.Debug.Log(string.Compare(testCase.output, terminalResult.output));
                    evalTask.Dispose();
                    return Status.KO;
                }
            }
            else
            {
                _trace = $"<color=#ff0000>Error:</color> Could not evaluate task :(";
                evalTask.Dispose();
                return Status.RuntimeError;
            }
#if !PLATFORM_STANDALONE_WIN
            //Task<TerminalOutput> leaksTask = Task.Run(() => CheckLeaks(testCase.args, _ex.folderName));
            //terminalResult = evalTask.Result;
            //terminalResult.output = terminalResult.output.Replace("\r\n", "\n").Replace("\r", "\n");
            //if (terminalResult.error != "")
            //{
            //    _trace = $"<color=#ff0000>Error:</color> {terminalResult.error}";
            //    return Status.RuntimeError;
            //}
            //else if (terminalResult.output != testCase.output)
            //{
            //    _trace = $"Result: \"{terminalResult.output.Replace("\n", "<color=#7f7f7f>\\n</color>")}\"";
            //}
            //break;
#endif
        }
        return Status.OK;
    }
    static TerminalOutput CompileScript(string _script, Exercise _ex)
    {
        bool foundMainFunc = false;
        string[] filePaths = new string[_ex.extraScripts.Length + 2];
        for (int i = 0; i < filePaths.Length; i++)
        {
            filePaths[i] = Path.Combine(evalFolder, $"script_{i}.c");
            if (i < filePaths.Length - 2)
            {
                if (!foundMainFunc && Regex.IsMatch(_ex.extraScripts[i], @"(.*?int\s+main\(.*?\)\s*\{)(.+)", RegexOptions.IgnoreCase | RegexOptions.Singleline))
                {
                    foundMainFunc = true;
                    File.WriteAllText(filePaths[i], AddSignalDetection(_ex.extraScripts[i]));
                }
                else { File.WriteAllText(filePaths[i], _ex.extraScripts[i]); }
            }
        }
        File.WriteAllText(filePaths[filePaths.Length - 2], sigHandlerScript);
        File.WriteAllText(filePaths[filePaths.Length - 1], foundMainFunc ? _script : AddSignalDetection(_script));
        StringBuilder sb = new StringBuilder();
        sb.Append("-std=c11 -Wall -Wextra -Werror -O0 ");
        //#if !PLATFORM_STANDALONE_WIN
        //        sb.Append("-ggdb3 ");
        //#endif
        sb.Append($"-o \"{exePath}\"");
        for (int i = 0; i < filePaths.Length; i++) { sb.Append($" \"{filePaths[i]}\""); }
        TerminalOutput result;
        terminal = new Process();
        terminal.StartInfo = new ProcessStartInfo
        {
            FileName = gccPath,
            Arguments = sb.ToString(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        terminal.Start();
        result = new TerminalOutput(terminal.StandardOutput.ReadToEnd().Replace(filePaths[filePaths.Length - 1], _ex.fileName), terminal.StandardError.ReadToEnd().Replace(filePaths[filePaths.Length - 1], _ex.fileName));
        terminal.WaitForExit();
        terminal.Close();
        terminal.Dispose();
        terminal = null;
        for (int i = 0; i < filePaths.Length; i++) { File.Delete(filePaths[i]); }
        return result;
    }
    static string AddSignalDetection(string _script)
    {
        string sigHandlerHeader = "void add_sighandlers();";
        string sigHandlerCall = "add_sighandlers(); // Ignore this line :P";
        StringBuilder sb = new StringBuilder(_script.Length + sigHandlerHeader.Length + sigHandlerCall.Length);
        // Look For Start Of Main Function
        Match m = Regex.Match(_script, @"(.*?int\s+main\(.*?\)\s*\{)(.+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        // Failsafe Early Return
        if (!m.Success || m.Groups.Count != 3) { return _script; }
        // Add Signal Handler At The Top Of The Script
        sb.AppendLine(sigHandlerHeader);
        // Add First Part Of Script
        sb.AppendLine(m.Groups[1].Value);
        // Add Signal Handler Subscription
        sb.AppendLine(sigHandlerCall);
        // Add Second Part Of Script
        sb.Append(m.Groups[2].Value);
        // Return New String
        return sb.ToString();
    }
    static string[] FindForbiddenFunctions(string _script, string[] _allowed)
    {
        string file = Path.Combine(evalFolder, "obj_test.c");
        File.WriteAllText(file, _script);
        string args = $" -O0 -std=c11 -c -o \"{objPath}\" \"{file}\"";
        terminal = new Process();
        terminal.StartInfo = new ProcessStartInfo
        {
            FileName = gccPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        terminal.Start();
        while (!terminal.HasExited) { terminal.WaitForExit(); }
        args = $" -u \"{objPath}\"";
        terminal = new Process();
        terminal.StartInfo = new ProcessStartInfo
        {
            FileName = nmPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        terminal.Start();
        TerminalOutput result = new TerminalOutput(terminal.StandardOutput.ReadToEnd(), terminal.StandardError.ReadToEnd());
        while (!terminal.HasExited) { terminal.WaitForExit(); }
        terminal.Close();
        terminal.Dispose();
        terminal = null;
        File.Delete(file);
        File.Delete(objPath);
        List<string> forbiddenFunctions = new List<string>();
        foreach (string function in result.output.Split("\r\n"))
        {
            if (string.IsNullOrEmpty(function)) { continue; }
            string functionName = function.Trim().Substring(2);
            if (!functionName.StartsWith("__") && !_allowed.Contains(functionName))
            {
                if (!_script.Contains(functionName))
                {
                    switch (functionName)
                    {
                        case "putchar":
                        case "puts":
                            functionName += " <color=#ffff00>(printf?)</color>";
                            break;
                    }
                }
                forbiddenFunctions.Add(functionName);
            }
        }
        return forbiddenFunctions.ToArray();
    }
    static TerminalOutput RunFile(string _args, string _exName)
    {
        TerminalOutput result;
        terminal = new Process();
        terminal.StartInfo = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = _args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        terminal.Start();
        result = new TerminalOutput(terminal.StandardOutput.ReadToEnd(), terminal.StandardError.ReadToEnd().Replace(exePath, _exName));
        terminal.WaitForExit();
        terminal.Close();
        terminal.Dispose();
        terminal = null;
        return result;
    }
#if !PLATFORM_STANDALONE_WIN
    //static TerminalOutput CheckLeaks(string _args, string _exName)
    //{
    //    TerminalOutput result;
    //    terminal = new Process();
    //    terminal.StartInfo = new ProcessStartInfo
    //    {
    //        FileName = valgrindPath,
    //        Arguments = $"./{exePath} {_args}",
    //        RedirectStandardOutput = true,
    //        RedirectStandardError = true,
    //        UseShellExecute = false,
    //        CreateNoWindow = true
    //    };
    //    terminal.Start();
    //    result = new TerminalOutput(terminal.StandardOutput.ReadToEnd().Replace(exePath, _exName), terminal.StandardError.ReadToEnd().Replace(exePath, _exName));
    //    terminal.WaitForExit();
    //    terminal.Close();
    //    terminal.Dispose();
    //    terminal = null;
    //    return result;
    //}
#endif
    public static void OnQuit()
    {
        if (terminal != null)
        {
            if (!terminal.HasExited) { terminal.Kill(); }
            terminal.Close();
            terminal.Dispose();
            terminal = null;
        }
    }
}