using UnityEngine;

public class Game : MonoBehaviour
{
    public int width = 24;
    public int height = 24;
    public int mineCount = 48;

    private Board board;
    private Cell[,] state;
    private bool gameOver;

    private void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width * height);
    }

    public void Awake()
    {
        board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        newGame();
    }

    private void newGame()
    {
        state = new Cell[width, height];
        gameOver = false;

        GenerateCell();
        GenerateMine();
        GenerateNumbers();

        Camera.main.transform.position = new Vector3(width / 2, height / 2, -10f);

        board.Draw(state);
    }

    private void GenerateCell()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                if (x == 0 && y == 0)
                {
                    cell.isCastle = true;
                    cell.isCastleOne = true;
                    cell.type = Cell.Type.castle;
                }
                else if (x == width - 1 && y == height - 1)
                {
                    cell.isCastle = true;
                    cell.isCastleOne = false;
                    cell.type = Cell.Type.castle;
                }
                else
                {
                    cell.type = Cell.Type.empty;
                }
                state[x, y] = cell;
            }
        }
    }

    private void GenerateMine()
    {
        for (int i = 0; i < mineCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            while (state[x, y].type == Cell.Type.mine)
            {
                x++;

                if (x >= width)
                {
                    x = 0;
                    y++;

                    if (y >= height)
                    {
                        y = 0;
                    }
                }

            }

            state[x, y].type = Cell.Type.mine;


        }
    }

    private void GenerateNumbers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.mine)
                {
                    continue;
                }

                cell.num = CountMine(x, y);

                if (cell.num > 0)
                {
                    cell.type = Cell.Type.number;
                }

                state[x, y] = cell;
            }
        }
    }

    private int CountMine(int cellX, int cellY)
    {

        int count = 0;

        for (int adjacencyX = -1; adjacencyX <= 1; adjacencyX++)
        {
            for (int adjacencyY = -1; adjacencyY <= 1; adjacencyY++)
            {
                if (adjacencyX == 0 && adjacencyY == 0)
                    continue;

                int x = adjacencyX + cellX;
                int y = adjacencyY + cellY;

                if (GetCell(x, y).type == Cell.Type.mine)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void Update()
    {
        if (!gameOver)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Flag();
            }
            if (Input.GetMouseButtonDown(0))
            {
                Reveal();
            }
        }
    }

    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.invalid || cell.isRevealed)
        {
            return;
        }

        cell.isFlagged = !cell.isFlagged;

        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);
    }

    private void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.invalid || cell.isRevealed || cell.isFlagged)
        {
            return;
        }

        switch (cell.type)
        {
            case Cell.Type.mine:
                Explode(cell);
                break;

            case Cell.Type.empty:
                Flood(cell);
                CheckWinState();
                break;

            default:
                cell.isRevealed = true;
                state[cellPosition.x, cellPosition.y] = cell;
                CheckWinState();
                break;
        }

        if (cell.type == Cell.Type.empty)
        {
            Flood(cell);
        }


        board.Draw(state);
    }

    private void Flood(Cell cell)
    {
        if (cell.isRevealed)
            return;
        if (cell.type == Cell.Type.mine || cell.type == Cell.Type.invalid)
            return;

        cell.isRevealed = true;
        state[cell.position.x, cell.position.y] = cell;

        if (cell.type == Cell.Type.empty)
        {
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
            Flood(GetCell(cell.position.x, cell.position.y - 1));
        }
    }

    private void Explode(Cell cell)
    {
        Debug.Log("Game Over");
        gameOver = true;

        cell.isExploded = true;
        cell.isRevealed = true;

        state[cell.position.x, cell.position.y] = cell;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                cell = state[x, y];

                if (cell.type == Cell.Type.mine)
                {
                    cell.isRevealed = true;
                    state[x, y] = cell;
                }
            }
        }
    }

    private void CheckWinState()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                Cell cell = state[x, y];

                if (cell.type != Cell.Type.mine && !cell.isRevealed)
                {
                    return;
                }
            }
        }

        Debug.Log("Winner");
        gameOver = true;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.mine)
                {
                    cell.isFlagged = true;
                    state[x, y] = cell;
                }
            }
        }


    }

    private Cell GetCell(int x, int y)
    {
        if (isValid(x, y))
        {
            return state[x, y];
        }
        else
            return new Cell();
    }

    private bool isValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
