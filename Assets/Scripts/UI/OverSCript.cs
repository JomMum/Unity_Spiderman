using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OverSCript : MonoBehaviour
{
    public bool isGameOver;
    public bool isGameClear;

    [SerializeField] CanvasGroup GameOverUI;
    [SerializeField] CanvasGroup GameClearUI;

    public IEnumerator UiScript()
    {
        Cursor.visible = true;

        CanvasGroup UI;

        if (isGameClear)
            UI = GameClearUI;
        else
            UI = GameOverUI;

        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(0.1f);
            UI.alpha += 0.1f;
        }

        UI.interactable = true;
    }
    public void OnclickReBtn()
    {
        SceneManager.LoadScene("IngameScene");
    }

    public void OnclickMainBtn()
    {
        SceneManager.LoadScene("StartScene");
    }
}