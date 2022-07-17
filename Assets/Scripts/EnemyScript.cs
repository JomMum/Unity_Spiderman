using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public bool isFaint; //���� ��忡 ���� ���� �����ΰ�
    public bool isAttack;

    GameObject groundObj; //���� �� �ؿ� �ִ� ������Ʈ

    void Update()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 3f))
        {
            //�ٴڿ� �������� ��
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
            //���� �ٴڰ� �浹�߰ų� �÷��̾�� �浹 �� ��ȯ
            if (collision.gameObject == groundObj ||
                collision.gameObject.CompareTag("Player"))
            {
                isFaint = false;
                return;
            }

            //���� ���� ���¿��� ���� �浹 ��
            if(isFaint)
            {
                Debug.Log("�ֱ�");
            }
        }
    }

    public IEnumerator DisableFaint()
    {
        yield return new WaitForSeconds(3f);

        isFaint = false;
    }
}
