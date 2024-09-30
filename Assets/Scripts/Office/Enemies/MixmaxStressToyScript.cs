using UnityEngine;

public class MixmaxStressToyScript : EnemyScript
{
    enum States { Waiting, Idle, Hit };
    States currentState;
    float stateTimer;
    int comboMeter;
    float angleX;
    float angleZ;
    float angleSpeedX;
    float angleSpeedZ;
    float angleSignZ;
    float moveCooldown { get { return Random.value > 0.75f ? Random.Range(16f, 24f) : Random.Range(32f, 42f); } }
    Sprite[] sprites;
    SpriteRenderer _r;
    SpriteRenderer r
    {
        get
        {
            if (!_r) { _r = GetComponent<SpriteRenderer>(); }
            return _r;
        }
    }
    BoxCollider clickTrigger;
    SpriteWobbleScript wobbleAnim;
    AudioClip squeakSound;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Tournament_MixmaxStressToy; }
    protected override void OnStart()
    {
        squeakSound = Resources.Load<AudioClip>("SFX/squeak");
        r.enabled = false;
        clickTrigger = GetComponent<BoxCollider>();
        clickTrigger.enabled = false;
        wobbleAnim = GetComponent<SpriteWobbleScript>();
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
        if (currentState != States.Waiting)
        {
            manager.CloseMonitor();
            manager.LockCamera(0);
        }
        angleSpeedX -= 600 * Time.deltaTime;
        angleSpeedZ -= 600 * Time.deltaTime;
        angleX = angleX + angleSpeedX * Time.deltaTime;
        if (angleSpeedX < 0)
        {
            angleSpeedX = 0;
            angleX = 0;
        }
        angleZ = angleZ + angleSpeedZ * angleSignZ * Time.deltaTime;
        if (angleSpeedZ < 0)
        {
            angleSpeedZ = 0;
            angleZ = 0;
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(angleX, 0, angleZ), 420 * Time.deltaTime);
        switch (currentState)
        {
            case States.Waiting:
                if (isMonitorUp)
                {
                    stateTimer -= Time.deltaTime * balanceFactor;
                    if (stateTimer <= 0)
                    {
                        if (Random.Range(0, 10) < aiLevel && manager.IsLocationAvailable(Locations.Monitor) && isMonitorUp && !manager.isPlayerTurnedAround && !manager.isMidnightKnocking)
                        {
                            angleSignZ = 0;
                            comboMeter = 0;
                            currentState = States.Idle;
                            currentLocation = Locations.Monitor;
                            manager.TriggerRoomOverlay();
                            r.enabled = true;
                            clickTrigger.enabled = true;
                            UpdateSprite();
                            if (IsEnemyComboAvailable(EnemyTypes.Chelsea)) { TriggerEnemyCombo(EnemyTypes.Chelsea); }
                            if (IsEnemyComboAvailable(EnemyTypes.Cupcake)) { TriggerEnemyCombo(EnemyTypes.Cupcake); }
                            if (IsEnemyComboAvailable(EnemyTypes.Midnight)) { TriggerEnemyCombo(EnemyTypes.Midnight); }
                            if (IsEnemyComboAvailable(EnemyTypes.Cassidy)) { TriggerEnemyCombo(EnemyTypes.Cassidy); }
                            if (IsEnemyComboAvailable(EnemyTypes.Aqua)) { TriggerEnemyCombo(EnemyTypes.Aqua); }
                            if (IsEnemyComboAvailable(EnemyTypes.H41)) { TriggerEnemyCombo(EnemyTypes.H41); }
                            if (IsEnemyComboAvailable(EnemyTypes.Cindy)) { TriggerEnemyCombo(EnemyTypes.Cindy); }
                            if (IsEnemyComboAvailable(EnemyTypes.H42)) { TriggerEnemyCombo(EnemyTypes.H42); }
                        }
                        else { stateTimer = moveCooldown; }
                    }
                }
                break;
            case States.Hit:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    comboMeter = 0;
                    currentState = States.Idle;
                    UpdateSprite();
                }
                break;
        }
    }
    protected override void OnEnemyLocked() { if (currentState != States.Waiting) { Dispawn(); } }
    void Dispawn()
    {
        currentState = States.Waiting;
        stateTimer = moveCooldown;
        currentLocation = Locations.None;
        manager.TriggerRoomOverlay();
        manager.UnlockCamera();
        r.enabled = false;
        clickTrigger.enabled = false;
    }
    public void OnClick()
    {
        currentState = States.Hit;
        stateTimer = 0.75f;
        comboMeter++;
        angleX = 0;
        angleZ = 0;
        angleSpeedX = Random.Range(90f, 120f);
        angleSpeedZ = Random.Range(150f, 180f);
        angleSignZ = angleSignZ == 0 ? Random.value > 0.5f ? 1 : -1 : -angleSignZ;
        if (comboMeter < 5)
        {
            wobbleAnim.PlayAnim();
            UpdateSprite();
        }
        else { Dispawn(); }
        manager.PlaySound(squeakSound, $"Squeak Combo [x{comboMeter}]", true, true, 1 + 0.05f * (comboMeter - 1));
    }
    public override void OnTexturePackChanged(string _folderName)
    {
        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Mixmax Stress Toy");
        UpdateSprite();
    }
    void UpdateSprite() { r.sprite = sprites[currentState == States.Hit ? 1 : 0]; }
}