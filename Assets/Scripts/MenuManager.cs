using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button playBtn;
    public Button exitBtn;

    public RectTransform blackPanel;
    public float duration = 0.8f;

    public void LoadScene( int sceneName)
    {
        blackPanel.DOAnchorPosY(0, duration).SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                SceneManager.LoadScene( sceneName );
            });
    }
    public void PlayGame()
    {
        LoadScene(1);
    }
    public void ExitGame()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        //Application.Quit();
    }
    public void MainMenu()
    {
        LoadScene(0);
    }
}
