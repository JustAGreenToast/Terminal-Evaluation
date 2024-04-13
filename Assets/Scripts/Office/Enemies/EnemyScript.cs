using UnityEngine;

public abstract class EnemyScript : MonoBehaviour
{
    public enum Locations { None, LeftWindow, RightWindow, Door, Monitor };
    public Locations currentLocation { get; protected set; }
    public enum EnemyTypes { Chelsea, Cupcake, Midnight, Cassidy, Melissa, Barcode, Carla, Aqua, H41, Cindy, H42, LightDress };
    public EnemyTypes enemyType { get { return GetEnemyType(); } }
    protected abstract EnemyTypes GetEnemyType();
    [SerializeField] protected GameManagerScript manager;
    bool isLocked;
    protected bool isMonitorUp { get; private set; }
    public int aiLevel { get; private set; }
    float _balanceFactor = -1;
    protected float balanceFactor
    {
        get
        {
            if (_balanceFactor < 0)
            {
                float totalAI = 0;
                foreach (int n in VirtualRAM.examData.aiLevels) { totalAI += n * 0.1f; }
                _balanceFactor = Mathf.Clamp01(Mathf.Lerp(1.5f, 0.625f, totalAI / VirtualRAM.examData.aiLevels.Length));
                if (VirtualRAM.examData.windowObstacle != VirtualRAM.ExamData.WindowObstacle.None) { _balanceFactor *= 0.8f; }
                //print(_balanceFactor);
            }
            return _balanceFactor;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        aiLevel = VirtualRAM.examData.aiLevels[(int)enemyType];
        OnStart();
    }
    protected virtual void OnStart() { }
    // Update is called once per frame
    void Update() { if (!isLocked) { OnUpdate(); } }
    protected virtual void OnUpdate() { }
    public void IncreaseAI(int n) { aiLevel += n; }
    public void LockEnemy()
    {
        isLocked = true;
        OnEnemyLocked();
    }
    public void UnlockEnemy()
    {
        isLocked = false;
        OnEnemyUnlocked();
    }
    protected virtual void OnEnemyLocked() { }
    protected virtual void OnEnemyUnlocked() { }
    public void MonitorFlipped(bool _isUp)
    {
        isMonitorUp = _isUp;
        OnMonitorFlipped();
    }
    protected virtual void OnMonitorFlipped() { }
    protected bool IsEnemyComboAvailable(EnemyTypes _target) { return manager.IsEnemyComboAvailable(_target, enemyType); }
    public virtual bool IsAvaliableForCombo(EnemyTypes _other) { return false; }
    protected void TriggerEnemyCombo(EnemyTypes _target) { manager.TriggerEnemyCombo(_target, enemyType); }
    public virtual void ComboTriggered(EnemyTypes _other) { }
    public virtual void OnLap2Started() { }
}