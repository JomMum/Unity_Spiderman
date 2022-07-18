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
        //���� �ð� �� �Ź��� ����
        yield return new WaitForSeconds(destroyTime);

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        //�÷��̾� �ܿ� �ٸ� ������Ʈ�� �浹 �� ����
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
