using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySlime : Enemy
{
    public float speed;
    public float startWaitTime;
    private float waitTime;

    // 移动到位置
    public Transform movePos;
    // 左下位置
    public Transform leftDownPos;
    // 右上位置
    public Transform rightUpPos;

    private Animator myAnim;

    private Vector2 lastPosition; // 记录上一帧的位置，用于判断移动方向

    public float damageInterval = 1f;         // 伤害间隔时间（秒）
    private bool isPlayerInContact = false;   // 玩家是否在接触中
    private float lastDamageTime;             // 上次造成伤害的时间
    private GameObject player;                // 玩家对象


    // Start is called before the first frame update
    public void Start()
    {
        base.Start();
        // 初始化：移动到指定位置后的等待时间
        waitTime = startWaitTime;
        // 初始化：指定随机位置
        movePos.position = GetRandomPos();
        myAnim = GetComponent<Animator>();
        lastPosition = transform.position; // 初始化上一帧位置
    }

    // Update is called once per frame
    public void Update()
    {
        //Filp();
        base.Update();

        // 在移动前记录当前位置
        Vector2 currentPosition = transform.position;

        // Vector2.MoveTowards(当前位置, 目标位置, 最大移动距离)
        // 每帧向目标位置移动，最多移动 speed * Time.deltaTime 的距离
        // 当前位置（每次update，当前位置是赋值后的新的值了）
        transform.position = Vector2.MoveTowards(transform.position, movePos.position, speed * Time.deltaTime);

        // 如果已经到达指定位置
        if (Vector2.Distance(transform.position, movePos.position) < 0.1f)
        {
            myAnim.SetBool("Run", false);
            // 等待结束
            if (waitTime <= 0)
            {
                // 获取下一个随机位置
                movePos.position = GetRandomPos();
                // 恢复等待时间
                waitTime = startWaitTime;
                // 等待
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
        else
        {
            myAnim.SetBool("Run", true);
            // 只有在移动时才检测方向
            FlipBasedOnMovement(currentPosition);
        }

        // 更新上一帧位置
        lastPosition = currentPosition;

        // 如果玩家在接触中且达到伤害间隔时间，造成伤害
        if (isPlayerInContact && Time.time >= lastDamageTime + damageInterval)
        {
            DealDamage();
        }
    }

    /// <summary>
    /// 根据移动方向翻转角色
    /// </summary>
    void FlipBasedOnMovement(Vector2 currentPos)
    {
        // 计算移动方向（当前帧位置 - 上一帧位置）
        Vector2 moveDirection = (Vector2)transform.position - lastPosition;

        // 如果有明显的水平移动（避免微小移动导致的频繁翻转）
        if (Mathf.Abs(moveDirection.x) > 0.01f)
        {
            // 向右移动，x缩放为正
            if (moveDirection.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            // 向左移动，x缩放为负
            else if (moveDirection.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    void Filp()
    {
        //transform.localRotation = Quaternion.Euler(0, 180, 0);
        // 翻转X轴来实现左右转向
        transform.localScale = new Vector3(-1, 1, 1);
    }

    Vector2 GetRandomPos()
    {
        Vector2 rndPos = new Vector2(Random.Range(leftDownPos.position.x, rightUpPos.position.x), Random.Range(leftDownPos.position.y, rightUpPos.position.y));
        return rndPos;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 检测碰撞对象是否是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            player = collision.gameObject;
            isPlayerInContact = true;

            // 立即造成第一次伤害
            DealDamage();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // 玩家离开接触范围
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInContact = false;
            player = null;
        }
    }

    // 使用Trigger方式检测接触（如果需要穿透但触发伤害）
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            isPlayerInContact = true;
            DealDamage();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInContact = false;
            player = null;
        }
    }

    void DealDamage()
    {
        if (player != null)
        {
            // 获取玩家的生命值组件并造成伤害
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.DamegePlayer(damage);
                lastDamageTime = Time.time;
            }
        }
    }
}
