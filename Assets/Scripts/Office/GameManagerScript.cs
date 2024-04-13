using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManagerScript : MonoBehaviour
{
    #region [Public Components]
    [SerializeField] PlayerScript player;
    [SerializeField] EnemyScript[] enemies;
    [SerializeField] CameraControllerScript cam;
    [SerializeField] MonitorScript monitor;
    [SerializeField] ServerGUIScript serverGUI;
    [SerializeField] GameConsoleManagerScript gameMonitor;
    [SerializeField] DoorScript door;
    [SerializeField] BlackOverlayScript hallOverlay;
    [SerializeField] BlackOverlayScript roomOverlay;
    #endregion
    #region [Sound Components]
    MusicPlayerScript musicPlayer;
    AudioSource[] sfxPlayers;
    #endregion
    #region [Servers]
    public class Server
    {
        enum Status { Working, Overheat, Off, Restarting, ShutdownQueued, Disabled };
        Status currentStatus;
        public float heatTimer { get; private set; }
        public bool ventOff { get; private set; }
        public bool powerOff { get { return currentStatus == Status.Off; } }
        public bool shutdownQueued { get { return currentStatus == Status.ShutdownQueued; } }
        float restartTimer;
        public bool isDown { get { return currentStatus != Status.Working && currentStatus != Status.ShutdownQueued; } }
        bool updateFlag;
        public Color powerButtonColor
        {
            get
            {
                switch (currentStatus)
                {
                    case Status.Off: return Color.HSVToRGB(345 / 360f, 1, 1);
                    case Status.Restarting: return new Color(0.25f, 0.25f, 0.25f);
                    case Status.ShutdownQueued: return Color.HSVToRGB(20 / 360f, 0.9f, 1); ;
                    default: return Color.HSVToRGB(125 / 360f, 1, 1);
                }
            }
        }
        public Color ventButtonColor
        {
            get
            {
                if (ventOff) { return heatTimer < 1 ? Color.HSVToRGB(45 / 360f, 0.9f, 1) : Color.HSVToRGB(20 / 360f, 0.9f, 1); }
                return Color.HSVToRGB(195 / 360f, 0.9f, 1);
            }
        }
        public Color temperatureMeterColor { get { return currentStatus == Status.Overheat ? Color.HSVToRGB(45 / 360f, 0.9f, 1) : Color.white; } }
        public string statusString
        {
            get
            {
                switch (currentStatus)
                {
                    case Status.Overheat: return "<color=#ffff00>Overheat</color>";
                    case Status.Off: return "<color=#ff0000>Off</color>";
                    case Status.Restarting: return "<color=#00ffff>Restarting</color>";
                    case Status.ShutdownQueued: return $"<color=#ff8000>Shutdown</color> in {10 - (int)restartTimer}";
                    default: return "<color=#00ff00>OK</color>";
                }
            }
        }
        public bool Update()
        {
            switch (currentStatus)
            {
                case Status.Working:
                    if (ventOff)
                    {
                        heatTimer += 0.1f * Time.deltaTime;
                        if (heatTimer >= 1)
                        {
                            heatTimer = 1;
                            currentStatus = Status.Overheat;
                        }
                        return true;
                    }
                    else if (heatTimer > 0)
                    {
                        heatTimer -= 0.5f * Time.deltaTime;
                        if (heatTimer < 0) { heatTimer = 0; }
                        return true;
                    }
                    break;
                case Status.Overheat:
                    if (ventOff) { return false; }
                    heatTimer -= 0.1f * Time.deltaTime;
                    if (heatTimer <= 0)
                    {
                        heatTimer = 0;
                        currentStatus = Status.Working;
                    }
                    return true;
                case Status.Restarting:
                    restartTimer += Time.deltaTime;
                    if (restartTimer >= 5)
                    {
                        currentStatus = Status.Working;
                        return true;
                    }
                    break;
                case Status.ShutdownQueued:
                    if (ventOff)
                    {
                        heatTimer += 0.1f * Time.deltaTime;
                        if (heatTimer >= 1) { heatTimer = 1; }
                    }
                    else if (heatTimer > 0)
                    {
                        heatTimer -= 0.5f * Time.deltaTime;
                        if (heatTimer < 0)
                        {
                            heatTimer = 0;
                            currentStatus = Status.Overheat;
                        }
                    }
                    restartTimer += Time.deltaTime;
                    if (restartTimer > 10) { currentStatus = Status.Off; }
                    return true;
            }
            if (updateFlag)
            {
                updateFlag = false;
                return true;
            }
            return false;
        }
        public void TurnVentilationOff()
        {
            if (!ventOff)
            {
                ventOff = true;
                updateFlag = true;
            }
        }
        void TurnVentilationOn()
        {
            if (ventOff)
            {
                ventOff = false;
                updateFlag = true;
            }
        }
        public void ToggleVentilation() { if (ventOff) { TurnVentilationOn(); } else { TurnVentilationOff(); } }
        public void TurnOff()
        {
            if (currentStatus != Status.Off)
            {
                currentStatus = Status.Off;
                updateFlag = true;
            }
        }
        void TurnOn()
        {
            restartTimer = 0;
            currentStatus = Status.Restarting;
            updateFlag = true;
        }
        public void ToggleState() { if (currentStatus == Status.Off) { TurnOn(); } else { TurnOff(); } }
        public void QueueShutdown()
        {
            currentStatus = Status.ShutdownQueued;
            restartTimer = 0;
            updateFlag = true;
        }
        public void CancelShutdown()
        {
            currentStatus = Status.Working;
            updateFlag = true;
        }
    }
    public Server[] servers { get; private set; }
    #endregion
    #region [UI Components]
    Sprite[] rankIcons;
    [SerializeField] Image rankIcon;
    Sprite[] guardianAngelIcons;
    [SerializeField] Image guardianAngelIcon;
    [SerializeField] TextMeshProUGUI timerLabel;
    [SerializeField] GameObject hintBG;
    [SerializeField] TextMeshProUGUI hintLabel;
    #endregion
    enum States { Hint, Lap1, Lap2, LastDance };
    States currentState;
    readonly string[] hints = new string[4]
    {
        "Chelsea will try to get your attention, whereas Cupcake is satisfied as long as you're busy.\n\nPress right click to start.",
        "If something appears behind your monitor, look at it until it leaves. Use your Game Monitor if something starts coming out of the box.\n\nPress right click to start.",
        "If something new appears behind your monitor, click on it to make it go away. Hold left click on the door to keep it closed. Keep an eye on the servers using 'halconfig'!\n\nPress right click to start.",
        "Remember to hold the door shut if something tries to get in and to keep an eye on the servers if you hear something unusual...\n\nPress right click to start."
    };
    float startTime;
    int failCounter;
    int currentRank { get { return failCounter == 0 && currentState == States.Lap2 ? 5 : Mathf.Clamp(4 - failCounter, 0, 4); } }
    bool guardianAngelUsed;
    bool endlessLastDance;
    float rollTimer;
    float rollDisplayTimer;
    float[] rollTimerIncrements = new float[6]
    {
        120, // Difficulty 1: +2 Minutes
        180, // Difficulty 2: +3 Minutes
        300, // Difficulty 3: +5 Minutes
        420, // Difficulty 4: +7 Minutes
        600, // Difficulty 5: +10 Minutes
        1200 // Difficulty *5: +20 Minutes
    };
    int rollTimerIndex;
    public float rollLap2BonusTime
    {
        get
        {
            if (!VirtualRAM.examData.rollBank) { return 0; }
            float t = 0;
            for (int i = 0; i < VirtualRAM.examData.minExercises; i++) { t += rollTimerIncrements[i] * 0.5f; }
            return t;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        musicPlayer = transform.GetChild(0).GetComponent<MusicPlayerScript>();
        sfxPlayers = transform.GetChild(1).GetComponentsInChildren<AudioSource>();
        foreach (AudioSource sfxPlayer in sfxPlayers) { sfxPlayer.volume = SettingsManager.settings.sfxVol * SettingsManager.settings.masterVol; }
        servers = new Server[4];
        for (int i = 0; i < servers.Length; i++) { servers[i] = new Server(); }
        startTime = Time.time;
        if (VirtualRAM.examData.examIndex < 4)
        {
            currentState = States.Hint;
            LockEnemies(null);
            LockPlayer();
            hintLabel.text = hints[VirtualRAM.examData.examIndex];
            hintBG.GetComponent<Image>().enabled = SettingsManager.settings.mainOfficeTextureSet == 1;
            hintBG.SetActive(true);
        }
        else
        {
            currentState = VirtualRAM.examData.examIndex < 8 ? States.Lap1 : States.LastDance;
            hintBG.SetActive(false);
            int songIndex = currentState == States.LastDance ? VirtualRAM.lastDanceSong : VirtualRAM.lap1Song;
            endlessLastDance = VirtualRAM.examData.endless || songIndex == -1;
            if (songIndex == -1 || VirtualRAM.examData.examIndex != 11)
            {
                AudioClip clip = songIndex == -1 ? Resources.Load<AudioClip>("SFX/ambience") : VirtualRAM.loadedSongs[songIndex];
                musicPlayer.PlaySong(clip, currentState != States.LastDance || endlessLastDance);
            }
            if (VirtualRAM.examData.examIndex == 11)
            {
                currentState = States.Lap1;
                rollTimer = 0;
                rollDisplayTimer = rollTimer;
            }
        }
        // Extended UI
        rankIcons = Resources.LoadAll<Sprite>("Sprites/Rank Icons");
        rankIcon.sprite = rankIcons[currentRank];
        guardianAngelIcons = Resources.LoadAll<Sprite>("Sprites/Guardian Angel Icons");
        switch (SettingsManager.settings.selectedGuardianAngel)
        {
            case SettingsManager.Settings.GuardianAngels.None:
            case SettingsManager.Settings.GuardianAngels.Lap2Only:
                guardianAngelUsed = true;
                break;
            case SettingsManager.Settings.GuardianAngels.Lap1Only:
            case SettingsManager.Settings.GuardianAngels.BothLaps:
            case SettingsManager.Settings.GuardianAngels.Default:
                guardianAngelUsed = false;
                break;
        }
        guardianAngelIcon.sprite = guardianAngelIcons[guardianAngelUsed ? 1 : 0];
        rankIcon.enabled = SettingsManager.settings.extenededUI;
        guardianAngelIcon.enabled = SettingsManager.settings.extenededUI;
        timerLabel.enabled = SettingsManager.settings.extenededUI;
        // Hall Stickers
        List<Vector2> hallStickers = new List<Vector2>();
        int edgeCase = Random.Range(0, 3);
        int edgeCaseSign = Random.value > 0.5f ? -1 : 1;
        for (int i = 0; i < 3; i++)
        {
            Transform currentSticker = transform.GetChild(2).GetChild(i);
            if (i == edgeCase) { currentSticker.localPosition = new Vector2(Random.Range(5.25f, 7.75f) * edgeCaseSign, Random.Range(0.8f, 2f)); }
            else
            {
                Vector2 p;
                do { p = new Vector2(Random.Range(5.5f, 7.5f) * -edgeCaseSign, Random.Range(0.9f, 1.9f)); } while (hallStickers.Count > 0 && Vector2.Distance(hallStickers[0], p) < 0.75f);
                hallStickers.Add(p);
                currentSticker.localPosition = p;
            }
            currentSticker.localRotation = Quaternion.Euler(Vector3.forward * (Random.value > 0.75f ? Random.value * 360 : Random.Range(-30f, 30f)));
        }
        // Miriam (Door Bodyguard)
        transform.GetChild(3).gameObject.SetActive(SettingsManager.settings.miriamDoor);
        VirtualRAM.lastDanceScore = currentState == States.LastDance ? 0 : -1;
    }
    // Update is called once per frame
    void Update()
    {
        bool serverChanged = false;
        for (int i = 0; i < servers.Length; i++) { if (servers[i].Update()) { serverChanged = true; } }
        if (serverChanged) { serverGUI.UpdateGUI(); }
        switch (currentState)
        {
            case States.Hint:
                if (Input.GetMouseButtonDown(1))
                {
                    UnlockEnemies();
                    player.UnlockPlayer();
                    hintLabel.text = "";
                    hintBG.SetActive(false);
                    currentState = States.Lap1;
                    startTime = Time.time;
                    int songIndex = VirtualRAM.lap1Song;
                    musicPlayer.PlaySong(songIndex == -1 ? Resources.Load<AudioClip>("SFX/ambience") : VirtualRAM.loadedSongs[songIndex]);
                }
                break;
            case States.Lap1:
            case States.Lap2:
                {
                    if (VirtualRAM.examData.examIndex == 11)
                    {
                        if (rollTimer > 0)
                        {
                            rollTimer -= Time.deltaTime;
                            if (rollTimer <= 0)
                            {
                                ExamFailed("Time's up!");
                                return;
                            }
                            rollDisplayTimer = Mathf.MoveTowards(rollDisplayTimer, rollTimer, 150 * Time.deltaTime);
                            timerLabel.color = rollDisplayTimer == rollTimer ? Color.white : Color.HSVToRGB(315 / 360f, 0.6f, 1);
                            timerLabel.text = TimeUtils.SecondsToTimerString(rollDisplayTimer);
                        }
                        else { timerLabel.text = TimeUtils.SecondsToTimerString(0); }
                    }
                    else { timerLabel.text = TimeUtils.SecondsToTimerString(Time.time - startTime); }
                }
                break;
            case States.LastDance:
                {
                    timerLabel.text = TimeUtils.SecondsToTimerString(endlessLastDance ? Time.time - startTime : musicPlayer.timeLeft);
                    if (!musicPlayer.isPlaying) { ExamFailed("Hope you enjoyed your last dance!"); }
                }
                break;
        }
    }
    public bool IsLocationAvailable(EnemyScript.Locations _location)
    {
        if (_location == EnemyScript.Locations.Door && SettingsManager.settings.miriamDoor) { return false; }
        foreach (EnemyScript enemy in enemies) { if (enemy.currentLocation == _location) { return false; } }
        return true;
    }
    public bool IsEnemyAtLocation(EnemyScript.EnemyTypes _enemy, EnemyScript.Locations _location)
    {
        foreach (EnemyScript enemy in enemies) { if (enemy.enemyType == _enemy && enemy.currentLocation == _location) { return true; } }
        return false;
    }
    public bool IsEnemyComboAvailable(EnemyScript.EnemyTypes _target, EnemyScript.EnemyTypes _caller)
    {
        foreach (EnemyScript enemy in enemies) { if (enemy.enemyType == _target) { return enemy.IsAvaliableForCombo(_caller); } }
        return false;
    }
    public void TriggerEnemyCombo(EnemyScript.EnemyTypes _target, EnemyScript.EnemyTypes _caller)
    {
        foreach (EnemyScript enemy in enemies)
        {
            if (enemy.enemyType == _target)
            {
                enemy.IsAvaliableForCombo(_caller);
                break;
            }
        }
    }
    public void LockEnemies(EnemyScript _self) { foreach (EnemyScript enemy in enemies) { if (enemy != _self) { enemy.LockEnemy(); } } }
    public void UnlockEnemies() { foreach (EnemyScript enemy in enemies) { enemy.UnlockEnemy(); } }
    public void LockPlayer() { player.LockPlayer(); }
    public void LockCamera(float _lockAngle = 0) { cam.Lock(true, _lockAngle); }
    public void UnlockCamera() { cam.Unlock(); }
    public void UnlockPlayer() { player.UnlockPlayer(); }
    public void OpenMonitor(MonitorScript.Windows _window)
    {
        monitor.PullUp(_window);
        cam.Lock(false);
        foreach (EnemyScript enemy in enemies) { enemy.MonitorFlipped(true); }
    }
    public void CloseMonitor()
    {
        monitor.PullDown();
        cam.Unlock();
        foreach (EnemyScript enemy in enemies) { enemy.MonitorFlipped(false); }
    }
    public bool IsWorkMonitorUp() { return player.currentState == PlayerScript.States.MainMonitor; }
    public bool IsGameMonitorUp() { return player.currentState == PlayerScript.States.GameMonitor; }
    public void TriggerHallOverlay() { hallOverlay.Activate(); }
    public void TriggerRoomOverlay() { roomOverlay.Activate(); }
    public void RotateDoor(float _angle, float _speed) { door.Rotate(_angle, _speed); }
    public void AddGameConsoleRounds() { gameMonitor.AddRounds(); }
    public bool IsDoorLocked() { return door.isClosed; }
    public bool IsPlayerLookingAtRightWindow() { return cam.transform.rotation.eulerAngles.y < 180 && cam.transform.rotation.eulerAngles.y > 2.5f; }
    public bool IsPlayerLookingAtLeftWindow() { return cam.transform.rotation.eulerAngles.y > 180 && 360 - cam.transform.rotation.eulerAngles.y > 2.5f; }
    public bool IsAnyServerDown()
    {
        foreach (Server server in servers) { if (server.isDown) { return true; } }
        return false;
    }
    public void PlaySound(AudioClip _sfx)
    {
        bool playedSound = false;
        for (int i = 0; i < sfxPlayers.Length && !playedSound; i++)
        {
            if (!sfxPlayers[i].isPlaying || sfxPlayers[i].clip == _sfx)
            {
                sfxPlayers[i].clip = _sfx;
                sfxPlayers[i].Play();
                playedSound = true;
            }
        }
        if (!playedSound)
        {
            sfxPlayers[0].clip = _sfx;
            sfxPlayers[0].Play();
        }
    }
    public void FadeOutMusic() { musicPlayer.StopSong(); }
    public void ResumeMusic() { musicPlayer.ResumeSong(); }
    public void Lap2Started()
    {
        currentState = States.Lap2;
        foreach (EnemyScript enemy in enemies) { enemy.OnLap2Started(); }
        if (VirtualRAM.examData.examIndex != 11)
        {
            int songIndex = VirtualRAM.lap2Song;
            if (songIndex != VirtualRAM.lap1Song)
            {
                musicPlayer.PlaySong(songIndex == -1 ? Resources.Load<AudioClip>("SFX/ambience") : VirtualRAM.loadedSongs[songIndex]);
                TriggerRoomOverlay();
            }
        }
        else if (VirtualRAM.examData.rollBank) { rollTimer += rollLap2BonusTime; }
        rankIcon.sprite = rankIcons[currentRank == 5 && VirtualRAM.examData.examIndex == 11 ? 6 : currentRank];
        switch (SettingsManager.settings.selectedGuardianAngel)
        {
            case SettingsManager.Settings.GuardianAngels.None:
            case SettingsManager.Settings.GuardianAngels.Lap1Only:
                guardianAngelUsed = true;
                break;
            case SettingsManager.Settings.GuardianAngels.Lap2Only:
            case SettingsManager.Settings.GuardianAngels.Default:
                guardianAngelUsed = failCounter != 0;
                break;
        }
        guardianAngelIcon.sprite = guardianAngelIcons[guardianAngelUsed ? 1 : 0];
        ExerciseStarted();
    }
    public void ExerciseStarted()
    {
        if (VirtualRAM.examData.examIndex == 11 && currentState == States.Lap1)
        {
            float extraTime = rollTimerIncrements[rollTimerIndex == 4 && SettingsManager.settings.finalFiveEnabled ? 5 : rollTimerIndex];
            if (VirtualRAM.examData.rollBank) { extraTime *= 0.5f; }
            rollTimer += extraTime;
            rollTimerIndex++;
        }
    }
    public void ExerciseFailed()
    {
        if (!guardianAngelUsed)
        {
            guardianAngelUsed = true;
            guardianAngelIcon.sprite = guardianAngelIcons[1];
        }
        else if (failCounter < 4)
        {
            failCounter++;
            rankIcon.sprite = rankIcons[currentRank];
        }
    }
    public void ExamStarted()
    {
        if (VirtualRAM.examData.examIndex == 11 && VirtualRAM.lastDanceSong != -1) { musicPlayer.PlaySong(VirtualRAM.loadedSongs[VirtualRAM.lastDanceSong], currentState != States.LastDance || endlessLastDance); }
    }
    public void ExercisePassed()
    {
        if (currentState == States.LastDance) { VirtualRAM.lastDanceScore++; }
        if (VirtualRAM.examData.examIndex == 12) { foreach (EnemyScript enemy in enemies) { enemy.IncreaseAI(1); } }
    }
    public void ExamFailed(string _failMessage)
    {
        VirtualRAM.failMessage = _failMessage;
        SceneManager.LoadScene(3);
    }
    public void ExamPassed()
    {
        VirtualRAM.clearTime = Mathf.Clamp(Time.time - startTime, 0, 5999.999f);
        VirtualRAM.clearRank = currentRank == 5 && VirtualRAM.examData.examIndex == 11 ? 6 : currentRank;
        if (VirtualRAM.examData.examIndex < 8 && SaveManager.saveData.clearData[VirtualRAM.examData.examIndex == 6 ? 5 : VirtualRAM.examData.examIndex].UpdateClearData(VirtualRAM.clearRank, VirtualRAM.clearTime)) { SaveManager.saveData.Save(); }
        SceneManager.LoadScene(4);
    }
}