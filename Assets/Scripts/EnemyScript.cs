using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public bool isFaint; //전투 모드에 의해 기절 상태인가
    public bool isAttack;

    GameObject groundObj; //현재 발 밑에 있는 오브젝트

    void Update()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 3f))
        {
            //바닥에 착지했을 시
            if (hit.collider != null)
            {
                groundObj = hit.collider.gameObject;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider != null)
        {
            //만약 바닥과 충돌했거나 플레이어와 충돌 시 반환
            if (collision.gameObject == groundObj ||
                collision.gameObject.CompareTag("Player"))
            {
                isFaint = false;
                return;
            }

            //만약 기절 상태에서 벽과 충돌 시
            if(isFaint)
            {
                Debug.Log("주금");
            }
        }
    }

    public IEnumerator DisableFaint()
    {
        yield return new WaitForSeconds(3f);

        isFaint = false;
    }
}
