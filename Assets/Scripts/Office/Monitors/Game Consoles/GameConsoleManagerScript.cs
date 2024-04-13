using UnityEngine;

public class GameConsoleManagerScript : MonitorWindow
{
    ConsoleWindow console
    {
        get
        {
            if (SettingsManager.settings.tetrisCartridge) { return transform.GetChild(1).GetComponent<ConsoleWindow>(); }
            if (SettingsManager.settings.quadCoreCartridge) { return transform.GetChild(2).GetComponent<ConsoleWindow>(); }
            return transform.GetChild(0).GetComponent<ConsoleWindow>();
        }
    }
    [SerializeField] CarlaScript enemyRef;
    public float aiLevel { get { return Mathf.Clamp01(enemyRef.aiLevel * 0.1f); } }
    protected override void OnStart() { console.gameObject.SetActive(true); }
    public override void Open()
    {
        base.Open();
        console.Open();
    }
    public override void Close()
    {
        base.Close();
        console.Close();
    }
    public override void OnPullUp() { console.OnPullUp(); }
    public override void OnPullDown() { console.OnPullDown(); }
    public void AddRounds() { console.AddRounds(); }
    public void RoundSetCleared() { enemyRef.RoundSetCleared(); }
}