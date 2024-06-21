using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TextMeshProTabFixScript : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
#if !PLATFORM_STANDALONE_WIN && !UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Tab) && EventSystem.current.currentSelectedGameObject == inputField.gameObject)
        {
            inputField.text = inputField.text.Insert(inputField.caretPosition, "\t");
            inputField.caretPosition++;
        }
#endif
    }
}