using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class CharactersMenuScript : MonoBehaviour
{
    [System.Serializable]
    public struct CharacterData
    {
        public string name;
        public string description;
        public Sprite sprite;
        public CharacterData(string _name, string _description, Sprite _sprite)
        {
            name = _name;
            description = _description;
            sprite = _sprite;
        }
    }
    [SerializeField] TextMeshProUGUI titleLabel;
    [SerializeField] Image spriteImage;
    [SerializeField] TextMeshProUGUI descriptionLabel;
    [SerializeField] CharacterData[] characters;
    [SerializeField] Sprite barcodeAlt;
    [SerializeField] Sprite cassidyAlt;
    [SerializeField] Sprite lightdressAlt;
    [SerializeField] Sprite mixmaxEasterEgg;
    [SerializeField] Sprite jesterPearlie;
    int characterIndex;
    bool isCharacterLocked
    {
        get
        {
            switch (characterIndex)
            {
                case 9: return !SaveManager.saveData.allPRanks;
                case 10: return !SaveManager.saveData.allSRanks;
                default: return false;
            }
        }
    }
    bool mixmaxFlag;
    float mixmaxTimer;
    // Start is called before the first frame update
    void Start()
    {
        UpdateCharacterData();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) { PrevButtonPressed(); }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) { NextButtonPressed(); }
        if (mixmaxTimer > 0)
        {
            mixmaxTimer -= Time.deltaTime;
            if (mixmaxTimer <= 0) { UpdateCharacterData(); }
        }
        else if (characters[characterIndex].name == "Barcode" && (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Z))) { UpdateCharacterData(); }
        else if (characters[characterIndex].name == "Proto-Carla" && (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.P))) { UpdateCharacterData(); }
    }
    public void PrevButtonPressed()
    {
        do
        {
            characterIndex--;
            if (characterIndex < 0) { characterIndex = 11; }
        }
        while (isCharacterLocked);
        UpdateCharacterData();
    }
    public void NextButtonPressed()
    {
        do
        {
            characterIndex++;
            if (characterIndex > 11) { characterIndex = 0; }
        }
        while (isCharacterLocked);
        UpdateCharacterData();
    }
    void UpdateCharacterData()
    {
        bool pearlieFlag = characters[characterIndex].name == "Proto-Carla" && (SettingsManager.settings.jesterPearlie || (System.DateTime.Now.Day == 1 && System.DateTime.Now.Month == 4 && Random.value < 0.1f));
        mixmaxFlag = characters[characterIndex].name != "Proto-Carla" && !mixmaxFlag && Random.value < 0.1f && !File.Exists(Path.Combine(Application.streamingAssetsPath, "minmax_algorythm_tutorial.not_an_mp4_nooooo_not_at_all"));
        mixmaxTimer = mixmaxFlag ? 0.2f : 0;
        CharacterData data = mixmaxFlag ? new CharacterData("Found you!~", "", mixmaxEasterEgg) : pearlieFlag ? new CharacterData("???", "April Fools! :D", jesterPearlie) : characters[characterIndex];
        titleLabel.text = data.name;
        SetCharacterSprite(characters[characterIndex].name == "Barcode" && SettingsManager.settings.barcodeAlt ? barcodeAlt : data.sprite);
        descriptionLabel.text = data.description.Replace("\\n", "\n");
        if (data.name == "Cassidy" && Random.value < 0.25f) { SetCharacterSprite(cassidyAlt); }
        else if (data.name == "Princess Lightdress" && Random.value < 0.1f)
        {
            SetCharacterSprite(lightdressAlt);
            titleLabel.text = "Princess Lighterdress";
            descriptionLabel.text = "\n\n\n\neepy -^w^-\n\n\nzzz mimimimimi";
        }
        descriptionLabel.transform.parent.GetComponentInParent<ScrollRect>().normalizedPosition = Vector2.up;
    }
    void SetCharacterSprite(Sprite _s)
    {
        spriteImage.sprite = _s;
        spriteImage.rectTransform.sizeDelta = new Vector2(_s.textureRect.width, _s.textureRect.height) * ((characterIndex == 8 || characterIndex == 10) && !mixmaxFlag ? 0.5f : 1);
    }
}