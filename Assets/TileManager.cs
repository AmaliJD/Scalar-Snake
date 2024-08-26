using EX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TileManager : MonoBehaviour
{
    // Wait Time
    float waitTime = 1.75f;
    int[] blockMilestone = new[] { 40, 100, 200, 500, 800, 1000, 4000, 6000, 8000 };
    float[] waitTimes = new[] { 1.5f, 1.25f, 1f, .8f, .7f, .5f, .4f, .3f, .25f };
    int blockMilestoneIndex = 0;

    float timeOfLastSpawn;
    float waitSpawnTime = 120f;
    float waitFirstSpawn = 4f;
    WaitForSeconds wait;

    bool stall, stalled;
    const float stallTime = 5;

    float timeOfLastVirusAudio;
    const float TIME_BETWEEN_VIRUS_AUDIO = .1f;

    // Main
    Main main;

    // Player
    PlayerMovement player;

    // Sfx
    [SerializeField] AudioClip virusSpreadSfx;

    // Trail Textures
    [SerializeField] Sprite horizontal, vertical, cornerUpLeft, cornerUpRight, cornerDownLeft, cornerDownRight;

    [SerializeField] Transform tileParent;
    [SerializeField] GameObject tileObj;

    private Transform[,] tileGrid = new Transform[Grid.gridSize, Grid.gridSize];
    [HideInInspector] public List<GameObject> trailList = new();
    [HideInInspector] public List<Vector2Int> dyingList = new();

    Vector2[] initWallPositions = new Vector2[]
    {
        new Vector2(-1,-1),
        new Vector2(0,-1),
        new Vector2(1,-1),
        new Vector2(2,-1),
        new Vector2(3,-1),
        new Vector2(4,-1),
        new Vector2(5,-1),
        new Vector2(6,-1),

        new Vector2(-1,0),
        new Vector2(6,0),

        new Vector2(-1,1),
        new Vector2(6,1),

        new Vector2(-1,2),
        new Vector2(6,2),

        new Vector2(-1,3),
        new Vector2(6,3),

        new Vector2(-1,4),
        new Vector2(6,4),

        new Vector2(-1,5),
        new Vector2(0,5),
        new Vector2(1,5),
        new Vector2(2,5),
        new Vector2(3,5),
        new Vector2(4,5),
        new Vector2(5,5),
        new Vector2(6,5),
    };

    private void Awake()
    {
        SetWaitTime(waitTime);

        main = GetComponent<Main>();
        player = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        StartCoroutine(Step());
    }

    IEnumerator Step()
    {
        while (true)
        {
            if (blockMilestoneIndex < blockMilestone.Count() && main.GetHighestBlockCount() > blockMilestone[blockMilestoneIndex])
            {
                SetWaitTime(waitTimes[blockMilestoneIndex]);
                blockMilestoneIndex++;
            }
            if(main.GetHighestBlockCount() < 50 && main.GetTime() > 30 && blockMilestoneIndex == 0)
            {
                blockMilestoneIndex++;
                blockMilestoneIndex++;
                SetWaitTime(waitTimes[blockMilestoneIndex]);
            }

            if (main.GetBlocksCaptured() <= 1 && dyingList.Count == 0)
            yield return new WaitUntil(() => main.GetBlocksCaptured() > 1);

            if (main.gameState == Main.GameState.Death || main.gameState == Main.GameState.End)
                SetWaitTime(.05f);

            if (dyingList.Count > 0)
            {
                yield return wait;
            }
            else
            {
                yield return new WaitForSeconds(waitFirstSpawn);
            }

            if (stall)
            {
                stalled = true;
                stall = false;
                foreach (var element in dyingList)
                {
                    // mark tile as revivable
                    tileParent.Find($"[{element.x},{element.y}]").GetChild(3).GetComponent<SpriteRenderer>().color = Color.white;
                    Grid.SetGrid(new Vector2(element.x, element.y), 9, true);
                }

                yield return new WaitForSeconds(stallTime);

                //foreach (var element in dyingList)
                //    tileParent.Find($"[{element.x},{element.y}]").GetChild(3).GetComponent<SpriteRenderer>().color = Color.red;
            }

            if (!stall)
            {
                if(stalled)
                {
                    stalled = false;

                    foreach (var element in dyingList)
                        tileParent.Find($"[{element.x},{element.y}]").GetChild(3).GetComponent<SpriteRenderer>().color = Color.red;
                }

                if (dyingList.Count == 0)
                {
                    int i, j;
                    do
                    {
                        i = UnityEngine.Random.Range(0, Grid.gridSize);
                        j = UnityEngine.Random.Range(0, Grid.gridSize);
                    }
                    while (!Grid.CheckCell(new Vector2(i, j), 1, true));

                    //EnableTile(i, j, 2);
                    DropDying(new Vector2Int(i, j));
                    Grid.SetGrid(new Vector2(i, j), 3, true);

                    if (Time.time > timeOfLastVirusAudio + TIME_BETWEEN_VIRUS_AUDIO)
                    {
                        SfxManager.instance.PlaySfxClip(virusSpreadSfx, 1f, 1, Grid.GridToWorld(new Vector2(i, j)));
                        timeOfLastVirusAudio = Time.time;
                    }

                    timeOfLastSpawn = Time.time;

                    dyingList.Add(new Vector2Int(i, j));
                    main.GetUI().SetMapPixels();
                }
                else // Find next dying cells
                {
                    // SPAWN SECOND VIRUS - disabled cuz that's unnessesary, game's already hard enough - nvm reenabled at 5K
                    if (Time.time > timeOfLastSpawn + waitSpawnTime && main.GetBlocksCaptured() > 5000)
                    {
                        timeOfLastSpawn = Time.time;
                        int i, j;
                        do
                        {
                            i = UnityEngine.Random.Range(0, Grid.gridSize);
                            j = UnityEngine.Random.Range(0, Grid.gridSize);
                        }
                        while (!Grid.CheckCell(new Vector2(i, j), 1, true));

                        //EnableTile(i, j, 2);
                        DropDying(new Vector2Int(i, j));
                        Grid.SetGrid(new Vector2(i, j), 3, true);

                        /*if (Time.time > timeOfLastVirusAudio + TIME_BETWEEN_VIRUS_AUDIO)
                        {
                            SfxManager.instance.PlaySfxClip(virusSpreadSfx, 1f, 1, Grid.GridToWorld(new Vector2(i, j)));
                            timeOfLastVirusAudio = Time.time;
                        }*/

                        dyingList.Add(new Vector2Int(i, j));
                        main.GetUI().SetMapPixels();
                    }

                    List<Vector2Int> deadList = new(dyingList);
                    dyingList.Clear();

                    foreach (var pos in deadList)
                    {
                        // revived cell
                        if (Grid.CheckCell(new Vector2(pos.x, pos.y), 1, true) || Grid.CheckCell(new Vector2(pos.x, pos.y), 0, true))
                        {
                            //dyingList.Remove(new Vector2Int(pos.x, pos.y));
                            continue;
                        }
                            
                        DropEmpty(pos);
                        Grid.SetGrid(new Vector2(pos.x, pos.y), 0, true);

                        // check left
                        if (Grid.CheckCell(new Vector2(pos.x - 1, pos.y), 1, true) && !dyingList.Contains(new Vector2Int(pos.x - 1, pos.y)))
                        {
                            dyingList.Add(new Vector2Int(pos.x - 1, pos.y));
                            DropDying(new Vector2Int(pos.x - 1, pos.y));
                            Grid.SetGrid(new Vector2(pos.x - 1, pos.y), 3, true);
                        }

                        // check right
                        if (Grid.CheckCell(new Vector2(pos.x + 1, pos.y), 1, true) && !dyingList.Contains(new Vector2Int(pos.x + 1, pos.y)))
                        {
                            dyingList.Add(new Vector2Int(pos.x + 1, pos.y));
                            DropDying(new Vector2Int(pos.x + 1, pos.y));
                            Grid.SetGrid(new Vector2(pos.x + 1, pos.y), 3, true);
                        }

                        // check up
                        if (Grid.CheckCell(new Vector2(pos.x, pos.y + 1), 1, true) && !dyingList.Contains(new Vector2Int(pos.x, pos.y + 1)))
                        {
                            dyingList.Add(new Vector2Int(pos.x, pos.y + 1));
                            DropDying(new Vector2Int(pos.x, pos.y + 1));
                            Grid.SetGrid(new Vector2(pos.x, pos.y + 1), 3, true);
                        }

                        // check down
                        if (Grid.CheckCell(new Vector2(pos.x, pos.y - 1), 1, true) && !dyingList.Contains(new Vector2Int(pos.x, pos.y - 1)))
                        {
                            dyingList.Add(new Vector2Int(pos.x, pos.y - 1));
                            DropDying(new Vector2Int(pos.x, pos.y - 1));
                            Grid.SetGrid(new Vector2(pos.x, pos.y - 1), 3, true);
                        }
                    }

                    if (deadList.Count > 0)
                    {
                        if (Time.time > timeOfLastVirusAudio + TIME_BETWEEN_VIRUS_AUDIO)
                        {
                            SfxManager.instance.PlaySfxClip(virusSpreadSfx, 1f, 1, Grid.GridToWorld(deadList.OrderBy(x => Vector2.Distance(x, (Vector2)player.GetPlayer().position)).First()));
                            timeOfLastVirusAudio = Time.time;
                        }
                    }

                    main.incrementBlocksCaptured(-deadList.Count);
                    main.GetUI().SetMapPixels();
                }

                // Setup Risk Tiles
                List<Vector2Int> riskList = new();
                foreach (var pos in dyingList)
                {
                    // check left
                    if (Grid.CheckCell(new Vector2(pos.x - 1, pos.y), 1, true) && !riskList.Contains(new Vector2Int(pos.x - 1, pos.y)))
                    {
                        riskList.Add(new Vector2Int(pos.x - 1, pos.y));
                        DropRisk(new Vector2Int(pos.x - 1, pos.y));
                    }

                    // check right
                    if (Grid.CheckCell(new Vector2(pos.x + 1, pos.y), 1, true) && !riskList.Contains(new Vector2Int(pos.x + 1, pos.y)))
                    {
                        riskList.Add(new Vector2Int(pos.x + 1, pos.y));
                        DropRisk(new Vector2Int(pos.x + 1, pos.y));
                    }

                    // check up
                    if (Grid.CheckCell(new Vector2(pos.x, pos.y + 1), 1, true) && !riskList.Contains(new Vector2Int(pos.x, pos.y + 1)))
                    {
                        riskList.Add(new Vector2Int(pos.x, pos.y + 1));
                        DropRisk(new Vector2Int(pos.x, pos.y + 1));
                    }

                    // check down
                    if (Grid.CheckCell(new Vector2(pos.x, pos.y - 1), 1, true) && !riskList.Contains(new Vector2Int(pos.x, pos.y - 1)))
                    {
                        riskList.Add(new Vector2Int(pos.x, pos.y - 1));
                        DropRisk(new Vector2Int(pos.x, pos.y - 1));
                    }
                }
            }

            //if(waitTime < .25f)
            //{
            //    Debug.Log("HELP");
            //}
        }
    }

    public void DropTrail(Vector3 position, bool addToList = true)
    {
        if (Grid.CheckCell(position, -1))
            return;

        Vector2Int gridPos = Grid.WorldToGrid(position).RoundToInt();

        if (addToList)
        {
            trailList.Add(tileGrid[gridPos.x, gridPos.y].gameObject);
        }
        
        tileGrid[gridPos.x, gridPos.y].localScale = Vector2.one;
        EnableTile(gridPos.x, gridPos.y, 1);
        tileGrid[gridPos.x, gridPos.y].gameObject.SetActive(true);
    }

    public void DropTrail(Vector3 position, Vector2 prevDirection, Vector2 currDirection, bool addToList = true)
    {
        if (Grid.CheckCell(position, -1))
            return;

        Vector2Int gridPos = Grid.WorldToGrid(position).RoundToInt();

        if (addToList)
            trailList.Add(tileGrid[gridPos.x, gridPos.y].gameObject);

        tileGrid[gridPos.x, gridPos.y].localScale = Vector2.one;
        EnableTile(gridPos.x, gridPos.y, 1);
        tileGrid[gridPos.x, gridPos.y].gameObject.SetActive(true);

        if ((prevDirection == Vector2.right || prevDirection == Vector2.left) && currDirection == prevDirection)
            tileGrid[gridPos.x, gridPos.y].GetChild(1).GetComponent<SpriteRenderer>().sprite = horizontal;
        else if ((prevDirection == Vector2.up || prevDirection == Vector2.down) && currDirection == prevDirection)
            tileGrid[gridPos.x, gridPos.y].GetChild(1).GetComponent<SpriteRenderer>().sprite = vertical;
        else if ((prevDirection == Vector2.up && currDirection == Vector2.right) || (prevDirection == Vector2.left && currDirection == Vector2.down))
            tileGrid[gridPos.x, gridPos.y].GetChild(1).GetComponent<SpriteRenderer>().sprite = cornerUpLeft;
        else if ((prevDirection == Vector2.up && currDirection == Vector2.left) || (prevDirection == Vector2.right && currDirection == Vector2.down))
            tileGrid[gridPos.x, gridPos.y].GetChild(1).GetComponent<SpriteRenderer>().sprite = cornerUpRight;
        else if ((prevDirection == Vector2.down && currDirection == Vector2.right) || (prevDirection == Vector2.left && currDirection == Vector2.up))
            tileGrid[gridPos.x, gridPos.y].GetChild(1).GetComponent<SpriteRenderer>().sprite = cornerDownLeft;
        else if ((prevDirection == Vector2.down && currDirection == Vector2.left) || (prevDirection == Vector2.right && currDirection == Vector2.up))
            tileGrid[gridPos.x, gridPos.y].GetChild(1).GetComponent<SpriteRenderer>().sprite = cornerDownRight;
    }

    public void DropCheck(Vector3 position)
    {
        if (Grid.CheckCell(position, -1))
            return;

        Vector2Int gridPos = Grid.WorldToGrid(position).RoundToInt();
        tileGrid[gridPos.x, gridPos.y].localScale = Vector2.one;
        EnableTile(gridPos.x, gridPos.y, 4);
        tileGrid[gridPos.x, gridPos.y].gameObject.SetActive(true);
    }

    public void DropBlock(Vector3 position)
    {
        if (Grid.CheckCell(position, -1))
            return;

        Vector2Int gridPos = Grid.WorldToGrid(position).RoundToInt();
        //tileGrid[gridPos.x, gridPos.y].localScale = Vector2.one;
        tileGrid[gridPos.x, gridPos.y].GetComponent<TileProperties>().TweenScale(Vector3.zero, Vector3.one, .5f);
        EnableTile(gridPos.x, gridPos.y, 0);
        tileGrid[gridPos.x, gridPos.y].gameObject.SetActive(true);
    }

    private void DropDying(Vector2Int gridPos)
    {
        //Vector2Int gridPos = Grid.WorldToGrid(position).RoundToInt();
        tileGrid[gridPos.x, gridPos.y].localScale = Vector2.one;
        EnableTile(gridPos.x, gridPos.y, 3);
        tileGrid[gridPos.x, gridPos.y].gameObject.SetActive(true);
    }

    private void DropRisk(Vector2Int gridPos)
    {
        //Vector2Int gridPos = Grid.WorldToGrid(position).RoundToInt();
        tileGrid[gridPos.x, gridPos.y].localScale = Vector2.one;
        EnableTile(gridPos.x, gridPos.y, 2);
        tileGrid[gridPos.x, gridPos.y].gameObject.SetActive(true);
    }

    private void DropEmpty(Vector2Int gridPos)
    {
        //Vector2Int gridPos = Grid.WorldToGrid(position).RoundToInt();
        tileGrid[gridPos.x, gridPos.y].localScale = Vector2.zero;
        EnableTile(gridPos.x, gridPos.y, -1);
        tileGrid[gridPos.x, gridPos.y].gameObject.SetActive(false);
    }

    public void DropWall(Vector3 position)
    {
        Vector2Int gridPos = Grid.WorldToGrid(position).RoundToInt();
        tileGrid[gridPos.x, gridPos.y].localScale = Vector2.one;
        EnableTile(gridPos.x, gridPos.y, 5);
        tileGrid[gridPos.x, gridPos.y].gameObject.SetActive(true);
    }

    private void DropWall(Vector2Int gridPos)
    {
        //Vector2Int gridPos = Grid.WorldToGrid(position).RoundToInt();
        tileGrid[gridPos.x, gridPos.y].localScale = Vector2.one;
        EnableTile(gridPos.x, gridPos.y, 5);
        tileGrid[gridPos.x, gridPos.y].gameObject.SetActive(true);
    }

    void EnableTile(int x, int y, int i)
    {
        tileGrid[x, y].GetChild(0).gameObject.SetActive(i == 0); // Fill
        tileGrid[x, y].GetChild(1).gameObject.SetActive(i == 1); // Trail
        tileGrid[x, y].GetChild(2).gameObject.SetActive(i == 2); // Risk
        tileGrid[x, y].GetChild(3).gameObject.SetActive(i == 3); // Dying
        tileGrid[x, y].GetChild(4).gameObject.SetActive(i == 4); // Check
        tileGrid[x, y].GetChild(5).gameObject.SetActive(i == 5); // Wall
    }

    public void InstantiateTiles()
    {
        for (int i = 0; i < Grid.grid.GetLength(0); i++)
        {
            for (int j = 0; j < Grid.grid.GetLength(1); j++)
            {
                GameObject obj = Instantiate(tileObj, Grid.GridToWorld(new Vector2(i, j)), Quaternion.identity, tileParent);
                obj.transform.localScale = Vector2.zero;
                obj.name = $"[{i},{j}]";
                obj.SetActive(false);

                tileGrid[i, j] = obj.transform;

                if(initWallPositions.Contains(Grid.GridToWorld(new Vector2(i, j))))
                {
                    Grid.SetGrid(new Vector2(i, j), -1, true);
                    DropWall(new Vector2Int(i, j));
                }
            }
        }
    }

    public void BreakInitWall()
    {
        foreach(Vector2 pos in initWallPositions)
        {
            Grid.SetGrid(pos, 0);
            DropEmpty(Grid.WorldToGrid(new Vector2Int((int)pos.x, (int)pos.y)).RoundToInt());
            main.PlayBGMusic();
        }
    }

    public void SetWaitTime(float value)
    {
        waitTime = value;
        wait = new WaitForSeconds(waitTime);
    }

    public float GetWaitTime() => waitTime;

    public void Stall()
    {
        stall = true;

        foreach (var element in dyingList)
            tileParent.Find($"[{element.x},{element.y}]").GetChild(3).GetComponent<SpriteRenderer>().color = Color.white;
    }

    public bool isStalled() => stalled;
    public int MilestoneIndex() => blockMilestoneIndex;
    public bool FullSpeed() => blockMilestoneIndex >= waitTimes.Count();
}
