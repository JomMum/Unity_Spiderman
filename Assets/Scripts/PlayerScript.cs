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
        }
    }

    void PlayerMove()
    {
        moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //이동 중일 시
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            //플레이어가 카메라 방향을 보도록 하기
            Vector3 camDir = camera.transform.forward;
            camDir.y = 0;
            transform.LookAt(transform.position + camDir);

            //목표 지점으로 이동
            moveDir = transform.TransformDirection(moveDir);
            moveDir.y = 0;

            if(Input.GetAxis("Run") != 0) //달리기
                transform.position += moveDir * runSpd * Time.deltaTime;
            else //걷기
                transform.position += moveDir * moveSpd * Time.deltaTime;
        }
    }

    void PlayerJump()
    {
        if (Input.GetAxis("Jump") != 0 && !isJump)
        {
            isJump = true;
            rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }
}
