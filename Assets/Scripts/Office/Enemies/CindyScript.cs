using UnityEngine;

public class CindyScript : EnemyScript
{
    enum States { None, OnWindow, OnWindowAnnoyed, BeforeAttack, Attack };
    States currentState;
    float stateTimer;
    const float windowAttackTime = 1.5f;
    float moveCooldown { get { return Random.value > 0.9f ? Random.Range(7.5f, 12.5f) : Random.Range(15f, 25f); } }
    float patienceTimer;
    Sprite[] sprites;
    SpriteRenderer r;
    AudioClip lStepClip;
    AudioClip rStepClip;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Cindy; }
    protected override void OnStart()
    {
        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Cindy");
        lStepClip = Resources.Load<AudioClip>("SFX/footstep_left_3");
        rStepClip = Resources.Load<AudioClip>("SFX/footstep_right_3");
        r = GetComponent<SpriteRenderer>();
        r.enabled = false;
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
        switch (currentState)
        {
            case States.None:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    if (AbleToMove()) { MoveToWindow(); }
                    else { stateTimer = moveCooldown; }
                }
                break;
            case States.OnWindow:
                if (patienceTimer == 0 && manager.IsGameMonitorUp())
                {
                    stateTimer -= Time.deltaTime;
                    if (stateTimer < 0)
                    {
                        currentState = States.None;
                        stateTimer = moveCooldown;
                        if (currentLocation == Locations.RightWindow) { manager.PlaySound(rStepClip, SettingsManager.settings.explicitSubtitles ? "Cindy Step" : "Sharp Step", false, true); }
                        else { manager.PlaySound(lStepClip, SettingsManager.settings.explicitSubtitles ? "Cindy Step" : "Sharp Step", true, false); }
                        currentLocation = Locations.None;
                        manager.TriggerHallOverlay();
                        r.enabled = false;
                    }
                }
                else if (!manager.IsGameMonitorUp())
                {
                    patienceTimer += Time.deltaTime * balanceFactor;
                    if (patienceTimer >= 1)
                    {
                        currentState = States.OnWindowAnnoyed;
                        stateTimer = 5;
                        manager.TriggerHallOverlay();
                        r.sprite = sprites[1];
                    }
                }
                else if (patienceTimer > 0) { patienceTimer = 0; }
                break;
            case States.OnWindowAnnoyed:
                if (manager.IsGameMonitorUp())
                {
                    currentState = States.OnWindow;
                    stateTimer = windowAttackTime;
                    patienceTimer = 0;
                    manager.TriggerHallOverlay();
                    r.sprite = sprites[0];
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
                        transform.position = new Vector3(0, 0, 5);
                        r.sprite = sprites[0];
                    }
                }
                break;
            case States.BeforeAttack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    currentState = States.Attack;
                    stateTimer = 1.5f;
                    //manager.TriggerRoomOverlay();
                    r.sprite = sprites[2];
                    manager.RotateDoor(120, 420);
                    manager.LockPlayer();
                    manager.LockEnemies(this);
                    manager.LockCamera();
                    manager.FadeOutMusic();
                }
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                    manager.ExamFailed("Cindy will show up on either the left or the right window, keep your Game Monitor up until she leaves.");
                break;
        }
    }
    protected override void OnMonitorFlipped() { if (currentState == States.None && manager.IsGameMonitorUp() && IsAvaliableForCombo(EnemyTypes.Carla) && AbleToMove()) { MoveToWindow(); } }
    void MoveToWindow()
    {
        currentState = States.OnWindow;
        stateTimer = windowAttackTime;
        manager.TriggerHallOverlay();
        r.sprite = sprites[0];
        r.enabled = true;
        PickWindow();
        if (currentLocation == Locations.RightWindow) { manager.PlaySound(rStepClip, SettingsManager.settings.explicitSubtitles ? "Cindy Step" : "Sharp Step", false, true); }
        else { manager.PlaySound(lStepClip, SettingsManager.settings.explicitSubtitles ? "Cindy Step" : "Sharp Step", true, false); }
    }
    bool AnyWindowAvailable()
    {
        if (!IsEnemyComboAvailable(EnemyTypes.Carla)) { return false; }
        if (!manager.IsLocationAvailable(Locations.LeftWindow) && !manager.IsLocationAvailable(Locations.RightWindow)) { return false; }
        if (manager.IsEnemyAtLocation(EnemyTypes.Chelsea, Locations.LeftWindow) || manager.IsEnemyAtLocation(EnemyTypes.Chelsea, Locations.RightWindow)) { return false; }
        if (manager.IsEnemyAtLocation(EnemyTypes.H42, Locations.LeftWindow) || manager.IsEnemyAtLocation(EnemyTypes.H42, Locations.RightWindow)) { return false; }
        return true;
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
                return AnyWindowAvailable();
            case States.OnWindowAnnoyed: return manager.IsLocationAvailable(Locations.Door);
            default: return true;
        }
    }
    public override bool IsAvaliableForCombo(EnemyTypes _other)
    {
        if (aiLevel == 0 || isLocked) { return false; }
        switch (_other)
        {
            case EnemyTypes.Carla:
                return currentState == States.None && AnyWindowAvailable() && Random.value < 0.5f;
            case EnemyTypes.Tournament_MixmaxStressToy:
                return currentState == States.None && AnyWindowAvailable() && Random.value < 0.25f;
        }
        return false;
    }
    public override void ComboTriggered(EnemyTypes _other)
    {
        switch (_other)
        {
            case EnemyTypes.Carla:
            case EnemyTypes.Tournament_MixmaxStressToy:
                MoveToWindow();
                break;
        }
    }
}