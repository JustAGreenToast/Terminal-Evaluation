using UnityEngine;
using UnityEngine.Events;

public class CursorTriggerScript : MonoBehaviour
{
    const int hoverFrameBuffer = 5;
    int frameCooldown;
    public bool special;
    public bool hoveredOn { get; private set; }
    public UnityEvent onHovered;
    public UnityEvent onClicked;
    public UnityEvent onClickHeld;
    // Update is called once per frame
    void Update()
    {
        if (frameCooldown > 0)
        {
            frameCooldown--;
            if (frameCooldown == 0) { hoveredOn = false; }
        }
    }
    public void RefreshHoverFlag()
    {
        hoveredOn = true;
        frameCooldown = hoverFrameBuffer;
    }
}