using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserHit : MonoBehaviour
{
    public Transform firePoint;
    public int damage = 1;
    public LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Attack();
    }

    /// <summary>
    /// 攻击方法，检测玩家是否按下攻击键
    /// </summary>
    void Attack()
    {
        // 检测玩家是否按下攻击键
        if (Input.GetButtonDown("Attack"))
        {
            // 默认碰撞框未勾选，攻击时启用（当前已注释）
            //collider2D.enabled = true;
            // 执行射击
            Shoot();
        }
    }

    void Shoot()
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(firePoint.position, firePoint.right);

        if (hitInfo)
        {
            Debug.Log(hitInfo.transform.name);
            Enemy enemy = hitInfo.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // LaserHit要拖到希望的激光发射位置上 lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, hitInfo.point);
        }
        else
        {
            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, firePoint.position + firePoint.right * 100);
        }

        lineRenderer.enabled = true;

        StartCoroutine(DisableLineRenderer());
    }

    IEnumerator DisableLineRenderer()
    {
        yield return new WaitForSeconds(0.02f);
        lineRenderer.enabled = false;
    }
}
