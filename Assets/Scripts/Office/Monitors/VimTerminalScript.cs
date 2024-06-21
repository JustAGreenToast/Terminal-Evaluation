using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class VimTerminalScript : MonitorWindow
{
    [SerializeField] MainTerminalScript mainTerminal;
    [SerializeField] Slider fontSizeSlider;
    [SerializeField] TMP_InputField textArea;
    [SerializeField] TextMeshProUGUI fileNameLabel;
    protected override void OnStart() { fontSizeSlider.value = SettingsManager.settings.vimFontSize; }
    public override void OnPullUp()
    {
        textArea.text = mainTerminal.localScript;
        EventSystem.current.SetSelectedGameObject(textArea.gameObject);
        fileNameLabel.text = $"File: {mainTerminal.currentExerciseFileName}";
    }
    public override void OnPullDown() { if (EventSystem.current.currentSelectedGameObject == textArea.gameObject) { EventSystem.current.SetSelectedGameObject(null); } }
    public void SetFontSize(float _val)
    {
        textArea.pointSize = _val;
        SettingsManager.settings.SetVimFontSize(_val);
    }
}