using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimScript : MonoBehaviour
{
    [SerializeField] GameObject camera;
    PlayerScript playerScript;

    private void Awake()
    {
        playerScript = gameObject.transform.parent.GetComponent<PlayerScript>();
    }

    public void OnClimbUp()
    {
        // 현재 위치에 위쪽 + 앞으로 이동
        Vector3 upwardMovement = new Vector3(0, 2f, 0); // 위쪽으로 이동
        Vector3 forwardMovement = camera.transform.forward * 1.5f; // 앞으로 이동

        playerScript.gameObject.transform.position += upwardMovement + forwardMovement;
        playerScript.isClimb = false;
    }
}
