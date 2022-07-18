using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{
    public GameObject canvas;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            canvas.SetActive(true);
            Invoke(nameof(DisableCanvas), 3);
        }
    }

    void DisableCanvas()
    {
        canvas.SetActive(false);
    }
}

    


