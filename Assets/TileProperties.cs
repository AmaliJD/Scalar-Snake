using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class TileProperties : MonoBehaviour
{
    static float timeOfLastSfxPlay;
    const float TIME_BETWEEN_SFX = .02f;

    [SerializeField]
    Color pulseColor;

    [SerializeField]
    AudioClip bubbleSfx;

    public void TweenScale(Vector3 from, Vector3 to, float duration)
    {
        float rand = UnityEngine.Random.Range(0, .2f);

        transform.localScale = from;
        transform.DOScale(to, duration).SetEase(Ease.OutBack).SetDelay(rand);

        transform.GetChild(0).GetComponent<SpriteRenderer>().color = pulseColor;
        transform.GetChild(0).GetComponent<SpriteRenderer>().DOColor(new Color(0.09803939f, 1, 0), duration).SetEase(Ease.InQuad).SetDelay(rand);

        Invoke("PlaySound", rand);
    }

    void PlaySound()
    {
        if(Time.time >= timeOfLastSfxPlay + TIME_BETWEEN_SFX)
        {
            SfxManager.instance.PlaySfxClip(bubbleSfx, 1f, UnityEngine.Random.Range(.9f, 1.2f), transform.position);
            timeOfLastSfxPlay = Time.time;
        }
    }
}
