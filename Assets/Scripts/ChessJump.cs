using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class ChessJump : MonoBehaviour
{
    public List<Vector3> positions = new List<Vector3>();
    public float jumpPower;
    public float jumpDuration;
    public float waitTime;

    void Start()
    {
        StartCoroutine(JumpLoop());
    }

    IEnumerator JumpLoop()
    {
        while (true)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                yield return JumpTo(positions[i]);
            }
            for (int i = positions.Count - 2; i > 0; i--)
            {
                yield return JumpTo(positions[i]);
            }
        }
    }
    IEnumerator JumpTo(Vector3 target)
    {
        bool done = false;
        transform.DOJump(target, jumpPower,1, jumpDuration).OnComplete(() =>done = true);

        yield return new WaitUntil(() => done);
        yield return new WaitForSeconds(waitTime);
    }
}
