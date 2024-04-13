using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TetrisScript : ConsoleWindow
{
    [SerializeField] RectTransform rows;
    [SerializeField] RectTransform nextDisplay;
    const int linesPerRound = 2;
    int linesLeft;
    float timeScale { get { return Mathf.Clamp(Mathf.Lerp(2, 0.25f, aiLevel), 0.25f, 1); } }
    float updateDelay { get { return 1 * timeScale; } }
    float updateTimer;
    bool[][] grid = new bool[20][];
    float moveDelay;
    const float moveStartDelay = 0.625f;
    const float moveAutoDelay = 0.05f;
    bool dasStarted;
    float topOutTimer;
    const float topOutDelay = 2.5f;
    int lineFlashesLeft;
    const int lineFlashes = 8;
    float lineFlashTimer;
    const float lineFlashDelay = 0.05f;
    class Piece
    {
        public enum Tetrominos { O, T, S, Z, J, L, I };
        Tetrominos tetromino;
        Vector2Int pos;
        int rot;
        bool[][] tiles = new bool[5][];
        List<Vector2Int[]> srsKicks
        {
            get
            {
                switch (tetromino)
                {
                    case Tetrominos.O:
                        return new List<Vector2Int[]>()
                        {
                            new Vector2Int[4] { new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 0) }
                        };
                    case Tetrominos.I:
                        return new List<Vector2Int[]>()
                        {
                            new Vector2Int[4] { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1) },
                            new Vector2Int[4] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(0, 1) },
                            new Vector2Int[4] { new Vector2Int(2, 0), new Vector2Int(0, 0), new Vector2Int(-2, 1), new Vector2Int(0, 1) },
                            new Vector2Int[4] { new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1) },
                            new Vector2Int[4] { new Vector2Int(2, 0), new Vector2Int(0, -2), new Vector2Int(-2, 0), new Vector2Int(0, 2) }
                        };
                    default:
                        return new List<Vector2Int[]>()
                        {
                            new Vector2Int[4] { new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0) },
                            new Vector2Int[4] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 0), new Vector2Int(-1, 0) },
                            new Vector2Int[4] { new Vector2Int(0, 0), new Vector2Int(1, -1), new Vector2Int(0, 0), new Vector2Int(-1, -1) },
                            new Vector2Int[4] { new Vector2Int(0, 0), new Vector2Int(0, 2), new Vector2Int(0, 0), new Vector2Int(0, 2) },
                            new Vector2Int[4] { new Vector2Int(0, 0), new Vector2Int(1, 2), new Vector2Int(0, 0), new Vector2Int(-1, 2) }
                        };
                }
            }
        }
        public Piece(Tetrominos _tetromino)
        {
            tetromino = _tetromino;
            pos = new Vector2Int(4, 3);
            rot = 0;
            UpdateTiles();
        }
        public bool TileFilled(Vector2Int _gridPos)
        {
            Vector2Int localPos = GridToLocalPos(_gridPos);
            if (OutOfLocalBounds(localPos)) { return false; }
            return tiles[localPos.y][localPos.x];
        }
        bool OutOfGridBounds(Vector2Int _p) { return _p.x < 0 || _p.x > 9 || _p.y < 0 || _p.y > 19; }
        bool OutOfLocalBounds(Vector2Int _p) { return _p.x < 0 || _p.x > 4 || _p.y < 0 || _p.y > 4; }
        Vector2Int LocalToGridPos(Vector2Int _localPos) { return _localPos + pos - new Vector2Int(2, 2); }
        Vector2Int GridToLocalPos(Vector2Int _gridPos) { return _gridPos - pos + new Vector2Int(2, 2); }
        public bool TryMove(Vector2Int _offset, bool[][] _grid)
        {
            Vector2Int prevPos = pos;
            pos += _offset;
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    if (tiles[y][x])
                    {
                        Vector2Int gridPos = LocalToGridPos(new Vector2Int(x, y));
                        if (OutOfGridBounds(gridPos) || _grid[gridPos.y][gridPos.x])
                        {
                            pos = prevPos;
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public bool TryRotate(int _rotDir, bool[][] _grid)
        {
            int prevRot = rot;
            rot += _rotDir;
            if (rot < 0) { rot = 3; } else if (rot > 3) { rot = 0; }
            UpdateTiles();
            foreach (Vector2Int[] srsKick in srsKicks)
            {
                Vector2Int kick = srsKick[prevRot] - srsKick[rot];
                kick.y *= -1;
                if (TryMove(kick, _grid)) { return true; }
            }
            rot = prevRot;
            UpdateTiles();
            return false;
        }
        void UpdateTiles()
        {
            switch (tetromino)
            {
                case Tetrominos.O:
                    switch (rot)
                    {
                        case 0:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, true, true, false };
                            tiles[2] = new bool[5] { false, false, true, true, false };
                            tiles[3] = new bool[5];
                            tiles[4] = new bool[5];
                            break;
                        case 1:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5];
                            tiles[2] = new bool[5] { false, false, true, true, false };
                            tiles[3] = new bool[5] { false, false, true, true, false };
                            tiles[4] = new bool[5];
                            break;
                        case 2:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5];
                            tiles[2] = new bool[5] { false, true, true, false, false };
                            tiles[3] = new bool[5] { false, true, true, false, false };
                            tiles[4] = new bool[5];
                            break;
                        case 3:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, true, true, false, false };
                            tiles[2] = new bool[5] { false, true, true, false, false };
                            tiles[3] = new bool[5];
                            tiles[4] = new bool[5];
                            break;
                    }
                    break;
                case Tetrominos.T:
                    switch (rot)
                    {
                        case 0:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, true, false, false };
                            tiles[2] = new bool[5] { false, true, true, true, false };
                            tiles[3] = new bool[5];
                            tiles[4] = new bool[5];
                            break;
                        case 1:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, true, false, false };
                            tiles[2] = new bool[5] { false, false, true, true, false };
                            tiles[3] = new bool[5] { false, false, true, false, false };
                            tiles[4] = new bool[5];
                            break;
                        case 2:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5];
                            tiles[2] = new bool[5] { false, true, true, true, false };
                            tiles[3] = new bool[5] { false, false, true, false, false };
                            tiles[4] = new bool[5];
                            break;
                        case 3:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, true, false, false };
                            tiles[2] = new bool[5] { false, true, true, false, false };
                            tiles[3] = new bool[5] { false, false, true, false, false };
                            tiles[4] = new bool[5];
                            break;
                    }
                    break;
                case Tetrominos.S:
                    switch (rot)
                    {
                        case 0:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, true, true, false };
                            tiles[2] = new bool[5] { false, true, true, false, false };
                            tiles[3] = new bool[5];
                            tiles[4] = new bool[5];
                            break;
                        case 1:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, true, false, false };
                            tiles[2] = new bool[5] { false, false, true, true, false };
                            tiles[3] = new bool[5] { false, false, false, true, false };
                            tiles[4] = new bool[5];
                            break;
                        case 2:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5];
                            tiles[2] = new bool[5] { false, true, true, false, false };
                            tiles[3] = new bool[5] { false, false, true, true, false };
                            tiles[4] = new bool[5];
                            break;
                        case 3:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, true, false, false, false };
                            tiles[2] = new bool[5] { false, true, true, false, false };
                            tiles[3] = new bool[5] { false, false, true, false, false };
                            tiles[4] = new bool[5];
                            break;
                    }
                    break;
                case Tetrominos.Z:
                    switch (rot)
                    {
                        case 0:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, true, true, false, false };
                            tiles[2] = new bool[5] { false, false, true, true, false };
                            tiles[3] = new bool[5];
                            tiles[4] = new bool[5];
                            break;
                        case 1:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, false, true, false };
                            tiles[2] = new bool[5] { false, false, true, true, false };
                            tiles[3] = new bool[5] { false, false, true, false, false };
                            tiles[4] = new bool[5];
                            break;
                        case 2:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5];
                            tiles[2] = new bool[5] { false, true, true, false, false };
                            tiles[3] = new bool[5] { false, false, true, true, false };
                            tiles[4] = new bool[5];
                            break;
                        case 3:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, true, false, false };
                            tiles[2] = new bool[5] { false, true, true, false, false };
                            tiles[3] = new bool[5] { false, true, false, false, false };
                            tiles[4] = new bool[5];
                            break;
                    }
                    break;
                case Tetrominos.J:
                    switch (rot)
                    {
                        case 0:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, true, false, false, false };
                            tiles[2] = new bool[5] { false, true, true, true, false };
                            tiles[3] = new bool[5];
                            tiles[4] = new bool[5];
                            break;
                        case 1:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, true, true, false };
                            tiles[2] = new bool[5] { false, false, true, false, false };
                            tiles[3] = new bool[5] { false, false, true, false, false };
                            tiles[4] = new bool[5];
                            break;
                        case 2:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5];
                            tiles[2] = new bool[5] { false, true, true, true, false };
                            tiles[3] = new bool[5] { false, false, false, true, false };
                            tiles[4] = new bool[5];
                            break;
                        case 3:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, true, false, false };
                            tiles[2] = new bool[5] { false, false, true, false, false };
                            tiles[3] = new bool[5] { false, true, true, false, false };
                            tiles[4] = new bool[5];
                            break;
                    }
                    break;
                case Tetrominos.L:
                    switch (rot)
                    {
                        case 0:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, false, true, false };
                            tiles[2] = new bool[5] { false, true, true, true, false };
                            tiles[3] = new bool[5];
                            tiles[4] = new bool[5];
                            break;
                        case 1:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, true, false, false };
                            tiles[2] = new bool[5] { false, false, true, false, false };
                            tiles[3] = new bool[5] { false, false, true, true, false };
                            tiles[4] = new bool[5];
                            break;
                        case 2:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5];
                            tiles[2] = new bool[5] { false, true, true, true, false };
                            tiles[3] = new bool[5] { false, true, false, false, false };
                            tiles[4] = new bool[5];
                            break;
                        case 3:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, true, true, false, false };
                            tiles[2] = new bool[5] { false, false, true, false, false };
                            tiles[3] = new bool[5] { false, false, true, false, false };
                            tiles[4] = new bool[5];
                            break;
                    }
                    break;
                case Tetrominos.I:
                    switch (rot)
                    {
                        case 0:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5];
                            tiles[2] = new bool[5] { false, true, true, true, true };
                            tiles[3] = new bool[5];
                            tiles[4] = new bool[5];
                            break;
                        case 1:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5] { false, false, true, false, false };
                            tiles[2] = new bool[5] { false, false, true, false, false };
                            tiles[3] = new bool[5] { false, false, true, false, false };
                            tiles[4] = new bool[5] { false, false, true, false, false };
                            break;
                        case 2:
                            tiles[0] = new bool[5];
                            tiles[1] = new bool[5];
                            tiles[2] = new bool[5] { true, true, true, true, false };
                            tiles[3] = new bool[5];
                            tiles[4] = new bool[5];
                            break;
                        case 3:
                            tiles[0] = new bool[5] { false, false, true, false, false };
                            tiles[1] = new bool[5] { false, false, true, false, false };
                            tiles[2] = new bool[5] { false, false, true, false, false };
                            tiles[3] = new bool[5] { false, false, true, false, false };
                            tiles[4] = new bool[5];
                            break;
                    }
                    break;
            }
        }
    }
    Piece piece = null;
    LinkedList<Piece.Tetrominos> pieceQueue;
    protected override void OnStart()
    {
        for (int i = 0; i < 20; i++) { grid[i] = new bool[10]; }
        linesLeft = 0;
        moveDelay = 0;
        dasStarted = false;
        pieceQueue = new LinkedList<Piece.Tetrominos>();
        SpawnPiece();
    }
    protected override void OnUpdate()
    {
        if (linesLeft == 0) { return; }
        if (topOutTimer > 0)
        {
            topOutTimer -= Time.deltaTime;
            return;
        }
        if (lineFlashesLeft > 0)
        {
            lineFlashTimer += Time.deltaTime;
            if (lineFlashTimer >= lineFlashDelay)
            {
                lineFlashesLeft--;
                if (lineFlashesLeft == 0)
                {
                    ClearLines();
                    if (linesLeft <= 0) { for (int y = 0; y < 20; y++) { for (int x = 0; x < 10; x++) { grid[y][x] = false; } } }
                }
                DrawScreen();
                lineFlashTimer -= lineFlashDelay;
            }
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            moveDelay -= Time.deltaTime;
            if (moveDelay <= 0)
            {
                moveDelay += dasStarted ? moveAutoDelay : moveStartDelay;
                dasStarted = true;
                if (!piece.TryMove(Input.GetKey(KeyCode.LeftArrow) ? Vector2Int.left : Vector2Int.right, grid))
                {
                    moveDelay = 0;
                    dasStarted = false;
                }
                DrawScreen();
            }
        }
        else
        {
            moveDelay = 0;
            dasStarted = false;
        }
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X))
        {
            piece.TryRotate(Input.GetKeyDown(KeyCode.Z) ? -1 : 1, grid);
            DrawScreen();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            while (piece.TryMove(Vector2Int.up, grid)) ;
            PlacePiece();
            DrawScreen();
        }
        if (Input.GetKey(KeyCode.DownArrow) && updateTimer > 0.05f) { updateTimer = 0.05f; }
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0)
        {
            if (!piece.TryMove(Vector2Int.up, grid)) { PlacePiece(); }
            DrawScreen();
            updateTimer = updateDelay;
        }
    }
    void SpawnPiece()
    {
        if (pieceQueue.Count < 2)
        {
            List<Piece.Tetrominos> bag = new List<Piece.Tetrominos>() { Piece.Tetrominos.O, Piece.Tetrominos.T, Piece.Tetrominos.S, Piece.Tetrominos.Z, Piece.Tetrominos.J, Piece.Tetrominos.L, Piece.Tetrominos.I };
            while (bag.Count > 0)
            {
                int n = Random.Range(0, bag.Count);
                pieceQueue.AddLast(bag[n]);
                bag.RemoveAt(n);
            }
        }
        piece = new Piece(pieceQueue.First.Value);
        pieceQueue.RemoveFirst();
        if (!piece.TryMove(Vector2Int.zero, grid))
        {
            for (int y = 0; y < 20; y++) { for (int x = 0; x < 10; x++) { grid[y][x] = false; } }
            topOutTimer = topOutDelay;
        }
    }
    void PlacePiece()
    {
        for (int y = 0; y < 20; y++)
        {
            for (int x = 0; x < 10; x++) { if (piece.TileFilled(new Vector2Int(x, y))) { grid[y][x] = true; } }
            if (IsFullLine(y))
            {
                lineFlashesLeft = lineFlashes;
                lineFlashTimer = 0;
            }
        }
        SpawnPiece();
    }
    void ClearLines()
    {
        int y = 19;
        while (y >= 0)
        {
            if (IsFullLine(y))
            {
                for (int y2 = y; y2 >= 0; y2--) { for (int x = 0; x < 10; x++) { grid[y2][x] = y2 > 0 ? grid[y2 - 1][x] : false; } }
                if (linesLeft > 0)
                {
                    linesLeft--;
                    if (linesLeft % linesPerRound == 0) { RoundSetCleared(); }
                }
            }
            else { y--; }
        }
    }
    bool IsFullLine(int _y)
    {
        for (int x = 0; x < 10; x++) { if (!grid[_y][x]) { return false; } }
        return true;
    }
    void DrawScreen()
    {
        for (int y = 0; y < 20; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                bool filled = false;
                if (linesLeft > 0)
                {
                    if (topOutTimer > 0) { filled = true; }
                    else if (lineFlashesLeft > 0 && IsFullLine(y)) { filled = lineFlashesLeft % 2 != 0; }
                    else { filled = grid[y][x] || piece.TileFilled(new Vector2Int(x, y)); }
                }
                rows.GetChild(y).GetChild(x).GetComponent<Image>().color = filled ? Color.white : new Color(0.25f, 0.25f, 0.25f);
            }
        }
        for (int i = 0; i < 7; i++) { nextDisplay.GetChild(i).GetComponent<Image>().color = topOutTimer > 0 || (linesLeft > 0 && i == (int)pieceQueue.First.Value) ? Color.white : new Color(0.25f, 0.25f, 0.25f); }
    }
    public override void AddRounds() { linesLeft += linesPerRound; }
    public override void OnPullUp() { DrawScreen(); }
}