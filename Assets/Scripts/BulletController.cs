using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField]
    private GameObject bulletDecal;

    private float speed = 50f;
    private float timeToDestroy = 3f;

    public Vector3 target { get; set; }

    public bool hit { get; set; }


    void OnEnable()
    {
        Destroy(gameObject, timeToDestroy);
    }


    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (!hit && Vector3.Distance(transform.position, target) < 0.01f)
        {
            Destroy(gameObject);
        }
    }


    void OnCollisionEnter(Collision other)
    {
        var contact = other.GetContact(0);
        // ヒットしたオブジェクトの法線から向きを取得してデカールを生成
        Instantiate(bulletDecal, contact.point + contact.normal * 0.001f, Quaternion.LookRotation(contact.normal));

        Destroy(gameObject);
    }

}
