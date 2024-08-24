using EX;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Fill
{
    public int minX, maxX, minY, maxY;
    public bool drawing;
    public Vector2 translate = Vector2.zero;
    int[,] area;

    public static Fill fill = new();

    public void ClearMinMax()
    {
        //minX = int.MaxValue;
        //maxX = int.MinValue;
        //minY = int.MaxValue;
        //maxY = int.MinValue;
        minX = 0; maxX = 0; minY = 0; maxY = 0;
    }

    public string PrintMinMax() => $"MinX: {minX}, MaxX: {maxX}, MinY: {minY}, MaxY: {maxY}";

    public void SelectArea()
    {
        area = new int[maxX - minX + 3, maxY - minY + 3];
    }

    public List<Vector2> FloodFillOutside()
    {
        //int[,] area = new int[maxX - minX + 3, maxY - minY + 3];
        translate = new Vector2(minX - 1, minY - 1);

        Queue<Vector2Int> callStack = new();
        callStack.Enqueue(Vector2Int.zero);

        Vector2Int currentCell;
        List<Vector2> outsidePositions = new();
        //List<Vector2> visitedCells = new();

        Dictionary<Vector2, int> cellVisitCount = new();

        int iterations = 0;

        while(callStack.Count > 0 && iterations <= (area.GetLength(0) * area.GetLength(1)))
        {
            currentCell = callStack.Dequeue();
            Vector2 worldPos = currentCell + translate;
            //visitedCells.Add(currentCell);

            if (cellVisitCount.ContainsKey(currentCell))
                continue;//cellVisitCount[currentCell]++;
            else
                cellVisitCount.Add(currentCell, 1);


            // turn outside 9's into 3's
            if (Grid.CheckCell(worldPos, 9))// && Grid.CheckNeighbor8Way(worldPos, 0))
            {
                Grid.SetGrid(worldPos, 3);
            }

            // mark cell as outside
            if (Grid.CheckCell(worldPos, 0) || Grid.CheckCell(worldPos, -1))// || Grid.CheckCell(worldPos, 9))
            {
                area[currentCell.x, currentCell.y] = 2;
                outsidePositions.Add(worldPos);

                // add left cell
                if (currentCell.x - 1 >= 0 && !cellVisitCount.ContainsKey(new Vector2(currentCell.x - 1, currentCell.y)) && !callStack.Contains(new Vector2Int(currentCell.x - 1, currentCell.y)))
                    callStack.Enqueue(new Vector2Int(currentCell.x - 1, currentCell.y));

                // add right cell
                if (currentCell.x + 1 < area.GetLength(0) && !cellVisitCount.ContainsKey(new Vector2(currentCell.x + 1, currentCell.y)) && !callStack.Contains(new Vector2Int(currentCell.x + 1, currentCell.y)))
                    callStack.Enqueue(new Vector2Int(currentCell.x + 1, currentCell.y));

                // add up cell
                if (currentCell.y + 1 < area.GetLength(1) && !cellVisitCount.ContainsKey(new Vector2(currentCell.x, currentCell.y + 1)) && !callStack.Contains(new Vector2Int(currentCell.x, currentCell.y + 1)))
                    callStack.Enqueue(new Vector2Int(currentCell.x, currentCell.y + 1));

                // add down cell
                if (currentCell.y - 1 >= 0 && !cellVisitCount.ContainsKey(new Vector2(currentCell.x, currentCell.y - 1)) && !callStack.Contains(new Vector2Int(currentCell.x, currentCell.y - 2)))
                    callStack.Enqueue(new Vector2Int(currentCell.x, currentCell.y - 1));
            }
            else
            {
                area[currentCell.x, currentCell.y] = 1;
            }

            iterations++;
        }

        //Debug.Log($"Iterations Outside: {iterations}");

        return outsidePositions;
    }

    public List<Vector2> FloodFillInside(List<Vector2> trailPositions, bool fillAreaWithTrail = false)
    {
        if(fillAreaWithTrail)
        {
            foreach (var trailPos in trailPositions)
            {
                Vector2Int areaPos = (trailPos - translate).RoundToInt();
                if (area[areaPos.x, areaPos.y] != 1)
                    area[areaPos.x, areaPos.y] = 1;
            }
        }
        
        int iterations = 0;
        Vector2 pos = Vector2.zero;
        Vector2 findPos = pos;
        List<Vector2> tempTrailPositions = new(trailPositions);

        do
        {
            int rand = UnityEngine.Random.Range(0, tempTrailPositions.Count - 1);

            pos = tempTrailPositions[rand];
            tempTrailPositions.RemoveAt(rand);

            findPos = FindInsidePosition(pos);

            iterations++;
        }
        while (findPos == pos && iterations < trailPositions.Count);

        //for(int index = trailPositions.Count - 1; index > 0; index--)
        //{
        //    pos = trailPositions[index];
        //    findPos = FindInsidePosition(pos);
        //    iterations++;

        //    if (findPos != pos)
        //        break;
        //}

        //Debug.Log($"Iterations Finding Inside Spawn: {iterations}");

        if (findPos == pos)
            return new List<Vector2>();

        //// cancel out revivable cells
        //foreach(Vector2 cell in trailPositions)
        //{
        //    if(Grid.CheckCell(cell, 9, true))
        //    {
        //        Grid.SetGrid(cell, 3);
        //    }
        //}

        Queue<Vector2> callStack = new();
        //callStack.Enqueue(FindInsidePosition(pos));
        callStack.Enqueue(findPos);

        Vector2 worldPos;
        List<Vector2> insidePositions = new();
        //List<Vector2> visitedCells = new();
        Dictionary<Vector2, int> cellVisitCount = new();

        iterations = 0;

        while (callStack.Count > 0 && iterations <= area.GetLength(0) * area.GetLength(1))
        {
            worldPos = callStack.Dequeue();
            //visitedCells.Add(worldPos);

            if (cellVisitCount.ContainsKey(worldPos))
                continue;// cellVisitCount[worldPos]++;
            else
                cellVisitCount.Add(worldPos, 1);

            // mark cell as inside
            if (Grid.CheckCell(worldPos, 0) || Grid.CheckCell(worldPos, 9))
            {
                insidePositions.Add(worldPos);
                Grid.SetGrid(worldPos, 1);

                Vector2Int areaPos = (worldPos - translate).RoundToInt();
                area[areaPos.x, areaPos.y] = 3;

                // add left cell
                if (!cellVisitCount.ContainsKey(new Vector2(worldPos.x - 1, worldPos.y)) && !callStack.Contains(new Vector2(worldPos.x - 1, worldPos.y)))
                    callStack.Enqueue(new Vector2(worldPos.x - 1, worldPos.y));

                // add right cell
                if (!cellVisitCount.ContainsKey(new Vector2(worldPos.x + 1, worldPos.y)) && !callStack.Contains(new Vector2(worldPos.x + 1, worldPos.y)))
                    callStack.Enqueue(new Vector2(worldPos.x + 1, worldPos.y));

                // add up cell
                if (!cellVisitCount.ContainsKey(new Vector2(worldPos.x, worldPos.y + 1)) && !callStack.Contains(new Vector2(worldPos.x, worldPos.y + 1)))
                    callStack.Enqueue(new Vector2(worldPos.x, worldPos.y + 1));

                // add down cell
                if (!cellVisitCount.ContainsKey(new Vector2(worldPos.x, worldPos.y - 1)) && !callStack.Contains(new Vector2(worldPos.x, worldPos.y - 1)))
                    callStack.Enqueue(new Vector2(worldPos.x, worldPos.y - 1));
            }

            iterations++;
        }

        //Debug.Log($"Iterations Inside: {iterations}");

        //List<Vector2Int> removedDyingCells = new();
        // add revivable cells in 1 block areas
        if(GameObject.FindGameObjectWithTag("Main").GetComponent<TileManager>().isStalled())
        {
            foreach (var cell in GameObject.FindGameObjectWithTag("Main").GetComponent<TileManager>().dyingList)
            {
                if (Grid.CheckCell(Grid.GridToWorld(cell), 9) && !Grid.CheckNeighbor(Grid.GridToWorld(cell), 0))
                {
                    //removedDyingCells.Add(cell);
                    insidePositions.Add(Grid.GridToWorld(cell));
                    Grid.SetGrid(Grid.GridToWorld(cell), 1);
                }
                else if (Grid.CheckCell(Grid.GridToWorld(cell), 3))
                {
                    Grid.SetGrid(Grid.GridToWorld(cell), 9);
                }
            }
        }

        //foreach (var cell in removedDyingCells)
        //{
        //    GameObject.FindGameObjectWithTag("Main").GetComponent<TileManager>().dyingList.Remove(cell);
        //}

        return insidePositions;
    }

    public Vector2 FindInsidePosition(Vector2 pos)
    {
        Vector2 worldPos = pos;
        Vector2Int areaPos = (worldPos - translate).RoundToInt();

        //if (area[areaPos.x - 1, areaPos.y] == 0 && area[areaPos.x + 1, areaPos.y] == 2)
        //    return worldPos.SetX(worldPos.x - 1);

        //if (area[areaPos.x + 1, areaPos.y] == 0 && area[areaPos.x - 1, areaPos.y] == 2)
        //    return worldPos.SetX(worldPos.x + 1);

        //if (area[areaPos.x, areaPos.y - 1] == 0 && area[areaPos.x, areaPos.y + 1] == 2)
        //    return worldPos.SetY(worldPos.y - 1);

        //if (area[areaPos.x, areaPos.y + 1] == 0 && area[areaPos.x, areaPos.y - 1] == 2)
        //    return worldPos.SetY(worldPos.y + 1);

        if ((Grid.CheckCell(new Vector2(worldPos.x - 1, worldPos.y), 0) || Grid.CheckCell(new Vector2(worldPos.x - 1, worldPos.y), 9)) && area[areaPos.x - 1, areaPos.y] != 3 && area[areaPos.x - 1, areaPos.y] != 2 && area[areaPos.x + 1, areaPos.y] == 2)
        {
            return worldPos.SetX(worldPos.x - 1);
        }

        if ((Grid.CheckCell(new Vector2(worldPos.x + 1, worldPos.y), 0) || Grid.CheckCell(new Vector2(worldPos.x + 1, worldPos.y), 9)) && area[areaPos.x + 1, areaPos.y] != 3 && area[areaPos.x + 1, areaPos.y] != 2 && area[areaPos.x - 1, areaPos.y] == 2)
        {
            return worldPos.SetX(worldPos.x + 1);
        }
        
        if((Grid.CheckCell(new Vector2(worldPos.x, worldPos.y - 1), 0) || Grid.CheckCell(new Vector2(worldPos.x, worldPos.y - 1), 0)) && area[areaPos.x, areaPos.y - 1] != 3 && area[areaPos.x, areaPos.y - 1] != 2 && area[areaPos.x, areaPos.y + 1] == 2)
        {
            return worldPos.SetY(worldPos.y - 1);
        }
        
        if((Grid.CheckCell(new Vector2(worldPos.x, worldPos.y + 1), 0) || Grid.CheckCell(new Vector2(worldPos.x, worldPos.y + 1), 9)) && area[areaPos.x, areaPos.y + 1] != 3 && area[areaPos.x, areaPos.y + 1] != 2 && area[areaPos.x, areaPos.y - 1] == 2)
        {
            return worldPos.SetY(worldPos.y + 1);
        }

        return worldPos;
    }
}
