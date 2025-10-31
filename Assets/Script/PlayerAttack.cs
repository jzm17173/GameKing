using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int damage;
    private Animator anim;
    private BoxCollider2D collider2D;
    public float time;

    // Start is called before the first frame update
    void Start()
    {
        //anim = GameObject.FindGameObjectWithTag("Player").GetComponent(Animator)();
        collider2D = GetComponent<BoxCollider2D>();
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
            collider2D.enabled = true;
            // 播放攻击动画
            // anim.SetTrigger("Attack");
            StartCoroutine(disableHitBox());
        }
    }

    // 碰撞框出现/消失时间，攻击动画出现/消失时间，利用协程1-控制碰撞框出现时间 协程2-控制碰撞框消失时间 可以碰撞框显示出来检查

    // 协程
    IEnumerator disableHitBox()
    {
        yield return new WaitForSeconds(time);
        collider2D.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            //other.GetComponent<EnemySkeleton>().TakeDamage(damage);
            other.GetComponent<Enemy>().TakeDamage(damage);
        }
    }
}
