using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// abstract 抽象类
public abstract class Enemy : MonoBehaviour
{
    // 血量
    public int health;
    // 伤害
    public int damage;

    // 
    public float flashTime;
    private SpriteRenderer sr;
    private Color originalColor;
    private Animator myAnim;

    private PlayerHealth playerHealth;

    // Start is called before the first frame update
    public void Start()
    {
        // 获取玩家血量组件
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        // 获取精灵渲染器组件
        sr = GetComponent<SpriteRenderer>();
        // 获取原始颜色
        originalColor = sr.color;
        myAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void Update()
    {
        if (health <= 0)
        {
            Die();
        }
    }

    // 受到玩家攻击
    public void TakeDamage(int damage)
    {
        Debug.Log("Enemy: 受到玩家攻击333"+damage);
        health -= damage;
        FlashColor(flashTime);
    }

    void FlashColor(float time)
    {
        sr.color = Color.red;
        Invoke("ResetColor", time);
    }

    void ResetColor()
    {
        sr.color = originalColor;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            print(collision.GetType().ToString());

        }


        if (collision.gameObject.CompareTag("Player") && collision.GetType().ToString() == "UnityEngine.PolygonCollider2D")
        {
            if (playerHealth != null)
            {
                playerHealth.DamegePlayer(damage);
            }
        }
    }

    public void Die()
    {
        // 播放死亡动画
        myAnim.SetTrigger("Die");

        // 禁用碰撞器和脚本
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        // 延迟销毁，确保动画播放完毕
        Destroy(gameObject, 1.5f);
    }
}
