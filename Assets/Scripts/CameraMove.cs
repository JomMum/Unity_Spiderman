using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] GameObject player;

    float xAxis = 0;
    float yAxis = 0;

    public float turnSpd = 5; //마우스 회전 감도
    public float distance = 3; //플레이어와 카메라 사이의 간격

    void Update()
    {
        xAxis += Input.GetAxis("Mouse X") * turnSpd; // 마우스 누적 좌우 이동량 저장
        yAxis += Input.GetAxis("Mouse Y") * turnSpd; // 마우스 누적 상하 이동량 저장

        yAxis = Mathf.Clamp(yAxis, -45, 80); //상하 회전 제한

        transform.rotation = Quaternion.Euler(yAxis, xAxis, 0); // 이동량에 따라 카메라 방향 조정
        Vector3 distanceVec = new Vector3(0.0f, 0.0f, distance); // 이동량에 따른 플레이어와 카메라의 간격 구하기

        // 플레이어 위치 - 카메라 방향에 간격을 적용한 상대 좌표
        transform.position = player.transform.position - transform.rotation * distanceVec;

        // 카메라 y축은 플레이어 머리를 기준으로 비추게 함
        transform.position = new Vector3(transform.position.x,
            transform.position.y + 1.5f,
            transform.position.z);
    }
}