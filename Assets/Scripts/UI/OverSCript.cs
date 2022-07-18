using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OverSCript : MonoBehaviour
{
    public CanvasGroup UI;

    private void Start()
    {
        UI = GetComponent<CanvasGroup>();
    }
    void Update()
    {
        StartCoroutine("UiScript");
    }


    public IEnumerator UiScript()
    {
        for (int i = 0; i < 100; i++)
        {
            yield return new WaitForSeconds(0.1f);
            UI.alpha += 0.01f;

            if (UI.alpha != 1)
            {
                UI.interactable = false;
            }
            
            if(UI.alpha == 1)
            {
                UI.interactable = true;
            }

        }
    }
    public void OnclickReBtn()
    {
        SceneManager.LoadScene("UI_Scene");
    }

    public void OnclickMainBtn()
    {
        SceneManager.LoadScene("StartScene");

        print("´­·¶");
    }
}