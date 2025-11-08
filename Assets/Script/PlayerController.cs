using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float runSpeed;
    public float jumpSpeed;

    private Rigidbody2D myRigidbody;
    private Animator myAnim;
    private BoxCollider2D myFeet;
    private bool isGround;

    // 左移动还是有残影，右移动正常很多
    // Start is called before the first frame update
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        // 移动时的残影问题
        myRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        myAnim = GetComponent<Animator>();
        myFeet = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Run();
        Filp();
        Jump();
        CheckGrounded();
    }

    void CheckGrounded()
    {
        isGround = myFeet.IsTouchingLayers(LayerMask.GetMask("Ground"));

    }

    // 移动时的残影问题
    // 当前的翻转逻辑基于速度，这可能导致延迟。改为基于输入方向
    //void Filp()
    //{
    //    bool playerHasXAxisSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
    //    if (playerHasXAxisSpeed)
    //    {
    //        if (myRigidbody.velocity.x > 0.1f)
    //        {
    //            transform.localRotation = Quaternion.Euler(0, 0, 0);
    //        }

    //        if (myRigidbody.velocity.x < -0.1f)
    //        {
    //            transform.localRotation = Quaternion.Euler(0, 180, 0);

    //        }
    //    }

    //}

    void Filp()
    {
        float moveDir = Input.GetAxis("Horizontal");
        bool playerHasXAxisSpeed = Mathf.Abs(moveDir) > Mathf.Epsilon;

        if (playerHasXAxisSpeed)
        {
            if (moveDir > 0.1f)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else if (moveDir < -0.1f)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }

    // 玩家跑
    void Run()
    {
        float moveDir = Input.GetAxis("Horizontal");
        Vector2 playerVel = new Vector2(moveDir * runSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVel;

        // idle与run的动画切换
        // x轴速度大于一个极小值
        bool playerHasXAxisSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        myAnim.SetBool("Run", playerHasXAxisSpeed);
    }

    // 玩家跳
    void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGround)
            {
                Vector2 jumpVel = new Vector2(0.0f, jumpSpeed);
                myRigidbody.velocity = Vector2.up * jumpVel;

            }
        }
    }

    //void Attack()
    //{
    //    if (Input.GetButtonDown("Attack")) {
    //    }
    //}
}
