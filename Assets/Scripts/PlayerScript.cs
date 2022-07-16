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

    Vector3 moveDir; //이동 방향
    Vector3 grapPoint; //이동용 거미줄 발사 방향
    public bool canMove = true;
    bool useDoubleJump;

    bool isJump;
    bool isClimb;
    bool isFall;

    public int moveSpd = 5; //이동속도
    public int runSpd = 10; //달리기 속도
    public int climbSpd = 3; //기어가는 속도
    public int jumpPower; //점프력


    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = playerModel.GetComponent<Animator>();
        lineRenderer = GetComponent<LineRenderer>();
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
            //바닥에 착지했을 시
            if(hit.collider != null)
            {
                isFall = false;
                isJump = false;
                useDoubleJump = false;
            }
        }
        //낙하 중일 시
        else if(hit.collider == null)
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
                Vector3 playerDir = camForward * moveDir.z + camRight * moveDir.x; //플레이어의 방향은 카메라의 벡터값과 키보드 입력값을 통해 구함
                transform.forward = playerDir;

                //목표 지점으로 이동
                if (!joint)
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
        else if(isClimb && hit.collider == null)
        {
            isClimb = false;

            //뒤로 튕겨나감
            rigidbody.AddForce(transform.up * 10, ForceMode.Impulse);
            isJump = true;
        }

        //벽을 오르는 중이 아닐 시
        if(!isClimb)
        {
            //중력 활성화
            rigidbody.useGravity = true;
        }
    }

    void PlayerShootMoveWeb()
    {
        //거미줄로 이동 중인가
        if (joint)
        {
            //거미줄 그리기
            lineRenderer.SetPosition(0, rightHand.position);
            lineRenderer.SetPosition(1, grapPoint);

            //만약 착지했을 시
            if(!isFall)
            {
                //거미줄 삭제
                lineRenderer.positionCount = 0;
                Destroy(joint);

                return;
            }
        }

        //거미줄 발사
        if (Input.GetMouseButtonDown(1))
        {
            //현재 캐릭터의 위치 구하기 (카메라 위치 + 카메라와 캐릭터 사이 간격)
            Vector3 hitPos = camera.transform.position + camera.transform.rotation * new Vector3(0.0f, 0.0f, camera.GetComponent<CameraMove>().distance);

            //만약 거미줄을 발사할 수 있는 오브젝트가 있을 시
            if (Physics.Raycast(hitPos, camera.transform.forward, out RaycastHit hit, 100))
            {
                grapPoint = hit.point; //해당 오브젝트의 위치 저장

                //SpiringJoint 컴포넌트 추가
                joint = gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grapPoint; //오브젝트와 조인트 연결

                //조인트의 최대/최소 거리 지정
                float distanceFromPoint = Vector3.Distance(rightHand.position, grapPoint); //플레이어와 오브젝트 간 거리 찾기
                joint.maxDistance = distanceFromPoint * 0.5f;
                joint.minDistance = distanceFromPoint * 0.25f;

                joint.spring = 4.5f;     
                joint.damper = 7f;      
                joint.massScale = 4.5f;

                //거미줄 라인렌더러는 시작점과 끝점 두 개만 존재함
                lineRenderer.positionCount = 2;
            }
        }
        //거미줄 해제
        else if (Input.GetMouseButtonUp(1))
        {
            lineRenderer.positionCount = 0;
            Destroy(joint);
        }
    }
}
