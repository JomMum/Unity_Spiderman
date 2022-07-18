using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EnemyHealth : MonoBehaviour
{
    public Image image;

    void Start()
    {
        image = transform.Find("HP_curHP").GetComponent<Image>();
    }

    void Update()
    {
      if(Input.GetKeyDown(KeyCode.E))
      {
            image.fillAmount -= 0.1f;
      }
    }
}
