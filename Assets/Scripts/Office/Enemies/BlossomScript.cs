using UnityEngine;

public class BlossomScript : EnemyScript
{
    enum States { Waiting, Idle, MaskPulled, Spooking, MaskDropped, Attack };
    States currentState;
    int stateCounter;
    float stateTimer;
    float moveCooldown { get { return Random.value < 0.25f ? Random.Range(8f, 10f) : Random.Range(12f, 16f); } }
    int missCounter;
    float maskTimer;
    int mouseBuffer;
    bool isPlayerHoldingMask { get { return mouseBuffer > 0; } }
    Sprite[] sprites;
    Sprite benjamin;
    bool benjaminFlag;
    SpriteRenderer r;
    Sprite[] maskSprites;
    SpriteRenderer mask;
    BoxCollider mouseTrigger;
    Vector2 maskPos;
    Vector2 defaultMaskPos { get { return Vector3.up * 36f / 44f; } }
    Vector2 maskFallSpeed;
    float maskFallAngleSpeed;
    AudioClip maskTwitchClip;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Halloween_Blossom; }
    // Start is called before the first frame update
    void Start()
    {
        sprites = Resources.LoadAll<Sprite>(Random.value < 0.25f ? "Sprites/Characters/HalloweenHorseGregg_V2" : "Sprites/Characters/HalloweenHorseGregg");
        benjamin = Resources.Load<Sprite>("Sprites/Characters/Benjamin");
        r = GetComponent<SpriteRenderer>();
        r.enabled = false;
        maskSprites = Resources.LoadAll<Sprite>("Sprites/Taurus_Mask");
        mask = transform.GetChild(0).GetComponent<SpriteRenderer>();
        mask.enabled = false;
        mouseTrigger = transform.GetChild(0).GetComponent<BoxCollider>();
        mouseTrigger.enabled = false;
        maskTwitchClip = Resources.Load<AudioClip>("SFX/mask_twitch");
        stateTimer = moveCooldown;
    }
    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case States.Waiting:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    if (Random.Range(0, 10) < aiLevel + missCounter && (isMonitorUp || Random.value < Mathf.Lerp(0.1f, 0.5f, aiLevel * 0.1f) || missCounter >= 3) && !manager.isMidnightKnocking && manager.IsLocationAvailable(Locations.Monitor))
                    {
                        currentState = States.Idle;
                        stateTimer = 0;
                        currentLocation = Locations.Monitor;
                        manager.TriggerRoomOverlay();
                        maskPos = defaultMaskPos;
                        mask.transform.localPosition = maskPos;
                        mask.transform.localRotation = Quaternion.identity;
                        benjaminFlag = !benjaminFlag && Random.value < 0.05f;
                        r.sprite = benjaminFlag ? benjamin : sprites[0];
                        r.enabled = true;
                        mask.sprite = maskSprites[0];
                        mask.enabled = true;
                        mouseTrigger.enabled = true;
                        maskTimer = 0;
                        missCounter = 0;
                    }
                    else
                    {
                        stateTimer = moveCooldown;
                        if (aiLevel > 0) { missCounter++; }
                    }
                }
                break;
            case States.Idle:
                if (mouseBuffer > 0)
                {
                    mouseBuffer--;
                    if (mouseBuffer == 0) { maskTimer = 0; }
                }
                if (!isPlayerHoldingMask) { maskPos = Vector2.MoveTowards(maskPos, defaultMaskPos, Vector2.Distance(maskPos, defaultMaskPos) * 7.5f * Time.deltaTime); }
                mask.transform.localPosition = maskPos;
                if (isMonitorUp)
                {
                    maskTimer = 0;
                    stateTimer += Time.deltaTime;
                    if (stateTimer >= 10) { Attack(); }
                }
                else
                {
                    stateTimer = Mathf.MoveTowards(stateTimer, 0, 1.5f * Time.deltaTime);
                    maskTimer += Time.deltaTime;
                    if (maskTimer >= (isPlayerHoldingMask ? 0.625f : Mathf.Lerp(2.5f, 1, aiLevel * 0.1f)))
                    {
                        manager.TriggerRoomOverlay();
                        if (isPlayerHoldingMask)
                        {
                            currentState = States.MaskPulled;
                            stateTimer = 1;
                            if (!benjaminFlag) { r.sprite = sprites[1]; }
                            mask.enabled = false;
                            mouseTrigger.enabled = false;
                        }
                        else
                        {
                            currentState = States.Spooking;
                            stateCounter = 0;
                            stateTimer = 0;
                            mask.sprite = maskSprites[1];
                        }
                    }
                }
                break;
            case States.MaskPulled:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { Dispawn(); }
                break;
            case States.Spooking:
                stateTimer += Time.deltaTime;
                maskPos = Vector2.MoveTowards(maskPos, defaultMaskPos, 0.5f * Time.deltaTime);
                mask.transform.localPosition = maskPos;
                mask.transform.localRotation = Quaternion.RotateTowards(mask.transform.localRotation, Quaternion.identity, 90 * Time.deltaTime);
                mask.sprite = maskSprites[maskPos == defaultMaskPos && mask.transform.localRotation == Quaternion.identity ? 1 : 0];
                if (stateTimer >= 5) { Attack(); }
                break;
            case States.MaskDropped:
                if (maskPos.y > 0)
                {
                    maskFallSpeed.y -= 5 * Time.deltaTime;
                    maskPos += maskFallSpeed * Time.deltaTime;
                    mask.transform.localPosition = maskPos;
                    mask.transform.localRotation = Quaternion.Euler(Vector3.forward * maskFallAngleSpeed * Time.deltaTime);
                }
                else
                {
                    stateTimer += Time.deltaTime;
                    if (stateTimer >= 0.5f) { Dispawn(); }
                }
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { manager.ExamFailed("Look, the rules of Trick and Treat are simple: if you get spooked, you're gonna have to cough up the goods.\n\nIf you're out of candy, which you are, you COULD use your mouse to take the creepy mask off and argue that you weren't fully spooked, so you don't have to pay up! Yeah, that should do it :3"); }
                break;
        }
    }
    void Attack()
    {
        manager.LockEnemies(this);
        manager.LockPlayer();
        manager.LockCamera();
        manager.TriggerRoomOverlay();
        manager.FadeOutMusic();
        if (!benjaminFlag) { r.sprite = sprites[3]; }
        mask.enabled = false;
        currentState = States.Attack;
        stateCounter = 0;
        stateTimer = 1.5f;
    }
    void Dispawn()
    {
        currentState = States.Waiting;
        stateCounter = 0;
        stateTimer = moveCooldown;
        currentLocation = Locations.None;
        manager.TriggerRoomOverlay();
        r.enabled = false;
        mask.enabled = false;
        mouseTrigger.enabled = false;
    }
    protected override void OnEnemyLocked() { if (currentState != States.Waiting) { Dispawn(); } }
    public void OnMouseClicked()
    {
        if (currentState != States.Spooking) { return; }
        stateCounter++;
        if (stateCounter == 5)
        {
            currentState = States.MaskDropped;
            stateCounter = 0;
            stateTimer = 0;
            if (!benjaminFlag) { r.sprite = sprites[2]; }
            maskFallSpeed = new Vector2(Random.Range(-1f, 1f) * 0.1f, Random.Range(0.75f, 2f));
            maskFallAngleSpeed = Random.Range(-30f, 30f);
        }
        else
        {
            maskPos = defaultMaskPos + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * 0.02f;
            mask.transform.localRotation = Quaternion.Euler(Vector3.forward * Random.Range(-15f, 15f));
        }
        mask.sprite = maskSprites[0];
        manager.PlaySound(maskTwitchClip, "Mask Twitch", true, true);
    }
    public void OnMouseHeld()
    {
        if (currentState != States.Idle) { return; }
        if (!isPlayerHoldingMask) { maskTimer = 0; }
        mouseBuffer = 5;
        maskPos = transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 1.25f));
    }
}