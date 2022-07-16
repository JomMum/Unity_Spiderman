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

    public int moveSpd = 5; //�̵��ӵ�
    public int runSpd = 10; //�޸��� �ӵ�
    public int jumpPower; //������


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

        //�̵� ���� ��
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            //�÷��̾ ī�޶� ������ ������ �ϱ�
            Vector3 camDir = camera.transform.forward;
            camDir.y = 0;
            transform.LookAt(transform.position + camDir);

            //��ǥ �������� �̵�
            moveDir = transform.TransformDirection(moveDir);
            moveDir.y = 0;

            if(Input.GetAxis("Run") != 0) //�޸���
                transform.position += moveDir * runSpd * Time.deltaTime;
            else //�ȱ�
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
