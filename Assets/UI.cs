using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using EX;
using UnityEngine.Audio;
using static UnityEngine.Rendering.DebugUI;
using System.Threading;

public class UI : MonoBehaviour
{
    // Text
    [SerializeField] TextMeshProUGUI blocksCapturedText;
    [SerializeField] TextMeshProUGUI captureThresholdText;
    [SerializeField] TextMeshProUGUI suddenDeathTimer;
    [SerializeField] TextMeshProUGUI HS_BlocksCapturedText;
    [SerializeField] TextMeshProUGUI TimeText;
    [SerializeField] TextMeshProUGUI LargestCaptureTextText;

    // Slider
    [SerializeField] Slider blocksSlider;
    [SerializeField] Slider thresholdSlider;
    [SerializeField] Image thresholdBar;
    [SerializeField] Slider barSlider;
    [SerializeField] Slider suddenDeathSlider;

    // Image
    [SerializeField] Image warningSign;
    [SerializeField] GameObject suddenDeathObject;

    RectTransform blockSliderRT, thresholdSliderRT, barSliderRT;

    public float startSliderPosY, startWidth;

    [SerializeField] Texture2D MapTexture;
    [SerializeField] Texture2D BestMapTexture;
    public Color32[] colors; // fill, dying, null

    [SerializeField] RectTransform PlayerPositionOnTexture;

    int blocksCaptured, captureThreshold, highestCaptureCount;

    Main main;
    //public Fill fill;

    // Screens
    bool pauseScreen;

    [SerializeField] GameObject pauseMenu;
    [SerializeField] Slider musicSlider, sfxSlider;
    [SerializeField] TextMeshProUGUI musicValue, sfxValue;
    [SerializeField] TextMeshProUGUI mutedText;
    [SerializeField] AudioMixer audioMixer;
    bool muted;

    [SerializeField] RectTransform HS_Capture, HS_Blocks, HS_Time, HS_Map;
    [SerializeField] Vector2 hs_capture_scale_end;
    [SerializeField] Vector2 hs_capture_pos_end;
    [SerializeField] Vector2 hs_blocks_scale_end;
    [SerializeField] Vector2 hs_blocks_pos_end;
    [SerializeField] Vector2 hs_time_scale_end;
    [SerializeField] Vector2 hs_time_pos_end;
    [SerializeField] Vector2 hs_map_scale_end;
    [SerializeField] Vector2 hs_map_pos_end;
    [SerializeField] Image BGPanel;

    [SerializeField] RectTransform AnyKeyText;
    public GameObject restartText;

    private void Awake()
    {
        main = GameObject.FindGameObjectWithTag("Main").GetComponent<Main>();

        blockSliderRT = blocksSlider.GetComponent<RectTransform>();
        thresholdSliderRT = thresholdSlider.GetComponent<RectTransform>();
        barSliderRT = barSlider.GetComponent<RectTransform>();

        startSliderPosY = blockSliderRT.anchoredPosition.y;
        startWidth = blockSliderRT.sizeDelta.x;

        HS_BlocksCapturedText.text = 1.ToString();
        HS_BlocksCapturedText.fontSize = 80;

        blocksCapturedText.color = blocksCapturedText.color.SetAlpha(0);
        captureThresholdText.color = captureThresholdText.color.SetAlpha(0);

        HS_Blocks.localScale = Vector2.one;
        HS_Capture.localScale = Vector2.zero;
        HS_Time.localScale = Vector2.one;
        HS_Map.localScale = Vector2.one;

        HS_Blocks.anchoredPosition = Vector2.zero.SetY(150);
        HS_Capture.anchoredPosition = Vector2.zero;
        HS_Time.anchoredPosition = Vector2.zero.SetY(150);
        HS_Map.anchoredPosition = new Vector2(760.0012f, -339.9983f);
    }

    private void Start()
    {
        BestMapPixels();
    }

    public void SetBlocksCapturedText(int b, int t, int h)
    {
        if (main.gameState != Main.GameState.Game && main.gameState != Main.GameState.Intro)
            return;

        blocksCapturedText.text = b.ToString();
        captureThresholdText.text = t.ToString();

        bool newHighest = highestCaptureCount < h;

        blocksCaptured = b;
        captureThreshold = t;
        highestCaptureCount = h;

        if(HS_BlocksCapturedText.color != Color.yellow && highestCaptureCount > main.GetOldHS())
        {
            HS_BlocksCapturedText.color = Color.yellow;
        }

        if(newHighest)
        {
            HS_BlocksCapturedText.fontSize = HS_BlocksCapturedText.fontSize * .8f;
            float fontSize = MathEX.Remap(1, 1000, 80, 220, Mathf.Clamp(highestCaptureCount, 1, 1000));
            DOTween.To(() => HS_BlocksCapturedText.fontSize, x => HS_BlocksCapturedText.fontSize = x, fontSize, .5f).SetEase(Ease.OutBack);

            BestMapPixels();
        }
    }

    public void SetTime(float time, int longestTime)
    {
        TimeText.text = time.FloorToInt().ToString();

        if (TimeText.color != Color.yellow && time.FloorToInt() > longestTime)
        {
            TimeText.color = Color.yellow;
        }
    }

    public void SetThreshold(int threshold)
    {
        captureThresholdText.text = threshold.ToString();
        captureThreshold = threshold;
    }

    public void PulseThresholdBar()
    {
        thresholdBar.color = Color.yellow;
        thresholdBar.DOColor(Color.red, .75f);
    }

    bool introed = false;

    public void Update()
    {
        switch (main.gameState)
        {
            case Main.GameState.Menu:
                if (pauseScreen)
                {
                    pauseScreen = false;
                    DisablePauseMenu();
                }
                break;
            case Main.GameState.Intro:
                if(!introed)
                {
                    HS_Blocks.DOAnchorPos(Vector2.zero, .5f);
                    HS_Time.DOAnchorPos(Vector2.zero, .5f);
                    AnyKeyText.DOAnchorPos(Vector2.zero.SetY(-30), .4f).SetEase(Ease.InBack);

                    introed = true;
                }

                if (pauseScreen)
                {
                    pauseScreen = false;
                    DisablePauseMenu();
                }

                blocksCapturedText.color = Color.white;

                blocksSlider.maxValue = MathEX.ExpDecay(blocksSlider.maxValue, highestCaptureCount, 15, Time.deltaTime);
                barSlider.maxValue = MathEX.ExpDecay(barSlider.maxValue, highestCaptureCount, 15, Time.deltaTime);
                thresholdSlider.maxValue = MathEX.ExpDecay(thresholdSlider.maxValue, highestCaptureCount, 15, Time.deltaTime);

                blocksSlider.value = MathEX.ExpDecay(blocksSlider.value, (float)blocksCaptured / 8, 20, Time.deltaTime);
                barSlider.value = MathEX.ExpDecay(barSlider.value, (float)blocksCaptured / 8, 20, Time.deltaTime);
                thresholdSlider.value = MathEX.ExpDecay(thresholdSlider.value, captureThreshold, 15, Time.deltaTime);

                blockSliderRT.anchoredPosition = blockSliderRT.anchoredPosition.SetY(MathEX.ExpDecay(blockSliderRT.anchoredPosition.y, MathEX.Remap(1, 1000, 166, 466, Mathf.Clamp(highestCaptureCount, 0, 1000)), 5, Time.deltaTime));
                blockSliderRT.sizeDelta = blockSliderRT.sizeDelta.SetX(MathEX.ExpDecay(blockSliderRT.sizeDelta.x, MathEX.Remap(1, 1000, 200, 800, Mathf.Clamp(highestCaptureCount, 0, 1000)), 5, Time.deltaTime));
                thresholdSliderRT.anchoredPosition = blockSliderRT.anchoredPosition;
                barSliderRT.anchoredPosition = blockSliderRT.anchoredPosition;
                barSliderRT.sizeDelta = blockSliderRT.sizeDelta;
                thresholdSliderRT.sizeDelta = blockSliderRT.sizeDelta;

                HS_BlocksCapturedText.text = highestCaptureCount.ToString();
                break;
            case Main.GameState.Game:
                if (pauseScreen)
                {
                    pauseScreen = false;
                    DisablePauseMenu();
                }

                blocksCapturedText.color = Color.yellow;
                captureThresholdText.color = captureThresholdText.color.SetAlpha(1);

                blocksSlider.maxValue = MathEX.ExpDecay(blocksSlider.maxValue, highestCaptureCount, 15, Time.deltaTime);
                barSlider.maxValue = MathEX.ExpDecay(barSlider.maxValue, highestCaptureCount, 15, Time.deltaTime);
                thresholdSlider.maxValue = MathEX.ExpDecay(thresholdSlider.maxValue, highestCaptureCount, 15, Time.deltaTime);

                blocksSlider.value = MathEX.ExpDecay(blocksSlider.value, blocksCaptured, 20, Time.deltaTime);
                barSlider.value = MathEX.ExpDecay(barSlider.value, blocksCaptured, 20, Time.deltaTime);
                thresholdSlider.value = MathEX.ExpDecay(thresholdSlider.value, captureThreshold, 15, Time.deltaTime);

                blockSliderRT.anchoredPosition = blockSliderRT.anchoredPosition.SetY(MathEX.ExpDecay(blockSliderRT.anchoredPosition.y, MathEX.Remap(1, 1000, 166, 466, Mathf.Clamp(highestCaptureCount, 0, 1000)), 5, Time.deltaTime));
                blockSliderRT.sizeDelta = blockSliderRT.sizeDelta.SetX(MathEX.ExpDecay(blockSliderRT.sizeDelta.x, MathEX.Remap(1, 1000, 200, 800, Mathf.Clamp(highestCaptureCount, 0, 1000)), 5, Time.deltaTime));
                thresholdSliderRT.anchoredPosition = blockSliderRT.anchoredPosition;
                barSliderRT.anchoredPosition = blockSliderRT.anchoredPosition;
                barSliderRT.sizeDelta = blockSliderRT.sizeDelta;
                thresholdSliderRT.sizeDelta = blockSliderRT.sizeDelta;

                HS_BlocksCapturedText.text = highestCaptureCount.ToString();

                warningSign.color = Vector4.MoveTowards(warningSign.color, main.Warning() ? new Vector4(1,1,1,1) : new Vector4(1, 1, 1, 0), 8 * Time.deltaTime);
                suddenDeathObject.SetActive(main.Warning());
                if (!main.Warning())
                    suddenDeathShake = false;
                break;

            case Main.GameState.Paused:
                if (!pauseScreen)
                {
                    pauseScreen = true;
                    EnablePauseMenu();
                }
                break;

            case Main.GameState.Death:
                if (pauseScreen)
                {
                    pauseScreen = false;
                    DisablePauseMenu();
                }
                break;

            case Main.GameState.End:

                if (pauseScreen)
                {
                    pauseScreen = false;
                    DisablePauseMenu();
                }

                if (!ended)
                {
                    HS_Blocks.DOScale(hs_blocks_scale_end, .5f);
                    HS_Capture.DOScale(hs_capture_scale_end, .5f);
                    HS_Time.DOScale(hs_time_scale_end, .5f);
                    HS_Map.DOScale(hs_map_scale_end, .5f);

                    HS_Blocks.DOAnchorPos(hs_blocks_pos_end, .5f);
                    HS_Capture.DOAnchorPos(hs_capture_pos_end, .5f);
                    HS_Time.DOAnchorPos(hs_time_pos_end, .5f);
                    HS_Map.DOAnchorPos(hs_map_pos_end, .5f);

                    BGPanel.DOColor(Color.black.SetAlpha(0.654902f), .5f);

                    LargestCaptureTextText.text = main.GetBiggestCapture().ToString();
                    HS_Map.GetChild(0).GetComponent<RawImage>().color = Color.white;

                    ended = false;

                    restartText.SetActive(true);
                }

                //HS_Blocks.localScale = Vector2.MoveTowards(HS_Blocks.localScale, hs_blocks_scale_end, 20 * Time.deltaTime);
                //HS_Capture.localScale = Vector2.MoveTowards(HS_Capture.localScale, hs_capture_scale_end, 20 * Time.deltaTime);
                //HS_Time.localScale = Vector2.MoveTowards(HS_Time.localScale, hs_time_scale_end, 20 * Time.deltaTime);
                //HS_Map.localScale = Vector2.MoveTowards(HS_Map.localScale, hs_map_scale_end, 20 * Time.deltaTime);

                break;
        }

        // SCREENS
        if (Input.GetKeyDown(KeyCode.M))
        {
            Mute();
        }
    }

    bool ended;

    void EnablePauseMenu()
    {
        pauseMenu.SetActive(true);
    }

    void DisablePauseMenu()
    {
        pauseMenu.SetActive(false);
    }

    void Mute()
    {
        if (!muted)
        {
            mutedText.color = Color.white;
            mutedText.text = "M: Muted";
            audioMixer.SetFloat("Master", -80);
        }
        else
        {
            mutedText.color = Color.white.SetAlpha(.1f);
            mutedText.text = "M: Mute";
            audioMixer.SetFloat("Master", Mathf.Log10(1));
        }

        muted = !muted;
    }

    public void SetMute(bool m)
    {
        muted = m;
        if (muted)
        {
            mutedText.color = Color.white;
            mutedText.text = "M: Muted";
            audioMixer.SetFloat("Master", -80);
        }
        else
        {
            mutedText.color = Color.white.SetAlpha(.1f);
            mutedText.text = "M: Mute";
            audioMixer.SetFloat("Master", Mathf.Log10(1));
        }
    }

    public void SetMusicVolume(float value)
    {
        musicValue.text = value.ToString();
        musicSlider.value = value;
        float adjustedValue = Mathf.Log10(MathEX.Remap(0, 100, .0001f, 1, value)) * 20;

        audioMixer.SetFloat("Music", adjustedValue);
    }

    public void SetMusicSliderValue(float value) => musicSlider.value = value;
    public void SetSfxSliderValue(float value) => sfxSlider.value = value;
    public void SetAudioMixerMusic(float value) => audioMixer.SetFloat("Music", Mathf.Log10(MathEX.Remap(0, 100, .0001f, 1, value)) * 20);
    public void SetAudioMixerSfx(float value) => audioMixer.SetFloat("Sfx", Mathf.Log10(MathEX.Remap(0, 100, .0001f, 1, value)) * 20);

    public void SetSfxVolume(float value)
    {
        sfxValue.text = value.ToString();
        sfxSlider.value = value;
        float adjustedValue = Mathf.Log10(MathEX.Remap(0, 100, .0001f, 1, value)) * 20;

        audioMixer.SetFloat("Sfx", adjustedValue);
    }

    public int GetMusicVolume() => musicSlider.value.RoundToInt();
    public int GetSfxVolume() => sfxSlider.value.RoundToInt();
    public bool GetMuted() => muted;

    bool suddenDeathShake;
    public void SetSuddenDeathTimerText(float time)
    {
        suddenDeathTimer.text = Mathf.Clamp(time, 0, 10).ToString();
        suddenDeathSlider.value = Mathf.Min(time, main.GetBlocksCaptured());

        if(!suddenDeathShake)
        {
            suddenDeathShake = true;
            DOTween.Shake(() => suddenDeathObject.transform.localPosition, x => suddenDeathObject.transform.localPosition = x, 10, 5, 10);
        }
    }

    public void SetMapPixels(bool fullTex = false)
    {
        Vector2Int min = Grid.WorldToGrid(new Vector2(Fill.fill.minX, Fill.fill.minY)).RoundToInt();
        Vector2Int max = Grid.WorldToGrid(new Vector2(Fill.fill.maxX, Fill.fill.maxY)).RoundToInt();

        if (fullTex)
        {
            min = Vector2Int.zero;
            max = Vector2Int.one * 100;
        }

        int width = max.x - min.x + 1;
        int height = max.y - min.y + 1;

        Color32[] sectionColors = new Color32[width * height];
        var mipCount = Mathf.Min(3, MapTexture.mipmapCount);

        int e = 0;
        for (int j = min.y; j <= max.y; j++)
        {
            for (int i = min.x; i <= max.x; i++)
            {
                int value = Grid.GetGrid(new Vector2(i, j), true);
                switch(value)
                {
                    case 1:
                        sectionColors[e] = colors[0];
                        break;
                    case 3:
                        sectionColors[e] = colors[1];
                        break;
                    default:
                        sectionColors[e] = colors[2];
                        break;
                }
                e++;
            }
        }

        for (var mip = 0; mip < mipCount; ++mip)
            MapTexture.SetPixels32(min.x, min.y, width, height, sectionColors, mip);

        MapTexture.Apply(false);
    }

    public void BestMapPixels()
    {
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.one * 100;

        int width = max.x - min.x + 1;
        int height = max.y - min.y + 1;

        Color32[] sectionColors = new Color32[width * height];
        var mipCount = Mathf.Min(3, BestMapTexture.mipmapCount);

        int e = 0;
        for (int j = min.y; j <= max.y; j++)
        {
            for (int i = min.x; i <= max.x; i++)
            {
                int value = Grid.GetGrid(new Vector2(i, j), true);
                switch (value)
                {
                    case 1:
                    case 3:
                        sectionColors[e] = colors[0];
                        break;
                    default:
                        sectionColors[e] = colors[2];
                        break;
                }
                e++;
            }
        }

        for (var mip = 0; mip < mipCount; ++mip)
            BestMapTexture.SetPixels32(min.x, min.y, width, height, sectionColors, mip);

        BestMapTexture.Apply(false);
    }

    public void SetPlayerPosition(Vector2 position)
    {
        PlayerPositionOnTexture.localPosition = position * 3;
    }
}
