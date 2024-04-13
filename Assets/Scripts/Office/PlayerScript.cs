using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] GameManagerScript manager;
    public enum States { Default, MainMonitor, GameMonitor, Locked };
    public States currentState { get; private set; }
    GameObject lastFocusedObj;
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        if (IsLocked()) { return; }
        bool clickedMouseTrigger = false;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition + Vector3.forward), out RaycastHit hit, 10, 1 << 3))
        {
            CursorTriggerScript trigger = hit.collider.GetComponent<CursorTriggerScript>();
            if (trigger.special || currentState == States.Default)
            {
                trigger.onHovered.Invoke();
                trigger.RefreshHoverFlag();
                if (Input.GetMouseButton(0))
                {
                    trigger.onClickHeld.Invoke();
                    if (Input.GetMouseButtonDown(0))
                    {
                        trigger.onClicked.Invoke();
                        clickedMouseTrigger = true;
                        EventSystem.current.SetSelectedGameObject(lastFocusedObj);
                    }
                }
            }
        }
        switch (currentState)
        {
            case States.Default:
                // Pull Up Main Terminal
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    currentState = States.MainMonitor;
                    manager.OpenMonitor(MonitorScript.Windows.Terminal);
                }
                // Pull Up Server GUI
                if (SaveManager.saveData.clearedExams > 0 && (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt) || Input.GetKeyDown(KeyCode.AltGr)))
                {
                    currentState = States.MainMonitor;
                    manager.OpenMonitor(MonitorScript.Windows.ServerGUI);
                }
                // Pull Up Game Monitor
                else if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftShift))
                {
                    currentState = States.GameMonitor;
                    manager.OpenMonitor(MonitorScript.Windows.Hopper);
                }
                break;
            case States.MainMonitor:
                if (Input.GetKeyDown(KeyCode.Escape) || (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject == null && !clickedMouseTrigger))
                {
                    currentState = States.Default;
                    manager.CloseMonitor();
                }
                break;
            case States.GameMonitor:
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftShift) || (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject == null && !clickedMouseTrigger))
                {
                    currentState = States.Default;
                    manager.CloseMonitor();
                }
                break;
        }
        lastFocusedObj = EventSystem.current.currentSelectedGameObject;
    }
    public void LockPlayer()
    {
        if (currentState == States.MainMonitor || currentState == States.GameMonitor) { manager.CloseMonitor(); }
        currentState = States.Locked;
    }
    public void UnlockPlayer() { if (currentState == States.Locked) { currentState = States.Default; } }
    public bool IsLocked() { return currentState == States.Locked; }
}