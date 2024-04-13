using System.Collections.Generic;
using UnityEngine;

public class AquaScript : EnemyScript
{
    enum States { None, OnDoor, BeforeServer, Attack };
    States currentState;
    float stateTimer;
    float moveCooldown { get { return Random.value > 0.9f ? Random.Range(10f, 15f) : Random.Range(15f, 30f); } }
    Sprite[] sprites;
    SpriteRenderer r;
    AudioClip openDoorSound;
    AudioClip closeDoorSound;
    AudioClip alarmSound;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Aqua; }
    protected override void OnStart()
    {
        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Aqua");
        openDoorSound = Resources.Load<AudioClip>("SFX/door_open");
        closeDoorSound = Resources.Load<AudioClip>("SFX/door_close");
        alarmSound = Resources.Load<AudioClip>("SFX/server_alarm");
        r = GetComponent<SpriteRenderer>();
        r.enabled = false;
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
        switch (currentState)
        {
            case States.None:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    if (AbleToMove())
                    {
                        currentState = States.OnDoor;
                        stateTimer = 10;
                        currentLocation = Locations.Door;
                        manager.PlaySound(openDoorSound);
                        manager.TriggerRoomOverlay();
                        manager.RotateDoor(30, 120);
                        transform.position = new Vector3(0, 0.125f, 3.375f);
                        transform.rotation = Quaternion.Euler(Vector3.forward * 30);
                        r.sprite = sprites[0];
                        r.enabled = true;
                    }
                    else { stateTimer = moveCooldown; }
                }
                break;
            case States.OnDoor:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (manager.IsDoorLocked())
                {
                    currentState = States.BeforeServer;
                    stateTimer = moveCooldown;
                    currentLocation = Locations.None;
                    manager.PlaySound(closeDoorSound);
                    manager.TriggerRoomOverlay();
                    manager.RotateDoor(0, 420);
                    r.enabled = false;
                }
                else if (stateTimer <= 0)
                {
                    currentState = States.Attack;
                    stateTimer = 1.5f;
                    manager.TriggerRoomOverlay();
                    transform.position = Vector3.forward * 2.5f;
                    transform.rotation = Quaternion.identity;
                    r.sortingOrder = 5;
                    r.color = Color.white;
                    r.sprite = sprites[1];
                    manager.RotateDoor(0, 270);
                    manager.LockPlayer();
                    manager.LockEnemies(this);
                    manager.LockCamera();
                    manager.FadeOutMusic();
                    manager.PlaySound(closeDoorSound);
                }
                break;
            case States.BeforeServer:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    List<GameManagerScript.Server> servers = new List<GameManagerScript.Server>();
                    for (int i = 0; i < 4; i++) { if (!manager.servers[i].ventOff && Random.value > 0.5f) { servers.Add(manager.servers[i]); } }
                    if (servers.Count > 0)
                    {
                        foreach (GameManagerScript.Server server in servers) { server.TurnVentilationOff(); }
                        currentState = States.None;
                        manager.PlaySound(alarmSound);
                    }
                    stateTimer = moveCooldown;
                }
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { manager.ExamFailed("Aqua will try to sneak in through the front door, close the door before she gets in. After closing the door, Aqua will turn off the ventilation on any of the 4 main servers, triggering an alarm in the process: if a server overheats, it will remain inactive until it cools down."); }
                break;
        }
    }
    bool AbleToMove()
    {
        switch (currentState)
        {
            case States.None:
                if (Random.Range(0, 10) >= aiLevel) { return false; }
                return manager.IsLocationAvailable(Locations.Door);
            case States.BeforeServer: return Random.Range(0, 10) < aiLevel;
            default: return true;
        }
    }
}