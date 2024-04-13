using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ErrorScreenScript : MonoBehaviour
{
    static string log;
    static string stackTrace;
    [SerializeField] TextMeshProUGUI errorLabel;
    // Start is called before the first frame update
    void Start()
    {
        errorLabel.text = $"{log}\n\n{stackTrace}";
    }
    public void Retry() { SceneManager.LoadScene(1); }
    public void Quit() { Application.Quit(); }
    public static void Init() { Application.logMessageReceived += OnMessageReceived; }
    static void OnMessageReceived(string _log, string _stackTrace, LogType _type)
    {
        if (_type == LogType.Log || _type == LogType.Warning) { return; }
        log = _log;
        stackTrace = _stackTrace;
        SceneManager.LoadScene(6);
    }
}