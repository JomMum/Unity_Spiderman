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
            //�̵� ���� ���°� �ƴϰų� ���� ������ ���� �̵� �Ұ�
            if (enemyScript.canMove &&
                !enemyScript.isFaint)
            {
                if (nav.enabled == false)
                    nav.enabled = true;

                //Ÿ�� �߰� �� Ÿ���� ���� �̵�
                if (targets != null)
                {
                    enemyScript.isMove = true;
                    nav.SetDestination(targets.transform.position);
                }
                //�ƴ� �� �̵� ����
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
        //�׾��� ��
        else
        {
            enemyScript.isMove = false;

            nav.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //�÷��̾ �þ� ���� ���� ��
        if (other.CompareTag("Player"))
        {
            //�ش� �÷��̾� ����
            targets = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //�÷��̾ �þ� �ۿ� ���� ��
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