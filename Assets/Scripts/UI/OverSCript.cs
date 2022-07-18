using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OverSCript : MonoBehaviour
{
    public bool isGameOver;
    public bool isGameClear;

    [SerializeField] Timer timerManager;

    [SerializeField] CanvasGroup GameOverUI;
    [SerializeField] CanvasGroup GameClearUI;

    [SerializeField] Text timeText;

    public IEnumerator UiScript()
    {
        Cursor.visible = true;

        CanvasGroup UI;

        if (isGameClear)
        {
            timeText.text = "TIME " + timerManager.Min + " : " + Mathf.RoundToInt(timerManager.sec);
            UI = GameClearUI;
        }
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