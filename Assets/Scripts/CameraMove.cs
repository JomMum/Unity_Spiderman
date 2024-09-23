using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] GameObject player;
    PlayerScript playerScript;

    float xAxis = 0;
    float yAxis = 0;

    public float turnSpd = 5; //마우스 회전 감도
    public float distance = 3; //플레이어와 카메라 사이의 간격

    private Vector3 originalPos;
    private float currentShakeDuration = 0f;

    void Awake()
    {
        playerScript = player.GetComponent<PlayerScript>();
    }

    void Update()
    {
        if (!playerScript.isDie)
        {
            SetCamera();

            // 흔들림이 발생할 때
            if (currentShakeDuration > 0)
            {
                transform.position += Random.insideUnitSphere * 0.01f; // 카메라 흔들림 추가
                currentShakeDuration -= Time.deltaTime;
            }
            else
            {
                currentShakeDuration = 0f;
                originalPos = transform.position;
            }
        }
    }

    void SetCamera()
    {
        xAxis += Input.GetAxis("Mouse X") * turnSpd; // 마우스 누적 좌우 이동량 저장
        yAxis += Input.GetAxis("Mouse Y") * turnSpd; // 마우스 누적 상하 이동량 저장

        yAxis = Mathf.Clamp(yAxis, -45, 80); //상하 회전 제한

        // 카메라 회전 설정
        transform.rotation = Quaternion.Euler(yAxis, xAxis, 0);

        Vector3 distanceVec = new Vector3(0.0f, 0.0f, distance); // 카메라와 플레이어 간격 계산
        transform.position = player.transform.position - transform.rotation * distanceVec;

        // 플레이어 머리 위에서 카메라 위치 설정
        transform.position = new Vector3(transform.position.x,
            transform.position.y + 1.4f,
            transform.position.z);
    }

    public void CameraShake()
    {
        currentShakeDuration = 0.2f;
    }
}