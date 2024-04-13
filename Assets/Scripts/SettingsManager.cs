using System.IO;
using UnityEngine;

public static class SettingsManager
{
    public class Settings
    {
        readonly static string path = Path.Combine(Application.persistentDataPath, "settings.json");
        public float masterVol { get; private set; } = 0.8f;
        public float musicVol { get; private set; } = 0.4f;
        public float sfxVol { get; private set; } = 1;
        public bool fullscreen { get; private set; } = false;
        public bool postProcess { get; private set; } = true;
        public bool extenededUI { get; private set; } = false;
        public int mainOfficeTextureSet { get; private set; } = 0;
        public enum ConsoleThemes { Default, Invaders, PacMan, Tetris };
        public ConsoleThemes selectedConsoleTheme = ConsoleThemes.Default;
        public enum GuardianAngels { None, Lap1Only, Lap2Only, BothLaps, Default };
        public GuardianAngels selectedGuardianAngel = GuardianAngels.Default;
        public bool miriamDoor { get; private set; } = false;
        public bool barcodeAlt { get; private set; } = false;
        public bool barcodeSave { get; private set; } = true;
        public bool midnightDoor { get; private set; } = false;
        public bool midnightAggressiveKnock { get; private set; } = false;
        public VirtualRAM.ExamData.DifficultyBumps examDifficulty = VirtualRAM.ExamData.DifficultyBumps.Auto;
        public bool finalFiveEnabled { get; private set; } = false;
        public bool deluxeStatusPanel { get; private set; } = false;
        public float vimFontSize { get; private set; } = 8;
        public bool tetrisCartridge { get; private set; } = false;
        public bool quadCoreCartridge { get; private set; } = false;
        public bool jesterPearlie { get; private set; } = false;
        public class SettingsJSON
        {
            public float masterVol;
            public float musicVol;
            public float sfxVol;
            public bool fullscreen;
            public bool postProcess;
            public bool extenededUI;
            public int mainOfficeTextureSet;
            public ConsoleThemes selectedConsoleTheme;
            public GuardianAngels selectedGuardianAngel;
            public bool miriamDoor;
            public bool barcodeAlt;
            public bool barcodeSnipe;
            public bool midnightDoor;
            public bool midnightAggressiveKnock;
            public VirtualRAM.ExamData.DifficultyBumps examDifficulty;
            public bool finalFiveEnabled;
            public bool deluxeStatusPanel;
            public float vimFontSize;
            public bool tetrisCartridge;
            public bool quadCoreCartridge;
            public bool jesterPearlie;
            public SettingsJSON(Settings _settings)
            {
                masterVol = _settings.masterVol;
                musicVol = _settings.musicVol;
                sfxVol = _settings.sfxVol;
                fullscreen = _settings.fullscreen;
                postProcess = _settings.postProcess;
                extenededUI = _settings.extenededUI;
                mainOfficeTextureSet = _settings.mainOfficeTextureSet;
                selectedConsoleTheme = _settings.selectedConsoleTheme;
                selectedGuardianAngel = _settings.selectedGuardianAngel;
                miriamDoor = _settings.miriamDoor;
                barcodeAlt = _settings.barcodeAlt;
                barcodeSnipe = _settings.barcodeSave;
                midnightDoor = _settings.midnightDoor;
                midnightAggressiveKnock = _settings.midnightAggressiveKnock;
                examDifficulty = _settings.examDifficulty;
                finalFiveEnabled = _settings.finalFiveEnabled;
                deluxeStatusPanel = _settings.deluxeStatusPanel;
                vimFontSize = _settings.vimFontSize;
                tetrisCartridge = _settings.tetrisCartridge;
                quadCoreCartridge = _settings.quadCoreCartridge;
                jesterPearlie = _settings.jesterPearlie;
            }
            public void Save() { using (StreamWriter sw = new StreamWriter(path)) { sw.Write(JsonUtility.ToJson(this)); } }
            public static SettingsJSON Load() { using (StreamReader sr = new StreamReader(path)) { return JsonUtility.FromJson<SettingsJSON>(sr.ReadToEnd()); } }
        }
        public Settings()
        {
            masterVol = 0.8f;
            musicVol = 0.4f;
            sfxVol = 1;
            fullscreen = false;
            postProcess = true;
            extenededUI = false;
            mainOfficeTextureSet = 0;
            selectedConsoleTheme = ConsoleThemes.Default;
            selectedGuardianAngel = GuardianAngels.Default;
            miriamDoor = false;
            barcodeAlt = false;
            barcodeSave = true;
            midnightDoor = true;
            midnightAggressiveKnock = false;
            examDifficulty = VirtualRAM.ExamData.DifficultyBumps.Auto;
            finalFiveEnabled = false;
            deluxeStatusPanel = false;
            vimFontSize = 20;
            tetrisCartridge = false;
            quadCoreCartridge = false;
            jesterPearlie = false;
        }
        public Settings(SettingsJSON _json)
        {
            masterVol = _json.masterVol;
            musicVol = _json.musicVol;
            sfxVol = _json.sfxVol;
            fullscreen = _json.fullscreen;
            postProcess = _json.postProcess;
            extenededUI = _json.extenededUI;
            mainOfficeTextureSet = _json.mainOfficeTextureSet;
            selectedConsoleTheme = _json.selectedConsoleTheme;
            selectedGuardianAngel = _json.selectedGuardianAngel;
            miriamDoor = _json.miriamDoor;
            barcodeAlt = _json.barcodeAlt;
            barcodeSave = _json.barcodeSnipe;
            midnightDoor = _json.midnightDoor;
            midnightAggressiveKnock = _json.midnightAggressiveKnock;
            examDifficulty = _json.examDifficulty;
            finalFiveEnabled = _json.finalFiveEnabled;
            deluxeStatusPanel = _json.deluxeStatusPanel;
            vimFontSize = _json.vimFontSize;
            tetrisCartridge = _json.tetrisCartridge;
            quadCoreCartridge = _json.quadCoreCartridge;
            jesterPearlie = _json.jesterPearlie;
        }
        public static Settings Load()
        {
            if (!File.Exists(path)) { new Settings().Save(); }
            return new Settings(SettingsJSON.Load());
        }
        public void Save() { new SettingsJSON(this).Save(); }
        public void SetMasterVolume(float _vol)
        {
            masterVol = _vol;
            Save();
        }
        public void SetMusicVolume(float _vol)
        {
            musicVol = _vol;
            Save();
        }
        public void SetSFXVolume(float _vol)
        {
            sfxVol = _vol;
            Save();
        }
        public void SetFullscreenFlag(bool _enabled)
        {
            fullscreen = _enabled;
            Resolution newRes = new Resolution();
            newRes.width = 1280;
            newRes.height = 720;
            newRes.refreshRate = Screen.currentResolution.refreshRate;
            if (fullscreen)
            {
                Resolution maxRes = Screen.resolutions[0];
                foreach (Resolution res in Screen.resolutions)
                {
                    float ratio = (float)res.width / (float)res.height;
                    if (res.width > maxRes.width && res.height > maxRes.height && Mathf.Abs((16f / 9f) - ratio) < 0.1f) { maxRes = res; }
                }
                newRes = maxRes;
            }
            Screen.SetResolution(newRes.width, newRes.height, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, newRes.refreshRate);
            Save();
        }
        public void SetPostProcessFlag(bool _enabled)
        {
            postProcess = _enabled;
            Save();
        }
        public void SetExtendedUIFlag(bool _enabled)
        {
            extenededUI = _enabled;
            Save();
        }
        public void SetMainOfficeTextureSet(int _val)
        {
            mainOfficeTextureSet = _val;
            Save();
        }
        public void SelectGuardianAngel(GuardianAngels _selected)
        {
            selectedGuardianAngel = _selected;
            Save();
        }
        public void SelectConsoleTheme(ConsoleThemes _selected)
        {
            selectedConsoleTheme = _selected;
            Save();
        }
        public void SetDoorBodyguardFlag(bool _enabled)
        {
            miriamDoor = _enabled;
            Save();
        }
        public void SetBarcodeAltFlag(bool _enabled)
        {
            barcodeAlt = _enabled;
            Save();
        }
        public void SetBarcodeSaveFlag(bool _enabled)
        {
            barcodeSave = _enabled;
            Save();
        }
        public void SetDoorStallFlag(bool _enabled)
        {
            midnightDoor = _enabled;
            Save();
        }
        public void SetMidnightAggressiveKnockFlag(bool _enabled)
        {
            midnightAggressiveKnock = _enabled;
            Save();
        }
        public void SelectExamDifficulty(VirtualRAM.ExamData.DifficultyBumps _difficulty)
        {
            examDifficulty = _difficulty;
            Save();
        }
        public void SetFinalFiveFlag(bool _enabled)
        {
            finalFiveEnabled = _enabled;
            Save();
        }
        public void SetDeluxeStatusPanelFlag(bool _enabled)
        {
            deluxeStatusPanel = _enabled;
            Save();
        }
        public void SetVimFontSize(float _val)
        {
            vimFontSize = _val;
            Save();
        }
        public void SetTetrisCartridgeFlag(bool _enabled)
        {
            tetrisCartridge = _enabled;
            if (_enabled) { quadCoreCartridge = false; }
            Save();
        }
        public void SetQuadCoreCartridgeFlag(bool _enabled)
        {
            quadCoreCartridge = _enabled;
            if (_enabled) { tetrisCartridge = false; }
            Save();
        }
        public void SetJesterPearlieFlag(bool _enabled)
        {
            jesterPearlie = _enabled;
            Save();
        }
    }
    public static Settings settings { get; private set; } = new Settings();
    public static void Load() { settings = Settings.Load(); }
}