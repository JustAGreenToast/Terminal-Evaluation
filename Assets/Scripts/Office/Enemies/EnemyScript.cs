using UnityEngine;

public abstract class EnemyScript : MonoBehaviour
{
    public enum Locations { None, LeftWindow, RightWindow, Door, Monitor, Behind };
    public Locations currentLocation { get; protected set; }
    public enum EnemyTypes { Chelsea, Cupcake, Midnight, Cassidy, Melissa, Barcode, Carla, Aqua, H41, Cindy, H42, Lightdress, Tournament_Heatspawn, Tournament_Wildcard, Tournament_Molly, Tournament_Pride, Tournament_MixmaxStressToy, Halloween_UrsulaSlimeDragon, Halloween_Blossom };
    public EnemyTypes enemyType { get { return GetEnemyType(); } }
    protected abstract EnemyTypes GetEnemyType();
    [SerializeField] protected GameManagerScript manager;
    protected bool isLocked { get; private set; }
    protected bool isMonitorUp { get; private set; }
    int _aiLevel;
    public int aiLevel
    {
        get { return _aiLevel; }
        private set { _aiLevel = Mathf.Clamp(value, 0, 10); }
    }
    static float _balanceFactor = -1;
    protected float balanceFactor
    {
        get
        {
            if (_balanceFactor < 0)
            {
                float totalAI = manager.GetEnemyAISum() * 0.1f;
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
        if (enemyType <= EnemyTypes.Lightdress) { aiLevel = VirtualRAM.examData.aiLevels[(int)enemyType]; }
        OnStart();
    }
    protected virtual void OnStart() { }
    // Update is called once per frame
    void Update() { if (!isLocked) { OnUpdate(); } }
    protected virtual void OnUpdate() { }
    public void IncreaseAI(int n)
    {
        aiLevel += n;
        RecalculateBalanceFactor();
    }
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
    public virtual void OnTexturePackChanged(string _folderName) { }
    private void RecalculateBalanceFactor() { _balanceFactor = -1; }
}