using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavScript : MonoBehaviour
{
    public GameObject targets = null; 
    
    NavMeshAgent nav;
    EnemyScript enemyScript;
  
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        enemyScript = GetComponent<EnemyScript>();
    }

    void Update()
    {
        if (!enemyScript.isDie)
        {
            //이동 가능 상태가 아니거나 기절 상태일 때는 이동 불가
            if (enemyScript.canMove &&
                !enemyScript.isFaint)
            {
                if (nav.enabled == false)
                    nav.enabled = true;

                //타겟 발견 시 타겟을 향해 이동
                if (targets != null)
                {
                    enemyScript.isMove = true;
                    nav.SetDestination(targets.transform.position);
                }
                //아닐 시 이동 정지
                else
                {
                    enemyScript.isMove = false;
                }
            }
            else
            {
                enemyScript.isMove = false;

                nav.enabled = false;
            }
        }
        //죽었을 시
        else
        {
            enemyScript.isMove = false;

            nav.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //플레이어가 시야 내에 들어올 시
        if (other.CompareTag("Player"))
        {
            //해당 플레이어 저장
            targets = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //플레이어가 시야 밖에 나갈 시
        if (other.CompareTag("Player") && targets != null)
        {
            Remove_Target();
        }
    }

    public void Remove_Target()
    {
        targets = null;

        if(nav.isOnNavMesh)
            nav.SetDestination(transform.position);
    }
}