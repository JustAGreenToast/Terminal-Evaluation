using System.Reflection;
using UnityEngine;

public class WindowBlindersScript : MonoBehaviour, IMainOfficeTexturable
{
    int frameBuffer;
    Sprite blindersClosed;
    Sprite blindersSmear;
    Sprite blindersOpen;
    SpriteRenderer _blinders;
    SpriteRenderer blinders
    {
        get
        {
            if (!_blinders) { _blinders = transform.GetChild(0).GetComponent<SpriteRenderer>(); }
            return _blinders;
        }
    }
    Sprite buttonClosed;
    Sprite buttonOpen;
    SpriteRenderer _button;
    SpriteRenderer button
    {
        get
        {
            if (!_button) { _button = transform.GetChild(1).GetComponent<SpriteRenderer>(); }
            return _button;
        }
    }
    bool isOpen;
    float smearTimer;
    // Start is called before the first frame update
    void Start()
    {
        blinders.gameObject.SetActive(VirtualRAM.examData.windowObstacle == VirtualRAM.ExamData.WindowObstacle.Blinders);
        button.gameObject.SetActive(VirtualRAM.examData.windowObstacle == VirtualRAM.ExamData.WindowObstacle.Blinders);
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
            if (smearTimer <= 0) { UpdateBlindersSprite(); }
        }
    }
    public void LoadTextures(string _folderName)
    {
        blindersClosed = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/BlindersClosed");
        blindersSmear = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/BlindersSmear");
        blindersOpen = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/BlindersOpen");
        UpdateBlindersSprite();
        // Sandy Shores Parasol
        if (_folderName == "10")
        {
            blinders.transform.localPosition = new Vector3(0, -0.375f, -0.25f);
            blinders.GetComponent<LookAtCamScript>().enabled = true;
        }
        else
        {
            blinders.transform.localPosition = Vector3.zero;
            blinders.GetComponent<LookAtCamScript>().enabled = false;
            blinders.transform.localRotation = Quaternion.identity;
        }
        buttonClosed = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/BlindersButtonOff");
        buttonOpen = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/BlindersButtonOn");
        UpdateButtonSprite();
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
            UpdateBlindersSprite();
            UpdateButtonSprite();
        }
    }
    void UpdateBlindersSprite() { blinders.sprite = smearTimer > 0 ? blindersSmear : isOpen ? blindersOpen : blindersClosed; }
    void UpdateButtonSprite() { button.sprite = isOpen ? buttonOpen : buttonClosed; }
}