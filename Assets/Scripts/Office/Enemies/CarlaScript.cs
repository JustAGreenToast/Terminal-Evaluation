using UnityEngine;

public class CarlaScript : EnemyScript
{
    public int stateCounter { get; private set; }
    float stateTimer;
    float moveCooldown { get { return Random.value > 0.9f ? Random.Range(10f, 15f) : Random.Range(24f, 32f); } }
    bool jesterPearlie;
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
    int jasmineAnimCounter;
    float jasmineAnimTimer;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Carla; }
    protected override void OnStart()
    {
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
        stateTimer -= Time.deltaTime * (stateCounter == 4 ? 1 : balanceFactor);
        if (sprites.Length > 5)
        {
            jasmineAnimTimer += Time.deltaTime;
            if (jasmineAnimTimer >= 0.125f)
            {
                jasmineAnimCounter++;
                jasmineAnimCounter %= 4;
                UpdateSprite();
                jasmineAnimTimer -= 0.125f;
            }
        }
        if (stateTimer <= 0)
        {
            if (stateCounter == 4) { manager.ExamFailed(sprites.Length > 5 ? "Catfished!~" : $"{(jesterPearlie ? "???" : "Proto-Carla")} will slowly crawl out of her box, fully coming out after moving 4 times. Use your Game Monitor to calm her down and make her crawl back into her box."); }
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
    public override bool IsAvaliableForCombo(EnemyTypes _other) { return _other == EnemyTypes.Cindy && stateCounter > 0; }
    public override void OnTexturePackChanged(string _folderName)
    {
        if (_folderName == "10" || _folderName == "11")
        {
            jesterPearlie = false;
            sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Jasmine");
        }
        else
        {
            jesterPearlie = SettingsManager.settings.jesterPearlie || (System.DateTime.Now.Day == 1 && System.DateTime.Now.Month == 4 && Random.value < 0.25f);
            sprites = Resources.LoadAll<Sprite>($"Sprites/Characters/{(jesterPearlie ? "JesterPearlie" : "Carla")}");
            jasmineAnimCounter = 0;
            jasmineAnimTimer = 0;
        }
        UpdateSprite();
    }
    void UpdateSprite()
    {
        if (stateCounter == 4)
        {
            if (sprites.Length > 5) { r.sprite = sprites[4 + (jasmineAnimCounter == 3 ? 1 : jasmineAnimCounter)]; }
            else { r.sprite = sprites[4]; }
        }
        else { r.sprite = sprites[stateCounter]; }
    }
}