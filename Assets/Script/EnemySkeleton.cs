using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkeleton : Enemy
{
    // Start is called before the first frame update
    public void Start()
    {
        base.Start();
        
    }

    // Update is called once per frame
    public void Update()
    {
        Filp();
        base.Update();
    }

    void Filp()
    {
        //transform.localRotation = Quaternion.Euler(0, 180, 0);
        // 翻转X轴来实现左右转向
        transform.localScale = new Vector3(-1, 1, 1);
    }
}
