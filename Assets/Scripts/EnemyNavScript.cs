using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavScript : MonoBehaviour
{
    public bool isAttacted = false;
    GameObject targets = null; 
    
    NavMeshAgent nav = null;
  
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        //Ÿ�� �߰� �� Ÿ���� ���� �̵�
        if(targets != null)
        {
            Debug.Log(targets);
            nav.SetDestination(targets.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //�÷��̾ �þ� ���� ���� ��
        if (other.CompareTag("Player"))
        {
            //�ش� �÷��̾� ����
            targets = other.gameObject;
            print("�߰�");
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

    }
}
       
        
        
    
