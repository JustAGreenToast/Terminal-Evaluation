using UnityEngine;
using UnityEngine.UI;

public class ExerciseSelectPanelScript : MonoBehaviour
{
    [SerializeField] Transform buttonGrid;
    Button[] buttons;
    // Start is called before the first frame update
    void Start()
    {
        buttons = new Button[15];
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = buttonGrid.GetChild(i).GetComponent<Button>();
            buttons[i].interactable = !VirtualRAM.tournamentData.round3SelectedExercises[i];
        }
    }
    bool IsGridFull()
    {
        foreach (bool selected in VirtualRAM.tournamentData.round3SelectedExercises) { if (!selected) { return false; } }
        return true;
    }
    public void SelectExercise(int _n)
    {
        VirtualRAM.tournamentData.round3SelectedExercises[_n] = true;
        VirtualRAM.tournamentData.ExerciseSelected(_n);
        if (IsGridFull())
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                VirtualRAM.tournamentData.round3SelectedExercises[i] = false;
                buttons[i].interactable = true;
            }
        }
        else { buttons[_n].interactable = false; }
    }
}