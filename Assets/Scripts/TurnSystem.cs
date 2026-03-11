using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    public BoardManager board;
    public PlayerTurn currentTurn = PlayerTurn.Player1;
    bool actionDone = false;
    public TurnSystemUI turnSystemUI;

    bool gameOver = false;

    [Header("Time")]
    public float startTime = 60f;
    public float player1Time;
    public float player2Time;
    void Start()
    {
        player1Time = startTime;
        player2Time = startTime;
    }
    void Update()
    {
        if (gameOver) return;

        UpdateTimer();
        if (actionDone) return;
        board.PreviewOldestWall(currentTurn);
        HandleWallPlacement();
    }
    void UpdateTimer()
    {
        if (currentTurn == PlayerTurn.Player1)
        {
            player1Time -= Time.deltaTime;
            if (player1Time <= 0)
                GameOver(PlayerTurn.Player2);
        }
        else
        {
            player2Time -= Time.deltaTime;
            if (player2Time <= 0)
                GameOver(PlayerTurn.Player1);
        }
    }
    void GameOver(PlayerTurn winner)
    {
        gameOver = true;

        Debug.Log("Winner: " + winner);
        string whowin = winner.ToString();
        turnSystemUI.WhoWinner(whowin);

        actionDone = true;
    }

    private void HandleWallPlacement()
    {
        if(!Input.GetMouseButtonDown(0)) return;
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z =0;

        bool placed = board.PlaceWall(mouse, currentTurn);

        if (!placed) return;

        StartCoroutine(ProcessTurn());
    }
    private IEnumerator ProcessTurn()
    {
        actionDone = true;

        Pawn pawn =
            currentTurn == PlayerTurn.Player1
            ? board.Player1
            : board.Player2;

        board.AutoMovePawn(pawn);

        if (board.CheckWin(pawn))
        {
            Debug.Log("Winner: " + currentTurn);
            string whowin = currentTurn.ToString();
            turnSystemUI.WhoWinner(whowin);
            yield break;
        }

        yield return new WaitForSeconds(0.5f);

        EndTurn();

        actionDone = false;
    }
    private void EndTurn()
    {
        currentTurn = currentTurn ==PlayerTurn.Player1 ?
            PlayerTurn.Player2 : PlayerTurn.Player1;

        Debug.Log("Turn:" + currentTurn);
    }
}
