using UnityEngine;

public class BarcodeScript : EnemyScript
{
    enum States { Waiting, BehindMonitor, Scanned, Retreat, BeforeAttack, Attack };
    States currentState;
    float stateTimer;
    int stateCounter;
    const float patienceTime = 10;
    const float gracePeriod = 0.75f;
    const float animTime = 0.1f;
    float moveCooldown { get { return Random.value > 0.75f ? Random.Range(0.75f, 5f) : Random.Range(7.5f, 15f); } }
    Sprite[] sprites;
    SpriteRenderer r;
    AudioClip barcodeScanSound;
    BoxCollider clickTrigger;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Barcode; }
    protected override void OnStart()
    {
        sprites = Resources.LoadAll<Sprite>($"Sprites/Characters/{(SettingsManager.settings.barcodeAlt ? "Barcode Alt" : "Barcode")}");
        barcodeScanSound = Resources.Load<AudioClip>("SFX/barcode_scan");
        r = GetComponent<SpriteRenderer>();
        r.enabled = false;
        clickTrigger = GetComponent<BoxCollider>();
        clickTrigger.enabled = false;
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
        if (manager.isPlayerTurnedAround && currentState != States.Waiting) { Dispawn(); }
        switch (currentState)
        {
            case States.Waiting:
                if (isMonitorUp)
                {
                    stateTimer -= Time.deltaTime * balanceFactor;
                    if (stateTimer <= 0)
                    {
                        if (Random.Range(0, 10) < aiLevel && AbleToSpawn()) { Spawn(); }
                        else { stateTimer = moveCooldown; }
                    }
                }
                break;
            case States.BehindMonitor:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0) { Attack(); }
                break;
            case States.Scanned:
                stateTimer += Time.deltaTime;
                if (stateTimer >= (stateCounter < 2 ? animTime : 1.25f))
                {
                    stateCounter++;
                    r.sprite = sprites[2 + stateCounter];
                    stateTimer = 0;
                    if (stateCounter == 3) { currentState = States.Retreat; }
                }
                break;
            case States.Retreat:
                stateTimer += 16 * Time.deltaTime;
                transform.localPosition += Vector3.up * stateTimer * Time.deltaTime;
                if (transform.localPosition.y >= 2.5f)
                {
                    currentState = States.Waiting;
                    r.enabled = false;
                    currentLocation = Locations.None;
                }
                break;
            case States.BeforeAttack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    currentState = States.Attack;
                    stateTimer = 1.5f;
                    manager.TriggerRoomOverlay();
                    r.sprite = sprites[1];
                    if (SettingsManager.settings.barcodeSave)
                    {
                        manager.CloseMonitor();
                        manager.LockPlayer();
                        manager.LockEnemies(this);
                        manager.LockCamera();
                        clickTrigger.enabled = false;
                    }
                }
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                    manager.ExamFailed("Barcode will drop from the top of the screen while your monitor is up, use your mouse to scan the barcode on her sweater. If you wait for too long or flip down your monitor before scanning, you lose.");
                break;
        }
    }
    public override bool IsAvaliableForCombo(EnemyTypes _other)
    {
        if (aiLevel == 0 || isLocked || currentState != States.Waiting || !AbleToSpawn()) { return false; }
        switch (_other)
        {
            case EnemyTypes.Chelsea: return Random.value < 0.1f;
            case EnemyTypes.Midnight: return Random.value < 0.1f;
        }
        return false;
    }
    public override void ComboTriggered(EnemyTypes _other)
    {
        switch (_other)
        {
            case EnemyTypes.Chelsea:
            case EnemyTypes.Midnight:
                Spawn();
                break;
        }
    }
    protected override void OnEnemyLocked()
    {
        if (currentState != States.Waiting)
        {
            currentState = States.Waiting;
            stateTimer = moveCooldown;
            manager.TriggerRoomOverlay();
            r.enabled = false;
        }
    }
    protected override void OnMonitorFlipped()
    {
        if (manager.isPlayerTurnedAround) { return; }
        switch (currentState)
        {
            case States.Waiting:
                if (!isMonitorUp) { stateTimer = moveCooldown; }
                break;
            case States.BehindMonitor:
                if (patienceTime - stateTimer > gracePeriod) { Attack(); } else { Dispawn(); }
                break;
        }
    }
    bool AbleToSpawn()
    {
        if (!manager.IsLocationAvailable(Locations.Monitor)) { return false; }
        if (manager.isPlayerTurnedAround) { return false; }
        if (manager.isMidnightKnocking) { return false; }
        return true;
    }
    void Spawn()
    {
        currentState = States.BehindMonitor;
        stateTimer = patienceTime;
        currentLocation = Locations.Monitor;
        manager.TriggerRoomOverlay();
        transform.localPosition = new Vector3(0, 1.425f, 1);
        r.sprite = sprites[0];
        r.enabled = true;
        clickTrigger.enabled = true;
    }
    void Attack()
    {
        currentState = States.BeforeAttack;
        stateTimer = 0.5f;
        if (!SettingsManager.settings.barcodeSave)
        {
            manager.CloseMonitor();
            manager.LockPlayer();
            manager.LockEnemies(this);
            manager.LockCamera();
            clickTrigger.enabled = false;
        }
        manager.FadeOutMusic();
    }
    public void Scanned()
    {
        currentState = States.Scanned;
        stateTimer = 0;
        stateCounter = 0;
        r.sprite = sprites[2];
        clickTrigger.enabled = false;
        manager.PlaySound(barcodeScanSound, "Barcode Scan", true, true);
        manager.ResumeMusic();
    }
    void Dispawn()
    {
        transform.localPosition = new Vector3(0, 2.5f, 1);
        currentState = States.Waiting;
        r.enabled = false;
        currentLocation = Locations.None;
        manager.TriggerRoomOverlay();
    }
}