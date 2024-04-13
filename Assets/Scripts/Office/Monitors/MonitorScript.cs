using UnityEngine;

public class MonitorScript : MonoBehaviour
{
    public enum States { Hidden, Rising, Up, Hiding };
    States currentState = States.Hidden;
    public bool isUp { get { return currentState == States.Rising || currentState == States.Up; } }
    public enum Windows { Terminal, Vim, ServerGUI, Hopper };
    public Windows currentWindow { get; private set; }
    float angle = 120;
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case States.Rising:
                angle = Mathf.MoveTowards(angle, 0, 1200 * Time.deltaTime);
                if (angle == 0)
                {
                    transform.GetChild((int)currentWindow + 1).GetComponent<MonitorWindow>().Open();
                    currentState = States.Up;
                }
                transform.localRotation = Quaternion.Euler(Vector3.right * angle);
                break;
            case States.Hiding:
                angle = Mathf.MoveTowards(angle, 120, 1200 * Time.deltaTime);
                if (angle == 120) { currentState = States.Hidden; }
                transform.localRotation = Quaternion.Euler(Vector3.right * angle);
                break;
        }
    }
    public void PullUp(Windows _window)
    {
        currentState = States.Rising;
        transform.GetChild((int)currentWindow + 1).GetComponent<CanvasGroup>().alpha = 0;
        transform.GetChild((int)currentWindow + 1).GetComponent<CanvasGroup>().blocksRaycasts = false;
        currentWindow = _window;
        transform.GetChild((int)currentWindow + 1).GetComponent<CanvasGroup>().alpha = 1;
        transform.GetChild((int)currentWindow + 1).GetComponent<CanvasGroup>().blocksRaycasts = true;
        transform.GetChild((int)currentWindow + 1).GetComponent<MonitorWindow>().OnPullUp();
    }
    public void PullDown()
    {
        transform.GetChild((int)currentWindow + 1).GetComponent<MonitorWindow>().OnPullDown();
        transform.GetChild((int)currentWindow + 1).GetComponent<MonitorWindow>().Close();
        currentState = States.Hiding;
    }
}