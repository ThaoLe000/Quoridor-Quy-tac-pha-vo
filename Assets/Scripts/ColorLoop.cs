using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorLoop : MonoBehaviour
{
    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        Sequence seq = DOTween.Sequence();
        sr.DOColor(Color.cyan, 2f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }
}
