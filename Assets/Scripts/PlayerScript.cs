using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] GameObject camera;
    [SerializeField] GameObject playerModel;
    [SerializeField] Transform rightHand;

    SpringJoint joint;

    Rigidbody rigidbody;
    Animator animator;
    LineRenderer lineRenderer;

    Vector3 moveDir; //�̵� ����
    Vector3 grapPoint; //�̵��� �Ź��� �߻� ����
    public bool canMove = true;
    bool useDoubleJump;

    bool isJump;
    bool isClimb;
    bool isFall;

    public int moveSpd = 5; //�̵��ӵ�
    public int runSpd = 10; //�޸��� �ӵ�
    public int climbSpd = 3; //���� �ӵ�
    public int jumpPower; //������


    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = playerModel.GetComponent<Animator>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        //�÷��̾� �� ��ü�� ��ġ�� ������� �ʴ´�
        playerModel.transform.localPosition = new Vector3(0, 0, 0);

        CheckIsFall(); //���� ������ üũ

        if (canMove)
        {
            moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            PlayerMove(); //�̵�
            PlayerJump(); //����
            PlayerClimb(); //�� Ÿ��
        }

        PlayerShootMoveWeb(); //�̵��� �Ź��� �߻�


        animator.SetBool("isWalk", moveDir != Vector3.zero);
        animator.SetBool("isRun", moveDir != Vector3.zero && Input.GetAxis("Run") != 0);
        animator.SetBool("isJump", isJump);
        animator.SetBool("isClimb", isClimb);
        animator.SetBool("isMoveClimb", moveDir != Vector3.zero && isClimb);
        animator.SetBool("isInAir", isFall);
        animator.SetBool("isSwing", joint);
    }

    void CheckIsFall()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 0.1f))
        {
            //�ٴڿ� �������� ��
            if(hit.collider != null)
            {
                isFall = false;
                isJump = false;
                useDoubleJump = false;
            }
        }
        //���� ���� ��
        else if(hit.collider == null)
        {
            isFall = true;
        }
    }

    void PlayerMove()
    {
        if (!isClimb)
        {
            //�̵� ���� ��
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                //�̵� �������� �÷��̾� ȸ��
                Vector3 camForward = new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z).normalized;
                Vector3 camRight = new Vector3(camera.transform.right.x, 0, camera.transform.right.z).normalized;
                Vector3 playerDir = camForward * moveDir.z + camRight * moveDir.x; //�÷��̾��� ������ ī�޶��� ���Ͱ��� Ű���� �Է°��� ���� ����
                transform.forward = playerDir;

                //��ǥ �������� �̵�
                if (!joint)
                {
                    if (Input.GetAxis("Run") != 0) //�޸���
                        transform.position += playerDir * runSpd * Time.deltaTime;
                    else //�ȱ�
                        transform.position += playerDir * moveSpd * Time.deltaTime;
                }
                //�Ź��ٷ� ��ǥ ���� �̵�
                else
                {
                    if (Input.GetAxis("Run") != 0) //���� �Ź��� �̵�
                        transform.position += playerDir * runSpd * 2f * Time.deltaTime;
                    else //�Ź��� �̵�
                        transform.position += playerDir * moveSpd * 2f * Time.deltaTime;
                }
            }
        }
    }

    void PlayerJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //�� ������ ���� ��
            if (isClimb)
            {
                isClimb = false;
                isJump = true;

                //�ڷ� ƨ�ܳ���
                rigidbody.AddForce(-transform.forward * 3, ForceMode.Impulse);

                return;
            }

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

    void PlayerClimb()
    {
        Vector3 raycastPos = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z); //�÷��̾ �ٶ󺸴� ���� (y���� �߿� ������ �����Ƿ� ���� ���� ������)

        //����ĳ��Ʈ�� ���� �� ã��
        if (Physics.Raycast(raycastPos, transform.forward, out RaycastHit hit, 0.3f))
        {
            //���� ���� ���
            if (hit.collider != null)
            {
                if (!isClimb)
                {
                    //���� �ʱ�ȭ
                    isJump = false;
                    useDoubleJump = false;

                    rigidbody.velocity = Vector3.zero; //AddForce �� �ʱ�ȭ
                    transform.rotation = Quaternion.LookRotation(-hit.normal); //���� �ݴ� �������� ĳ����ȸ��

                    rigidbody.useGravity = false; //���� ������ �߿��� �߷��� ������ ���� ����
                }

                isClimb = true;

                //�� �̵�
                Vector3 playerDir = transform.up * moveDir.z + transform.right * moveDir.x; //�÷��̾��� ������ ĳ������ ���Ͱ��� Ű���� �Է°��� ���� ����
                transform.position += playerDir * climbSpd * Time.deltaTime;
            }
        }
        //���� ���� ���
        else if(isClimb && hit.collider == null)
        {
            isClimb = false;

            //�ڷ� ƨ�ܳ���
            rigidbody.AddForce(transform.up * 10, ForceMode.Impulse);
            isJump = true;
        }

        //���� ������ ���� �ƴ� ��
        if(!isClimb)
        {
            //�߷� Ȱ��ȭ
            rigidbody.useGravity = true;
        }
    }

    void PlayerShootMoveWeb()
    {
        //�Ź��ٷ� �̵� ���ΰ�
        if (joint)
        {
            //�Ź��� �׸���
            lineRenderer.SetPosition(0, rightHand.position);
            lineRenderer.SetPosition(1, grapPoint);

            //���� �������� ��
            if(!isFall)
            {
                //�Ź��� ����
                lineRenderer.positionCount = 0;
                Destroy(joint);

                return;
            }
        }

        //�Ź��� �߻�
        if (Input.GetMouseButtonDown(1))
        {
            //���� ĳ������ ��ġ ���ϱ� (ī�޶� ��ġ + ī�޶�� ĳ���� ���� ����)
            Vector3 hitPos = camera.transform.position + camera.transform.rotation * new Vector3(0.0f, 0.0f, camera.GetComponent<CameraMove>().distance);

            //���� �Ź����� �߻��� �� �ִ� ������Ʈ�� ���� ��
            if (Physics.Raycast(hitPos, camera.transform.forward, out RaycastHit hit, 100))
            {
                grapPoint = hit.point; //�ش� ������Ʈ�� ��ġ ����

                //SpiringJoint ������Ʈ �߰�
                joint = gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grapPoint; //������Ʈ�� ����Ʈ ����

                //����Ʈ�� �ִ�/�ּ� �Ÿ� ����
                float distanceFromPoint = Vector3.Distance(rightHand.position, grapPoint); //�÷��̾�� ������Ʈ �� �Ÿ� ã��
                joint.maxDistance = distanceFromPoint * 0.5f;
                joint.minDistance = distanceFromPoint * 0.25f;

                joint.spring = 4.5f;     
                joint.damper = 7f;      
                joint.massScale = 4.5f;

                //�Ź��� ���η������� �������� ���� �� ���� ������
                lineRenderer.positionCount = 2;
            }
        }
        //�Ź��� ����
        else if (Input.GetMouseButtonUp(1))
        {
            lineRenderer.positionCount = 0;
            Destroy(joint);
        }
    }
}
