using UnityEngine;

public class CarlaScript : EnemyScript
{
    public int stateCounter { get; private set; }
    float stateTimer;
    float moveCooldown { get { return Random.value > 0.9f ? Random.Range(10f, 15f) : Random.Range(24f, 32f); } }
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
                case Appearances.ProtoCarla: return $"Proto-Carla will slowly crawl out of her box, fully coming out after moving 4 times. Use your Game Monitor to calm her down and make her crawl back into her box.";
                case Appearances.JesterPearlie: return $"Tee-hee, the joke's on you now! :3";
                case Appearances.Jasmine: return "Catfished!~";
                case Appearances.Pumpqueen: return "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                default: throw new System.Exception($"oi dumbass you forgor to give Appearances.{appearance} a fail msg bozo");
            }
        }
    }
    public bool gameMonitorWarningEnabled
    {
        get
        {
            switch (appearance)
            {
                /*
                case Appearances.Pumpqueen:
                    break;
                case Appearances.Cauldoom_Mixmax:
                    break;
                case Appearances.Cauldoom_Ursula:
                    break;
                */
                default: return stateCounter > 2;
            }
        }
    }
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Carla; }
    protected override void OnStart()
    {
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
        stateTimer -= Time.deltaTime * (stateCounter == 4 ? 1 : balanceFactor);
        UpdateAnim();
        if (stateTimer <= 0)
        {
            if (stateCounter == 4) { manager.ExamFailed(failMessage); }
            else if (Random.Range(0, 10) < aiLevel)
            {
                stateCounter++;
                if (stateCounter == 4)
                {
                    manager.CloseMonitor();
                    manager.LockPlayer();
                    manager.LockEnemies(this);
                    manager.LockCamera(-5);
                    manager.FadeOutMusic();
                    stateTimer = 1.5f;
                }
                else
                {
                    manager.AddGameConsoleRounds();
                    if (stateCounter >= 2 && IsEnemyComboAvailable(EnemyTypes.Cindy)) { TriggerEnemyCombo(EnemyTypes.Cindy); }
                    stateTimer = moveCooldown;
                }
                manager.TriggerRoomOverlay();
                r.sprite = sprites[stateCounter];
            }
            else { stateTimer = moveCooldown; }
        }
    }
    public void RoundSetCleared()
    {
        if (stateCounter == 0) { return; }
        stateCounter--;
        manager.TriggerRoomOverlay();
        UpdateSprite();
    }
    public override bool IsAvaliableForCombo(EnemyTypes _other) { return _other == EnemyTypes.Cindy && stateCounter > 0 && !isLocked; }
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
                break;
            case Appearances.Cauldoom_Mixmax:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Cauldoom_Mixmax");
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
                float frameDelay = stateCounter == 0 ? 0.4f : 0.2f;
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
        if (stateCounter == 4)
        {
            if (sprites.Length > 5) { r.sprite = sprites[4 + (animCounter == 3 ? 1 : animCounter)]; }
            else { r.sprite = sprites[4]; }
        }
        else { r.sprite = sprites[stateCounter]; }
    }
}