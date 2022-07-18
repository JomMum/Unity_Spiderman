using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] GameObject camera;
    [SerializeField] GameObject playerModel;

    [SerializeField] Transform rightHand;
    [SerializeField] Transform leftHand;
    
    [SerializeField] GameObject mode1_web;
    [SerializeField] GameObject mode2_web;

    [SerializeField] OverSCript resultScript;

    GameObject attackHitObj;
    EnemyScript attackMode2_EnemyScript;

    SpringJoint moveJoint; //�̵��� �Ź��� ����Ʈ
    SpringJoint attackJoint; //���ݿ� �Ź��� ����Ʈ (���� ���)

    Rigidbody rigidbody;
    Animator animator;

    LineRenderer moveLineRenderer; //�̵��� �Ź��� ���� ������
    LineRenderer attackLineRenderer; //���ݿ� �Ź��� ���� ������ (���� ���)

    Vector3 moveDir; //Ű���� �Է� ����
    Vector3 playerDir; //�Է� ����� ī�޶� ������ ������ ���� �̵� ����
    Vector3 moveGrapPoint; //�̵��� �Ź��� �߻� ����
    Vector3 attackGrapPoint; //���ݿ� �Ź��� �߻� ����

    public bool canMove = true;
    bool useDoubleJump;

    bool isJump;
    bool isClimb;
    bool isFall;
    public bool isDie;

    public int moveSpd = 5; //�̵��ӵ�
    public int runSpd = 10; //�޸��� �ӵ�
    public int climbSpd = 3; //���� �ӵ�
    public int jumpPower; //������

    public int attackMode = 1; //���� ���


    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = playerModel.GetComponent<Animator>();
        moveLineRenderer = GetComponent<LineRenderer>();
        attackLineRenderer = mode2_web.GetComponent<LineRenderer>();

        Cursor.visible = false;
    }

    void Update()
    {
        //�÷��̾� �� ��ü�� ��ġ�� ������� �ʴ´�
        playerModel.transform.localPosition = new Vector3(0, 0, 0);

        CheckIsFall(); //���� ������ üũ

        if (!isDie)
        {
            if (canMove)
            {
                moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

                PlayerMove(); //�̵�
                PlayerJump(); //����
                PlayerClimb(); //�� Ÿ��
            }

            PlayerShootMoveWeb(); //�̵��� �Ź��� �߻�
            PlayerShootAttackWeb(); //���ݿ� �Ź��� �߻�

            ChangeAttackMode(); //���� ��� ����
        }

        animator.SetBool("isWalk", moveDir != Vector3.zero);
        animator.SetBool("isRun", moveDir != Vector3.zero && Input.GetAxis("Run") != 0);
        animator.SetBool("isJump", isJump);
        animator.SetBool("isClimb", isClimb);
        animator.SetBool("isMoveClimb", moveDir != Vector3.zero && isClimb);
        animator.SetBool("isInAir", isFall);
        animator.SetBool("isSwing", moveJoint);
    }

    void OnCollisionStay(Collision collision)
    {
        if(collision.GetComponent<Collider>().CompareTag("Enemy"))
        {
            if (!isDie)
            {
                //���� ������ �� ���
                isDie = true;
                animator.SetTrigger("isDie");

                resultScript.isGameOver = true;
                resultScript.StartCoroutine(nameof(resultScript.UiScript));
            }
        }
    }

    void CheckIsFall()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 0.1f))
        {
            //�ٴڿ� �������� ��
            if (hit.collider != null)
            {
                isFall = false;
                isJump = false;
                useDoubleJump = false;
            }
        }
        //���� ���� ��
        else if (hit.collider == null)
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
                playerDir = camForward * moveDir.z + camRight * moveDir.x; //�÷��̾��� ������ ī�޶��� ���Ͱ��� Ű���� �Է°��� ���� ����
                transform.forward = playerDir;

                //��ǥ �������� �̵�
                if (!moveJoint)
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
            else if (!useDoubleJump)
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
                if (hit.collider.CompareTag("Enemy"))
                    return;

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
        else if (isClimb && hit.collider == null)
        {
            isClimb = false;

            //�ڷ� ƨ�ܳ���
            rigidbody.AddForce(transform.up * 10, ForceMode.Impulse);
            isJump = true;
        }

        //���� ������ ���� �ƴ� ��
        if (!isClimb)
        {
            //�߷� Ȱ��ȭ
            rigidbody.useGravity = true;
        }
    }

    void PlayerShootMoveWeb()
    {
        //�Ź��ٷ� �̵� ���ΰ�
        if (moveJoint)
        {
            //�Ź��� �׸���
            moveLineRenderer.SetPosition(0, rightHand.position);
            moveLineRenderer.SetPosition(1, moveGrapPoint);
        }

        //�Ź��� �߻�
        if (Input.GetMouseButtonDown(1))
        {
            //���� ĳ������ ��ġ ���ϱ� (ī�޶� ��ġ + ī�޶�� ĳ���� ���� ����)
            Vector3 hitPos = camera.transform.position + camera.transform.rotation * new Vector3(0, 0, camera.GetComponent<CameraMove>().distance);

            //���� �Ź����� �߻��� �� �ִ� ������Ʈ�� ���� ��
            if (Physics.Raycast(hitPos, camera.transform.forward, out RaycastHit hit, 100))
            {
                if (!hit.collider.CompareTag("Enemy"))
                {
                    moveGrapPoint = hit.point; //�ش� ������Ʈ�� ��ġ ����

                    //SpiringJoint ������Ʈ �߰�
                    moveJoint = gameObject.AddComponent<SpringJoint>();
                    moveJoint.autoConfigureConnectedAnchor = false;
                    moveJoint.connectedAnchor = moveGrapPoint; //������Ʈ�� ����Ʈ ����

                    //����Ʈ�� �ִ�/�ּ� �Ÿ� ����
                    float distanceFromPoint = Vector3.Distance(rightHand.position, moveGrapPoint); //�÷��̾�� ������Ʈ �� �Ÿ� ã��
                    moveJoint.maxDistance = distanceFromPoint * 0.5f;
                    moveJoint.minDistance = distanceFromPoint * 0.25f;

                    moveJoint.spring = 4.5f;
                    moveJoint.damper = 7f;
                    moveJoint.massScale = 4.5f;

                    //�Ź��� ���η������� �������� ���� �� ���� ������
                    moveLineRenderer.positionCount = 2;
                }
            }
        }
        //�Ź��� ����
        else if (Input.GetMouseButtonUp(1))
        {
            moveLineRenderer.positionCount = 0;
            Destroy(moveJoint);
        }
    }

    void PlayerShootAttackWeb()
    {
        //�Ź��ٷ� ���� ���ΰ�
        if (attackJoint)
        {
            //���� ����ΰ�
            if(attackMode == 2)
            {
                if(attackMode2_EnemyScript == null)
                    attackMode2_EnemyScript = attackHitObj.GetComponent<EnemyScript>();

                attackMode2_EnemyScript.isFaint = true; //�� ���� ����

                attackGrapPoint = attackHitObj.transform.position; //�ش� ������Ʈ�� ��ġ ����
                attackJoint.connectedAnchor = transform.position; //������Ʈ ����Ʈ�� �÷��̾� ����

                //�Ź��� �׸���
                attackLineRenderer.SetPosition(0, leftHand.position);
                attackLineRenderer.SetPosition(1, attackGrapPoint);
            }
            //�ӻ� ����� ��
            else
            {
                //�Ź��� ����
                attackLineRenderer.positionCount = 0;
                Destroy(attackJoint);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            //���� ������ �߿��� ���� �Ұ�
            if (!isClimb)
            {
                switch (attackMode)
                {
                    case 1:
                        UseAttackMode1(); //�ӻ� ���
                        break;
                    case 2:
                        UseAttackMode2(); //���� ���
                        break;
                }
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            //���� ��� �� ���콺�� �� ��
            if(attackMode == 2 && attackJoint)
            {
                //�Ź��� ����
                attackLineRenderer.positionCount = 0;
                Destroy(attackJoint);

                if (attackHitObj != null)
                {
                    //���� �ð� �� ���� ����
                    attackMode2_EnemyScript.StartCoroutine(nameof(attackMode2_EnemyScript.DisableFaint), 3);

                    //ī�޶� �������� ������
                    Rigidbody enemyRigid = attackHitObj.GetComponent<Rigidbody>();
                    enemyRigid.AddForce(camera.transform.forward * 50, ForceMode.Impulse);

                    attackHitObj = null;
                    attackMode2_EnemyScript = null;
                }
            }
        }
    }

    void UseAttackMode1()
    {
        animator.SetTrigger("isShoot");

        GameObject web = Instantiate(mode1_web);

        //���� ĳ������ ��ġ ���ϱ� (ī�޶� ��ġ + ī�޶�� ĳ���� ���� ����)
        Vector3 charPos = camera.transform.position + camera.transform.rotation * new Vector3(0, 0, camera.GetComponent<CameraMove>().distance + 0.3f);
        web.transform.position = charPos;

        web.transform.rotation = camera.transform.rotation;
    }

    void UseAttackMode2()
    {
        //���� ĳ������ ��ġ ���ϱ� (ī�޶� ��ġ + ī�޶�� ĳ���� ���� ����)
        Vector3 hitPos = camera.transform.position + camera.transform.rotation * new Vector3(0, 0, camera.GetComponent<CameraMove>().distance);

        //���� �Ź����� �߻��� �� �ִ� ������Ʈ�� ���� ��
        if (Physics.Raycast(hitPos, camera.transform.forward, out RaycastHit hit, 100))
        {
            if(hit.collider.CompareTag("Enemy"))
            {
                if (!hit.collider.gameObject.GetComponent<EnemyScript>().isDie)
                {
                    animator.SetTrigger("isShoot");

                    attackHitObj = hit.collider.gameObject;

                    //SpiringJoint ������Ʈ �߰�
                    attackJoint = hit.collider.gameObject.AddComponent<SpringJoint>();
                    attackJoint.autoConfigureConnectedAnchor = false;

                    //����Ʈ�� �ִ�/�ּ� �Ÿ� ����
                    float distanceFromPoint = Vector3.Distance(leftHand.position, attackGrapPoint); //�÷��̾�� ������Ʈ �� �Ÿ� ã��
                    attackJoint.maxDistance = distanceFromPoint * 0.7f;
                    attackJoint.minDistance = distanceFromPoint * 0.5f;

                    attackJoint.spring = 4f;
                    attackJoint.damper = 5f;
                    attackJoint.massScale = 4.5f;

                    //�Ź��� ���η������� �������� ���� �� ���� ������
                    attackLineRenderer.positionCount = 2;
                }
            }
        }
    }

    void ChangeAttackMode()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            attackMode = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            attackMode = 2;
    }
}
