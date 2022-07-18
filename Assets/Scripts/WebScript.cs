using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebScript : MonoBehaviour
{
    public int moveSpd;
    public int destroyTime;

    void Start()
    {
        StartCoroutine(DestroyWeb());
    }

    void Update()
    {
        transform.Translate(Vector3.forward * moveSpd * Time.deltaTime, Space.Self);
    }

    IEnumerator DestroyWeb()
    {
        //일정 시간 후 거미줄 삭제
        yield return new WaitForSeconds(destroyTime);

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        //플레이어 외에 다른 오브젝트와 충돌 시 삭제
        if(!other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                if (!other.gameObject.GetComponent<EnemyScript>().isDie)
                {
                    Rigidbody rigidbody = other.gameObject.GetComponent<Rigidbody>();
                    rigidbody.AddForce(transform.forward * 10, ForceMode.Impulse);
                }
            }

            Destroy(gameObject);
        }
    }
}
