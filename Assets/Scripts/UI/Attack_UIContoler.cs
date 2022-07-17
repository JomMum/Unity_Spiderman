using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Attack_UIContoler : MonoBehaviour
{
    [SerializeField] PlayerScript playerScript;
    public GameObject Attack_1;
    public GameObject Attack_1sub;
    public GameObject Attack_2;
    public GameObject Attack_2sub;

    void Update()
    {
        if (playerScript.attackMode == 1)
        {
            Attack_1.SetActive(true);
            Attack_1sub.SetActive(true);
            Attack_2.SetActive(false);
            Attack_2sub.SetActive(false);
        }
        else if (playerScript.attackMode == 2)
        {
            Attack_1.SetActive(false);
            Attack_1sub.SetActive(false);
            Attack_2.SetActive(true);
            Attack_2sub.SetActive(true);
        }
    }
}
