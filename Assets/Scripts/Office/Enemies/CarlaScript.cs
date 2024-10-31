using UnityEngine;

public class CarlaScript : EnemyScript
{
    public int stateCounter { get; private set; }
    float stateTimer;
    float moveCooldown
    {
        get
        {
            switch (appearance)
            {
                case Appearances.Pumpqueen: return Random.value < 0.25f ? Random.Range(12f, 16f) : Random.Range(20f, 24f);
                case Appearances.Cauldoom_Ursula: return Random.value < 0.25f ? Random.Range(8f, 12f) : Random.Range(12f, 16f);
            }
            return Random.value < 0.1f ? Random.Range(16f, 20f) : Random.Range(24f, 32f);
        }
    }
    enum Appearances { ProtoCarla, JesterPearlie, Jasmine, Pumpqueen, Cauldoom_Mixmax, Cauldoom_Ursula }
    Appearances appearance;
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
    int animCounter;
    float animTimer;
    string failMessage
    {
        get
        {
            switch (appearance)
            {
                case Appearances.ProtoCarla: return $"Proto-Carla will slowly crawl out of her box, fully getting out after moving {attackStateCounter} times. Use your Game Monitor to calm her down and make her hide back into her box.";
                case Appearances.JesterPearlie: return $"Tee-hee, the joke's on you now! :3";
                case Appearances.Jasmine: return "Catfished!~";
                case Appearances.Pumpqueen: return "If y'all are gonna take MY seasonal decorations and leave me with a generic witch hat, the least you can do is pay some attention to me!\n\nI don't think I'm being unreasonable here...";
                case Appearances.Cauldoom_Mixmax: return "Found you!~";
                default: throw new System.Exception($"oi dumbass you forgor to give Appearances.{appearance} a fail msg bozo");
            }
        }
    }
    int attackStateCounter
    {
        get
        {
            switch (appearance)
            {
                case Appearances.Pumpqueen:
                case Appearances.Cauldoom_Ursula:
                    return 3;
            }
            return 4;
        }
    }
    public bool gameMonitorWarningEnabled { get { return stateCounter >= attackStateCounter - 1 && !isUrsulaOut; } }
    bool isUrsulaOut { get { return appearance == Appearances.Cauldoom_Ursula && stateCounter == attackStateCounter; } }
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Carla; }
    protected override void OnStart()
    {
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
        if (isUrsulaOut) { return; }
        stateTimer -= Time.deltaTime * (stateCounter < attackStateCounter ? balanceFactor : 1);
        UpdateAnim();
        if (stateTimer <= 0)
        {
            if (stateCounter == attackStateCounter && appearance != Appearances.Cauldoom_Ursula) { manager.ExamFailed(failMessage); }
            else if (Random.Range(0, 10) < aiLevel)
            {
                stateCounter++;
                if (stateCounter == attackStateCounter)
                {
                    if (appearance == Appearances.Cauldoom_Ursula)
                    {
                        manager.ClearGameConsoleRounds();
                        manager.EnableUrsula(aiLevel);
                    }
                    else
                    {
                        manager.CloseMonitor();
                        manager.LockPlayer();
                        manager.LockEnemies(this);
                        manager.LockCamera(-5);
                        manager.FadeOutMusic();
                        stateTimer = 1.5f;
                    }
                }
                else
                {
                    manager.AddGameConsoleRounds();
                    if (stateCounter >= attackStateCounter / 2 && IsEnemyComboAvailable(EnemyTypes.Cindy)) { TriggerEnemyCombo(EnemyTypes.Cindy); }
                    stateTimer = moveCooldown;
                }
                manager.TriggerRoomOverlay();
                UpdateSprite();
            }
            else { stateTimer = moveCooldown; }
        }
    }
    public void RoundSetCleared()
    {
        if (stateCounter == 0) { return; }
        if (appearance == Appearances.Cauldoom_Ursula && stateCounter == attackStateCounter) { return; }
        stateCounter--;
        manager.TriggerRoomOverlay();
        UpdateSprite();
    }
    public override bool IsAvaliableForCombo(EnemyTypes _other) { return _other == EnemyTypes.Cindy && stateCounter > 0 && !isLocked && !isUrsulaOut; }
    public override void OnTexturePackChanged(string _folderName)
    {
        switch (_folderName)
        {
            case "10": // Sandy Shores
            case "11": // Pool Party
                appearance = Appearances.Jasmine;
                break;
            case "13": // Mischief Mansion
                appearance = new Appearances[3] { Appearances.Pumpqueen, Appearances.Cauldoom_Mixmax, Appearances.Cauldoom_Ursula }[Random.Range(0, 3)];
                break;
            default:
                appearance = SettingsManager.settings.jesterPearlie || (System.DateTime.Now.Day == 1 && System.DateTime.Now.Month == 4 && Random.value < 0.25f) ? Appearances.JesterPearlie : Appearances.ProtoCarla;
                break;
        }
        switch (appearance)
        {
            case Appearances.ProtoCarla:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Carla");
                break;
            case Appearances.JesterPearlie:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/JesterPearlie");
                break;
            case Appearances.Jasmine:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Jasmine");
                break;
            case Appearances.Pumpqueen:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Pumpqueen");
                break;
            case Appearances.Cauldoom_Mixmax:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Cauldoom_Mixmax");
                break;
            case Appearances.Cauldoom_Ursula:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Cauldoom_Ursula");
                break;
        }
        UpdateSprite();
    }
    void UpdateAnim()
    {
        switch (appearance)
        {
            case Appearances.Jasmine:
                animTimer += Time.deltaTime;
                if (animTimer >= 0.125f)
                {
                    animCounter++;
                    animCounter %= 4;
                    UpdateSprite();
                    animTimer -= 0.125f;
                }
                break;
            case Appearances.Cauldoom_Mixmax:
            case Appearances.Cauldoom_Ursula:
                float frameDelay = stateCounter == 0 ? 0.5f : 0.2f;
                animTimer += Time.deltaTime;
                if (animTimer >= frameDelay)
                {
                    animCounter++;
                    animCounter %= stateCounter == 0 ? 2 : 3;
                    UpdateSprite();
                    animTimer -= frameDelay;
                }
                break;
        }
    }
    void UpdateSprite()
    {
        switch (appearance)
        {
            case Appearances.Jasmine:
                r.sprite = sprites[stateCounter + (stateCounter == 4 ? (animCounter == 3 ? 1 : animCounter) : 0)];
                break;
            case Appearances.Cauldoom_Mixmax:
            case Appearances.Cauldoom_Ursula:
                r.sprite = sprites[new int[5] { animCounter, 2 + animCounter, 5, 6, 7 }[stateCounter]];
                break;
            default:
                r.sprite = sprites[stateCounter];
                break;
        }
    }
}