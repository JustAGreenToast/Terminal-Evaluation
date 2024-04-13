using System.Collections.Generic;
using UnityEngine;

public class H41Script : EnemyScript
{
    enum States { None, BeforeAttack, Attack };
    States currentState;
    float stateTimer;
    int stateCounter;
    float moveCooldown { get { return Random.value > 0.9f ? Random.Range(12f, 20f) : Random.Range(24f, 42f); } }
    Sprite[] sprites;
    SpriteRenderer r;
    AudioClip glitchSound;
    AudioClip doorClip;
    AudioClip lightUpSound;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.H41; }
    protected override void OnStart()
    {
        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/H41");
        glitchSound = Resources.Load<AudioClip>("SFX/hal_glitch");
        doorClip = Resources.Load<AudioClip>("SFX/door_slam_hard");
        lightUpSound = Resources.Load<AudioClip>("SFX/hal_lightup");
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
                        if (manager.IsAnyServerDown())
                        {
                            if (manager.IsLocationAvailable(Locations.Door))
                            {
                                currentState = States.BeforeAttack;
                                stateTimer = Random.Range(1.5f, 5);
                                currentLocation = Locations.Door;
                                manager.TriggerHallOverlay();
                                transform.position = new Vector3(0, 1.275f, 5);
                            }
                        }
                        else
                        {
                            List<GameManagerScript.Server> servers = new List<GameManagerScript.Server>();
                            for (int i = 0; i < 4; i++) { if (!manager.servers[i].powerOff && Random.value > 0.5f) { servers.Add(manager.servers[i]); } }
                            if (servers.Count > 0)
                            {
                                foreach (GameManagerScript.Server server in servers) { server.QueueShutdown(); }
                                currentState = States.None;
                                manager.PlaySound(glitchSound);
                            }
                            stateTimer = moveCooldown;
                        }
                    }
                    else { stateTimer = moveCooldown; }
                }
                break;
            case States.BeforeAttack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    currentState = States.Attack;
                    stateTimer = 1.5f;
                    manager.TriggerRoomOverlay();
                    r.sprite = sprites[0];
                    r.enabled = true;
                    manager.RotateDoor(150, 720);
                    manager.PlaySound(doorClip);
                    manager.LockPlayer();
                    manager.LockEnemies(this);
                    manager.LockCamera();
                    manager.FadeOutMusic();
                }
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    if (stateCounter == 4) { manager.ExamFailed("H41 will occasionally shut down any of the main 4 servers, which can be monitored via the 'halconfig' command. If any of the servers stay inactive for too long, you lose."); }
                    else
                    {
                        r.sprite = sprites[stateCounter];
                        stateTimer = stateCounter < 3 ? 0.1f : 1.5f;
                        if (stateCounter == 0) { manager.PlaySound(lightUpSound); }
                        stateCounter++;
                    }
                }
                break;
        }
    }
    bool AbleToMove() { return currentState != States.None || Random.Range(0, 10) < aiLevel; }
}