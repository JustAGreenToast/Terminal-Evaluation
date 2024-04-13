using UnityEngine;
using UnityEngine.SceneManagement;

public class EasterEggScreenScript : MonoBehaviour
{
    float t;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<AudioSource>().volume = SettingsManager.settings.masterVol * SettingsManager.settings.sfxVol;
        for (int i = 0; i < 5; i++) { SaveManager.saveData.clearData[i].UpdateClearData(0, 6039.999f); }
        SaveManager.saveData.Save();
    }
    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        if (t > 10) { SceneManager.LoadScene(1); }
    }
}
