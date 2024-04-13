using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ServerGUIScript : MonitorWindow
{
    [SerializeField] GameManagerScript manager;
    [SerializeField] TextMeshProUGUI statusLabel;
    [SerializeField] Image ventButton;
    [SerializeField] Image powerButton;
    [SerializeField] Image temperatureSliderBorder;
    [SerializeField] Transform temperatureSlider;
    [SerializeField] Transform selectButtons;
    [SerializeField] Sprite[] serverIcons;
    [SerializeField] Sprite[] powerIcons;
    GameManagerScript.Server server { get { return manager.servers[selectedIndex]; } }
    int selectedIndex;
    public override void OnPullUp() { UpdateGUI(); }
    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            selectedIndex++;
            if (selectedIndex == 4) { selectedIndex = 0; }
            UpdateGUI();
            return;
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            selectedIndex--;
            if (selectedIndex == -1) { selectedIndex = 3; }
            UpdateGUI();
            return;
        }
        KeyCode[][] keys = new KeyCode[4][]
        {
            new KeyCode[]{ KeyCode.Alpha1, KeyCode.Keypad1 },
            new KeyCode[]{ KeyCode.Alpha2, KeyCode.Keypad2 },
            new KeyCode[]{ KeyCode.Alpha3, KeyCode.Keypad3 },
            new KeyCode[]{ KeyCode.Alpha4, KeyCode.Keypad4 },
        };
        for (int i = 0; i < 4; i++)
        {
            foreach (KeyCode key in keys[i])
            {
                if (Input.GetKeyDown(key))
                {
                    SelectServer(i);
                    return;
                }
            }
        }
    }
    public void SelectServer(int n)
    {
        selectedIndex = n;
        UpdateGUI();
    }
    public void UpdateGUI()
    {
        for (int i = 0; i < selectButtons.childCount; i++) { selectButtons.GetChild(i).GetComponent<Image>().sprite = serverIcons[selectedIndex == i ? 1 : 0]; }
        statusLabel.text = $"Status: {server.statusString}";
        powerButton.sprite = powerIcons[server.shutdownQueued ? 1 : 0];
        powerButton.color = server.powerButtonColor;
        ventButton.color = server.ventButtonColor;
        temperatureSliderBorder.color = server.temperatureMeterColor;
        temperatureSlider.localScale = new Vector3(server.heatTimer, 1, 1);
    }
    public void ToggleVentilation()
    {
        server.ToggleVentilation();
        UpdateGUI();
    }
    public void ToggleStatus()
    {
        if (server.shutdownQueued) { server.CancelShutdown(); } else { server.ToggleState(); }
        UpdateGUI();
    }
}