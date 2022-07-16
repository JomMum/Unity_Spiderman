using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] GameObject camera;
    [SerializeField] GameObject playerModel;

    Rigidbody rigidbody;
    Animator animator;

    Vector3 moveDir;
    public bool canMove = true;
    bool useDoubleJump;

    bool isJump;

    public int moveSpd = 5; //이동속도
    public int runSpd = 10; //달리기 속도
    public int jumpPower; //점프력


    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = playerModel.GetComponent<Animator>();
    }

    void Update()
    {
        //플레이어 모델 자체는 위치가 변경되지 않는다
        playerModel.transform.localPosition = new Vector3(0, 0, 0);

        if (canMove)
        {
            PlayerMove();
            PlayerJump();
        }

        animator.SetBool("isWalk", moveDir != Vector3.zero);
        animator.SetBool("isRun", moveDir != Vector3.zero && Input.GetAxis("Run") != 0);
        animator.SetBool("isJump", isJump);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            isJump = false;
            useDoubleJump = false;
        }
    }

    void PlayerMove()
    {
        moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //이동 중일 시
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            //이동 방향으로 플레이어 회전
            Vector3 camForward = new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z).normalized;
            Vector3 camRight = new Vector3(camera.transform.right.x, 0, camera.transform.right.z).normalized;
            Vector3 playerDir = camForward * moveDir.z + camRight * moveDir.x; //플레이어의 방향은 카메라의 벡터값과 키보드 입력값을 통해 구함
            transform.forward = playerDir;

            //목표 지점으로 이동
            if(Input.GetAxis("Run") != 0) //달리기
                transform.position += playerDir * runSpd * Time.deltaTime;
            else //걷기
                transform.position += playerDir * moveSpd * Time.deltaTime;
        }
    }

    void PlayerJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //1단 점프
            if (!isJump)
            {
                isJump = true;
                rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                return;
            }
            //2단 점프
            else if(!useDoubleJump)
            {
                animator.SetTrigger("isDoubleJump");

                useDoubleJump = true;
                rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                return;
            }
        }
    }
}
