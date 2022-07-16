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
        //�÷��̾� �� ��ü�� ��ġ�� ������� �ʴ´�
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

        //�̵� ���� ��
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            //�̵� �������� �÷��̾� ȸ��
            Vector3 camForward = new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z).normalized;
            Vector3 camRight = new Vector3(camera.transform.right.x, 0, camera.transform.right.z).normalized;
            Vector3 playerDir = camForward * moveDir.z + camRight * moveDir.x; //�÷��̾��� ������ ī�޶��� ���Ͱ��� Ű���� �Է°��� ���� ����
            transform.forward = playerDir;

            //��ǥ �������� �̵�
            if(Input.GetAxis("Run") != 0) //�޸���
                transform.position += playerDir * runSpd * Time.deltaTime;
            else //�ȱ�
                transform.position += playerDir * moveSpd * Time.deltaTime;
        }
    }

    void PlayerJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //1�� ����
            if (!isJump)
            {
                isJump = true;
                rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                return;
            }
            //2�� ����
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
