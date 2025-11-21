using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// fileName 产生对象的默认名称
// menuName 产生对象的菜单名称
[CreateAssetMenu(fileName = "New Card", menuName = "Card")]

public class CardMessage : ScriptableObject
{
    public Sprite Card_Art;
    public Sprite Card_Frame;
    public string Card_Name;
    public string Card_Desc;
}
