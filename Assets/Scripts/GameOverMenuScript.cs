using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverMenuScript : MonoBehaviour
{
    readonly string[] regularGameOverMessages = { "- TKO -", "Better luck next time!", "Final Rank: F", "At least you tried...", "Pro Tip: Don't do that.", "Blackholed!", "At least you didn't get a TIG..." };
    readonly string[] specialGameOverMessages = { "Good riddance.", "./final_exam: segmentation fault", "Definitely gonna get a TIG for that one...", "$ sudo shutdown -h now" };
    float t;
    [SerializeField] TextMeshProUGUI gameOverMessage;
    [SerializeField] TextMeshProUGUI failMessage;
    [SerializeField] Image overlay;
    // Start is called before the first frame update
    void Start()
    {
        gameOverMessage.text = VirtualRAM.lastDanceScore != -1 ? $"Final Score: {VirtualRAM.lastDanceScore}" : regularGameOverMessages[Random.Range(0, regularGameOverMessages.Length)];
        failMessage.text = VirtualRAM.failMessage;
        t = 1;
    }
    // Update is called once per frame
    void Update()
    {
        if (t > 0)
        {
            t -= 5 * Time.deltaTime;
            overlay.color = new Color(1, 1, 1, Mathf.Clamp01(t));
            if (t <= 0) { overlay.enabled = false; }
        }
    }
    public void Retry() { SceneManager.LoadScene(2); }
    public void Quit() { SceneManager.LoadScene(1); }
}