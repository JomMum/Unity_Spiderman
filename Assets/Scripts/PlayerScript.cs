using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.SceneView;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] GameObject camera;
    [SerializeField] GameObject playerModel;

    [SerializeField] Transform rightHand;
    [SerializeField] Transform leftHand;
    
    [SerializeField] GameObject mode1_web;
    [SerializeField] GameObject mode2_web;

    [SerializeField] OverSCript resultScript;
    CameraMove cameraMove;

    GameObject attackHitObj;
    EnemyScript attackMode2_EnemyScript;

    SpringJoint moveJoint; //�̵��� �Ź��� ����Ʈ
    SpringJoint attackJoint; //���ݿ� �Ź��� ����Ʈ (���� ���)

    Rigidbody rigidbody;
    Animator animator;
    AnimatorStateInfo stateInfo;

    LineRenderer moveLineRenderer; //�̵��� �Ź��� ���� ������
    LineRenderer attackLineRenderer; //���ݿ� �Ź��� ���� ������ (���� ���)

    Vector3 moveDir; //Ű���� �Է� ����
    Vector3 playerDir; //�Է� ����� ī�޶� ������ ������ ���� �̵� ����
    Vector3 moveGrapPoint; //�̵��� �Ź��� �߻� ����
    Vector3 attackGrapPoint; //���ݿ� �Ź��� �߻� ����

    public bool canMove = true;
    bool canClimb = true;
    private bool canAttack = true;
    bool canJump = true;
    bool canDoubleJump = false;

    public bool isJump;
    public bool isDoubleJump;
    public bool isClimb;
    bool isRun;
    bool isFall;
    bool isBackJump; // 벽을 기다가 하는 점프
    public bool isDie;
    public bool rotateToCamera;
    bool wasRunningBeforeAir;

    public int moveSpd = 5; //�̵��ӵ�
    public int runSpd = 10; //�޸��� �ӵ�
    public int climbSpd = 3; //���� �ӵ�
    public int jumpPower = 10;

    public int attackMode = 1; //���� ���
    bool isAttackUsingLeftHand = true; // 어떤 손으로 공격할 지 설정

    private float attackCooldown = 0.7f; // 0.3초 쿨다운 시간
    private float cooldownTimer = 0f;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = playerModel.GetComponent<Animator>();
        moveLineRenderer = GetComponent<LineRenderer>();
        attackLineRenderer = mode2_web.GetComponent<LineRenderer>();
        cameraMove = camera.GetComponent<CameraMove>();

        Cursor.visible = false;
    }

    void Update()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        playerModel.transform.localPosition = new Vector3(0, 0, 0);

        if (stateInfo.IsName("Landing") || stateInfo.IsName("ClimbUp"))
        {
            canJump = true;
            canDoubleJump = false;

            isJump = false;
            isDoubleJump = false;
            canMove = false;
        }
        else if (isBackJump)
        {
            canMove = false;
        }
        else
        {
            canMove = true;
        }

        CheckIsFall();
        PlayerRotate();

        if (!isDie)
        {
            if (canMove == true)
            {
                moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

                PlayerMove();
                PlayerJump();
                PlayerClimb();
            }

            PlayerShootMoveWeb(); //�̵��� �Ź��� �߻�
            PlayerShootAttackWeb(); //���ݿ� �Ź��� �߻�

            ChangeAttackMode(); //���� ��� ����
        }

        animator.SetBool("isWalk", moveDir != Vector3.zero);
        animator.SetBool("isRun", moveDir != Vector3.zero && isRun);
        animator.SetBool("isJump", isJump);
        animator.SetBool("isDoubleJump", isDoubleJump);
        animator.SetBool("isSwing", moveJoint);

        // 백점프 동안에는 이하 애니메이션 무시
        animator.SetBool("isClimb", isClimb && !isBackJump);
        animator.SetBool("isMoveClimb", moveDir != Vector3.zero && isClimb && !isBackJump);
        animator.SetBool("isInAir", isFall && !isBackJump);
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
        // 충돌체 위에 서 있을 경우
        Vector3[] rayOrigins = new Vector3[]
        {
            transform.position,
            transform.position + Vector3.forward * 0.5f,
            transform.position + Vector3.back * 0.5f
        };

        bool isGrounded = false;
        foreach (var origin in rayOrigins)
        {
            if (Physics.Raycast(origin, -transform.up, out RaycastHit hit, 0.8f))
            {
                isGrounded = true;
                break;
            }
        }

        // 벽을 오르거나 착지 중일 때는 낙하 상태로 인식 안 함
        if (isClimb || stateInfo.IsName("Landing"))
        {
            isFall = false;
        }
        else
        {
            isFall = !isGrounded;
        }
    }

    void PlayerRotate()
    {
        if (rotateToCamera)
        {
            Quaternion targetRotation = Quaternion.Euler(0, camera.transform.rotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);

            // 각도 차이가 임계값(rotationThreshold) 이하이면 회전 완료로 간주
            float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
            if (angleDifference < 0.1f)
            {
                rotateToCamera = false;
            }
        }
    }

    void PlayerMove()
    {
        if (!isClimb)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                // 카메라 바라보는 방향
                Vector3 camForward = new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z).normalized;
                Vector3 camRight = new Vector3(camera.transform.right.x, 0, camera.transform.right.z).normalized;

                // 이동 방향 지정
                playerDir = camForward * moveDir.z + camRight * moveDir.x;

                // 캐릭터가 바라보는 각도 지정 (카메라 기준)
                Vector3 newForward = new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z).normalized;
                transform.forward = newForward;

                isRun = Input.GetAxis("Run") != 0 && Input.GetAxis("Vertical") > 0;

                // 공중에 있기 전부터 달리고 있었는가
                if (!isFall)
                {
                    if (isRun)
                    {
                        wasRunningBeforeAir = true;
                    } else
                    {
                        wasRunningBeforeAir = false;
                    }
                }
                else if (Input.GetAxis("Run") == 0)
                {
                    wasRunningBeforeAir = false; // 전에 달리고 있었더라도, 낙하 중에 달리기 취소하면 착지 전까지 다시 달리기 불가
                }

                bool checkRun = isRun && (!isFall || wasRunningBeforeAir);
                if (!moveJoint)
                {
                    if (checkRun)
                    {
                        transform.position += playerDir * runSpd * Time.deltaTime;
                    }
                    else
                    {
                        transform.position += playerDir * moveSpd * Time.deltaTime;
                    }
                }
                else
                {
                    if (checkRun)
                    {
                        transform.position += playerDir * runSpd * 1.7f * Time.deltaTime;
                    }
                    else
                    {
                        transform.position += playerDir * moveSpd * 1.7f * Time.deltaTime;
                    }
                }
            }
        }
    }

    void PlayerJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 기어 오르는 경우
            if (!stateInfo.IsName("ClimbUp") && isClimb)
            {
                // 뒤로 점프
                canMove = false;

                canClimb = false;
                isClimb = false;

                isDoubleJump = true;
                isBackJump = true;

                rigidbody.velocity = Vector3.zero;
                rigidbody.AddForce(-transform.forward * 3 + Vector3.up * (jumpPower / 2), ForceMode.Impulse);

                StartCoroutine(EndBackJump());
                return;
            } 

            // 점프
            if (canJump)
            {
                canJump = false;
                canDoubleJump = true;

                isJump = true;
                rigidbody.velocity = Vector3.zero;
                rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                return;
            }
            // 2단 점프
            else if (canDoubleJump)
            {
                canDoubleJump = false;

                isJump = false;
                isDoubleJump = true;

                rigidbody.velocity = Vector3.zero;
                rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                return;
            }
        }
    }

    IEnumerator EndBackJump()
    {
        yield return new WaitForSeconds(0.5f);

        canMove = true;
        canClimb = true;
        isBackJump = false;
    }

    void PlayerClimb()
    {
        if (!canClimb) return;

        // 기어오를 때는 중력 무시
        rigidbody.useGravity = !isClimb;

        bool disableClimb = false;

        Vector3 direction = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * Vector3.forward;

        // 앞에 벽이 있는지 확인
        Vector3 raycastPos = new Vector3(transform.position.x, transform.position.y + 1.2f, transform.position.z);
        if (Physics.Raycast(raycastPos, direction, out RaycastHit hit, 0.3f))
        {
            if (hit.collider != null)
            {
                // 벽 외의 오브젝트와 접촉 시 무시
                if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Player") || hit.collider.CompareTag("Web"))
                {
                    disableClimb = true;
                }
                // 벽에 접촉 시 기어오르기
                else
                {
                    // 건물 위쪽일 시 넘기
                    Vector3 raycastUpPos = new Vector3(transform.position.x, transform.position.y + 1.6f, transform.position.z);
                    if (!Physics.Raycast(raycastUpPos, direction, 0.3f))
                    {
                        animator.SetTrigger("doClimbUp");
                        return;
                    }
                    // 벽 기어 오르기
                    else
                    {
                        // 기어오르기 초기 설정
                        if (!isClimb)
                        {
                            canJump = false;
                            canDoubleJump = false;

                            isJump = false;
                            isDoubleJump = false;

                            rigidbody.velocity = Vector3.zero;
                            transform.rotation = Quaternion.LookRotation(-hit.normal);

                            rigidbody.useGravity = false;
                        }

                        isClimb = true;

                        Vector3 playerDir = transform.up * moveDir.z + transform.right * moveDir.x;
                        transform.position += playerDir * climbSpd * Time.deltaTime;
                    }
                }
            }
            // 접촉한 오브젝트가 없을 시 무시
            else
            {
                disableClimb = true;
            }
        }
        else
        {
            disableClimb = true;
        }
        
        // 기어오르기 비활성화
        if (isClimb && disableClimb)
        {
            isClimb = false;
            canJump = false;
            isJump = true;
            canDoubleJump = true;

            // 건물 양옆 끝에 다다를 경우 점프
            rigidbody.AddForce(transform.up * 5, ForceMode.Impulse);
        }
    }

    // 거미줄 이동
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
            Vector3 hitPos = camera.transform.position + camera.transform.rotation * new Vector3(0, 0, 60);

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

    // 거미줄 공격
    void PlayerShootAttackWeb()
    {
        AttackCoolDown();

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

        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            if (!isClimb)
            {
                canAttack = false;

                // 공격 손 설정. 웹스윙 중에는 왼손만 사용.
                if (moveJoint)
                {
                    isAttackUsingLeftHand = true;
                }
                else
                {
                    isAttackUsingLeftHand = !isAttackUsingLeftHand;
                }

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

    // 공격 쿨타임
    void AttackCoolDown()
    {
        if (!canAttack)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= attackCooldown)
            {
                canAttack = true;
                cooldownTimer = 0f;
            }
        }
    }

    void UseAttackMode1()
    {
        if (isAttackUsingLeftHand)
        {
            animator.SetTrigger("doShootLeft");
        }else
        {
            animator.SetTrigger("doShootRight");
        }

        rotateToCamera = true;

        GameObject web = Instantiate(mode1_web);

        Vector3 charPos = leftHand.transform.position;
        web.transform.position = charPos;
        web.transform.rotation = camera.transform.rotation;
    }

    void RotatePlayerTowardsCamera()
    {
        // 카메라의 회전 값을 가져와 Y축만 적용
        Quaternion targetRotation = Quaternion.Euler(0, camera.transform.rotation.eulerAngles.y, 0);

        // 현재 플레이어 회전을 타겟 회전으로 Slerp(서서히 회전)
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
    }

    void UseAttackMode2()
    {
        //���� ĳ������ ��ġ ���ϱ� (ī�޶� ��ġ + ī�޶�� ĳ���� ���� ����)
        Vector3 hitPos = camera.transform.position + camera.transform.rotation * new Vector3(0, 0, 60);

        //���� �Ź����� �߻��� �� �ִ� ������Ʈ�� ���� ��
        if (Physics.Raycast(hitPos, camera.transform.forward, out RaycastHit hit, 100))
        {
            if(hit.collider.CompareTag("Enemy"))
            {
                if (!hit.collider.gameObject.GetComponent<EnemyScript>().isDie)
                {
                    if (isAttackUsingLeftHand)
                    {
                        animator.SetTrigger("doShootLeft");
                    } else
                    {
                        animator.SetTrigger("doShootRight");
                    }
                    

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
