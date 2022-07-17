using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack_UIContoler : MonoBehaviour
{
    public GameObject Attack_1;
    public GameObject Attack_1sub;
    public GameObject Attack_2;
    public GameObject Attack_2sub;
    void Start()
    {
        Attack_1.SetActive(true);
        Attack_1sub.SetActive(true);

        Attack_2.SetActive(false);
        Attack_2sub.SetActive(false);
    }
        

   
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Alpha1))
        {
            Attack_1.SetActive(true);
            Attack_1sub.SetActive(true);
            Attack_2.SetActive(false);
            Attack_2sub.SetActive(false);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            Attack_1.SetActive(false);
            Attack_1sub.SetActive(false);
            Attack_2.SetActive(true);
            Attack_2sub.SetActive(true);
        }
    }
}

