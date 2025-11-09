using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    /// <summary>
    /// 箭矢的移动速度
    /// </summary>
    public float speed;
    
    /// <summary>
    /// 箭矢造成的伤害值
    /// </summary>
    public int damage;
    
    /// <summary>
    /// 箭矢的最大飞行距离（超过此距离将自动销毁）
    /// </summary>
    public float destroyDistance;

    /// <summary>
    /// 箭矢的刚体组件
    /// </summary>
    private Rigidbody2D rb2d;
    
    /// <summary>
    /// 箭矢的起始位置
    /// </summary>
    private Vector3 startPos;

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    void Start()
    {
        // 获取刚体组件
        rb2d = GetComponent<Rigidbody2D>();
        // 设置箭矢的初始速度，沿右方向移动
        rb2d.velocity = transform.right * speed;
        // 记录起始位置
        startPos = transform.position;
    }

    /// <summary>
    /// 每帧更新方法，检查箭矢是否超过最大飞行距离
    /// </summary>
    void Update()
    {
        // 计算当前距离起始位置的平方距离（使用平方距离避免开方运算，提高性能）
        float distance = (transform.position - startPos).sqrMagnitude;
        // 如果超过销毁距离，则销毁箭矢
        if (distance > destroyDistance)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 触发器进入事件，当箭矢碰撞到敌人时触发
    /// </summary>
    /// <param name="other">碰撞到的碰撞器</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 如果碰撞到的是敌人
        if (other.gameObject.CompareTag("Enemy"))
        {
            // 对敌人造成伤害
            other.GetComponent<Enemy>().TakeDamage(damage);
            // 销毁箭矢
            Destroy(gameObject);
        }
    }
}
