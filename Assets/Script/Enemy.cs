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

    public float flashTime;
    private SpriteRenderer sr;
    private Color originalColor;

    // Start is called before the first frame update
    public void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    // Update is called once per frame
    public void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    // 受到玩家攻击
    public void TakeDamage(int damage)
    {
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
}
