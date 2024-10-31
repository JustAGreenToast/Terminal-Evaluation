using UnityEngine;

public class CauldoomUrsulaScript : EnemyScript
{
    enum States { Waiting, PoppingIn, Idle, PoppingOut, Attack };
    States currentState;
    int stateCounter;
    float stateTimer;
    float moveCooldown { get { return Random.value < 0.25f ? Random.Range(5f, 10f) : Random.Range(12f, 16f); } }
    int missCounter;
    int movesLeft;
    float hoverTimer;
    int hoverBuffer;
    bool isHoveredOn { get { return hoverBuffer > 0; } }
    Sprite[] sprites;
    SpriteRenderer r;
    BoxCollider mouseTrigger;
    Vector3 localPos;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Halloween_UrsulaSlimeDragon; }
    // Start is called before the first frame update
    void Start()
    {
        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Ursula Slime Dragon");
        r = GetComponent<SpriteRenderer>();
        r.enabled = false;
        mouseTrigger = GetComponent<BoxCollider>();
        mouseTrigger.enabled = false;
        stateTimer = moveCooldown;
    }
    // Update is called once per frame
    void Update()
    {
        if (hoverBuffer > 0) { hoverBuffer--; }
        switch (currentState)
        {
            case States.Waiting:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    if (Random.Range(0, 10) < aiLevel + missCounter && !manager.isMidnightKnocking && manager.IsLocationAvailable(Locations.Monitor))
                    {
                        PopIn();
                        movesLeft = Random.Range(3, 8);
                        currentLocation = Locations.Monitor;
                        r.enabled = true;
                        missCounter = 0;
                    }
                    else
                    {
                        stateTimer = moveCooldown;
                        if (aiLevel > 0) { missCounter++; }
                    }
                }
                break;
            case States.PoppingIn:
                stateTimer += Time.deltaTime;
                if (stateTimer >= 0.05f)
                {
                    stateCounter++;
                    r.sprite = sprites[stateCounter];
                    if (stateCounter == 2)
                    {
                        stateCounter = 0;
                        currentState = States.Idle;
                        mouseTrigger.enabled = true;
                    }
                    stateTimer -= 0.05f;
                }
                break;
            case States.Idle:
                r.sprite = sprites[isHoveredOn ? hoverTimer > 0.2f ? 4 : 3 : 2];
                float xOffset = isHoveredOn ? Random.Range(-1f, 1f) * 0.01f : 0;
                transform.localPosition = localPos + Vector3.right * xOffset;
                mouseTrigger.center = new Vector2(xOffset, mouseTrigger.center.y);
                if (isHoveredOn)
                {
                    hoverTimer += Time.deltaTime;
                    stateTimer = Mathf.MoveTowards(stateTimer, 0, 0.5f * Time.deltaTime);
                    if (hoverTimer >= 0.75f)
                    {
                        currentState = States.PoppingOut;
                        stateCounter = 0;
                        stateTimer = 0;
                        transform.localPosition = localPos;
                        r.sprite = sprites[4];
                        mouseTrigger.enabled = false;
                    }
                }
                else
                {
                    stateTimer += Time.deltaTime * balanceFactor;
                    hoverTimer = 0;
                    if (stateTimer >= 7.5f) { Attack(); }
                }
                break;
            case States.PoppingOut:
                stateTimer += Time.deltaTime;
                if (stateTimer >= 0.05f)
                {
                    stateCounter++;
                    r.sprite = sprites[4 + stateCounter];
                    if (stateCounter == 2)
                    {
                        stateCounter = 0;
                        if (movesLeft > 0)
                        {
                            movesLeft--;
                            PopIn();
                        }
                        else { Dispawn(); }
                    }
                    else { stateTimer -= 0.05f; }
                }
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { manager.ExamFailed("The dragon in the cauldron doesn't like being seen, keep it inside the cauldron with the Game Monitor or lure it away with your mouse if it gets too close."); }
                break;
        }
    }
    void PopIn()
    {
        localPos = new Vector3(0.8f * Random.Range(-1, 2), 1.5f, 1.25f);
        transform.localPosition = localPos;
        currentState = States.PoppingIn;
        stateCounter = 0;
        stateTimer = 0;
        hoverTimer = 0;
        r.sprite = sprites[0];
    }
    void Attack()
    {
        manager.LockEnemies(this);
        manager.LockPlayer();
        manager.LockCamera();
        manager.TriggerRoomOverlay();
        manager.FadeOutMusic();
        transform.localPosition = new Vector3(0, -1.5f, 0.75f);
        transform.localRotation = Quaternion.identity;
        r.sprite = sprites[2];
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
        r.enabled = false;
    }
    protected override void OnEnemyLocked()
    {
        if (currentState != States.Waiting)
        {
            manager.TriggerRoomOverlay();
            Dispawn();
        }
    }
    public void OnMouseHover() { hoverBuffer = 5; }
}