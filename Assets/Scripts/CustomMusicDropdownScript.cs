using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CustomMusicDropdownScript : MonoBehaviour
{
    [SerializeField] VirtualRAM.SongRegisters targetRegister;
    // Start is called before the first frame update
    void Start()
    {
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData(targetRegister == VirtualRAM.SongRegisters.TitleScreen ? "(None)" : "(Default Ambience)") };
        foreach (AudioClip song in VirtualRAM.loadedSongs) { options.Add(new TMP_Dropdown.OptionData(song.name)); }
        GetComponent<TMP_Dropdown>().options = options;
        GetComponent<TMP_Dropdown>().value = VirtualRAM.songIndices[(int)targetRegister];
    }
}