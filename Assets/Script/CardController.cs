using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 卡牌控制器类
/// 负责在抽卡结束状态下处理卡牌的交互：悬停放大、点击选中、拖拽释放
/// </summary>
public class CardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// CreateCard 实例，用于检查抽卡流程状态
    /// </summary>
    private CreateCard createCard;
    
    /// <summary>
    /// 卡牌的 RectTransform 组件
    /// </summary>
    private RectTransform rectTransform;
    
    /// <summary>
    /// 卡牌的原始位置（用于拖拽后返回）
    /// </summary>
    private Vector3 originalPosition;
    
    /// <summary>
    /// 卡牌的原始缩放（用于悬停效果）
    /// </summary>
    private Vector3 originalScale;
    
    /// <summary>
    /// 悬停时的放大倍数
    /// </summary>
    private readonly float hoverScale = 1.2f;
    
    /// <summary>
    /// 动画持续时间（秒）
    /// </summary>
    private readonly float animationDuration = 0.2f;
    
    /// <summary>
    /// 是否处于选中状态
    /// </summary>
    private bool isSelected = false;
    
    /// <summary>
    /// 是否正在拖拽
    /// </summary>
    private bool isDragging = false;
    
    /// <summary>
    /// 拖拽时的偏移量
    /// </summary>
    private Vector2 dragOffset;
    
    /// <summary>
    /// 卡牌的 Canvas 组件（用于拖拽时的层级管理）
    /// </summary>
    private Canvas canvas;
    
    /// <summary>
    /// 卡牌的原始 Canvas Group（用于拖拽时禁用射线检测）
    /// </summary>
    private CanvasGroup canvasGroup;
    
    /// <summary>
    /// 卡牌的图像组件（用于选中效果）
    /// </summary>
    private Image cardImage;
    
    /// <summary>
    /// 选中时的颜色
    /// </summary>
    private readonly Color selectedColor = new Color(1f, 1f, 0.5f, 1f); // 淡黄色高亮
    
    /// <summary>
    /// 原始颜色
    /// </summary>
    private Color originalColor;
    
    /// <summary>
    /// 原始的 sibling index（用于恢复层次关系）
    /// </summary>
    private int originalSiblingIndex;

    /// <summary>
    /// Unity 生命周期方法，在游戏开始时调用一次
    /// 初始化组件和变量
    /// </summary>
    void Start()
    {
        // 获取组件
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        cardImage = GetComponent<Image>();
        
        // 如果没有 CanvasGroup，添加一个
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // 记录原始位置和缩放
        originalPosition = rectTransform.anchoredPosition;
        originalScale = transform.localScale;
        
        // 记录原始颜色
        if (cardImage != null)
        {
            originalColor = cardImage.color;
        }
        
        // 查找 CreateCard 实例
        createCard = FindObjectOfType<CreateCard>();
    }

    /// <summary>
    /// 检查是否处于抽卡结束状态
    /// </summary>
    /// <returns>如果抽卡流程已结束（CardFlow == 3），返回 true</returns>
    private bool IsCardFlowEnded()
    {
        return createCard != null && createCard.CardFlow == 3;
    }

    /// <summary>
    /// 鼠标悬停进入事件
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsCardFlowEnded() || isDragging) return;
        
        // 悬停放大效果
        LeanTween.scale(gameObject, originalScale * hoverScale, animationDuration)
            .setEaseOutQuad();
    }

    /// <summary>
    /// 鼠标悬停离开事件
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsCardFlowEnded() || isDragging) return;
        
        // 如果不是选中状态，恢复原始大小
        if (!isSelected)
        {
            LeanTween.scale(gameObject, originalScale, animationDuration)
                .setEaseOutQuad();
        }
    }

    /// <summary>
    /// 鼠标点击事件
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsCardFlowEnded() || isDragging) return;
        
        // 切换选中状态
        isSelected = !isSelected;
        
        if (isSelected)
        {
            // 选中效果：保持放大并改变颜色
            LeanTween.scale(gameObject, originalScale * hoverScale, animationDuration)
                .setEaseOutQuad();
            
            if (cardImage != null)
            {
                LeanTween.color(rectTransform, selectedColor, animationDuration);
            }
        }
        else
        {
            // 取消选中：恢复原始大小和颜色
            LeanTween.scale(gameObject, originalScale, animationDuration)
                .setEaseOutQuad();
            
            if (cardImage != null)
            {
                LeanTween.color(rectTransform, originalColor, animationDuration);
            }
        }
    }

    /// <summary>
    /// 开始拖拽事件
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsCardFlowEnded()) return;
        
        isDragging = true;
        
        // 更新原始位置（因为卡牌位置可能在 Start() 之后被动画改变）
        originalPosition = rectTransform.anchoredPosition;
        
        // 计算拖拽偏移量：鼠标位置相对于父对象（CardContainer）的坐标
        // 减去卡牌当前位置相对于父对象的坐标，得到偏移量
        Vector2 mouseLocalPoint;
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, 
            eventData.position, 
            eventData.pressEventCamera, 
            out mouseLocalPoint))
        {
            // 偏移量 = 鼠标位置 - 卡牌当前位置
            dragOffset = mouseLocalPoint - rectTransform.anchoredPosition;
        }
        else
        {
            dragOffset = Vector2.zero;
        }
        
        // 记录原始的 sibling index（用于恢复层次关系）
        originalSiblingIndex = transform.GetSiblingIndex();
        
        // 禁用射线检测，避免拖拽时触发其他事件
        canvasGroup.blocksRaycasts = false;
        
        // 将卡牌移到最上层（如果 Canvas 有 GraphicRaycaster）
        transform.SetAsLastSibling();
        
        // 保持悬停大小
        LeanTween.scale(gameObject, originalScale * hoverScale, 0.1f)
            .setEaseOutQuad();
    }

    /// <summary>
    /// 拖拽中事件
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!IsCardFlowEnded() || !isDragging) return;
        
        // 将屏幕坐标转换为相对于父对象（CardContainer）的本地坐标
        Vector2 localPoint;
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            // 更新卡牌位置（减去偏移量以保持鼠标位置不变）
            rectTransform.anchoredPosition = localPoint - dragOffset;
        }
    }

    /// <summary>
    /// 结束拖拽事件
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsCardFlowEnded() || !isDragging) return;
        
        isDragging = false;
        
        // 恢复射线检测
        canvasGroup.blocksRaycasts = true;
        
        // 恢复原始的 sibling index（恢复层次关系）
        transform.SetSiblingIndex(originalSiblingIndex);
        
        // 回到原位置（使用 LeanTween.value 直接操作 anchoredPosition，确保坐标系统正确）
        Vector2 currentPos = rectTransform.anchoredPosition;
        LeanTween.value(gameObject, (Vector2 pos) => {
            rectTransform.anchoredPosition = pos;
        }, currentPos, originalPosition, animationDuration)
            .setEaseOutQuad();
        
        // 如果未选中，恢复原始大小
        if (!isSelected)
        {
            LeanTween.scale(gameObject, originalScale, animationDuration)
                .setEaseOutQuad();
        }
    }
}
