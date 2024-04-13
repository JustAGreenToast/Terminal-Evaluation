using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessManagerScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<PostProcessVolume>().enabled = SettingsManager.settings.postProcess;
    }
}