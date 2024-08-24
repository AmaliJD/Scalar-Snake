using EX;
using UnityEngine;

public static class Grid
{
    public static int gridSize = 101;
    public static int[,] grid = new int[gridSize, gridSize];

    static int translate = (gridSize - 1) / 2;

    public static void SetGrid(Vector2 pos, int i, bool isGrid = false)
    {
        Vector2 gridPos = !isGrid ? WorldToGrid(pos) : pos;
        grid[((int)gridPos.x), (int)gridPos.y] = i;
    }

    public static int GetGrid(Vector2 pos, bool isGrid = false)
    {
        Vector2 gridPos = !isGrid ? WorldToGrid(pos) : pos;

        if (gridPos.x < 0 || gridPos.x >= gridSize || gridPos.y < 0 || gridPos.y >= gridSize)
            return -1;
        else
            return grid[((int)gridPos.x), (int)gridPos.y];
    }

    public static Vector2 WorldToGrid(Vector2 pos) => pos + Vector2.one * translate;
    public static Vector2 GridToWorld(Vector2 pos) => pos - Vector2.one * translate;

    public static bool CheckNeighbor(Vector2 pos, int i)
    {
        Vector2 gridPos = WorldToGrid(pos);

        if (gridPos.x > 0 && GetGrid(gridPos.SetX(gridPos.x - 1), true) == i)
            return true;

        if (gridPos.x < gridSize - 1 && GetGrid(gridPos.SetX(gridPos.x + 1), true) == i)
            return true;

        if (gridPos.y > 0 && GetGrid(gridPos.SetY(gridPos.y - 1), true) == i)
            return true;

        if (gridPos.y < gridSize - 1 && GetGrid(gridPos.SetY(gridPos.y + 1), true) == i)
            return true;
        return false;
    }

    public static bool CheckNeighbor8Way(Vector2 pos, int i)
    {
        Vector2 gridPos = WorldToGrid(pos);

        // left
        if (gridPos.x > 0 && GetGrid(gridPos.SetX(gridPos.x - 1), true) == i)
            return true;

        // right
        if (gridPos.x < gridSize - 1 && GetGrid(gridPos.SetX(gridPos.x + 1), true) == i)
            return true;

        // down
        if (gridPos.y > 0 && GetGrid(gridPos.SetY(gridPos.y - 1), true) == i)
            return true;

        // up
        if (gridPos.y < gridSize - 1 && GetGrid(gridPos.SetY(gridPos.y + 1), true) == i)
            return true;

        // left down
        if (gridPos.x > 0 && gridPos.y > 0 && GetGrid(new Vector2(gridPos.x - 1, gridPos.y - 1), true) == i)
            return true;

        // left up
        if (gridPos.x > 0 && gridPos.y < gridSize - 1 && GetGrid(new Vector2(gridPos.x - 1, gridPos.y + 1), true) == i)
            return true;

        // right down
        if (gridPos.x < gridSize - 1 && gridPos.y > 0 && GetGrid(new Vector2(gridPos.x + 1, gridPos.y - 1), true) == i)
            return true;

        // right up
        if (gridPos.x < gridSize - 1 && gridPos.y < gridSize - 1 && GetGrid(new Vector2(gridPos.x + 1, gridPos.y + 1), true) == i)
            return true;
        return false;
    }

    public static bool CheckCell(Vector2 pos, int i, bool isGrid = false) => GetGrid(pos, isGrid) == i;
}
