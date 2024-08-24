using EX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PowerupManager : MonoBehaviour
{
    [SerializeField] GameObject powerup;
    [SerializeField] GameObject powerupMarker;
    [SerializeField] Transform powerupParent;
    [SerializeField] Transform powerupMapParent;

    public GameObject effectNotitication;
    public Transform notificationsParent;
    public Color[] notificationColors;

    // References
    Main main;
    PlayerMovement player;
    TileManager tileManager;

    // Wait
    float waitTime = 10;
    WaitForSeconds wait;
    int maxPowerups = 3;

    private void Awake()
    {
        main = GetComponent<Main>();
        player = GetComponent<PlayerMovement>();
        tileManager = GetComponent<TileManager>();
        SetWaitTime(waitTime);
    }

    private void Start()
    {
        StartCoroutine(Step());
    }

    bool StepState() => main.gameState == Main.GameState.Game;
    IEnumerator Step()
    {
        while (true)
        {
            if(StepState())
            {
                yield return wait;

                if (powerupParent.childCount < maxPowerups && main.gameState == Main.GameState.Game)
                    SpawnPowerup();

            }
            else
            {
                yield return new WaitUntil(() => StepState());
            }
        }
    }

    void SpawnPowerup()
    {
        Vector2 gridPos = Grid.WorldToGrid(player.GetPlayer().position);
        int revX = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;
        int revY = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;
        int radius = -(MathEX.Remap(.1f, .3f, -12, -5, player.GetWaitTime()).FloorToInt());
        int posX = Mathf.Clamp((int)gridPos.x + (revX * radius), 1, Grid.gridSize - 2);
        int posY = Mathf.Clamp((int)gridPos.y + (revY * radius), 1, Grid.gridSize - 2);

        Vector2 spawnPosition = Grid.GridToWorld(new Vector2(posX, posY));

        Powerup PU = Instantiate(powerup, spawnPosition, Quaternion.identity, powerupParent).GetComponent<Powerup>();
        PU.name = $"[{spawnPosition.x},{spawnPosition.y}]";
        PU.powerType = (Powerup.PowerupType)CalculatePowerupSpawnRates();
        PU.Init();

        GameObject marker = Instantiate(powerupMarker, spawnPosition, Quaternion.identity, powerupMapParent);
        marker.GetComponent<RectTransform>().localPosition = spawnPosition * 3;
        marker.name = $"[{spawnPosition.x},{spawnPosition.y}]";
    }

    int CalculatePowerupSpawnRates() // 0 - speed, 1 - stall, 2 - threshold
    {
        float percentageCaptured = ((float)main.GetBlocksCaptured()) / ((float)main.GetHighestBlockCount());
        if (main.GetThresholdPercentage() > .7f && percentageCaptured < .8f && percentageCaptured > .25f
            && main.GetHighestBlockCount() > 50)
        {
            return 2;
        }
        else if(main.GetThresholdPercentage() < .5f && percentageCaptured < .5f && main.GetBlocksCaptured() > 50 && !player.HighSpeed())
        {
            return 0;
        }
        else if (main.GetBlocksCaptured() < 40 && tileManager.MilestoneIndex() > 2)
        {
            return 1;
        }
        else if(main.GetThresholdPercentage() < .35f && percentageCaptured > .65f)
        {
            if (player.FullSpeed())
                return 1;
            else
                return UnityEngine.Random.Range(0, 2);
        }
        else
        {
            if (player.FullSpeed())
                return UnityEngine.Random.Range(1, 3);
            else
                return UnityEngine.Random.Range(0, 3);
        }
    }

    public Transform GetPowerupHolder() => powerupParent;
    public Transform GetPowerupMapHolder() => powerupMapParent;

    public void SetWaitTime(float value)
    {
        waitTime = value;
        wait = new WaitForSeconds(waitTime);
    }
}
