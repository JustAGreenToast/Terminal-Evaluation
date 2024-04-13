using UnityEngine;

public class CassidyScript : EnemyScript
{
    enum States { None, OnWindow, OnWindowAnnoyed, BeforeAttack, Attack };
    States currentState;
    float stateTimer;
    const float attackTime = 1.5f;
    float moveCooldown { get { return Random.value > 0.9f ? Random.Range(2.5f, 7.5f) : Random.Range(10f, 15f); } }
    float patienceTimer;
    Sprite[] sprites;
    SpriteRenderer r;
    AudioClip lStepClip;
    AudioClip rStepClip;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Cassidy; }
    protected override void OnStart()
    {
        sprites = Resources.LoadAll<Sprite>($"Sprites/Characters/Cassidy" + (Random.value > 0.75f ? " Alt" : ""));
        lStepClip = Resources.Load<AudioClip>("SFX/footstep_left_2");
        rStepClip = Resources.Load<AudioClip>("SFX/footstep_right_2");
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
                        currentState = States.OnWindow;
                        stateTimer = attackTime;
                        patienceTimer = 0;
                        manager.TriggerHallOverlay();
                        r.sprite = sprites[0];
                        r.enabled = true;
                        PickWindow();
                        if (IsAvaliableForCombo(EnemyTypes.Cupcake)) { ComboTriggered(EnemyTypes.Cupcake); }
                        else if (IsAvaliableForCombo(EnemyTypes.Chelsea)) { ComboTriggered(EnemyTypes.Chelsea); }
                        else if (IsAvaliableForCombo(EnemyTypes.Midnight)) { ComboTriggered(EnemyTypes.Midnight); }
                    }
                    else { stateTimer = moveCooldown; }
                }
                break;
            case States.OnWindow:
                if (currentLocation == Locations.LeftWindow ? manager.IsPlayerLookingAtLeftWindow() : manager.IsPlayerLookingAtRightWindow())
                {
                    patienceTimer = 0;
                    stateTimer -= Time.deltaTime;
                    if (stateTimer < 0)
                    {
                        currentState = States.None;
                        stateTimer = moveCooldown;
                        manager.PlaySound(currentLocation == Locations.RightWindow ? rStepClip : lStepClip);
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
                        r.sprite = sprites[currentLocation == Locations.LeftWindow ? 1 : 2];
                    }
                }
                break;
            case States.OnWindowAnnoyed:
                if (currentLocation == Locations.LeftWindow ? manager.IsPlayerLookingAtLeftWindow() : manager.IsPlayerLookingAtRightWindow())
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
                        r.sprite = sprites[0];
                        transform.position = Vector3.forward * 5;
                    }
                }
                break;
            case States.BeforeAttack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    currentState = States.Attack;
                    stateTimer = 1.5f;
                    r.sprite = sprites[3];
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
                    manager.ExamFailed("Cassidy will show up on either the left or the right window, make sure you're facing the window she's at until she leaves. Whether you're looking at her is based on your camera's angle, not on your monitor being up or down.");
                break;
        }
    }
    void PickWindow()
    {
        bool left = manager.IsLocationAvailable(Locations.LeftWindow);
        bool right = manager.IsLocationAvailable(Locations.RightWindow);
        if (left != right) { currentLocation = left ? Locations.LeftWindow : Locations.RightWindow; }
        else { currentLocation = Random.value > 0.5f ? Locations.LeftWindow : Locations.RightWindow; }
        transform.position = new Vector3(5 * (currentLocation == Locations.LeftWindow ? -1 : 1), 0, 5);
        manager.PlaySound(currentLocation == Locations.RightWindow ? rStepClip : lStepClip);
    }
    public override bool IsAvaliableForCombo(EnemyTypes _other) { return aiLevel > 0 && _other == EnemyTypes.Midnight && (manager.IsLocationAvailable(Locations.LeftWindow) || manager.IsLocationAvailable(Locations.RightWindow)) && Random.value > 0.5f; }
    bool AbleToMove()
    {
        switch (currentState)
        {
            case States.None:
                if (Random.Range(0, 10) >= aiLevel) { return false; }
                else if (manager.IsLocationAvailable(Locations.LeftWindow) || manager.IsLocationAvailable(Locations.RightWindow)) { return true; }
                return false;
            case States.OnWindowAnnoyed: return manager.IsLocationAvailable(Locations.Door);
            default: return true;
        }
    }
}