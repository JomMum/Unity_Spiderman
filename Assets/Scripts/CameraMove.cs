using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] GameObject player;

    float xAxis = 0;
    float yAxis = 0;

    public float turnSpd = 5; //���콺 ȸ�� ����
    public float distance = 3; //�÷��̾�� ī�޶� ������ ����

    void Update()
    {
        xAxis += Input.GetAxis("Mouse X") * turnSpd; // ���콺 ���� �¿� �̵��� ����
        yAxis += Input.GetAxis("Mouse Y") * turnSpd; // ���콺 ���� ���� �̵��� ����

        yAxis = Mathf.Clamp(yAxis, -45, 80); //���� ȸ�� ����

        transform.rotation = Quaternion.Euler(yAxis, xAxis, 0); // �̵����� ���� ī�޶� ���� ����
        Vector3 distanceVec = new Vector3(0.0f, 0.0f, distance); // �̵����� ���� �÷��̾�� ī�޶��� ���� ���ϱ�

        // �÷��̾� ��ġ - ī�޶� ���⿡ ������ ������ ��� ��ǥ
        transform.position = player.transform.position - transform.rotation * distanceVec;

        // ī�޶� y���� �÷��̾� �Ӹ��� �������� ���߰� ��
        transform.position = new Vector3(transform.position.x,
            transform.position.y + 1.5f,
            transform.position.z);
    }
}