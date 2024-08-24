using EX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager instance;

    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }

    public void PlaySfxClip(AudioClip audioClip, float volume, float pitch, Vector3 spawnAt, bool distVolume = true)
    {
        AudioSource audioSource = Instantiate(sfxSource, spawnAt, Quaternion.identity);
        audioSource.clip = audioClip;

        float distFromCenterScreen = Vector2.Distance((Vector2)spawnAt, (Vector2)Camera.main.transform.position);
        float minDist = 2 * Camera.main.orthographicSize * (16f / 9);
        float multiplier = 1;

        if(distVolume)
            multiplier = 1 - MathEX.Remap(minDist, minDist + 3, 0, 1, distFromCenterScreen);

        if (multiplier == 0)
            return;

        audioSource.volume = volume * multiplier;
        audioSource.pitch = pitch;
        audioSource.Play();

        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }
}
