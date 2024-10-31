using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HopperScript : ConsoleWindow
{
    class BarrelColumn
    {
        public List<int> barrels { get; private set; }
        Transform cells;
        public BarrelColumn(Transform _cells)
        {
            barrels = new List<int>();
            cells = _cells;
        }
        public void Update()
        {
            for (int i = barrels.Count - 1; i >= 0; i--)
            {
                barrels[i]++;
                if (barrels[i] > 3) { barrels.RemoveAt(i); }
            }
        }
        public void Draw()
        {
            for (int i = 0; i < cells.childCount; i++) { cells.GetChild(i).GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f); }
            for (int i = 0; i < barrels.Count; i++) { cells.GetChild(barrels[i]).GetComponent<Image>().color = Color.white; }
        }
        public bool CanAddBarrel(BarrelColumn _nextCol)
        {
            if (barrels.Count >= 2 || (barrels.Count > 0 && barrels[barrels.Count - 1] < 1)) { return false; }
            if (_nextCol != null && _nextCol.barrels.Count > 0 && _nextCol.barrels[_nextCol.barrels.Count - 1] == 1) { return false; }
            return true;
        }
        public void AddBarrel()
        {
            barrels.Add(0);
            Draw();
        }
        public void Clear()
        {
            barrels.Clear();
            Draw();
        }
        public bool CollisionCheck() { return barrels.Count > 0 && barrels[0] == 3; }
    }
    [SerializeField] Transform playerCells;
    int playerPos;
    [SerializeField] Transform[] barrelCells;
    BarrelColumn[] barrelColumns;
    const int roundsPerSet = 3;
    int roundsLeft;
    float timeScale { get { return Mathf.Clamp01(Mathf.Lerp(1.5f, 0.5f, aiLevel)); } }
    float updateDelay { get { return 1 * timeScale; } }
    float updateTimer;
    float spawnDelay { get { return Random.Range(1, 4) * updateDelay; } }
    float spawnTimer;
    protected override void OnStart()
    {
        DrawPlayer(-1);
        barrelColumns = new BarrelColumn[barrelCells.Length];
        for (int i = 0; i < barrelColumns.Length; i++)
        {
            barrelColumns[i] = new BarrelColumn(barrelCells[i]);
            barrelColumns[i].Draw();
        }
    }
    protected override void OnUpdate()
    {
        if (roundsLeft == 0) { return; }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            playerPos++;
            if (playerPos > 5)
            {
                roundsLeft--;
                if (roundsLeft % roundsPerSet == 0) { RoundSetCleared(); }
                playerPos = 0;
            }
            if (roundsLeft == 0)
            {
                DrawPlayer(-1);
                foreach (BarrelColumn col in barrelColumns) { col.Clear(); }
                return;
            }
            DrawPlayer(playerPos);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && playerPos > 1)
        {
            playerPos--;
            DrawPlayer(playerPos);
        }
        updateTimer -= Time.deltaTime;
        spawnTimer -= Time.deltaTime;
        if (updateTimer <= 0)
        {
            for (int i = 0; i < barrelColumns.Length; i++)
            {
                barrelColumns[i].Update();
                barrelColumns[i].Draw();
            }
            updateTimer = updateDelay;
            if (spawnTimer <= 0) { spawnTimer = SpawnBarrel() ? spawnDelay : updateDelay; }
        }
        if (playerPos > 0 && playerPos < 5 && barrelColumns[playerPos - 1].CollisionCheck())
        {
            playerPos = 0;
            DrawPlayer(0);
        }
    }
    public override void AddRounds()
    {
        if (roundsLeft == 0)
        {
            updateTimer = updateDelay;
            spawnTimer = spawnDelay;
            for (int i = 0; i < 3 && Random.value > 0.5f; i++) { SpawnBarrel(); }
            DrawPlayer(0);
        }
        roundsLeft += roundsPerSet;
    }
    public override void ClearRounds()
    {
        roundsLeft = 0;
        DrawPlayer(-1);
        foreach (BarrelColumn col in barrelColumns) { col.Clear(); }
    }
    void DrawPlayer(int _pos) { for (int i = 0; i < playerCells.childCount; i++) { playerCells.GetChild(i).GetComponent<Image>().color = i == _pos ? Color.white : new Color(0.2f, 0.2f, 0.2f); } }
    bool SpawnBarrel()
    {
        List<int> columns = new List<int>();
        for (int i = 0; i < barrelColumns.Length; i++) { if (barrelColumns[i].CanAddBarrel(i == barrelColumns.Length - 1 ? null : barrelColumns[i + 1])) { columns.Add(i); } }
        if (columns.Count > 0)
        {
            BarrelColumn col = barrelColumns[columns[Random.Range(0, columns.Count)]];
            col.AddBarrel();
            return true;
        }
        return false;
    }
    public override void OnPullUp()
    {
        if (roundsLeft > 0)
        {
            DrawPlayer(playerPos);
            for (int i = 0; i < barrelColumns.Length; i++) { barrelColumns[i].Draw(); }
        }
    }
}