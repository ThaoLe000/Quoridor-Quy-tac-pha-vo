using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class TurnSystemUI : MonoBehaviour
{
    public TurnSystem system;

    public Text player1Text;
    public Text player2Text;
    public Text turnText;
    public Text winnerText;
    public GameObject winnerImage;

    void Update()
    {
        if (system == null) return;
        UpdateTimerUI();
        UpdateTurnUI();
    }
    void UpdateTimerUI()
    {
        player1Text.text = FormatTime(system.player1Time);
        player2Text.text = FormatTime(system.player2Time);
    }

    void UpdateTurnUI()
    {
        turnText.text = "Turn: " + system.currentTurn;
    }

    string FormatTime(float t)
    {
        if (t < 0) t = 0;

        int m = Mathf.FloorToInt(t / 60);
        int s = Mathf.FloorToInt(t % 60);

        return m.ToString("00") + ":" + s.ToString("00");
    }
    public void WhoWinner(string who)
    {
        winnerImage.SetActive(true);
        winnerText.text = who.ToString() + "Winner";
    }
}
