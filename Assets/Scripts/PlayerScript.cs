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

    GameObject attackHitObj; 

    SpringJoint moveJoint; //이동용 거미줄 조인트
    SpringJoint attackJoint; //공격용 거미줄 조인트 (전투 모드)

    Rigidbody rigidbody;
    Animator animator;

    LineRenderer moveLineRenderer; //이동용 거미줄 라인 렌더러
    LineRenderer attackLineRenderer; //공격용 거미줄 라인 렌더러 (전투 모드)

    Vector3 moveDir; //키보드 입력 방향
    Vector3 playerDir; //입력 방향과 카메라 방향을 조합한 최종 이동 방향
    Vector3 moveGrapPoint; //이동용 거미줄 발사 방향
    Vector3 attackGrapPoint; //공격용 거미줄 발사 방향

    public bool canMove = true;
    bool useDoubleJump;

    bool isJump;
    bool isClimb;
    bool isFall;

    public int moveSpd = 5; //이동속도
    public int runSpd = 10; //달리기 속도
    public int climbSpd = 3; //기어가는 속도
    public int jumpPower; //점프력

    public int attackMode = 1; //공격 모드


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
        //플레이어 모델 자체는 위치가 변경되지 않는다
        playerModel.transform.localPosition = new Vector3(0, 0, 0);

        CheckIsFall(); //낙하 중인지 체크

        if (canMove)
        {
            moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            PlayerMove(); //이동
            PlayerJump(); //점프
            PlayerClimb(); //벽 타기
        }

        PlayerShootMoveWeb(); //이동용 거미줄 발사
        PlayerShootAttackWeb(); //공격용 거미줄 발사

        ChangeAttackMode(); //공격 모드 변경

        animator.SetBool("isWalk", moveDir != Vector3.zero);
        animator.SetBool("isRun", moveDir != Vector3.zero && Input.GetAxis("Run") != 0);
        animator.SetBool("isJump", isJump);
        animator.SetBool("isClimb", isClimb);
        animator.SetBool("isMoveClimb", moveDir != Vector3.zero && isClimb);
        animator.SetBool("isInAir", isFall);
        animator.SetBool("isSwing", moveJoint);
    }

    void CheckIsFall()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 0.1f))
        {
            //바닥에 착지했을 시
            if (hit.collider != null)
            {
                isFall = false;
                isJump = false;
                useDoubleJump = false;
            }
        }
        //낙하 중일 시
        else if (hit.collider == null)
        {
            isFall = true;
        }
    }

    void PlayerMove()
    {
        if (!isClimb)
        {
            //이동 중일 시
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                //이동 방향으로 플레이어 회전
                Vector3 camForward = new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z).normalized;
                Vector3 camRight = new Vector3(camera.transform.right.x, 0, camera.transform.right.z).normalized;
                playerDir = camForward * moveDir.z + camRight * moveDir.x; //플레이어의 방향은 카메라의 벡터값과 키보드 입력값을 통해 구함
                transform.forward = playerDir;

                //목표 지점으로 이동
                if (!moveJoint)
                {
                    if (Input.GetAxis("Run") != 0) //달리기
                        transform.position += playerDir * runSpd * Time.deltaTime;
                    else //걷기
                        transform.position += playerDir * moveSpd * Time.deltaTime;
                }
                //거미줄로 목표 지점 이동
                else
                {
                    if (Input.GetAxis("Run") != 0) //빠른 거미줄 이동
                        transform.position += playerDir * runSpd * 2f * Time.deltaTime;
                    else //거미줄 이동
                        transform.position += playerDir * moveSpd * 2f * Time.deltaTime;
                }
            }
        }
    }

    void PlayerJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //벽 오르는 중일 시
            if (isClimb)
            {
                isClimb = false;
                isJump = true;

                //뒤로 튕겨나감
                rigidbody.AddForce(-transform.forward * 3, ForceMode.Impulse);

                return;
            }

            //1단 점프
            if (!isJump)
            {
                isJump = true;
                rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                return;
            }
            //2단 점프
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
        Vector3 raycastPos = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z); //플레이어가 바라보는 방향 (y값이 발에 맞춰져 있으므로 값을 조금 더해줌)

        //레이캐스트를 통해 벽 찾기
        if (Physics.Raycast(raycastPos, transform.forward, out RaycastHit hit, 0.3f))
        {
            //벽이 있을 경우
            if (hit.collider != null)
            {
                if (!isClimb)
                {
                    //점프 초기화
                    isJump = false;
                    useDoubleJump = false;

                    rigidbody.velocity = Vector3.zero; //AddForce 값 초기화
                    transform.rotation = Quaternion.LookRotation(-hit.normal); //벽의 반대 방향으로 캐릭터회전

                    rigidbody.useGravity = false; //벽을 오르는 중에는 중력의 영향을 받지 않음
                }

                isClimb = true;

                //벽 이동
                Vector3 playerDir = transform.up * moveDir.z + transform.right * moveDir.x; //플레이어의 방향은 캐릭터의 벡터값과 키보드 입력값을 통해 구함
                transform.position += playerDir * climbSpd * Time.deltaTime;
            }
        }
        //벽이 없을 경우
        else if (isClimb && hit.collider == null)
        {
            isClimb = false;

            //뒤로 튕겨나감
            rigidbody.AddForce(transform.up * 10, ForceMode.Impulse);
            isJump = true;
        }

        //벽을 오르는 중이 아닐 시
        if (!isClimb)
        {
            //중력 활성화
            rigidbody.useGravity = true;
        }
    }

    void PlayerShootMoveWeb()
    {
        //거미줄로 이동 중인가
        if (moveJoint)
        {
            //거미줄 그리기
            moveLineRenderer.SetPosition(0, rightHand.position);
            moveLineRenderer.SetPosition(1, moveGrapPoint);

            //만약 착지했을 시
            if (!isFall)
            {
                //거미줄 삭제
                moveLineRenderer.positionCount = 0;
                Destroy(moveJoint);

                return;
            }
        }

        //거미줄 발사
        if (Input.GetMouseButtonDown(1))
        {
            //현재 캐릭터의 위치 구하기 (카메라 위치 + 카메라와 캐릭터 사이 간격)
            Vector3 hitPos = camera.transform.position + camera.transform.rotation * new Vector3(0, 0, camera.GetComponent<CameraMove>().distance);

            //만약 거미줄을 발사할 수 있는 오브젝트가 있을 시
            if (Physics.Raycast(hitPos, camera.transform.forward, out RaycastHit hit, 100))
            {
                moveGrapPoint = hit.point; //해당 오브젝트의 위치 저장

                //SpiringJoint 컴포넌트 추가
                moveJoint = gameObject.AddComponent<SpringJoint>();
                moveJoint.autoConfigureConnectedAnchor = false;
                moveJoint.connectedAnchor = moveGrapPoint; //오브젝트와 조인트 연결

                //조인트의 최대/최소 거리 지정
                float distanceFromPoint = Vector3.Distance(rightHand.position, moveGrapPoint); //플레이어와 오브젝트 간 거리 찾기
                moveJoint.maxDistance = distanceFromPoint * 0.5f;
                moveJoint.minDistance = distanceFromPoint * 0.25f;

                moveJoint.spring = 4.5f;
                moveJoint.damper = 7f;
                moveJoint.massScale = 4.5f;

                //거미줄 라인렌더러는 시작점과 끝점 두 개만 존재함
                moveLineRenderer.positionCount = 2;
            }
        }
        //거미줄 해제
        else if (Input.GetMouseButtonUp(1))
        {
            moveLineRenderer.positionCount = 0;
            Destroy(moveJoint);
        }
    }

    void PlayerShootAttackWeb()
    {
        //거미줄로 공격 중인가
        if (attackJoint)
        {
            //전투 모드인가
            if(attackMode == 2)
            {
                attackGrapPoint = attackHitObj.transform.position; //해당 오브젝트의 위치 저장
                attackJoint.connectedAnchor = transform.position; //오브젝트 조인트와 플레이어 연결

                //거미줄 그리기
                attackLineRenderer.SetPosition(0, leftHand.position);
                attackLineRenderer.SetPosition(1, attackGrapPoint);
            }
            //속사 모드일 시
            else
            {
                //거미줄 삭제
                attackLineRenderer.positionCount = 0;
                Destroy(attackJoint);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            //벽을 오르는 중에는 공격 불가
            if (!isClimb)
            {
                switch (attackMode)
                {
                    case 1:
                        UseAttackMode1(); //속사 모드
                        break;
                    case 2:
                        UseAttackMode2(); //전투 모드
                        break;
                }
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            //전투 모드 중 마우스를 뗄 시
            if(attackMode == 2 && attackJoint)
            {
                //거미줄 해제
                attackLineRenderer.positionCount = 0;
                Destroy(attackJoint);

                if (attackHitObj != null)
                {
                    //일정 시간동안 기절
                    EnemyScript enemyScript = attackHitObj.GetComponent<EnemyScript>();
                    enemyScript.isFaint = true;
                    enemyScript.StartCoroutine("DisableFaint");

                    //카메라 방향으로 날리기
                    Rigidbody enemyRigid = attackHitObj.GetComponent<Rigidbody>();
                    enemyRigid.AddForce(camera.transform.forward * 50, ForceMode.Impulse);
                }
            }
        }
    }

    void UseAttackMode1()
    {
        animator.SetTrigger("isShoot");

        GameObject web = Instantiate(mode1_web);

        //현재 캐릭터의 위치 구하기 (카메라 위치 + 카메라와 캐릭터 사이 간격)
        Vector3 charPos = camera.transform.position + camera.transform.rotation * new Vector3(0, 0, camera.GetComponent<CameraMove>().distance + 0.3f);
        web.transform.position = charPos;

        web.transform.rotation = camera.transform.rotation;
    }

    void UseAttackMode2()
    {
        //현재 캐릭터의 위치 구하기 (카메라 위치 + 카메라와 캐릭터 사이 간격)
        Vector3 hitPos = camera.transform.position + camera.transform.rotation * new Vector3(0, 0, camera.GetComponent<CameraMove>().distance);

        //만약 거미줄을 발사할 수 있는 오브젝트가 있을 시
        if (Physics.Raycast(hitPos, camera.transform.forward, out RaycastHit hit, 100))
        {
            if(hit.collider.CompareTag("Enemy"))
            {
                animator.SetTrigger("isShoot");

                attackHitObj = hit.collider.gameObject;

                //SpiringJoint 컴포넌트 추가
                attackJoint = hit.collider.gameObject.AddComponent<SpringJoint>();
                attackJoint.autoConfigureConnectedAnchor = false;

                //조인트의 최대/최소 거리 지정
                float distanceFromPoint = Vector3.Distance(leftHand.position, attackGrapPoint); //플레이어와 오브젝트 간 거리 찾기
                attackJoint.maxDistance = distanceFromPoint * 0.7f;
                attackJoint.minDistance = distanceFromPoint * 0.5f;

                attackJoint.spring = 4f;
                attackJoint.damper = 5f;
                attackJoint.massScale = 4.5f;

                //거미줄 라인렌더러는 시작점과 끝점 두 개만 존재함
                attackLineRenderer.positionCount = 2;
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
