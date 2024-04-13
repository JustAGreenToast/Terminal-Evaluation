using UnityEngine;

public class WindowBlindersScript : MonoBehaviour
{
    int frameBuffer;
    Sprite blindersClosed;
    Sprite blindersSmear;
    Sprite blindersOpen;
    SpriteRenderer blinders;
    Sprite buttonClosed;
    Sprite buttonOpen;
    SpriteRenderer button;
    bool isOpen;
    float smearTimer;
    // Start is called before the first frame update
    void Start()
    {
        blindersClosed = Resources.Load<Sprite>($"Sprites/Main Office Textures/{SettingsManager.settings.mainOfficeTextureSet}/BlindersClosed");
        blindersSmear = Resources.Load<Sprite>($"Sprites/Main Office Textures/{SettingsManager.settings.mainOfficeTextureSet}/BlindersSmear");
        blindersOpen = Resources.Load<Sprite>($"Sprites/Main Office Textures/{SettingsManager.settings.mainOfficeTextureSet}/BlindersOpen");
        blinders = transform.GetChild(0).GetComponent<SpriteRenderer>();
        buttonClosed = Resources.Load<Sprite>($"Sprites/Main Office Textures/{SettingsManager.settings.mainOfficeTextureSet}/BlindersButtonOff");
        buttonOpen = Resources.Load<Sprite>($"Sprites/Main Office Textures/{SettingsManager.settings.mainOfficeTextureSet}/BlindersButtonOn");
        button = transform.GetChild(1).GetComponent<SpriteRenderer>();
        blinders.gameObject.SetActive(VirtualRAM.examData.windowObstacle == VirtualRAM.ExamData.WindowObstacle.Blinders);
        button.gameObject.SetActive(VirtualRAM.examData.windowObstacle == VirtualRAM.ExamData.WindowObstacle.Blinders);
        isOpen = false;
        blinders.sprite = blindersClosed;
        button.sprite = buttonClosed;
    }
    // Update is called once per frame
    void Update()
    {
        if (frameBuffer > 0)
        {
            frameBuffer--;
            if (frameBuffer == 0) { SetState(false); }
        }
        if (smearTimer > 0)
        {
            smearTimer -= Time.deltaTime;
            if (smearTimer <= 0) { blinders.sprite = isOpen ? blindersOpen : blindersClosed; }
        }
    }
    public void ButtonPressed()
    {
        if (frameBuffer == 0) { SetState(true); }
        frameBuffer = 5;
    }
    void SetState(bool _isOpen)
    {
        if (isOpen != _isOpen)
        {
            isOpen = _isOpen;
            smearTimer = 0.025f;
            blinders.sprite = blindersSmear;
            button.sprite = isOpen ? buttonOpen : buttonClosed;
        }
    }
}