using UnityEngine;

public class WindowBalloonsScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (VirtualRAM.examData.windowObstacle == VirtualRAM.ExamData.WindowObstacle.Balloons)
        {
            int sign = Random.value > 0.5f ? 1 : -1;
            transform.GetChild(0).localPosition = Vector3.right * 3.5f * sign;
            transform.GetChild(1).localPosition = Vector3.right * 3.5f * -sign;
        }
        else { gameObject.SetActive(false); }
    }
}