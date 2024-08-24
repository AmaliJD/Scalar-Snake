using EX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    // Wait Time
    float waitTime = 0.3f;
    WaitForSeconds wait;

    float[] waitTimes = new[] { .25f, .2f, .16f, .14f, .125f, .1f, .08f, .075f, .05f};
    int waitTimeIndex = 0;

    // Player Input
    PlayerInput playerInput;
    InputAction inputAction;

    // Player
    [SerializeField] Transform player;

    // Sfx
    [SerializeField] AudioClip playerMoveSfx;
    [SerializeField] AudioClip fillSfx;
    [SerializeField] AudioClip explosionSfx;
    [SerializeField] AudioClip powerupSfx;

    // References
    TileManager tileManager;
    //static Fill fill = new();
    Main main;
    PowerupManager powerupManager;

    // Data
    Vector2 moveInput;
    Vector2 moveDirection;
    Vector2 currDirection;
    Vector2 prevDirection;
    Vector2 prevPosition;
    int numberOfFills;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        inputAction = playerInput.actions["Movement"];
        SetWaitTime(waitTime);

        tileManager = GetComponent<TileManager>();
        tileManager.InstantiateTiles();
        prevPosition = player.position;

        Grid.SetGrid(Vector2.zero, 1);
        tileManager.DropBlock(Vector2.zero);
        //Fill.fill.ClearMinMax();

        main = GetComponent<Main>();
        main.GetUI().SetMapPixels(true);

        powerupManager = GetComponent<PowerupManager>();
    }

    private void Start()
    {
        StartCoroutine(Step());
    }

    private void Update()
    {
        if(main.gameState == Main.GameState.Intro || main.gameState == Main.GameState.Game)
            InputDetect();
    }

    void InputDetect()
    {
        moveInput = inputAction.ReadValue<Vector2>().normalized.Truncate();

        if (moveInput != Vector2.zero && moveInput != -currDirection && !Grid.CheckCell(player.position.XY() + moveInput, -1))
            moveDirection = moveInput;
    }

    bool PlayState() => main.gameState == Main.GameState.Game || main.gameState == Main.GameState.Intro;

    IEnumerator Step()
    {
        while (true)
        {
            //bool playState = main.gameState == Main.GameState.Game || main.gameState == Main.GameState.Intro;
            if (PlayState())
            {
                MovePlayer();
                DetectPowerup();
                DropTrail();
                FillArea();
                prevPosition = player.position;

                //Debug.Log($"MoveInput: {moveInput}, MoveDirection: {moveDirection}, {Fill.fill.PrintMinMax()}");
                yield return wait;
            }
            else
            {
                yield return new WaitUntil(() => PlayState());
            }
        }
    }

    void MovePlayer()
    {
        if (Grid.CheckCell(player.position.XY() + moveDirection, -1) || moveDirection == Vector2.zero)
        {
            if (Grid.CheckCell(player.position.XY(), 1))
            {
                moveDirection = Vector2.zero;
                currDirection = Vector2.zero;
            }

            return;
        }

        if (Grid.CheckCell(player.position.XY() + moveDirection, 2))
        {
            main.gameState = Main.GameState.Death;
            SfxManager.instance.PlaySfxClip(explosionSfx, 1f, 1, player.position);
        }

        prevDirection = currDirection;
        player.Translate(moveDirection);
        currDirection = moveDirection;

        if (!Grid.CheckNeighbor8Way(player.position.XY(), 1) && Grid.CheckNeighbor8Way(prevPosition, 1))
            timeLiftedDuringTrail++;

        main.GetUI().SetPlayerPosition(player.position);

        if (moveDirection !=  Vector2.zero)
            SfxManager.instance.PlaySfxClip(playerMoveSfx, 1f, 1, player.position);
    }

    void DropTrail()
    {
        if (!(prevPosition != player.position.XY() && Grid.GetGrid(prevPosition) == 0))
            return;
        
        tileManager.DropTrail(prevPosition, prevDirection, currDirection);

        Grid.SetGrid(prevPosition, 2);

        Fill.fill.drawing = true;
    }

    int timeLiftedDuringTrail = -1;

    void FillArea()
    {
        /*if (!Grid.CheckNeighbor(player.position, 1))
            return;*/

        Fill.fill.minX = MathEX.Min(Fill.fill.minX, (int)player.position.x);
        Fill.fill.maxX = MathEX.Max(Fill.fill.maxX, (int)player.position.x);
        Fill.fill.minY = MathEX.Min(Fill.fill.minY, (int)player.position.y);
        Fill.fill.maxY = MathEX.Max(Fill.fill.maxY, (int)player.position.y);

        if (!(Grid.CheckCell(player.position, 1) && Fill.fill.drawing))
            return;

        if (tileManager.trailList.Count == 0)
            return;

        SfxManager.instance.PlaySfxClip(fillSfx, 1f, 1, player.position);

        // break init
        if(numberOfFills == 0)
        {
            tileManager.BreakInitWall();
            numberOfFills++;

            main.gameState = Main.GameState.Game;
        }

        int trailCount = tileManager.trailList.Count;
        List<Vector2> trailPositions = tileManager.trailList.Select(x => (Vector2)x.transform.position).ToList();
        for (int i = 0; i < trailCount; i++)
        {
            GameObject obj = tileManager.trailList[0];
            tileManager.trailList.RemoveAt(0);
            Vector2 pos = obj.transform.position;
            //Destroy(obj);
            tileManager.DropBlock(pos);
            Grid.SetGrid(pos, 1);
        }

        Fill.fill.SelectArea();
        List<Vector2> outsidePositions = Fill.fill.FloodFillOutside();
        List<Vector2> insidePositions = Fill.fill.FloodFillInside(trailPositions, true);

        for(int i = 0; i < timeLiftedDuringTrail-1; i++)
            insidePositions.AddRange(Fill.fill.FloodFillInside(trailPositions));

        //Debug.Log($"Times Lifted: {timeLiftedDuringTrail}");

        timeLiftedDuringTrail = 0;

        foreach (Vector2 pos in outsidePositions)
        {
            if(Grid.CheckNeighbor8Way(pos, 1))
            {
                tileManager.DropCheck(pos);
            }
        }

        foreach (Vector2 pos in insidePositions)
            tileManager.DropBlock(pos);

        main.incrementBlocksCaptured(insidePositions.Count + trailCount);
        main.GetUI().SetMapPixels();

        moveDirection = Vector2.zero;
        currDirection = Vector2.zero;
        Fill.fill.drawing = false;
    }

    void DetectPowerup()
    {
        if (powerupManager.GetPowerupHolder().Find($"[{player.position.x},{player.position.y}]") == null)
            return;

        Powerup PU = powerupManager.GetPowerupHolder().Find($"[{player.position.x},{player.position.y}]").GetComponent<Powerup>();

        if (PU.got)
            return;
        
        switch(PU.powerType)
        {
            case Powerup.PowerupType.Speed:
                if(waitTimeIndex < waitTimes.Count())
                {
                    SetWaitTime(waitTimes[waitTimeIndex]);
                    waitTimeIndex++;
                }
                //EffectNotification effNotif = Instantiate(powerupManager.effectNotitication, powerupManager.notificationsParent).GetComponent<EffectNotification>();
                //effNotif.text.color = powerupManager.notificationColors[0];
                //effNotif.text.text = "Speed Up+";
                break;

            case Powerup.PowerupType.Threshold:
                main.LowerCapturethreshold();
                main.GetUI().PulseThresholdBar();
                //effNotif = Instantiate(powerupManager.effectNotitication, powerupManager.notificationsParent).GetComponent<EffectNotification>();
                //effNotif.text.color = powerupManager.notificationColors[2];
                //effNotif.text.text = "Threshold Lowered!";
                break;

            case Powerup.PowerupType.Stall:
                tileManager.Stall();
                //effNotif = Instantiate(powerupManager.effectNotitication, powerupManager.notificationsParent).GetComponent<EffectNotification>();
                //effNotif.text.color = powerupManager.notificationColors[1];
                //effNotif.text.text = "Virus Stopped";
                break;
        }

        PU.got = true;
        PU.transform.DOScale(Vector3.zero, .3f).SetEase(Ease.InBack).OnComplete(() => Destroy(PU.gameObject));
        Destroy(powerupManager.GetPowerupMapHolder().Find($"[{player.position.x},{player.position.y}]").gameObject);

        SfxManager.instance.PlaySfxClip(powerupSfx, 1f, 1, player.position);
    }

    public void SetWaitTime(float value)
    {
        waitTime = value;
        wait = new WaitForSeconds(waitTime);
    }

    public float GetWaitTime() => waitTime;
    public Transform GetPlayer() => player;
    public float StepTime() => waitTime;

    public Vector2 GetCurrentDirection() => currDirection;
    public Vector2 GetPreviousDirection() => prevDirection;

    public bool FullSpeed() => waitTimeIndex >= waitTimes.Count();
    public bool HighSpeed() => waitTimeIndex >= waitTimes.Count() - 1;
}
