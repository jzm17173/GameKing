using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 箭矢射击类，处理玩家的箭矢攻击逻辑
/// </summary>
public class ArrowHit : MonoBehaviour
{
    /// <summary>
    /// 箭矢预制体，用于实例化箭矢
    /// </summary>
    public GameObject arrowPrefab;

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    void Start()
    {
    }

    /// <summary>
    /// 每帧更新方法，持续检测攻击输入
    /// </summary>
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

    /// <summary>
    /// 射击方法，在当前位置和旋转角度实例化箭矢
    /// </summary>
    void Shoot()
    {
        // 在当前位置和旋转角度实例化箭矢预制体
        // transform.position 获取当前物体的世界坐标
        // transform.rotation 获取当前物体的旋转角度
        // Instantiate(arrowPrefab, transform.position, transform.rotation);
    }
}
