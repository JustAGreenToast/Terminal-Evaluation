using UnityEngine;

public class MidnightScript : EnemyScript
{
    enum States { None, OnWindow, OnWindowAnnoyed, OnDoor, KnockingDoor, BeforeAttack, Attack };
    States currentState;
    float stateTimer;
    int stateCounter;
    const float windowAttackTime = 1.5f;
    float doorKnockDelay { get { return 0.75f * (SettingsManager.settings.midnightAggressiveKnock ? 0.4f : 1); } }
    int maxKnocks { get { return SettingsManager.settings.midnightAggressiveKnock ? 20 : 8; } }
    float moveCooldown { get { return Random.value > 0.9f ? Random.Range(2.5f, 7.5f) : Random.Range(10f, 15f); } }
    bool onLap2;
    Sprite[] sprites;
    SpriteRenderer r;
    AudioClip lStepClip;
    AudioClip rStepClip;
    AudioClip cStepClip;
    AudioClip doorKnockClip;
    AudioClip doorSlamClip;
    public bool isKnocking { get { return currentState == States.KnockingDoor; } }
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Midnight; }
    protected override void OnStart()
    {
        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Midnight");
        lStepClip = Resources.Load<AudioClip>("SFX/midnight_left");
        rStepClip = Resources.Load<AudioClip>("SFX/midnight_right");
        cStepClip = Resources.Load<AudioClip>("SFX/footstep_center");
        doorKnockClip = Resources.Load<AudioClip>("SFX/door_slam");
        doorSlamClip = Resources.Load<AudioClip>("SFX/door_slam_hard");
        r = GetComponent<SpriteRenderer>();
        r.enabled = false;
        stateTimer = moveCooldown;
        if (VirtualRAM.examData.examIndex == 6 && SettingsManager.settings.midnightDoor) { stateTimer += Random.Range(42f, 80f); }
    }
    protected override void OnUpdate()
    {
        switch (currentState)
        {
            case States.None:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    if (AbleToMove())
                    {
                        bool windowsAvailable = WindowsAvailable();
                        bool doorAvailable = DoorAvailable();
                        if (windowsAvailable && doorAvailable)
                        {
                            if (Random.value > 0.5f) { MoveToWindow(); }
                            else { MoveToDoor(); }
                        }
                        else if (windowsAvailable) { MoveToWindow(); }
                        else if (doorAvailable) { MoveToDoor(); }
                        else { stateTimer = moveCooldown; }
                    }
                    else { stateTimer = moveCooldown; }
                }
                break;
            case States.OnWindow:
                if (currentLocation == Locations.LeftWindow ? manager.IsPlayerLookingAtRightWindow() : manager.IsPlayerLookingAtLeftWindow())
                {
                    stateTimer -= Time.deltaTime;
                    if (stateTimer < 0)
                    {
                        currentState = States.None;
                        stateTimer = moveCooldown;
                        if (currentLocation == Locations.RightWindow) { manager.PlaySound(rStepClip, SettingsManager.settings.explicitSubtitles ? "Midnight Move" : "Ominous Whisper", false, true); }
                        else { manager.PlaySound(lStepClip, SettingsManager.settings.explicitSubtitles ? "Midnight Move" : "Ominous Whisper", true, false); }
                        currentLocation = Locations.None;
                        manager.TriggerHallOverlay();
                        r.enabled = false;
                    }
                }
                else
                {
                    currentState = States.OnWindowAnnoyed;
                    stateTimer = 5;
                }
                break;
            case States.OnWindowAnnoyed:
                if (currentLocation == Locations.LeftWindow ? manager.IsPlayerLookingAtRightWindow() : manager.IsPlayerLookingAtLeftWindow())
                {
                    currentState = States.OnWindow;
                    stateTimer = windowAttackTime;
                }
                else
                {
                    stateTimer -= Time.deltaTime * balanceFactor;
                    if (stateTimer <= 0 && AbleToMove())
                    {
                        currentState = States.BeforeAttack;
                        stateTimer = Random.Range(1.5f, 5);
                        currentLocation = Locations.Door;
                        manager.TriggerHallOverlay();
                        transform.position = Vector3.forward * 5;
                        r.sprite = sprites[3];
                    }
                }
                break;
            case States.OnDoor:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    currentState = States.KnockingDoor;
                    stateCounter = 0;
                }
                break;
            case States.KnockingDoor:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    manager.TriggerHallOverlay();
                    if (stateCounter == 0)
                    {
                        r.sprite = sprites[(Random.value < 0.25f != SettingsManager.settings.midnightAggressiveKnock) && onLap2 ? 5 : 3];
                        if (IsEnemyComboAvailable(EnemyTypes.Melissa)) { TriggerEnemyCombo(EnemyTypes.Melissa); }
                        else if (IsEnemyComboAvailable(EnemyTypes.Barcode)) { TriggerEnemyCombo(EnemyTypes.Barcode); }
                        else if (IsEnemyComboAvailable(EnemyTypes.Chelsea)) { TriggerEnemyCombo(EnemyTypes.Chelsea); }
                    }
                    stateCounter++;
                    if (manager.IsDoorLocked())
                    {
                        if (stateCounter == maxKnocks && SettingsManager.settings.midnightDoor)
                        {
                            if (manager.IsDoorLocked()) { MoveToDoor(); }
                            else { currentState = States.BeforeAttack; }
                        }
                        else if (stateCounter % 2 == 0 || (stateCounter == 7 && maxKnocks == 8))
                        {
                            currentState = States.None;
                            stateTimer = moveCooldown;
                            currentLocation = Locations.None;
                            manager.TriggerHallOverlay();
                            r.enabled = false;
                        }
                        else { stateTimer = doorKnockDelay; }
                    }
                    else if (stateCounter == maxKnocks) { currentState = States.BeforeAttack; }
                    else { stateTimer = doorKnockDelay; }
                    if (currentState != States.BeforeAttack) { manager.PlaySound(doorKnockClip, "Door Knock", true, true); }
                }
                break;
            case States.BeforeAttack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    currentState = States.Attack;
                    stateTimer = 1.5f;
                    manager.TriggerRoomOverlay();
                    r.sprite = sprites[4];
                    manager.RotateDoor(180, 1600);
                    manager.PlaySound(doorSlamClip, "Door Slam", true, true);
                    manager.LockPlayer();
                    manager.LockEnemies(this);
                    manager.LockCamera();
                    manager.FadeOutMusic();
                }
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { manager.ExamFailed($"Midnight will show up on either the left or the right window, make sure you're facing the opposite window until she leaves. Whether you're looking at the right window is based on your camera's angle, not on your monitor being up or down.\n\nMidnight can also show up and your door and start knocking: if she knocks {maxKnocks} times and the door isn't being held shut, you lose."); }
                break;
        }
    }
    void MoveToWindow()
    {
        currentState = States.OnWindow;
        stateTimer = windowAttackTime;
        manager.TriggerHallOverlay();
        PickWindow();
        if (VirtualRAM.examData.windowObstacle == VirtualRAM.ExamData.WindowObstacle.Blinders && Random.value < 0.25f) { r.sprite = sprites[currentLocation == Locations.RightWindow ? 2 : 1]; }
        else { r.sprite = sprites[3]; }
        r.enabled = true;
        if (currentLocation == Locations.RightWindow) { manager.PlaySound(rStepClip, SettingsManager.settings.explicitSubtitles ? "Midnight Move" : "Ominous Whisper", false, true); }
        else { manager.PlaySound(lStepClip, SettingsManager.settings.explicitSubtitles ? "Midnight Move" : "Ominous Whisper", true, false); }
        if (IsEnemyComboAvailable(EnemyTypes.Barcode)) { TriggerEnemyCombo(EnemyTypes.Barcode); }
        else if (IsEnemyComboAvailable(EnemyTypes.Cassidy)) { TriggerEnemyCombo(EnemyTypes.Cassidy); }
    }
    void MoveToDoor()
    {
        currentState = States.OnDoor;
        stateTimer = moveCooldown;
        currentLocation = Locations.Door;
        manager.PlaySound(cStepClip, SettingsManager.settings.explicitSubtitles ? "Midnight Step" : "Strong Step", true, true);
        manager.TriggerHallOverlay();
        r.sprite = sprites[3];
        r.enabled = true;
        transform.position = Vector3.forward * 5;
    }
    public override void OnLap2Started()
    {
        onLap2 = true;
        if (VirtualRAM.examData.tiredMidnight) { IncreaseAI(3); }
    }
    void PickWindow()
    {
        bool left = manager.IsLocationAvailable(Locations.LeftWindow);
        bool right = manager.IsLocationAvailable(Locations.RightWindow);
        if (left != right) { currentLocation = left ? Locations.LeftWindow : Locations.RightWindow; }
        else { currentLocation = Random.value > 0.5f ? Locations.LeftWindow : Locations.RightWindow; }
        transform.position = new Vector3(5 * (currentLocation == Locations.LeftWindow ? -1 : 1), 0, 5);
    }
    bool AbleToMove()
    {
        switch (currentState)
        {
            case States.None:
                if (Random.Range(0, 10) >= aiLevel) { return false; }
                if (DoorAvailable()) { return true; }
                else if (WindowsAvailable()) { return true; }
                return false;
            case States.OnWindowAnnoyed: return manager.IsLocationAvailable(Locations.Door);
            default: return true;
        }
    }
    bool DoorAvailable() { return VirtualRAM.examData.tiredMidnight && manager.IsLocationAvailable(Locations.Door); }
    bool WindowsAvailable() { return (onLap2 || VirtualRAM.examData.examIndex > 5) && (manager.IsLocationAvailable(Locations.LeftWindow) || manager.IsLocationAvailable(Locations.RightWindow)); }
    public override bool IsAvaliableForCombo(EnemyTypes _other)
    {
        if (aiLevel == 0 || isLocked) { return false; }
        switch (_other)
        {
            case EnemyTypes.Cassidy:
            case EnemyTypes.Tournament_MixmaxStressToy:
                return currentState == States.None && (manager.IsLocationAvailable(Locations.LeftWindow) || manager.IsLocationAvailable(Locations.RightWindow)) && Random.value > 0.5f;
        }
        return false;
    }
    public override void ComboTriggered(EnemyTypes _other)
    {
        switch (_other)
        {
            case EnemyTypes.Cassidy:
            case EnemyTypes.Tournament_MixmaxStressToy:
                MoveToWindow();
                break;
        }
    }
}