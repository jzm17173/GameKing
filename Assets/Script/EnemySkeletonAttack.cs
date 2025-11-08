using UnityEngine;

public class EnemySkeletonAttack : MonoBehaviour
{
    [Header("攻击设置")]
    public int attackDamage = 1;           // 攻击伤害

    [Header("攻击检测")]
    public bool canAttack = false;          // 是否可以攻击
    public bool isAttacking = false;        // 是否正在攻击

    // 组件引用
    private EnemySkeletonAI enemyAI;        // 敌人AI脚本引用
    private PolygonCollider2D weaponCollider; // 武器碰撞体

    // 私有变量
    private bool hasDealtDamage = false;    // 是否已造成伤害

    void Start()
    {
        // 获取碰撞组件
        weaponCollider = GetComponent<PolygonCollider2D>();

        // 获取父对象的EnemySkeletonAI脚本
        enemyAI = GetComponentInParent<EnemySkeletonAI>();
        if (enemyAI == null)
        {
            Debug.LogError("在父对象中未找到EnemySkeletonAI脚本！");
        }

        // 初始时禁用武器碰撞体
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }

    void Update()
    {
        // 从AI脚本获取攻击状态
        //UpdateAttackStateFromAI();

        // 控制武器碰撞体的启用/禁用
        UpdateWeaponCollider();
    }

    // 从AI脚本更新攻击状态
    //private void UpdateAttackStateFromAI()
    //{
    //    if (enemyAI != null)
    //    {
    //        // 可以根据需要从AI脚本获取信息
    //    }
    //}

    // 更新武器碰撞体状态
    private void UpdateWeaponCollider()
    {
        if (weaponCollider != null)
        {
            // 只有当正在攻击且可以造成伤害时才启用碰撞体
            weaponCollider.enabled = isAttacking && canAttack && !hasDealtDamage;
        }
    }

    // 攻击开始 - 在动画开始时调用
    public void OnAttackStart()
    {
        isAttacking = true;
        canAttack = true;
        hasDealtDamage = false;

        Debug.Log("攻击开始 - 碰撞体启用");
    }

    // 攻击有效帧 - 在武器挥出的关键时刻调用
    public void OnAttackActive()
    {
        canAttack = true;
        hasDealtDamage = false;

        Debug.Log("攻击有效帧 - 可以造成伤害");
    }

    // 攻击结束 - 在动画结束时调用
    public void OnAttackEnd()
    {
        isAttacking = false;
        canAttack = false;
        hasDealtDamage = false;

        Debug.Log("攻击结束 - 碰撞体禁用");
    }

    // 碰撞检测
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查是否碰到玩家且可以攻击且尚未造成伤害
        if (canAttack && isAttacking && !hasDealtDamage && IsPlayer(collision))
        {
            DealDamageToPlayer(collision);
        }
    }

    // 检查碰撞对象是否是玩家
    private bool IsPlayer(Collider2D collision)
    {
        return collision.gameObject.CompareTag("Player") && collision.GetType().ToString() == "UnityEngine.PolygonCollider2D";
    }

    // 对玩家造成伤害
    private void DealDamageToPlayer(Collider2D playerCollider)
    {
        // 获取玩家的生命值组件
        PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // 造成伤害
            playerHealth.DamegePlayer(attackDamage);

            // 标记已造成伤害，避免一帧内多次伤害
            hasDealtDamage = true;

            Debug.Log($"对玩家造成 {attackDamage} 点伤害");
        }
        else
        {
            Debug.LogWarning("未找到玩家的PlayerHealth组件");
        }
    }

    // 获取攻击状态（供AI脚本查询）
    public bool IsAttacking()
    {
        return isAttacking;
    }

    // 重置攻击状态
    public void ResetAttack()
    {
        isAttacking = false;
        canAttack = false;
        hasDealtDamage = false;
    }
}
