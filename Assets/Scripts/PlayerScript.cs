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
    bool isClimb;

    public int moveSpd = 5; //이동속도
    public int runSpd = 10; //달리기 속도
    public int climbSpd = 3; //기어가는 속도
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
            moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            PlayerMove(); //이동
            PlayerJump(); //점프
            PlayerClimb(); //벽 타기
        }

        animator.SetBool("isWalk", moveDir != Vector3.zero);
        animator.SetBool("isRun", moveDir != Vector3.zero && Input.GetAxis("Run") != 0);
        animator.SetBool("isJump", isJump);
        animator.SetBool("isClimb", isClimb);
        animator.SetBool("isMoveClimb", moveDir != Vector3.zero && isClimb);
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
                if (Input.GetAxis("Run") != 0) //달리기
                    transform.position += playerDir * runSpd * Time.deltaTime;
                else //걷기
                    transform.position += playerDir * moveSpd * Time.deltaTime;
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
}
