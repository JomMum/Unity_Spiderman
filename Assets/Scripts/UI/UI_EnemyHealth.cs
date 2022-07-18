using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EnemyHealth : MonoBehaviour
{
    [SerializeField] EnemyScript enemyScript;
    public Image image;


    void Start()
    {
        image = transform.Find("HP_curHP").GetComponent<Image>();
    }

    void Update()
    {
        image.fillAmount = enemyScript.enemyCurHP / (float)enemyScript.enemyMaxHP;
    }
}
