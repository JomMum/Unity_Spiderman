using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Start_UI : MonoBehaviour
{
    public void OnclickStartBTN()
    {
        SceneManager.LoadScene("IngameScene");
    }


    public void OnclickEndBTN()
    {
        Application.Quit();
    }
}
