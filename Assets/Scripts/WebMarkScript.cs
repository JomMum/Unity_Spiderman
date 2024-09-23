using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebMarkScript : MonoBehaviour
{
    [SerializeField] Sprite[] webMarkList;
    Animation anim;

    void Awake()
    {
        anim = GetComponent<Animation>();

        int randomIndex = Random.Range(0, webMarkList.Length);
        gameObject.GetComponent<SpriteRenderer>().sprite = webMarkList[randomIndex];
    }

    void Start()
    {
        StartCoroutine(DestroyWebMark());
    }

    IEnumerator DestroyWebMark()
    {
        //일정 시간 후 거미줄 삭제
        yield return new WaitForSeconds(3);

        anim.Play();

        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
}
