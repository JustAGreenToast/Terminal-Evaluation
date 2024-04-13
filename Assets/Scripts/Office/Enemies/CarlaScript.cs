using UnityEngine;

public class CarlaScript : EnemyScript
{
    int stateCounter;
    float stateTimer;
    float moveCooldown { get { return Random.value > 0.9f ? Random.Range(10f, 15f) : Random.Range(24f, 32f); } }
    bool jesterPearlie;
    Sprite[] sprites;
    SpriteRenderer r;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Carla; }
    protected override void OnStart()
    {
        jesterPearlie = SettingsManager.settings.jesterPearlie || (System.DateTime.Now.Day == 1 && System.DateTime.Now.Month == 4 && Random.value < 0.25f);
        sprites = Resources.LoadAll<Sprite>($"Sprites/Characters/{(jesterPearlie ? "JesterPearlie" : "Carla")}");
        r = GetComponent<SpriteRenderer>();
        r.sprite = sprites[0];
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
        stateTimer -= Time.deltaTime * balanceFactor;
        if (stateTimer <= 0)
        {
            if (stateCounter == 4) { manager.ExamFailed($"{(jesterPearlie ? "???" : "Proto-Carla")} will slowly crawl out of her box, fully coming out after moving 4 times. Use your Game Monitor to calm her down and make her crawl back into her box."); }
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
        r.sprite = sprites[stateCounter];
    }
    public override bool IsAvaliableForCombo(EnemyTypes _other) { return _other == EnemyTypes.Cindy && stateCounter > 0; }
}