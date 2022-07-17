using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavScript : MonoBehaviour
{
    NavMeshAgent nav = null;
    [SerializeField] Transform[] Waypoints = null;
    int count = 0;

        
    
      public Transform targets = null; 


    
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        InvokeRepeating("MoveToNextWay", 0f, 2f);
    }

    void Update()
    {
        if(targets != null)
        {
            nav.SetDestination(targets.position);
        }
    }

    void MoveToNextWay()
    {
        if(targets == null)
        {
            if(nav.velocity == Vector3.zero)
            {
                nav.SetDestination(Waypoints[count++].position);

                if(count >= Waypoints.Length)
                {
                    count = 0;
                }

            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            nav.SetDestination(targets.position); 
            print("¹ß°ß");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Remove_Target();
        }
    }


    public void SetTarget(Transform transform)
    {
        CancelInvoke();
        transform = targets;
    }

    public void Remove_Target()
    {
        targets = null;
        InvokeRepeating("MoveToNextWay", 0f, 2f);
    }
}
       
        
        
    
