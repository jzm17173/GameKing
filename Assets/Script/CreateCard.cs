using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 卡牌创建和管理类
/// 负责从卡组中抽取卡牌，管理抽牌流程，并将卡牌添加到手牌中
/// </summary>
public class CreateCard : MonoBehaviour
{
    /// <summary>
    /// 卡牌的父对象Transform，用于实例化卡牌时设置父级
    /// </summary>
    public Transform parent;
    
    /// <summary>
    /// 卡牌预制体，用于实例化新卡牌
    /// </summary>
    public GameObject NewCard;

    /// <summary>
    /// 游戏运行所处的阶段
    /// 0 - 初始状态
    /// 1 - 抽出一张牌的阶段
    /// 2 - 将抽出的卡牌在展示一小段时间后加到手牌的阶段
    /// 3 - 抽牌流程结束
    /// </summary>
    public int CardFlow = 0;
    
    /// <summary>
    /// 要抽几张牌
    /// </summary>
    public int TakeHowManyCard = 0;
    
    /// <summary>
    /// 储存已拥有的卡牌对象数组
    /// </summary>
    public GameObject[] HavingCard = null;

    /// <summary>
    /// 所有卡牌的数据数组，包含卡牌的艺术图、边框、名称、描述等信息
    /// </summary>
    public CardMessage[] Card = null;
    
    /// <summary>
    /// 卡组字典，Key为卡牌ID，Value为该ID卡牌在卡组中的数量
    /// </summary>
    public IDictionary<int, int> CardGroup = new Dictionary<int, int>();
    
    /// <summary>
    /// 抽取的卡牌的ID
    /// </summary>
    public int TakeCardID;
    
    /// <summary>
    /// 卡组中卡牌的种类数
    /// </summary>
    public int CardGroup_Species;
    
    /// <summary>
    /// 卡组中有多少张卡（总数）
    /// </summary>
    public int CardGroup_Num;
    
    /// <summary>
    /// 当前手牌数量（卡牌对象数组的序号）
    /// </summary>
    private int HavingCardNum = 0;
    
    /// <summary>
    /// 当前正在抽取第几张牌（从1开始计数）
    /// </summary>
    private int TakingCardNum = 1;
    
    /// <summary>
    /// 抽牌后的展示时间计数器
    /// </summary>
    private int TakeTime = 0;
    
    /// <summary>
    /// 卡牌在手牌中的位置间距
    /// </summary>
    private float CardPosition = 0;

    /// <summary>
    /// 抽牌后的展示时间最大值
    /// </summary>
    private readonly int TakeTimeMax = 550;

    /// <summary>
    /// 父对象的宽度
    /// </summary>
    private float ParentWidth;

    /// <summary>
    /// Unity生命周期方法，在游戏开始时调用一次
    /// 初始化卡组和抽牌流程
    /// </summary>
    void Start()
    {
        // 初始化卡组：ID为1的卡牌有5张，ID为2的卡牌有10张
        CardGroup.Add(1, 5);
        CardGroup.Add(2, 10);
        CardGroup_Species = 2;  // 卡组中有2种卡牌
        CardGroup_Num = 15;     // 卡组中共有15张卡

        // 初始化抽牌流程
        CardFlow = 1;           // 设置为抽牌阶段
        HavingCardNum = 0;      // 初始手牌数为0
        TakeHowManyCard = 10;   // 要抽取10张牌
        // TakeCard(0);
        // 获取父对象的宽度
        ParentWidth = parent.GetComponent<RectTransform>().sizeDelta.x;
    }

    /// <summary>
    /// 根据卡组中卡牌的数量比例，随机选择一张卡牌的ID
    /// 使用加权随机算法，确保每种卡牌被抽到的概率与其在卡组中的数量成正比
    /// </summary>
    public void ChooseCardID() {
        int cardTypeIndex = 0;      // 记录字典遍历次数（当前遍历到第几种卡牌）
        int accumulatedCount = 0;      // 正在遍历的卡牌之前一共有多少张牌（累计数量）
        int randomValue = 0;      // 生成的随机数（1到CardGroup_Num之间）
        int currentCardCount = 0;      // 正在遍历的卡牌在卡组中的数量
        bool isSearching = true;  // 遍历到最后一种卡时，用来跳出判断的标志

        isSearching = true;
        // 生成1到卡组总数之间的随机数
        randomValue = Random.Range(1, CardGroup_Num + 1);
        
        if (CardGroup_Num > 0) {
            // 遍历卡组字典中的所有卡牌ID
            foreach(int A in CardGroup.Keys) {
                cardTypeIndex = cardTypeIndex + 1;  // 增加遍历计数
                
                if (isSearching) {
                    // 如果不是最后一种卡牌
                    if (cardTypeIndex < CardGroup_Species && isSearching) {
                        currentCardCount = CardGroup[A];  // 获取当前卡牌的数量
                        // 如果随机数落在当前卡牌的数量范围内，则选中该卡牌
                        if (randomValue <= (accumulatedCount + currentCardCount)) {
                            TakeCardID = A;
                            isSearching = false;  // 找到目标卡牌，停止遍历
                        } else {
                            accumulatedCount = accumulatedCount + currentCardCount;  // 累计已遍历的卡牌数量
                        }
                    } else {
                        // 如果是最后一种卡牌，直接选中
                        TakeCardID = A;
                        isSearching = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Unity生命周期方法，每帧调用一次
    /// 检查并执行卡牌抽取流程
    /// </summary>
    void Update()
    {
        // 执行阶段1：抽取卡牌
        CardFlow_1();

        // 执行阶段2：展示卡牌并添加到手牌
        CardFlow_2();
    }

    /// <summary>
    /// 卡牌流程阶段1：抽取卡牌阶段
    /// 当处于阶段1且还有卡牌要抽取时，执行抽牌操作
    /// </summary>
    public void CardFlow_1() {
        // 如果抽卡阶段，并且要抽取的卡牌数量还有剩余
        if (CardFlow == 1 && TakeHowManyCard != 0) {
            // 如果当前抽取的卡牌数量小于要抽取的卡牌数量
            if (TakingCardNum <= TakeHowManyCard) {
                // 抽取一张卡牌
                TakeCard(HavingCardNum);
                TakingCardNum++;  // 增加已抽取计数
                CardFlow = 2;     // 进入展示阶段
            } else {
                // 所有卡牌都已抽取完毕，结束流程
                TakingCardNum = 1;
                TakeHowManyCard = 0;
                CardFlow = 3;     // 设置为流程结束状态
            }
        }
    }

    /// <summary>
    /// 卡牌流程阶段2：展示卡牌并添加到手牌阶段
    /// 在卡牌展示一段时间后，将其移动到手牌区域，并更新卡组信息
    /// </summary>
    public void CardFlow_2() {
        if (CardFlow == 2) {
            TakeTime++;  // 增加展示时间计数
            
            // 当展示时间达到550帧时，开始将卡牌移动到手牌区域
            if (TakeTime >= TakeTimeMax) {
                // 计算手牌中每张卡牌的位置间距
                CardPosition = ParentWidth / (HavingCardNum + 1);
                
                // 将当前卡牌缩放到正常大小
                HavingCard[HavingCardNum].transform.LeanScale(new Vector3(1.0f, 1.0f, 1.0f), 0.5f);
                
                // 将当前卡牌移动到其在手牌中的位置（使用RectTransform的move方法以正确操作anchoredPosition）
                LeanTween.move(HavingCard[HavingCardNum].GetComponent<RectTransform>(), new Vector3(66 + CardPosition / 2 + HavingCardNum * CardPosition, 100, 0), 0.5f);
                
                // 调整所有已存在的手牌位置，为新卡牌腾出空间
                for (int i = 0; i < 81; i++) {
                    if (HavingCardNum - i >= 0) {
                        LeanTween.move(HavingCard[HavingCardNum - i].GetComponent<RectTransform>(), new Vector3(66 + CardPosition / 2 + (HavingCardNum - i) * CardPosition, 100, 0), 0.12f);
                    }
                }
                TakeTime = 0;  // 重置时间计数
            }
        }

        // 当卡牌已经移动到手牌区域（y坐标 <= -360）时，更新卡组信息并进入下一轮抽牌
        if (CardFlow == 2 && HavingCard[HavingCardNum].GetComponent<RectTransform>().anchoredPosition.y <= 100) {
            // 从卡组中移除一张已抽取的卡牌
            CardGroup[TakeCardID] = CardGroup[TakeCardID] - 1;
            
            // 如果该ID的卡牌已经全部抽完，从卡组中移除该ID
            if (CardGroup[TakeCardID] <= 0) {
                CardGroup_Species = CardGroup_Species - 1;  // 减少卡牌种类数
                CardGroup.Remove(TakeCardID);               // 从字典中移除该卡牌ID
            }
            
            // 更新卡组总数和手牌数量
            CardGroup_Num = CardGroup_Num - 1;  // 卡组总数减1
            HavingCardNum = HavingCardNum + 1;  // 手牌数量加1
            TakeTime = 0;                       // 重置时间计数
            CardFlow = 1;                       // 返回抽牌阶段，准备抽取下一张
        }
    }

    /// <summary>
    /// 抽取一张卡牌并创建卡牌对象
    /// 根据随机选择的卡牌ID，从卡牌数据中获取信息，实例化卡牌对象，并播放抽牌动画
    /// </summary>
    /// <param name="A">手牌数组中的索引位置，用于存储新创建的卡牌对象</param>
    public void TakeCard(int A) {
        // 随机选择一张卡牌的ID
        ChooseCardID();
        if (TakeCardID >= 0) {
            // 卡牌ID有效时的处理（当前为空，可在此处添加额外逻辑）
        }

        // 从卡牌数据数组中获取对应ID的卡牌信息，并设置到卡牌预制体上
        NewCard.GetComponent<CardCreater>().Card_Art.sprite = Card[TakeCardID - 1].Card_Art;      // 设置卡牌艺术图
        NewCard.GetComponent<CardCreater>().Card_Frame.sprite = Card[TakeCardID - 1].Card_Frame;  // 设置卡牌边框
        NewCard.GetComponent<CardCreater>().Card_Name.text = Card[TakeCardID - 1].Card_Name;      // 设置卡牌名称
        NewCard.GetComponent<CardCreater>().Card_Desc.text = Card[TakeCardID - 1].Card_Desc;      // 设置卡牌描述

        // 实例化卡牌对象，并设置父级为parent
        HavingCard[A] = Instantiate(NewCard, new Vector2(0, 0), NewCard.transform.rotation, parent);
        // 设置卡牌的初始位置（屏幕左侧外部）
        HavingCard[A].GetComponent<RectTransform>().anchoredPosition = new Vector2(-1090, 200);

        // 播放抽牌动画效果
        LeanTween.rotate(HavingCard[A], new Vector3(0, 0, 0), 1.8f).setEaseInOutQuart();        // 旋转动画
        LeanTween.moveX(HavingCard[A].GetComponent<RectTransform>(), 80.0f, 1.8f).setEaseInOutQuart();                   // 水平移动动画（使用RectTransform的moveX方法以正确操作anchoredPosition）
        LeanTween.scale(HavingCard[A], new Vector3(2.0f, 2.0f, 1.0f), 1.8f).setEaseInOutQuart(); // 缩放动画（放大到2倍）
    }
}
