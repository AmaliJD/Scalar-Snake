using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Powerup : MonoBehaviour
{
    [SerializeField] Sprite SP_Speed, SP_Threshold, SP_Stall;

    [SerializeField]
    AudioClip enterSfx, despawnSfx;

    public enum PowerupType
    {
        Speed, Stall, Threshold
    }

    [HideInInspector]
    public PowerupType powerType;

    private void SetType()
    {
        switch (powerType)
        {
            case PowerupType.Speed:
                GetComponent<SpriteRenderer>().sprite = SP_Speed;
                break;

            case PowerupType.Threshold:
                GetComponent<SpriteRenderer>().sprite = SP_Threshold;
                break;

            case PowerupType.Stall:
                GetComponent<SpriteRenderer>().sprite = SP_Stall;
                break;
        }
    }

    private void Enter()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, .3f).SetEase(Ease.OutBack);

        SfxManager.instance.PlaySfxClip(enterSfx, 1f, UnityEngine.Random.Range(.85f, 1.3f), transform.position, false);
    }

    [SerializeField]
    public bool got = false;

    public void Init()
    {
        SetType();
        Enter();
        Invoke("Despawn", 15);
    }

    void Despawn()
    {
        if(!got)
        {
            got = true;
            SfxManager.instance.PlaySfxClip(despawnSfx, 1f, 1, transform.position);

            GameObject mainObj = GameObject.FindGameObjectWithTag("Main");
            //Transform player = mainObj.GetComponent<PlayerMovement>().GetPlayer();
            GetComponent<SpriteRenderer>().color = Color.black;
            //Vector2 gridPos = Grid.WorldToGrid(player.position);

            Destroy(mainObj.GetComponent<PowerupManager>().GetPowerupMapHolder().Find($"[{transform.position.x},{transform.position.y}]").gameObject);
            transform.DOScale(Vector3.zero, .3f).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject));
        }
    }
}
