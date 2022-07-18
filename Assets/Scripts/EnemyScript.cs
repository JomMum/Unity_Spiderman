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

    public bool isFaint; //전투 모드에 의해 기절 상태인가
    public bool isMove;
    bool isFalling;
    bool isAttacked;
    public bool isDie;

    GameObject groundObj; //현재 발 밑에 있는 오브젝트

    void Awake()
    {
        animator = enemyModel.GetComponent<Animator>();

        enemyCurHP = enemyMaxHP;
    }

    void Update()
    {
        //적 모델 자체는 위치가 변경되지 않는다
        enemyModel.transform.localPosition = new Vector3(0, 0, 0);

        if (!isDie)
        {
            CheckIsFall(); //낙하 중인지 체크
            CheckIsDie(); //사망했는지 체크

            //애니메이션 처리
            animator.SetBool("isRun", isMove);
            animator.SetBool("isInAir", isFalling);
            animator.SetBool("isFaint", isFaint);
            animator.SetBool("isAttacked", isAttacked);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider != null)
        {
            //만약 바닥과 충돌했거나 플레이어와 충돌 시 반환
            if (collision.gameObject == groundObj ||
                collision.gameObject.CompareTag("Player"))
            {
                if(collision.gameObject.CompareTag("Player"))
                    animator.SetTrigger("isAttack");

                isFaint = false;
            }

            //만약 기절 상태에서 벽과 충돌할 시
            if (isFaint)
            {
                EnemyHit(3);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //만약 거미줄과 접촉 시
        if (other.gameObject.CompareTag("Web"))
        {
            EnemyHit(1);
        }
    }

    void CheckIsFall()
    {
        //바닥에 착지했을 시
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 0.1f))
        {
            if (hit.collider != null)
            {
                groundObj = hit.collider.gameObject;
                isFalling = false;
            }
        }
        //낙하 중일 시
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
