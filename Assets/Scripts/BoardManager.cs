using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board")]
    public GameObject cellPrefab;
    public int size;
    public float cellSpacing;
    private Cell[,] grid;

    [Header("Pawn")]
    public GameObject pawnPrefab;
    private Pawn player1;
    private Pawn player2;

    public Pawn Player1 => player1;
    public Pawn Player2 => player2;
    public int Size => size;

    [Header("Wall")]
    public GameObject wallPrefab;
    private HashSet<(Vector2Int, Vector2Int)> blockedEdges
        = new HashSet<(Vector2Int, Vector2Int)>();

    private Queue<WallInstance> player1Walls = new Queue<WallInstance>();
    private Queue<WallInstance> player2Walls = new Queue<WallInstance>();
    private const int MAX_WALLS = 5;

    private Coroutine blinkingCoroutine;
    private WallInstance blinkingWall;

    void Start()
    {
        GenerateBoard();
        SpawnPawns();
    }

    public void GenerateBoard()
    {
        grid = new Cell[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector3 pos = new Vector3(x * cellSpacing, y *
                    cellSpacing, 0);

                GameObject obj = Instantiate(cellPrefab, pos, Quaternion.identity, transform);

                Cell cell = obj.GetComponent<Cell>();
                cell.x = x;
                cell.y = y;

                grid[x, y] = cell;
            }
        }
    }

    public void SpawnPawns()
    {
        int mid = size / 2;

        player1 = Instantiate(pawnPrefab, transform).GetComponent<Pawn>();
        player1.playerId = 1;
        player1.boardPos = new Vector2Int(mid, 0);
        MovePawnToCell(player1);

        player2 = Instantiate(pawnPrefab, transform).GetComponent<Pawn>();
        player2.playerId = 2;
        player2.boardPos = new Vector2Int(mid, size - 1);
        MovePawnToCell(player2);
    }
    public void MovePawnToCell(Pawn pawn)
    {
        Cell cell = grid[pawn.boardPos.x, pawn.boardPos.y];
        pawn.transform.position = cell.transform.position;
    }
    bool IsValidCell(Vector2Int p)
    {
        return p.x >= 0 && p.x < size &&
            p.y >= 0 && p.y < size;
    }
    private bool HasPath(Pawn pawn, int goalRow)
    {
        Queue<Vector2Int> q = new();
        HashSet<Vector2Int> visited = new();

        q.Enqueue(pawn.boardPos);
        visited.Add(pawn.boardPos);

        while(q.Count > 0)
        {
            var cur = q.Dequeue();

            if (cur.y == goalRow)
                return true;

            Vector2Int[] dirs =
            {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

            foreach (var d in dirs)
            {
                var next = cur + d;

                if (!IsValidCell(next)) continue;
                if (visited.Contains(next)) continue;
                if (blockedEdges.Contains((cur, next))) continue;

                visited.Add(next);
                q.Enqueue(next);
            }
        }
        return false;
    }
    Vector2Int GetNextStep(Vector2Int start, int goalRow)
    {
        List<Vector2Int> openSet = new List<Vector2Int>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, int> fScore = new Dictionary<Vector2Int, int>();

        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goalRow);

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet[0];
            foreach (var node in openSet)
            {
                if (fScore.ContainsKey(node) && fScore[node] < fScore[current])
                    current = node;
            }

            if ((goalRow > start.y && current.y >= goalRow) ||
                (goalRow <= start.y && current.y <= goalRow))
            {
                return ReconstructFirstStep(cameFrom, current, start);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                int tentativeG = gScore[current] + 1;

                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);
                else if (tentativeG >= gScore.GetValueOrDefault(neighbor, int.MaxValue))
                    continue;

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeG;
                fScore[neighbor] = tentativeG + Heuristic(neighbor, goalRow);
            }
        }

        return start;
    }

    private int Heuristic(Vector2Int pos, int goalRow)
    {
        return Mathf.Abs(goalRow - pos.y);
    }
    Vector2Int ReconstructFirstStep(
        Dictionary<Vector2Int, Vector2Int> cameFrom,
    Vector2Int current,
    Vector2Int start)
    {
        while (cameFrom.ContainsKey(current) && cameFrom[current] != start)
        {
            current = cameFrom[current];
        }

        return current;
    }
    List<Vector2Int> GetNeighbors(Vector2Int cur)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        Vector2Int[] dirs =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        foreach (var dir in dirs)
        {
            Vector2Int next = cur + dir;

            if (!IsValidCell(next)) continue;
            if (blockedEdges.Contains((cur, next))) continue;

            if (IsPawnAt(next))
            {
                Vector2Int jump = next + dir;

                if (IsValidCell(jump) &&
                    !blockedEdges.Contains((next, jump)) &&
                    !IsPawnAt(jump))
                {
                    result.Add(jump);
                }
                else
                {
                    Vector2Int side1, side2;

                    if (dir == Vector2Int.up || dir == Vector2Int.down)
                    {
                        side1 = next + Vector2Int.left;
                        side2 = next + Vector2Int.right;
                    }
                    else
                    {
                        side1 = next + Vector2Int.up;
                        side2 = next + Vector2Int.down;
                    }

                    if (IsValidCell(side1) &&
                        !blockedEdges.Contains((next, side1)))
                        result.Add(side1);

                    if (IsValidCell(side2) &&
                        !blockedEdges.Contains((next, side2)))
                        result.Add(side2);
                }
            }
            else
            {
                result.Add(next);
            }
        }

        return result;
    }
    public void AutoMovePawn(Pawn pawn)
    {
        int goalRow = pawn.playerId == 1 ? size - 1 : 0;

        Vector2Int next = GetNextStep(pawn.boardPos, goalRow);

        if(next == pawn.boardPos)
        {
            return;
        }

        pawn.boardPos = next;
        MovePawnToCell(pawn);
        Debug.Log(next);
    }
    public bool CheckWin(Pawn pawn)
    {
        if (pawn.playerId == 1 && pawn.boardPos.y == size - 1) return true;
        if (pawn.playerId == 2 && pawn.boardPos.y == 0) return true;
        
        return false;
    }

    public bool PlaceWall(Vector3 worldPos, PlayerTurn turn)
    {
        if(worldPos.x < 0 || worldPos.y < 0 ||
            worldPos.x > (size - 1) * cellSpacing||
            worldPos.y > (size -1) * cellSpacing)
        {
            return false;
        }
        Vector3 pos = SnapToWallPosition(worldPos);

        float cellX = Mathf.Round(worldPos.x / cellSpacing) * cellSpacing;
        float cellY = Mathf.Round(worldPos.y / cellSpacing) * cellSpacing;

        Vector3 cellCenter = new Vector3(cellX, cellY, 0);

        Vector3 diff = worldPos - cellCenter;

        bool horizontal = Mathf.Abs(diff.y) > Mathf.Abs(diff.x);

        Quaternion rot = horizontal ? Quaternion.identity : Quaternion.Euler(0, 0, 90);

        AddWallBlock(pos, horizontal);

        bool p1HasPath = HasPath(player1, size - 1);
        bool p2HasPath = HasPath(player2, 0);

        if (!p1HasPath || !p2HasPath)
        {
            Debug.Log("Invalid wall! Blocked all path");

            RemoveWallBlock(pos, horizontal);
            return false;
        }
        GameObject wallObj = Instantiate(wallPrefab, pos, rot, transform);

        WallInstance newWall = new WallInstance(wallObj, pos, horizontal);

        Queue<WallInstance> targetQueue =
            turn == PlayerTurn.Player1 ? player1Walls : player2Walls;

        targetQueue.Enqueue(newWall);

        if (targetQueue.Count > MAX_WALLS)
        {
            WallInstance oldWall = targetQueue.Dequeue();

            if (blinkingWall == oldWall && blinkingCoroutine != null)
            {
                StopCoroutine(blinkingCoroutine);
                blinkingCoroutine = null;
                blinkingWall = null;
            }
            RemoveWallBlock(oldWall.pos, oldWall.horizontal);
            Destroy(oldWall.wallObject);
        }
        return true;
    }
    private Vector3 SnapToWallPosition(Vector3 pos)
    {
        float x = Mathf.Round(pos.x / cellSpacing - 0.5f) * cellSpacing + cellSpacing / 2;
        float y = Mathf.Round(pos.y / cellSpacing - 0.5f) * cellSpacing + cellSpacing / 2;

        return new Vector3(x, y, 0);
    }
    
    private void AddWallBlock(Vector3 wallPos, bool horizontal)
    {
        int x = Mathf.FloorToInt(wallPos.x / cellSpacing);
        int y = Mathf.FloorToInt(wallPos.y / cellSpacing);

        if (horizontal)
        {
            Block(new Vector2Int(x, y), new Vector2Int(x, y + 1));
            Block(new Vector2Int(x + 1, y), new Vector2Int(x + 1, y + 1));
        }
        else
        {
            Block(new Vector2Int(x, y), new Vector2Int(x + 1, y));
            Block(new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1));
        }
    }
    private void Block(Vector2Int a, Vector2Int b)
    {
        blockedEdges.Add((a, b));
        blockedEdges.Add((b, a));
    }

    private void UnBLock(Vector2Int a, Vector2Int b)
    {
        blockedEdges.Remove((a, b));
        blockedEdges.Remove((b, a));
    }
    private void RemoveWallBlock(Vector3 wallPos, bool horizontal)
    {
        int x = Mathf.FloorToInt(wallPos.x / cellSpacing);
        int y = Mathf.FloorToInt(wallPos.y / cellSpacing);

        if (horizontal)
        {
            UnBLock(new Vector2Int(x, y), new Vector2Int(x, y + 1));
            UnBLock(new Vector2Int(x + 1, y), new Vector2Int(x + 1, y + 1));
        }
        else
        {
            UnBLock(new Vector2Int(x, y), new Vector2Int(x + 1, y));
            UnBLock(new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1));
        }
    }
    private bool IsPawnAt(Vector2Int pos)
    {
        return player1.boardPos == pos || player2.boardPos == pos;
    }
    public void PreviewOldestWall(PlayerTurn turn)
    {
        Queue<WallInstance> targetQueue =
        turn == PlayerTurn.Player1 ? player1Walls : player2Walls;

        if (targetQueue.Count < MAX_WALLS)
            return;

        WallInstance oldest = targetQueue.Peek();

        if(blinkingWall == oldest)
        {
            return;
        }
        if (blinkingCoroutine != null)
            StopCoroutine(blinkingCoroutine);

        blinkingWall = oldest;
        blinkingCoroutine = StartCoroutine(BlinkLoop(blinkingWall));
    }
    IEnumerator BlinkLoop(WallInstance wall)
    {
        while (wall != null && wall.wallObject != null)
        {
            Renderer[] renderers = wall.wallObject.GetComponentsInChildren<Renderer>();

            foreach (var r in renderers)
                if (r != null)
                    r.enabled = false;

            yield return new WaitForSeconds(0.2f);

            foreach (var r in renderers)
                if (r != null)
                    r.enabled = true;

            yield return new WaitForSeconds(0.2f);
        }
    }
    class WallInstance
    {
        public GameObject wallObject;
        public Vector3 pos;
        public bool horizontal;

        public WallInstance(GameObject obj, Vector3 p, bool h)
        {
            wallObject = obj;
            pos = p;
            horizontal = h;
        }
    }
}
