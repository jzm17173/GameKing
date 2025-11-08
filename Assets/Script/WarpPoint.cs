using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPoint : MonoBehaviour
{
    private Transform playerTransform;
    public KeyCode teleportKey = KeyCode.T; // 设置快捷键

    // 传送点的位置
    private Vector3 warpPosition;

    void Start()
    {
        // 获取玩家Transform
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.GetComponent<Transform>();
        }

        // 获取当前传送点对象的位置
        warpPosition = transform.position;
        Debug.Log("传送点位置: " + warpPosition);
    }

    void Update()
    {
        // 检测按键输入
        if (Input.GetKeyDown(teleportKey) && playerTransform != null)
        {
            TeleportPlayer();
        }
    }

    void TeleportPlayer()
    {
        // 传送玩家到当前对象位置
        playerTransform.position = warpPosition;
        Debug.Log("玩家已传送到: " + warpPosition);
    }
}