using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHit : MonoBehaviour
{
    public GameObject arrowPrefab;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Attack();
    }

    void Attack()
    {
        if (Input.GetButtonDown("Attack"))
        {
            // 默认碰撞框未勾选，攻击时启用
            //collider2D.enabled = true;
            Shoot();
        }
    }

    void Shoot()
    {
        Instantiate(arrowPrefab, transform.position, transform.rotation);
    }
}
