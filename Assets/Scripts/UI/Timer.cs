using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    float Min = 0f;
    float sec = 0f;

    [SerializeField]
    Text _Timer;


    void Update()
    {
        TImer();    
    }


    void TImer()
    {
        sec += Time.deltaTime;
        _Timer.text = string.Format("Timer " + "{0:00}:{1:00}", Min, (int)sec);

        if((int)sec >59)
        {
            sec = 0;
            Min++;
        }
    }
}
