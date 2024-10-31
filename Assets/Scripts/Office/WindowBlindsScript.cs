using UnityEngine;

public class WindowBlindsScript : MonoBehaviour, IMainOfficeTexturable
{
    int frameBuffer;
    Sprite blindsClosed;
    Sprite blindsSmear;
    Sprite blindsOpen;
    SpriteRenderer _blinds;
    SpriteRenderer blinds
    {
        get
        {
            if (!_blinds) { _blinds = transform.GetChild(0).GetComponent<SpriteRenderer>(); }
            return _blinds;
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
        blinds.gameObject.SetActive(VirtualRAM.examData.windowObstacle == VirtualRAM.ExamData.WindowObstacle.Blinds);
        button.gameObject.SetActive(VirtualRAM.examData.windowObstacle == VirtualRAM.ExamData.WindowObstacle.Blinds);
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
            if (smearTimer <= 0) { UpdateblindsSprite(); }
        }
    }
    public void LoadTextures(string _folderName)
    {
        blindsClosed = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/blindersClosed");
        blindsSmear = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/blindersSmear");
        blindsOpen = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/blindersOpen");
        UpdateblindsSprite();
        // Sandy Shores Parasol
        if (_folderName == "10")
        {
            blinds.transform.localPosition = new Vector3(0, -0.375f, -0.25f);
            blinds.GetComponent<LookAtCamScript>().enabled = true;
        }
        else
        {
            blinds.transform.localPosition = Vector3.zero;
            blinds.GetComponent<LookAtCamScript>().enabled = false;
            blinds.transform.localRotation = Quaternion.identity;
        }
        buttonClosed = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/blindersButtonOff");
        buttonOpen = Resources.Load<Sprite>($"Sprites/Main Office Textures/{_folderName}/blindersButtonOn");
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
            UpdateblindsSprite();
            UpdateButtonSprite();
        }
    }
    void UpdateblindsSprite() { blinds.sprite = smearTimer > 0 ? blindsSmear : isOpen ? blindsOpen : blindsClosed; }
    void UpdateButtonSprite() { button.sprite = isOpen ? buttonOpen : buttonClosed; }
}