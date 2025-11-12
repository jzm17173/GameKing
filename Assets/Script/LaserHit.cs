using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 不直观
public class LaserHit : MonoBehaviour
{
    public Transform firePoint;
    public int damage = 1;
    public LineRenderer lineRenderer;
    public float attackRange = 10f; // 射击范围 100f不正常 10f正常
    public float attackInterval = 0.5f; // 攻击间隔时间

    private bool isAttacking = false; // 是否正在攻击
    private Coroutine attackCoroutine; // 攻击协程
    private Coroutine continuousAttackCoroutine; // 持续攻击协程

    // Start is called before the first frame update
    void Start()
    {
        // 检查LineRenderer是否已分配
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                Debug.LogError("LaserHit: LineRenderer未分配且未找到！");
            }
        }

        // 初始化LineRenderer
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
        }

        // 开始自动检测和攻击
        StartAutoAttack();
        Debug.Log("LaserHit: 自动攻击系统已启动，检测范围: " + attackRange);
    }

    // Update is called once per frame
    void Update()
    {
        // 不再需要手动攻击
    }

    /// <summary>
    /// 开始自动攻击检测
    /// </summary>
    void StartAutoAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AutoAttackCoroutine());
    }

    /// <summary>
    /// 自动攻击协程，持续检测范围内敌人并攻击
    /// </summary>
    IEnumerator AutoAttackCoroutine()
    {
        while (true)
        {
            // 检测范围内的敌人
            Enemy nearestEnemy = FindNearestEnemyInRange();

            if (nearestEnemy != null)
            {
                // 如果找到敌人，且当前没有在攻击，则开始攻击
                if (!isAttacking)
                {
                    isAttacking = true;
                    Debug.Log($"LaserHit: 开始攻击敌人 {nearestEnemy.name}，isAttacking: {isAttacking}");
                    // 停止之前的持续攻击协程（如果有）
                    if (continuousAttackCoroutine != null)
                    {
                        StopCoroutine(continuousAttackCoroutine);
                    }
                    continuousAttackCoroutine = StartCoroutine(ContinuousAttackCoroutine(nearestEnemy));
                }
            }
            else
            {
                // 如果没有敌人，停止攻击
                if (isAttacking)
                {
                    Debug.Log("LaserHit: 范围内无敌人，停止攻击");
                    // 停止持续攻击协程
                    if (continuousAttackCoroutine != null)
                    {
                        StopCoroutine(continuousAttackCoroutine);
                        continuousAttackCoroutine = null;
                    }
                    isAttacking = false;
                }
            }

            yield return new WaitForSeconds(0.1f); // 每0.1秒检测一次
        }
    }

    /// <summary>
    /// 持续攻击协程，间隔一段时间攻击敌人
    /// </summary>
    IEnumerator ContinuousAttackCoroutine(Enemy targetEnemy)
    {
        Debug.Log($"LaserHit: 持续攻击协程启动，目标: {targetEnemy?.name}");
        
        while (isAttacking)
        {
            // 重新检测敌人是否还在范围内
            Enemy currentEnemy = FindNearestEnemyInRange();
            if (currentEnemy == null)
            {
                Debug.Log("LaserHit: 持续攻击协程检测到无敌人，退出");
                isAttacking = false;
                continuousAttackCoroutine = null;
                yield break;
            }

            // 攻击敌人
            ShootAtEnemy(currentEnemy);

            // 等待攻击间隔
            yield return new WaitForSeconds(attackInterval);
        }
        
        Debug.Log("LaserHit: 持续攻击协程结束");
        continuousAttackCoroutine = null;
    }

    /// <summary>
    /// 在范围内查找最近的敌人
    /// </summary>
    Enemy FindNearestEnemyInRange()
    {
        // 使用 OverlapCircle 检测范围内的所有碰撞体
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        
        Enemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            // 检查是否是敌人
            if (col.CompareTag("Enemy"))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    float distance = Vector2.Distance(transform.position, col.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemy;
                    }
                }
            }
        }

        // 调试信息
        if (nearestEnemy != null)
        {
            Debug.Log($"LaserHit: 发现敌人 {nearestEnemy.name}，距离: {nearestDistance:F2}");
        }

        return nearestEnemy;
    }

    /// <summary>
    /// 向指定敌人射击
    /// </summary>
    void ShootAtEnemy(Enemy enemy)
    {
        if (enemy == null)
        {
            Debug.LogWarning("LaserHit: 尝试射击但敌人为null");
            return;
        }

        if (lineRenderer == null)
        {
            Debug.LogError("LaserHit: LineRenderer未分配，无法显示激光！");
            return;
        }

        Vector2 laserPosition = firePoint != null ? firePoint.position : transform.position;
        Vector2 enemyPosition = enemy.transform.position;
        Vector2 direction = (enemyPosition - laserPosition).normalized;

        Debug.Log($"LaserHit: 向敌人 {enemy.name} 发射激光，方向: {direction}");

        // 从激光位置向敌人位置发射射线
        RaycastHit2D hitInfo = Physics2D.Raycast(laserPosition, direction, attackRange);

        if (hitInfo)
        {
            Debug.Log($"LaserHit: 击中物体 {hitInfo.transform.name}");
            Enemy hitEnemy = hitInfo.transform.GetComponent<Enemy>();
            if (hitEnemy != null)
            {
            Debug.Log("LaserHit: 击中敌人2222"+hitEnemy.name);
                hitEnemy.TakeDamage(damage);
                Debug.Log($"LaserHit: 对 {hitEnemy.name} 造成 {damage} 点伤害");
            }

            // 设置激光线的起点和终点
            lineRenderer.SetPosition(0, laserPosition);
            lineRenderer.SetPosition(1, hitInfo.point);
        }
        else
        {
            // 如果没有击中任何物体，激光延伸到敌人位置或最大范围
            float distance = Vector2.Distance(laserPosition, enemyPosition);
            Vector2 endPoint = distance <= attackRange ? enemyPosition : laserPosition + direction * attackRange;
            
            lineRenderer.SetPosition(0, laserPosition);
            lineRenderer.SetPosition(1, endPoint);
            
            // 即使射线没击中，也尝试对敌人造成伤害（因为敌人已经在范围内）
            enemy.TakeDamage(damage);
            Debug.Log($"LaserHit: 对 {enemy.name} 造成 {damage} 点伤害（直接攻击）");
        }

        lineRenderer.enabled = true;
        StartCoroutine(DisableLineRenderer());
    }

    /// <summary>
    /// 禁用激光渲染器
    /// </summary>
    IEnumerator DisableLineRenderer()
    {
        yield return new WaitForSeconds(0.02f);
        lineRenderer.enabled = false;
    }

    /// <summary>
    /// 在编辑器中绘制检测范围（可选，用于调试）
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
