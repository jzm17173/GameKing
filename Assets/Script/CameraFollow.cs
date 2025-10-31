using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothing; // 相机移动平滑因子

    public Vector2 minPosition;
    public Vector2 maxPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            if (transform.position != target.position)
            {
                Vector3 targetPos = target.position;
                // 限制镜头范围
                //targetPos.x = Mathf.Clamp(targetPos.x, minPosition.x, maxPosition.x);
                //targetPos.y = Mathf.Clamp(targetPos.y, minPosition.y, maxPosition.y);
                // 线性差值
                transform.position = Vector3.Lerp(transform.position, targetPos, smoothing);
            }
        }
    }

    // 外部调用，比如切换关卡
    public void SetCamPosLimit(Vector2 minPos, Vector2 maxPos)
    {
        minPosition = minPos;
        maxPosition = maxPos;
    }
}
