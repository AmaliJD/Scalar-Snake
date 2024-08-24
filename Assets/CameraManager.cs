using EX;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraManager : MonoBehaviour
{
    Camera cam;
    Main main;
    PlayerMovement player;

    public Color IntroColor, GameColor, WarningColor;
    public Volume warningVolume, pausedVolume;

    private void Awake()
    {
        cam = Camera.main;
        main = GetComponent<Main>();
        player = GetComponent<PlayerMovement>();

        cam.backgroundColor = IntroColor;
    }

    private void Update()
    {
        if(main.gameState != Main.GameState.Paused)
            pausedVolume.weight = Mathf.MoveTowards(pausedVolume.weight, 0, 5 * Time.unscaledDeltaTime);

        switch (main.gameState)
        {
            case Main.GameState.Intro:
                cam.orthographicSize = MathEX.ExpDecay(cam.orthographicSize, 4, 12, Time.deltaTime);
                cam.backgroundColor = Color.Lerp(cam.backgroundColor, IntroColor, 4 * Time.deltaTime);
                break;

            case Main.GameState.Game:
                cam.orthographicSize = MathEX.ExpDecay(cam.orthographicSize, Mathf.Clamp(Mathf.Pow(main.GetBlocksCaptured(), 1f/3f), 5f, 20), 1, Time.deltaTime);
                cam.backgroundColor = Color.Lerp(cam.backgroundColor, !main.Warning() ? GameColor : WarningColor, 4 * Time.deltaTime);
                warningVolume.weight = Mathf.MoveTowards(warningVolume.weight, main.Warning() ? 1 : 0, 4 * Time.deltaTime);

                //float x = cam.transform.position.x;
                //float y = cam.transform.position.y;
                //x = Mathf.MoveTowards(x, player.GetPlayer().position.x, (1 / player.StepTime()) * .3f * Time.deltaTime);
                //y = Mathf.MoveTowards(y, player.GetPlayer().position.y, (1 / player.StepTime()) * .1f * Time.deltaTime);
                //cam.transform.position = cam.transform.position.SetXY(x, y);

                cam.transform.position = Vector3.MoveTowards(cam.transform.position, player.GetPlayer().position + (Vector3)player.GetCurrentDirection().MultiplyEach(1, 4), (1 / player.StepTime()) * 1.4f * Time.deltaTime).SetZ(-10);
                
                // MOVE TO FOCUS POSITION
                //Vector3 direction = (Vector3)((player.GetCurrentDirection() + previousDirectionBlend));
                //Vector3 focusPosition = direction.MultiplyEach(2, 4)
                //                        * -MathEX.Remap(0.0025f, .09f, -1.25f, -1, Mathf.Pow(player.GetWaitTime(), 2)) // speed
                //                        * MathEX.RemapClamped(0, Grid.gridSize / 2, 1, 3, Vector2.Distance(player.GetPlayer().position, Vector2.zero)); // distance from center
                //Debug.Log($"Blend: {previousDirectionBlend}, Direction: {direction}");

                //cam.transform.position = Vector3.MoveTowards(cam.transform.position,
                //                                             player.GetPlayer().position + focusPosition, /*(1 / player.StepTime()) * 1.2f*/6f * Time.deltaTime).SetZ(-10);
                //previousDirectionBlend = MathEX.ExpDecay(previousDirectionBlend, Vector2.zero, .000001f, Time.deltaTime);
                previousDirectionBlend = Vector2.MoveTowards(previousDirectionBlend, Vector2.zero, .5f * Time.deltaTime);

                // CLAMP CAMERA
                float camHalfHeight = cam.orthographicSize;
                float camHalfWidth = cam.orthographicSize * 16f / 9f;
                float halfGridWidth = Grid.gridSize / 2;
                Vector2 cornerPos = new Vector2(halfGridWidth - camHalfWidth + 3, halfGridWidth - camHalfHeight + 3);
                cam.transform.position = MathEX.Vector2AreaClamp(cam.transform.position, -cornerPos, cornerPos);

                // RECALC PREVIOUS POSITION
                if(previousDirection != player.GetPreviousDirection())
                {
                    previousDirection = player.GetPreviousDirection();
                    previousDirectionBlend = previousDirection;
                }
                break;

            case Main.GameState.Paused:
                pausedVolume.weight = Mathf.MoveTowards(pausedVolume.weight, 1, 5 * Time.unscaledDeltaTime);
                break;
        }
    }

    Vector2 previousDirection, previousDirectionBlend;
}
