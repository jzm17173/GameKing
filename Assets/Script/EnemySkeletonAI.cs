using UnityEngine;
using System.Collections;

public class EnemySkeletonAI : Enemy
{
    [Header("攻击组件")]
    public EnemySkeletonAttack attackComponent; // 攻击脚本引用

    [Header("状态设置")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("巡逻设置")]
    public float patrolSpeed = 1.5f;        // 巡逻速度
    public float idleTime = 2f;             // 待机时间

    [Header("追击设置")]
    public float chaseSpeed = 2f;           // 追击速度
    public float attackRange = 1f;          // 攻击范围

    [Header("脱离战斗设置")]
    public float loseTargetTime = 3f;       // 丢失目标后多久返回
    public float returnSpeed = 2f;          // 返回速度

    [Header("玩家碰撞体设置")]
    public bool usePlayerColliderBounds = true; // 是否使用玩家碰撞体边界(可以用来获取playerWidth)
    public float playerWidth = 3.37f;        // 玩家宽度（如果不使用碰撞体检测）性能更好 界面填写的改掉

    // 巡逻区域设置
    public Transform leftPos;               // 巡逻区域左边界
    public Transform rightPos;              // 巡逻区域右边界

    // 私有变量
    private Vector2 startPosition;          // 起始位置
    private Vector2 patrolTarget;           // 巡逻目标点
    private Transform player;               // 玩家引用
    private Collider2D playerCollider;      // 玩家碰撞体引用
    private Animator animator;

    [Header("动画设置")]
    public float animationSmoothTime = 0.1f;    // 动画过渡平滑时间
    private float currentSpeed = 0f;            // 当前速度值，用于平滑动画

    // 移动相关
    private bool isMoving = false;

    // 计时器
    private float idleTimer;
    private float loseTargetTimer;
    private float attackAnimationTimer;     // 攻击动画计时器

    // 组件引用
    public void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // 获取玩家碰撞体
        playerCollider = player.GetComponent<PolygonCollider2D>();
        if (playerCollider == null)
        {
            Debug.LogWarning("未找到玩家的碰撞体组件，将使用默认宽度");
        }

        startPosition = transform.position;
        // 设置初始巡逻目标
        SetNewPatrolTarget();

        // 获取攻击组件
        attackComponent = GetComponentInChildren<EnemySkeletonAttack>();
        if (attackComponent == null)
        {
            Debug.LogWarning("未找到EnemySkeletonAttack组件");
        }
    }

    // 攻击开始 - 由动画事件调用
    public void OnAttackStart()
    {
        if (attackComponent != null)
        {
            attackComponent.OnAttackStart();
        }
        else
        {
            Debug.LogWarning("attackComponent为null");
        }
    }

    // 攻击有效帧 - 由动画事件调用
    public void OnAttackActive()
    {
        if (attackComponent != null)
        {
            attackComponent.OnAttackActive();
        }
    }

    // 攻击结束 - 由动画事件调用
    public void OnAttackEnd()
    {
        if (attackComponent != null)
        {
            attackComponent.OnAttackEnd();
        }
    }

    public void Update()
    {
        base.Update();

        // 状态机
        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolState();
                break;
            case EnemyState.Idle:
                IdleState();
                break;
            case EnemyState.Chase:
                ChaseState();
                break;
            case EnemyState.Attack:
                AttackState();
                break;
            case EnemyState.Return:
                ReturnState();
                break;
        }

        // 更新动画参数
        UpdateAnimation();
    }

    // 巡逻状态
    private void PatrolState()
    {
        // 检查玩家是否在巡逻区域内
        if (PlayerInPatrolArea())
        {
            currentState = EnemyState.Chase;
            return;
        }

        // 移动到巡逻目标点（只移动X轴）
        MoveToTargetXOnly(patrolTarget, patrolSpeed);

        // 如果到达目标点，切换到待机状态
        if (Mathf.Abs(transform.position.x - patrolTarget.x) < 0.1f)
        {
            currentState = EnemyState.Idle;
            idleTimer = idleTime;
            isMoving = false;
        }
    }

    // 待机状态
    private void IdleState()
    {
        // 检查玩家是否在巡逻区域内
        if (PlayerInPatrolArea())
        {
            currentState = EnemyState.Chase;
            return;
        }

        // 停止移动
        isMoving = false;

        // 待机计时
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            // 设置新的巡逻目标并返回巡逻状态
            SetNewPatrolTarget();
            currentState = EnemyState.Patrol;
        }
    }

    // 追击状态
    private void ChaseState()
    {
        // 玩家离开巡逻区域
        if (!PlayerInPatrolArea())
        {
            isMoving = false;
            loseTargetTimer += Time.deltaTime;
            if (loseTargetTimer >= loseTargetTime)
            {
                currentState = EnemyState.Return;
                loseTargetTimer = 0;
                return;
            }
        }
        else
        {
            loseTargetTimer = 0; // 重置计时器
        }

        // 检查是否进入攻击范围
        if (PlayerInRange(attackRange))
        {
            currentState = EnemyState.Attack;
            isMoving = false;
            attackAnimationTimer = 1f; // 攻击动画时长
            return;
        }

        if (PlayerInPatrolArea())
        {
            // 向玩家移动（只移动X轴）
            MoveToTargetWithBoundary(player.position, chaseSpeed);
        }
    }

    // 修改AttackState方法
    private void AttackState()
    {
        // 停止移动
        isMoving = false;

        // 攻击动画计时
        attackAnimationTimer -= Time.deltaTime;

        // 如果攻击动画结束
        if (attackAnimationTimer <= 0)
        {
            // 如果玩家离开攻击范围，返回追击状态
            if (!PlayerInRange(attackRange))
            {
                currentState = EnemyState.Chase;
                return;
            }

            // 如果玩家仍在攻击范围内，可以准备下一次攻击
            if (PlayerInRange(attackRange))
            {
                // 重置攻击计时器进行下一次攻击
                attackAnimationTimer = 1f; // 攻击间隔
            }
        }

        // 如果玩家离开巡逻区域，开始返回计时
        if (!PlayerInPatrolArea())
        {
            loseTargetTimer += Time.deltaTime;
            if (loseTargetTimer >= loseTargetTime)
            {
                currentState = EnemyState.Return;
                loseTargetTimer = 0;
            }
        }
        else
        {
            loseTargetTimer = 0;
        }
    }

    // 返回起始点状态
    private void ReturnState()
    {
        // 向起始点移动（只移动X轴）
        MoveToTargetXOnly(startPosition, returnSpeed);

        // 如果到达起始点，返回巡逻状态
        if (Mathf.Abs(transform.position.x - startPosition.x) < 0.1f)
        {
            SetNewPatrolTarget();
            currentState = EnemyState.Patrol;
            isMoving = false;
        }

        // 返回途中如果发现玩家在巡逻区域内，重新追击
        if (PlayerInPatrolArea())
        {
            currentState = EnemyState.Chase;
            loseTargetTimer = 0;
        }
    }

    // 设置新的巡逻目标点（在巡逻区域内，只考虑X轴）
    private void SetNewPatrolTarget()
    {
        patrolTarget = GetRandomPosXOnly();
    }

    // 获取巡逻区域内的随机位置（只考虑X轴）
    private Vector2 GetRandomPosXOnly()
    {
        if (leftPos == null || rightPos == null)
        {
            Debug.LogError("请设置LeftPos和RightPos");
            return transform.position;
        }

        Vector2 rndPos = new Vector2(
            Random.Range(leftPos.position.x, rightPos.position.x),
            transform.position.y // 保持当前Y轴位置
        );
        return rndPos;
    }

    // 移动到目标点（只移动X轴，且限制在巡逻边界内）
    private void MoveToTargetWithBoundary(Vector2 target, float speed)
    {
        if (leftPos == null || rightPos == null) return;

        // 计算目标X位置，限制在巡逻边界内
        float targetX = Mathf.Clamp(target.x, leftPos.position.x, rightPos.position.x);

        // 计算到目标的距离
        float distanceToTarget = Mathf.Abs(targetX - transform.position.x);
        
        // 如果已经到达目标，停止移动
        if (distanceToTarget < 0.1f)
        {
            isMoving = false;
            return;
        }

        // 计算X轴方向
        float directionX = Mathf.Sign(targetX - transform.position.x);
        float movementX = directionX * speed * Time.deltaTime;

        // 计算新的X位置
        float newX = transform.position.x + movementX;

        // 确保新位置不超过巡逻边界
        newX = Mathf.Clamp(newX, leftPos.position.x, rightPos.position.x);

        // 使用Transform只移动X轴
        transform.position = new Vector3(
            newX,
            transform.position.y,
            transform.position.z
        );

        // 检查是否真的在移动（基于到目标的距离，而不是movementX的大小）
        isMoving = distanceToTarget > 0.1f;

        // 更新面向方向（用于动画）
        if (directionX != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(directionX), 1, 1);
        }
    }

    // 移动到目标点（只移动X轴）
    private void MoveToTargetXOnly(Vector2 target, float speed)
    {
        // 计算X轴方向
        float directionX = Mathf.Sign(target.x - transform.position.x);
        float movementX = directionX * speed * Time.deltaTime;

        // 使用Transform只移动X轴
        transform.position = new Vector3(
            transform.position.x + movementX,
            transform.position.y,
            transform.position.z
        );

        isMoving = true;

        // 更新面向方向（用于动画）
        if (directionX != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(directionX), 1, 1);
        }
    }

    // 检查玩家是否在指定范围内（考虑玩家宽度）
    private bool PlayerInRange(float range)
    {
        if (player == null) return false;

        float actualDistance = GetPlayerDistanceWithWidth();
        return actualDistance <= range;
    }

    // 检查玩家是否在巡逻区域内（考虑玩家宽度）
    private bool PlayerInPatrolArea()
    {
        if (player == null || leftPos == null || rightPos == null) return false;

        // 获取玩家实际边界（考虑宽度）
        float playerLeftEdge = GetPlayerLeftEdge();
        float playerRightEdge = GetPlayerRightEdge();

        // 检查玩家任何部分是否在巡逻区域内
        return (playerLeftEdge <= rightPos.position.x && playerRightEdge >= leftPos.position.x);
    }

    // 获取玩家左边界
    private float GetPlayerLeftEdge()
    {
        if (usePlayerColliderBounds && playerCollider != null)
        {
            return playerCollider.bounds.min.x;
        }
        else
        {
            return player.position.x - playerWidth / 2f;
        }
    }

    // 获取玩家右边界
    private float GetPlayerRightEdge()
    {
        if (usePlayerColliderBounds && playerCollider != null)
        {
            return playerCollider.bounds.max.x;
        }
        else
        {
            return player.position.x + playerWidth / 2f;
        }
    }

    // 获取考虑玩家宽度后的实际距离
    private float GetPlayerDistanceWithWidth()
    {
        if (player == null) return float.MaxValue;

        if (usePlayerColliderBounds && playerCollider != null)
        {
            // 使用碰撞体边界计算最近距离
            float closestPointX = Mathf.Clamp(transform.position.x,
                playerCollider.bounds.min.x, playerCollider.bounds.max.x);
            Vector2 closestPoint = new Vector2(closestPointX, player.position.y);
            return Vector2.Distance(transform.position, closestPoint);
        }
        else
        {
            // 使用简单宽度计算
            float playerLeftEdge = player.position.x - playerWidth / 2f;
            float playerRightEdge = player.position.x + playerWidth / 2f;

            float closestPointX = Mathf.Clamp(transform.position.x, playerLeftEdge, playerRightEdge);
            Vector2 closestPoint = new Vector2(closestPointX, player.position.y);
            return Vector2.Distance(transform.position, closestPoint);
        }
    }

    // 更新动画参数
    private void UpdateAnimation()
    {
        if (animator == null) return;

        // 根据是否移动来设置速度参数
        float speedValue = isMoving ? 1f : 0f;
        animator.SetFloat("Speed", speedValue);

        // 只在攻击状态且攻击动画计时器运行时显示攻击动画
        bool isAttacking = currentState == EnemyState.Attack && attackAnimationTimer > 0;
        animator.SetBool("IsAttacking", isAttacking);
    }
}
