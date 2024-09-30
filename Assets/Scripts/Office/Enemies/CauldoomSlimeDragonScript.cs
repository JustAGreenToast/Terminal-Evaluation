using UnityEngine;

public class CauldoomSlimeDragonScript : EnemyScript
{
    enum States { Waiting, PoppingIn, Idle, PoppingOut, Attack };
    States currentState;
    int stateCounter;
    float stateTimer;
    int hoverBuffer;
    bool isHoveredOn { get { return hoverBuffer > 0; } }
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Halloween_UrsulaSlimeDragon; }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        if (hoverBuffer > 0) { hoverBuffer--; }
        switch (currentState)
        {
            case States.Waiting:
                break;
            case States.PoppingIn:
                stateTimer += Time.deltaTime;
                if (stateTimer <= 0.04f)
                {

                }
                break;
            case States.Idle:
                break;
            case States.PoppingOut:
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { manager.ExamFailed("The creature in the cauldron doesn't like being seen, keep it at bay with the Game Monitor or lure it away with your mouse if it gets too close."); }
                break;
        }
    }
    void PopIn()
    {
        transform.localPosition = new Vector3(0.8f * Random.Range(-1, 2), 1.5f, 1.25f);
    }
    void Attack()
    {
        manager.LockEnemies(this);
        manager.LockCamera();
        manager.LockPlayer();
        transform.position = new Vector3(0, -1.5f, 0.75f);
    }
    public void OnMouseHover()
    {
        hoverBuffer = 5;
    }
}
