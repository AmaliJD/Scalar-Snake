using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class EffectNotification : MonoBehaviour
{
    public TextMeshProUGUI text;
    //[SerializeField] AudioClip notificationSfx;

    private void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.DOScale(Vector2.one * .85f, .25f)
                     .SetEase(Ease.OutBack)
                     .OnComplete(() => rectTransform.DOScale(Vector2.zero, .25f)
                                                    .SetDelay(.75f)
                                                    .SetEase(Ease.InBack)
                                                    .OnComplete(() => Destroy(gameObject)));
    }
}
