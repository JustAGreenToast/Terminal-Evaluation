using UnityEngine;

public class H42Script : EnemyScript
{
    enum States { None, OnWindow, OnWindowAnnoyed, BeforeAttack, Attack };
    States currentState;
    float stateTimer;
    int stateCounter;
    const float attackTime = 1.5f;
    float moveCooldown { get { return Random.value > 0.9f ? Random.Range(7.5f, 12.5f) : Random.Range(15f, 25f); } }
    Sprite[] sprites;
    SpriteRenderer r;
    AudioClip lStepClip;
    AudioClip rStepClip;
    AudioClip lightUpSound;
    AudioClip doorClip;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.H42; }
    protected override void OnStart()
    {
        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/H42");
        lStepClip = Resources.Load<AudioClip>("SFX/hal_move_left");
        rStepClip = Resources.Load<AudioClip>("SFX/hal_move_right");
        lightUpSound = Resources.Load<AudioClip>("SFX/hal_lightup");
        doorClip = Resources.Load<AudioClip>("SFX/door_slam_hard");
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
                if (manager.IsWorkMonitorUp())
                {
                    stateTimer -= Time.deltaTime;
                    if (stateTimer < 0)
                    {
                        currentState = States.None;
                        stateTimer = moveCooldown;
                        if (currentLocation == Locations.RightWindow) { manager.PlaySound(rStepClip, SettingsManager.settings.explicitSubtitles ? "H42 Move" : "Digital Whirr", false, true); }
                        else { manager.PlaySound(lStepClip, SettingsManager.settings.explicitSubtitles ? "H42 Move" : "Digital Whirr", true, false); }
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
                if (manager.IsWorkMonitorUp())
                {
                    currentState = States.OnWindow;
                    stateTimer = attackTime;
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
                        transform.position = new Vector3(0, 1.275f, 5);
                    }
                }
                break;
            case States.BeforeAttack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    currentState = States.Attack;
                    stateTimer = 1.5f;
                    manager.TriggerRoomOverlay();
                    r.sprite = sprites[0];
                    manager.RotateDoor(150, 720);
                    manager.PlaySound(doorClip, "Door Slam", true, true);
                    manager.LockPlayer();
                    manager.LockEnemies(this);
                    manager.LockCamera();
                    manager.FadeOutMusic();
                }
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    if (stateCounter == 4) { manager.ExamFailed("H42 will show up on either the left or the right window, keep your Work Monitor up until he leaves."); }
                    else
                    {
                        r.sprite = sprites[stateCounter];
                        stateTimer = stateCounter < 3 ? 0.1f : 2.5f;
                        if (stateCounter == 0) { manager.PlaySound(lightUpSound, "Light-up Jingle", true, true); }
                        stateCounter++;
                    }
                }
                break;
        }
    }
    void MoveToWindow()
    {
        currentState = States.OnWindow;
        stateTimer = attackTime;
        if (currentLocation == Locations.RightWindow) { manager.PlaySound(rStepClip, SettingsManager.settings.explicitSubtitles ? "H42 Move" : "Digital Whirr", false, true); }
        else { manager.PlaySound(lStepClip, SettingsManager.settings.explicitSubtitles ? "H42 Move" : "Digital Whirr", true, false); }
        manager.TriggerHallOverlay();
        r.sprite = sprites[3];
        r.enabled = true;
        PickWindow();
    }
    bool IsAnyWindowAvailable()
    {
        if (!manager.IsLocationAvailable(Locations.LeftWindow) && !manager.IsLocationAvailable(Locations.RightWindow)) { return false; }
        if (manager.IsEnemyAtLocation(EnemyTypes.Chelsea, Locations.LeftWindow) || manager.IsEnemyAtLocation(EnemyTypes.Chelsea, Locations.RightWindow)) { return false; }
        if (manager.IsEnemyAtLocation(EnemyTypes.Cindy, Locations.LeftWindow) || manager.IsEnemyAtLocation(EnemyTypes.Cindy, Locations.RightWindow)) { return false; }
        return true;
    }
    void PickWindow()
    {
        bool left = manager.IsLocationAvailable(Locations.LeftWindow);
        bool right = manager.IsLocationAvailable(Locations.RightWindow);
        if (left != right) { currentLocation = left ? Locations.LeftWindow : Locations.RightWindow; }
        else { currentLocation = Random.value > 0.5f ? Locations.LeftWindow : Locations.RightWindow; }
        transform.position = new Vector3(5 * (currentLocation == Locations.LeftWindow ? -1 : 1), 1.375f, 5);
    }
    bool AbleToMove()
    {
        switch (currentState)
        {
            case States.None:
                if (Random.Range(0, 10) >= aiLevel) { return false; }
                return IsAnyWindowAvailable();
            case States.OnWindowAnnoyed: return manager.IsLocationAvailable(Locations.Door);
            default: return true;
        }
    }
    public override bool IsAvaliableForCombo(EnemyTypes _other)
    {
        if (aiLevel == 0 || isLocked) { return false; }
        switch (_other)
        {
            case EnemyTypes.Tournament_MixmaxStressToy:
                return IsAnyWindowAvailable() && Random.value < 0.25f;
        }
        return false;
    }
    public override void ComboTriggered(EnemyTypes _other)
    {
        switch (_other)
        {
            case EnemyTypes.Tournament_MixmaxStressToy:
                MoveToWindow();
                break;
        }
    }
}