using EX;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        Intro,
        Game,
        Death,
        Paused,
        End,
        Restart
    }

    public GameState gameState;
    [SerializeField] UI ui;
    [SerializeField] AudioSource BGMusicSource;
    [SerializeField] AudioLowPassFilter BGMusicLowPass;
    [SerializeField] AudioClip explosionSfx;

    bool gameOn = true;

    int blocksCaptured = 0;
    int highestCaptureCount = 0;
    float captureThreshold = .25f;
    int captureThresholdValue;
    int biggestCapture = 0;
    float time = 0;

    const float SUDDEN_DEATH_TIME = 10;
    float suddenDeathTimer = SUDDEN_DEATH_TIME;

    [SerializeField]
    GameObject[] faces;

    PlayerMovement player;
    TileManager tileManager;

    GameState previousState;

    private void Awake()
    {
        Grid.grid = new int[Grid.gridSize, Grid.gridSize];

        ui = GameObject.FindGameObjectWithTag("UI").GetComponent<UI>();

        BGMusicSource.mute = true;
        BGMusicSource.Play();

        player = GetComponent<PlayerMovement>();
        tileManager = GetComponent<TileManager>();

        LoadPlayerPrefs();
    }

    private void Start()
    {
        gameState = GameState.Menu;
        previousState = gameState;
        incrementBlocksCaptured(1);
        BGMusicSource.Stop();
    }

    public void incrementBlocksCaptured(int num)
    {
        blocksCaptured += num;

        highestCaptureCount = Mathf.Max(highestCaptureCount, blocksCaptured);
        captureThresholdValue = (highestCaptureCount * captureThreshold).FloorToInt();

        ui.SetBlocksCapturedText(blocksCaptured, captureThresholdValue, highestCaptureCount);

        CheckCaptureSize(num);
    }

    void CheckCaptureSize(int size)
    {
        biggestCapture = Mathf.Max(biggestCapture, size);
    }

    void Restart()
    {
        gameState = GameState.Menu;
        gameOn = false;
        Time.timeScale = 1;
        SaveAudioPrefs();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.R))
        //    Restart();

        //if (Input.GetKeyDown(KeyCode.Z))
        //{
        //    player.SetWaitTime(Mathf.Clamp(player.GetWaitTime() - .05f, .075f, 1));
        //    Debug.Log($"New Wait Time: {player.GetWaitTime()}");
        //}

        //if (Input.GetKeyDown(KeyCode.X))
        //{
        //    tileManager.SetWaitTime(Mathf.Clamp(tileManager.GetWaitTime() - .5f, .3f, 10));
        //    Debug.Log($"New Spread Time: {tileManager.GetWaitTime()}");
        //}

        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    captureThreshold = .25f;
        //    ui.SetThreshold((highestCaptureCount * captureThreshold).FloorToInt());
        //}

        if (Input.GetKeyDown(KeyCode.Escape) && gameState != GameState.Paused && gameState != GameState.Death && gameState != GameState.Restart)
        {
            Time.timeScale = 0;
            previousState = gameState;
            gameState = GameState.Paused;
            return;
        }

        switch (gameState)
        {
            case GameState.Menu:
                if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape))
                {
                    gameState = GameState.Intro;
                    incrementBlocksCaptured(0);
                }
                break;
            case GameState.Intro:

                captureThreshold = Mathf.MoveTowards(captureThreshold, .75f, -MathEX.RemapClamped(3000, 7000, -.01f, -.005f, blocksCaptured) * Time.deltaTime);
                ui.SetThreshold((highestCaptureCount * captureThreshold).FloorToInt());
                break;
            case GameState.Game:
                BGMusicSource.volume = Mathf.MoveTowards(BGMusicSource.volume, 1f, 4 * Time.unscaledDeltaTime);
                BGMusicLowPass.cutoffFrequency = Mathf.MoveTowards(BGMusicLowPass.cutoffFrequency, 22000, 100000 * Time.unscaledDeltaTime);

                captureThreshold = Mathf.MoveTowards(captureThreshold, .75f, .01f * Time.deltaTime);
                ui.SetThreshold((highestCaptureCount * captureThreshold).FloorToInt());

                if (blocksCaptured < captureThresholdValue)
                {
                    suddenDeathTimer -= Time.deltaTime;
                    ui.SetSuddenDeathTimerText(suddenDeathTimer);
                }
                else if (suddenDeathTimer != SUDDEN_DEATH_TIME)
                {
                    suddenDeathTimer = SUDDEN_DEATH_TIME;
                    ui.SetSuddenDeathTimerText(suddenDeathTimer);
                }

                ui.SetTime(time, HS_timeAlive);

                if (suddenDeathTimer < 0 || blocksCaptured < 1)
                {
                    gameState = GameState.Death;
                    SfxManager.instance.PlaySfxClip(explosionSfx, 1f, 1, transform.position, false);
                }

                if(blocksCaptured >= captureThresholdValue && !faces[0].activeSelf)
                {
                    faces[0].SetActive(true);
                    faces[1].SetActive(false);
                    faces[2].SetActive(false);
                }
                else if(blocksCaptured < captureThresholdValue && !faces[1].activeSelf)
                {
                    faces[0].SetActive(false);
                    faces[1].SetActive(true);
                    faces[2].SetActive(false);
                }

                time += Time.deltaTime;

                HS_blocksCaptured = Mathf.Max(HS_blocksCaptured, highestCaptureCount);
                HS_biggestCapture = Mathf.Max(HS_biggestCapture, biggestCapture);
                HS_timeAlive = Mathf.Max(HS_timeAlive, time.FloorToInt());

                break;

            case GameState.Death:
                if(gameOn)
                {
                    SavePlayerPrefs();
                    SaveAudioPrefs();
                    gameOn = false;

                    faces[0].SetActive(false);
                    faces[1].SetActive(false);
                    faces[2].SetActive(true);
                }

                Invoke("ToEnd", .5f);
                break;

            case GameState.Paused:
                if (Input.GetKeyDown(KeyCode.R))
                {
                    gameState = GameState.Restart;
                    Invoke("Restart", .8f);
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Time.timeScale = 1;
                    gameState = previousState;
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    SaveAudioPrefs();
                    SavePlayerPrefs();
                    Application.Quit();
                }

                if(previousState != GameState.End)
                {
                    BGMusicSource.volume = Mathf.MoveTowards(BGMusicSource.volume, .5f, 4 * Time.unscaledDeltaTime);
                    BGMusicLowPass.cutoffFrequency = Mathf.MoveTowards(BGMusicLowPass.cutoffFrequency, 1800, 100000 * Time.unscaledDeltaTime);
                }
                
                break;

            case GameState.End:
                if (Input.GetKeyDown(KeyCode.R))
                {
                    gameState = GameState.Restart;
                    Invoke("Restart", .8f);
                }

                BGMusicSource.volume = Mathf.MoveTowards(BGMusicSource.volume, 0f, 1 * Time.unscaledDeltaTime);

                break;

            case GameState.Restart:
                if(BGMusicSource.isPlaying)
                    BGMusicSource.Stop();

                break;
        }
    }

    void ToEnd() => gameState = GameState.End;

    public void PlayBGMusic()
    {
        BGMusicSource.Stop();
        BGMusicSource.mute = false;
        BGMusicSource.Play();
    }

    public void LowerCapturethreshold()
    {
        captureThreshold = .15f;
        ui.SetThreshold((highestCaptureCount * captureThreshold).FloorToInt());
    }

    public float GetTime() => time;
    public int GetBlocksCaptured() => blocksCaptured;
    public int GetHighestBlockCount() => highestCaptureCount;
    public float GetThresholdPercentage() => captureThreshold;
    public int GetBiggestCapture() => biggestCapture;
    public UI GetUI() => ui;
    public PlayerMovement GetPlayer() => player;
    public int GetOldBlocksHS() => oldHS_blocksCaptured;
    public int GetOldCaptureHS() => oldHS_biggestCapture;
    public int GetOldTimeHS() => oldHS_timeAlive;
    public bool Warning() => blocksCaptured < captureThresholdValue;

    public AudioSource GetBGMusic() => BGMusicSource;

    int HS_blocksCaptured;
    int HS_biggestCapture;
    int HS_timeAlive;

    int oldHS_blocksCaptured;
    int oldHS_biggestCapture;
    int oldHS_timeAlive;

    private void SaveAudioPrefs()
    {
        PlayerPrefs.SetInt("music volume", ui.GetMusicVolume());
        PlayerPrefs.SetInt("sfx volume", ui.GetSfxVolume());
        PlayerPrefs.SetInt("muted", ui.GetMuted() ? 1 : 0);
    }

    private void SavePlayerPrefs()
    {
        //PlayerPrefs.SetInt("music volume", ui.GetMusicVolume());
        //PlayerPrefs.SetInt("sfx volume", ui.GetSfxVolume());
        //PlayerPrefs.SetInt("muted", ui.GetMuted() ? 1 : 0);

        if (HS_biggestCapture > 8500)
            return;

        PlayerPrefs.SetInt("HS_blocksCaptured", HS_blocksCaptured);
        PlayerPrefs.SetInt("HS_biggestCapture", HS_biggestCapture);
        PlayerPrefs.SetInt("HS_timeAlive", HS_timeAlive);
    }

    private void LoadPlayerPrefs()
    {
        HS_blocksCaptured = PlayerPrefs.GetInt("HS_blocksCaptured", 1);
        HS_biggestCapture = PlayerPrefs.GetInt("HS_biggestCapture", 0);
        HS_timeAlive = PlayerPrefs.GetInt("HS_timeAlive", 0);

        ui.SetMusicVolume(PlayerPrefs.GetInt("music volume", 50));
        ui.SetSfxVolume(PlayerPrefs.GetInt("sfx volume", 75));
        ui.SetMute(PlayerPrefs.GetInt("muted", 0) == 1);

        if (HS_blocksCaptured > 7000)
        {
            Debug.LogError("probably a bug to have this high of a score. Back to zero for you");
            HS_blocksCaptured = 0;
            HS_biggestCapture = 0;
            SavePlayerPrefs();
        }

        oldHS_blocksCaptured = HS_blocksCaptured;
        oldHS_biggestCapture = HS_biggestCapture;
        oldHS_timeAlive = HS_timeAlive;
    }
}
