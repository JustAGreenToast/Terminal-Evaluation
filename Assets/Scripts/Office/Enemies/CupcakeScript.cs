using UnityEngine;

public class CupcakeScript : EnemyScript
{
    enum States { None, BeforeWindow, OnWindow, OnWindowAnnoyed, BeforeAttack, Attack };
    States currentState;
    float stateTimer;
    const float attackTime = 1.5f;
    float moveCooldown { get { return Random.value > 0.9f ? Random.Range(2.5f, 5f) : Random.Range(7.5f, 12.5f); } }
    float patienceTimer;
    Sprite[] sprites;
    SpriteRenderer r;
    AudioClip walkClip;
    AudioClip lStepClip;
    AudioClip rStepClip;
    AudioClip doorClip;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Cupcake; }
    protected override void OnStart()
    {
        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Cupcake");
        walkClip = Resources.Load<AudioClip>("SFX/footsteps_0");
        lStepClip = Resources.Load<AudioClip>("SFX/footstep_left");
        rStepClip = Resources.Load<AudioClip>("SFX/footstep_right");
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
                    if (AbleToMove())
                    {
                        currentState = States.BeforeWindow;
                        manager.PlaySound(walkClip, SettingsManager.settings.explicitSubtitles ? "Faint Cupcake Step" : "Faint, Strong Step", true, true);
                    }
                    stateTimer = moveCooldown;
                }
                break;
            case States.BeforeWindow:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    if (AbleToMove()) { MoveToWindow(); }
                    else { stateTimer = moveCooldown; }
                }
                break;
            case States.OnWindow:
                if (isMonitorUp)
                {
                    patienceTimer = 0;
                    stateTimer -= Time.deltaTime;
                    if (stateTimer < 0)
                    {
                        currentState = States.None;
                        stateTimer = moveCooldown;
                        if (currentLocation == Locations.RightWindow) { manager.PlaySound(rStepClip,  SettingsManager.settings.explicitSubtitles ? "Cupcake Step" : "Strong Step", false, true); }
                        else { manager.PlaySound(lStepClip,  SettingsManager.settings.explicitSubtitles ? "Cupcake Step" : "Strong Step", true, false); }
                        currentLocation = Locations.None;
                        manager.TriggerHallOverlay();
                        r.enabled = false;
                    }
                }
                else
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
                break;
            case States.OnWindowAnnoyed:
                if (isMonitorUp)
                {
                    currentState = States.OnWindow;
                    stateTimer = attackTime;
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
                        r.sprite = sprites[Random.value > 0.5f ? 4 : 2];
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
                    r.sprite = sprites[3];
                    manager.RotateDoor(180, 1200);
                    manager.PlaySound(doorClip, "Door Slam", true, true);
                    manager.LockPlayer();
                    manager.LockEnemies(this);
                    manager.LockCamera();
                    manager.FadeOutMusic();
                }
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { manager.ExamFailed("Cupcake will show up on either the left or the right window, keep any monitor up until she leaves."); }
                break;
        }
    }
    bool AnyWindowAvailable()
    {
        if (!manager.IsLocationAvailable(Locations.LeftWindow) && !manager.IsLocationAvailable(Locations.RightWindow)) { return false; }
        if (manager.IsEnemyAtLocation(EnemyTypes.Chelsea, Locations.LeftWindow) || manager.IsEnemyAtLocation(EnemyTypes.Chelsea, Locations.RightWindow)) { return false; }
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
    void MoveToWindow()
    {
        currentState = States.OnWindow;
        stateTimer = attackTime;
        manager.TriggerHallOverlay();
        r.sprite = sprites[0];
        r.enabled = true;
        PickWindow();
        if (currentLocation == Locations.RightWindow) { manager.PlaySound(rStepClip,  SettingsManager.settings.explicitSubtitles ? "Cupcake Step" : "Strong Step", false, true); }
        else { manager.PlaySound(lStepClip,  SettingsManager.settings.explicitSubtitles ? "Cupcake Step" : "Strong Step", true, false); }
    }
    public override bool IsAvaliableForCombo(EnemyTypes _other)
    {
        if (aiLevel == 0 || isLocked) { return false; }
        switch (_other)
        {
            case EnemyTypes.Cassidy:
                return (currentState == States.None || currentState == States.BeforeWindow) && AnyWindowAvailable() && Random.value < 0.25f;
            case EnemyTypes.Tournament_MixmaxStressToy:
                return (currentState == States.None || currentState == States.BeforeWindow) && AnyWindowAvailable() && Random.value < 0.5f;
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
    bool AbleToMove()
    {
        switch (currentState)
        {
            case States.None: return Random.Range(0, 10) < aiLevel;
            case States.BeforeWindow:
                if (Random.Range(0, 10) >= aiLevel) { return false; }
                return AnyWindowAvailable();
            case States.OnWindowAnnoyed: return manager.IsLocationAvailable(Locations.Door);
            default: return true;
        }
    }
}