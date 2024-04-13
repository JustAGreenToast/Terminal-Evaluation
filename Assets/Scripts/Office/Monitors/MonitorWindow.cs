using UnityEngine;

public class MonitorWindow : MonoBehaviour
{
    protected bool isOpen { get; private set; } = false;
    // Start is called before the first frame update
    void Start()
    {
        OnStart();
    }
    // Update is called once per frame
    void Update()
    {
        if (isOpen) { OnUpdate(); }
    }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    public virtual void Open() { isOpen = true; }
    public virtual void Close() { isOpen = false; }
    public virtual void OnPullUp() { }
    public virtual void OnPullDown() { }
}