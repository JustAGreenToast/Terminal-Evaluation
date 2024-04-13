using UnityEngine;

public abstract class ConsoleWindow : MonitorWindow
{
    GameConsoleManagerScript _manager = null;
    GameConsoleManagerScript manager
    {
        get
        {
            if (_manager == null) { _manager = transform.GetComponentInParent<GameConsoleManagerScript>(); }
            return _manager;
        }
    }
    protected float aiLevel { get { return Mathf.Clamp01(manager.aiLevel * 0.1f); } }
    public abstract void AddRounds();
    protected void RoundSetCleared() { manager.RoundSetCleared(); }
}