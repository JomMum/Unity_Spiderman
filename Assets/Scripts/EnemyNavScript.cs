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
        //타겟 발견 시 타겟을 향해 이동
        if(targets != null)
        {
            Debug.Log(targets);
            nav.SetDestination(targets.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //플레이어가 시야 내에 들어올 시
        if (other.CompareTag("Player"))
        {
            //해당 플레이어 저장
            targets = other.gameObject;
            print("발견");
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

    }
}
       
        
        
    
