using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    [SerializeField] GameObject enemyModel;
    [SerializeField] OverSCript resultScript;

    Animator animator;

    public int enemyMaxHP;
    public int enemyCurHP;

    public bool canMove = true;

    public bool isFaint; //���� ��忡 ���� ���� �����ΰ�
    public bool isMove;
    bool isFalling;
    bool isAttacked;
    public bool isDie;

    GameObject groundObj; //���� �� �ؿ� �ִ� ������Ʈ

    void Awake()
    {
        animator = enemyModel.GetComponent<Animator>();

        enemyCurHP = enemyMaxHP;
    }

    void Update()
    {
        //�� �� ��ü�� ��ġ�� ������� �ʴ´�
        enemyModel.transform.localPosition = new Vector3(0, 0, 0);

        if (!isDie)
        {
            CheckIsFall(); //���� ������ üũ
            CheckIsDie(); //����ߴ��� üũ

            //�ִϸ��̼� ó��
            animator.SetBool("isRun", isMove);
            animator.SetBool("isInAir", isFalling);
            animator.SetBool("isFaint", isFaint);
            animator.SetBool("isAttacked", isAttacked);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.GetComponent<Collider>() != null)
        {
            //���� �ٴڰ� �浹�߰ų� �÷��̾�� �浹 �� ��ȯ
            if (collision.gameObject == groundObj ||
                collision.gameObject.CompareTag("Player"))
            {
                if(collision.gameObject.CompareTag("Player"))
                    animator.SetTrigger("isAttack");

                isFaint = false;
            }

            //���� ���� ���¿��� ���� �浹�� ��
            if (isFaint)
            {
                EnemyHit(3);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //���� �Ź��ٰ� ���� ��
        if (other.gameObject.CompareTag("Web"))
        {
            EnemyHit(1);
        }
    }

    void CheckIsFall()
    {
        //�ٴڿ� �������� ��
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 0.1f))
        {
            if (hit.collider != null)
            {
                groundObj = hit.collider.gameObject;
                isFalling = false;
            }
        }
        //���� ���� ��
        else if (hit.collider == null)
        {
            isFalling = true;
        }
    }

    void CheckIsDie()
    {
        if(enemyCurHP <= 0)
        {
            isDie = true;
            animator.SetTrigger("isDie");

            StartCoroutine(nameof(DestroyEnemy));

            resultScript.isGameClear = true;
            resultScript.StartCoroutine(nameof(resultScript.UiScript));
        }
    }

    void EnemyHit(int damage)
    {
        enemyCurHP -= damage;

        if (enemyCurHP <= 0)
            return;

        animator.SetTrigger("isHit");

        canMove = false;
        isAttacked = true;
        StartCoroutine(nameof(EnableMove), 1);
    }

    public IEnumerator DisableFaint(float time)
    {
        yield return new WaitForSeconds(time);

        isFaint = false;
    }

    public IEnumerator EnableMove(float time)
    {
        yield return new WaitForSeconds(time);

        canMove = true;
        isAttacked = false;
    }

    IEnumerator DestroyEnemy()
    {
        yield return new WaitForSeconds(5);

        Destroy(gameObject);
    }
}
