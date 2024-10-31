using System.Collections.Generic;
using UnityEngine;

public class HeatspawnScript : EnemyScript
{
    enum States { None, OnWindow, OnWindowAnnoyed, BeforeAttack, Attack, BeforeHeatwave };
    States currentState;
    float stateTimer;
    const float windowAttackTime = 3;
    float moveCooldown { get { return Random.value < 0.25f ? Random.Range(2.5f, 5f) : Random.Range(7.5f, 12.5f); } }
    GameObject left;
    GameObject right;
    GameObject center;
    AudioClip lStepClip;
    AudioClip rStepClip;
    AudioClip doorKnockClip;
    AudioClip doorSlamClip;
    AudioClip heatwaveClip;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Tournament_Heatspawn; }
    protected override void OnStart()
    {
        lStepClip = Resources.Load<AudioClip>("SFX/midnight_left");
        rStepClip = Resources.Load<AudioClip>("SFX/midnight_right");
        doorKnockClip = Resources.Load<AudioClip>("SFX/door_slam");
        doorSlamClip = Resources.Load<AudioClip>("SFX/door_slam_hard");
        heatwaveClip = Resources.Load<AudioClip>("SFX/server_alarm_glitch");
        left = transform.GetChild(0).gameObject;
        right = transform.GetChild(1).gameObject;
        center = transform.GetChild(2).gameObject;
        stateTimer = moveCooldown;
        //IncreaseAI(10);
    }
    protected override void OnUpdate()
    {
        switch (currentState)
        {
            case States.None:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    if (AbleToMove()) { MoveToWindow(); }
                    else { stateTimer = moveCooldown; }
                }
                break;
            case States.OnWindow:
                if (currentLocation == Locations.LeftWindow ? manager.IsPlayerLookingAtLeftWindow() : manager.IsPlayerLookingAtRightWindow())
                {
                    stateTimer -= Time.deltaTime;
                    if (stateTimer < 0)
                    {
                        currentState = States.None;
                        stateTimer = moveCooldown;
                        if (currentLocation == Locations.RightWindow) { manager.PlaySound(rStepClip, SettingsManager.settings.explicitSubtitles ? "Heatspawn Move" : "Ominous Whisper", false, true); }
                        else { manager.PlaySound(lStepClip, SettingsManager.settings.explicitSubtitles ? "Heatspawn Move" : "Ominous Whisper", true, false); }
                        currentLocation = Locations.None;
                        manager.TriggerHallOverlay();
                        left.SetActive(false);
                        right.SetActive(false);
                    }
                }
                else
                {
                    currentState = States.OnWindowAnnoyed;
                    stateTimer = 3;
                }
                break;
            case States.OnWindowAnnoyed:
                if (currentLocation == Locations.LeftWindow ? manager.IsPlayerLookingAtLeftWindow() : manager.IsPlayerLookingAtRightWindow())
                {
                    currentState = States.OnWindow;
                    stateTimer = windowAttackTime;
                }
                else
                {
                    stateTimer -= Time.deltaTime * balanceFactor;
                    if (stateTimer <= 0 && AbleToMove())
                    {
                        currentState = States.BeforeAttack;
                        stateTimer = Random.Range(1.5f, 5);
                        currentLocation = Locations.Door;
                        manager.TriggerHallOverlay();
                        left.SetActive(false);
                        right.SetActive(false);
                        center.SetActive(true);
                    }
                }
                break;
            case States.BeforeAttack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    if (manager.IsDoorLocked())
                    {
                        currentState = States.BeforeHeatwave;
                        currentLocation = Locations.None;
                        stateTimer = moveCooldown;
                        manager.TriggerRoomOverlay();
                        center.SetActive(false);
                        manager.PlaySound(doorKnockClip, "Door Knock", true, true);
                    }
                    else
                    {
                        currentState = States.Attack;
                        stateTimer = 1.5f;
                        manager.TriggerRoomOverlay();
                        manager.RotateDoor(180, 1600);
                        manager.PlaySound(doorSlamClip, "Door Slam", true, true);
                        manager.LockPlayer();
                        manager.LockEnemies(this);
                        manager.LockCamera();
                        manager.FadeOutMusic();
                    }
                }
                break;
            case States.BeforeHeatwave:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    List<GameManagerScript.Server> servers = new List<GameManagerScript.Server>();
                    for (int i = 0; i < 4; i++) { if (!manager.servers[i].ventOff && Random.value > 0.5f) { servers.Add(manager.servers[i]); } }
                    if (servers.Count > 0)
                    {
                        foreach (GameManagerScript.Server server in servers) { server.TurnVentilationOff(); }
                        currentState = States.None;
                        manager.PlaySound(heatwaveClip, SettingsManager.settings.explicitSubtitles ? "Server Alarm (Glitched)" : "Faint Alarm...?", true, true);
                    }
                    stateTimer = moveCooldown;
                }
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { manager.ExamFailed(""); }
                break;
        }
    }
    void MoveToWindow()
    {
        currentState = States.OnWindow;
        stateTimer = windowAttackTime;
        manager.TriggerHallOverlay();
        PickWindow();
        left.SetActive(true);
        right.SetActive(true);
        if (currentLocation == Locations.RightWindow) { manager.PlaySound(rStepClip, SettingsManager.settings.explicitSubtitles ? "Heatspawn Move" : "Ominous Whisper", false, true); }
        else { manager.PlaySound(lStepClip, SettingsManager.settings.explicitSubtitles ? "Heatspawn Move" : "Ominous Whisper", true, false); }
    }
    void PickWindow()
    {
        bool left = manager.IsLocationAvailable(Locations.LeftWindow);
        bool right = manager.IsLocationAvailable(Locations.RightWindow);
        if (left != right) { currentLocation = left ? Locations.LeftWindow : Locations.RightWindow; }
        else { currentLocation = Random.value > 0.5f ? Locations.LeftWindow : Locations.RightWindow; }
    }
    bool AbleToMove()
    {
        switch (currentState)
        {
            case States.None:
                if (Random.Range(0, 10) >= aiLevel) { return false; }
                if (WindowsAvailable()) { return true; }
                return false;
            case States.OnWindowAnnoyed: return manager.IsLocationAvailable(Locations.Door);
            default: return true;
        }
    }
    bool WindowsAvailable() { return manager.IsLocationAvailable(Locations.LeftWindow) || manager.IsLocationAvailable(Locations.RightWindow); }
}