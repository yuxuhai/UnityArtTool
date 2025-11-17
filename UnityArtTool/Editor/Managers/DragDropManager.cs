/**
 * 文件名: DragDropManager.cs
 * 作用: 负责 ArtTools 工具窗口的拖拽排序功能管理
 * 作者: yuxuhai
 * 日期: 2024
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 拖拽管理器类，负责处理标签页和工具项的拖拽排序操作。
    /// 提供统一的拖拽逻辑处理，支持标签页重排序和工具项重排序。
    /// </summary>
    public class DragDropManager
    {
        #region 事件定义
        
        /// <summary>
        /// 标签页拖拽排序完成时触发的事件
        /// </summary>
        public event Action<int, int> OnTabReordered;
        
        /// <summary>
        /// 工具项拖拽排序完成时触发的事件
        /// </summary>
        public event Action<int, int, int> OnToolItemReordered; // tabIndex, fromIndex, toIndex
        
        /// <summary>
        /// 拖拽状态改变时触发的事件
        /// </summary>
        public event Action<bool> OnDragStateChanged;
        
        #endregion
        
        #region 私有字段
        
        // 标签页拖拽相关
        /// <summary>
        /// 正在被拖拽的标签页索引
        /// </summary>
        private int _draggedTabIndex = -1;
        
        /// <summary>
        /// 标签页拖拽的目标位置索引
        /// </summary>
        private int _tabDropTargetIndex = -1;
        
        /// <summary>
        /// 是否正在拖拽标签页
        /// </summary>
        private bool _isDraggingTab = false;
        
        // 工具项拖拽相关
        /// <summary>
        /// 正在被拖拽的工具项索引
        /// </summary>
        private int _draggedItemIndex = -1;
        
        /// <summary>
        /// 工具项拖拽的目标位置索引
        /// </summary>
        private int _itemDropTargetIndex = -1;
        
        /// <summary>
        /// 是否正在拖拽工具项
        /// </summary>
        private bool _isDraggingItem = false;
        
        /// <summary>
        /// 当前拖拽的标签页索引（用于工具项拖拽）
        /// </summary>
        private int _currentTabIndex = -1;
        
        // 拖拽标识符
        /// <summary>
        /// 标签页拖拽的唯一标识符
        /// </summary>
        private const string TabDragAndDropId = "ArtToolsTab_Drag";
        
        /// <summary>
        /// 工具项拖拽的唯一标识符
        /// </summary>
        private const string ItemDragAndDropId = "ArtToolItem_Drag";
        
        #endregion
        
        #region 公共属性
        
        /// <summary>
        /// 获取是否正在进行任何拖拽操作
        /// </summary>
        public bool IsDragging => _isDraggingTab || _isDraggingItem;
        
        /// <summary>
        /// 获取是否正在拖拽标签页
        /// </summary>
        public bool IsDraggingTab => _isDraggingTab;
        
        /// <summary>
        /// 获取是否正在拖拽工具项
        /// </summary>
        public bool IsDraggingItem => _isDraggingItem;
        
        /// <summary>
        /// 获取标签页拖拽的目标位置索引
        /// </summary>
        public int TabDropTargetIndex => _tabDropTargetIndex;
        
        /// <summary>
        /// 获取工具项拖拽的目标位置索引
        /// </summary>
        public int ItemDropTargetIndex => _itemDropTargetIndex;
        
        #endregion
        
        #region 标签页拖拽方法
        
        /// <summary>
        /// 处理标签页的拖拽与放置逻辑
        /// </summary>
        /// <param name="tabIndex">标签页索引</param>
        /// <param name="tabRect">标签页的矩形区域</param>
        public void HandleTabDragAndDrop(int tabIndex, Rect tabRect)
        {
            Event evt = Event.current;

            // 1. 鼠标按下，开始拖拽
            if (evt.type == EventType.MouseDown && tabRect.Contains(evt.mousePosition) && evt.button == 0)
            {
                StartTabDrag(tabIndex);
            }
            
            // 2. 拖拽更新
            if (_isDraggingTab && (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform))
            {
                if (DragAndDrop.GetGenericData(TabDragAndDropId) != null)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    
                    // 判断应该在当前项的上方还是下方插入
                    if (tabRect.Contains(evt.mousePosition))
                    {
                        if (evt.mousePosition.y < tabRect.y + tabRect.height / 2)
                            _tabDropTargetIndex = tabIndex;
                        else
                            _tabDropTargetIndex = tabIndex + 1;
                    }
                }
            }
        }
        
        /// <summary>
        /// 完成标签页拖拽排序操作
        /// </summary>
        /// <param name="totalTabCount">标签页总数</param>
        public void CompleteTabDragAndDrop(int totalTabCount)
        {
            Event evt = Event.current;
            if (evt.type == EventType.MouseUp && _isDraggingTab)
            {
                if (_draggedTabIndex != -1 && _tabDropTargetIndex != -1 && _draggedTabIndex != _tabDropTargetIndex)
                {
                    // 计算最终的插入位置
                    int finalDropIndex = _tabDropTargetIndex;
                    if (_draggedTabIndex < _tabDropTargetIndex)
                    {
                        finalDropIndex--;
                    }
                    
                    // 确保索引在有效范围内
                    finalDropIndex = Mathf.Clamp(finalDropIndex, 0, totalTabCount - 1);
                    
                    // 触发标签页重排序事件
                    OnTabReordered?.Invoke(_draggedTabIndex, finalDropIndex);
                }
                
                EndTabDrag();
            }
        }
        
        /// <summary>
        /// 绘制标签页拖拽指示器
        /// </summary>
        /// <param name="tabIndex">标签页索引</param>
        /// <param name="tabRect">标签页矩形区域</param>
        public void DrawTabDropIndicator(int tabIndex, Rect tabRect)
        {
            if (_isDraggingTab && tabIndex == _tabDropTargetIndex && Event.current.type == EventType.Repaint)
            {
                Rect indicatorRect = new Rect(tabRect.x, tabRect.yMax, tabRect.width, 4);
                EditorGUI.DrawRect(indicatorRect, Color.cyan);
            }
        }
        
        #endregion
        
        #region 工具项拖拽方法
        
        /// <summary>
        /// 处理工具项的拖拽与放置逻辑
        /// </summary>
        /// <param name="tabIndex">所属标签页索引</param>
        /// <param name="itemIndex">工具项索引</param>
        /// <param name="itemRect">工具项的矩形区域</param>
        public void HandleItemDragAndDrop(int tabIndex, int itemIndex, Rect itemRect)
        {
            Event evt = Event.current;
            
            // 1. 开始拖拽
            if (evt.type == EventType.MouseDown && itemRect.Contains(evt.mousePosition) && evt.button == 0)
            {
                StartItemDrag(tabIndex, itemIndex);
                evt.Use();
            }
            
            // 2. 拖拽更新
            if (_isDraggingItem && itemRect.Contains(evt.mousePosition) && 
                (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform))
            {
                if (DragAndDrop.GetGenericData(ItemDragAndDropId) != null)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;

                    // 判断应该在当前项的上方还是下方插入
                    if (evt.mousePosition.y < itemRect.y + itemRect.height / 2)
                    {
                        _itemDropTargetIndex = itemIndex;
                    }
                    else
                    {
                        _itemDropTargetIndex = itemIndex + 1;
                    }
                    
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        evt.Use();
                    }
                }
            }
        }
        
        /// <summary>
        /// 完成工具项拖拽排序操作
        /// </summary>
        /// <param name="totalItemCount">工具项总数</param>
        public void CompleteItemDragAndDrop(int totalItemCount)
        {
            Event currentEvent = Event.current;
            if ((currentEvent.type == EventType.DragExited || currentEvent.type == EventType.MouseUp) && _isDraggingItem)
            {
                if (_draggedItemIndex != -1 && _itemDropTargetIndex != -1)
                {
                    // 确保索引有效
                    if (_draggedItemIndex >= 0 && _draggedItemIndex < totalItemCount &&
                        _itemDropTargetIndex >= 0 && _itemDropTargetIndex <= totalItemCount)
                    {
                        // 计算最终的插入位置
                        int finalDropIndex = (_draggedItemIndex < _itemDropTargetIndex) ? 
                            _itemDropTargetIndex - 1 : _itemDropTargetIndex;
                        
                        // 触发工具项重排序事件
                        OnToolItemReordered?.Invoke(_currentTabIndex, _draggedItemIndex, finalDropIndex);
                    }
                }
                
                EndItemDrag();
                currentEvent.Use();
            }
        }
        
        /// <summary>
        /// 绘制工具项拖拽指示器
        /// </summary>
        /// <param name="itemIndex">工具项索引</param>
        /// <param name="itemRect">工具项矩形区域</param>
        public void DrawItemDropIndicator(int itemIndex, Rect itemRect)
        {
            if (_isDraggingItem && itemIndex == _itemDropTargetIndex && Event.current.type == EventType.Repaint)
            {
                Rect indicatorRect = new Rect(itemRect.x, itemRect.yMax, itemRect.width, 2);
                EditorGUI.DrawRect(indicatorRect, Color.green);
            }
        }
        
        #endregion
        
        #region 公共控制方法
        
        /// <summary>
        /// 重置所有拖拽状态
        /// </summary>
        public void ResetDragState()
        {
            EndTabDrag();
            EndItemDrag();
        }
        
        /// <summary>
        /// 强制结束当前的拖拽操作
        /// </summary>
        public void ForceEndDrag()
        {
            if (_isDraggingTab || _isDraggingItem)
            {
                DragAndDrop.AcceptDrag();
                ResetDragState();
            }
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 开始标签页拖拽操作
        /// </summary>
        /// <param name="tabIndex">标签页索引</param>
        private void StartTabDrag(int tabIndex)
        {
            _draggedTabIndex = tabIndex;
            _isDraggingTab = true;
            _tabDropTargetIndex = -1;
            
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(TabDragAndDropId, tabIndex);
            DragAndDrop.StartDrag($"Tab_{tabIndex}");
            
            OnDragStateChanged?.Invoke(true);
        }
        
        /// <summary>
        /// 结束标签页拖拽操作
        /// </summary>
        private void EndTabDrag()
        {
            _draggedTabIndex = -1;
            _tabDropTargetIndex = -1;
            _isDraggingTab = false;
            
            DragAndDrop.AcceptDrag();
            
            if (!_isDraggingItem)
            {
                OnDragStateChanged?.Invoke(false);
            }
        }
        
        /// <summary>
        /// 开始工具项拖拽操作
        /// </summary>
        /// <param name="tabIndex">所属标签页索引</param>
        /// <param name="itemIndex">工具项索引</param>
        private void StartItemDrag(int tabIndex, int itemIndex)
        {
            _currentTabIndex = tabIndex;
            _draggedItemIndex = itemIndex;
            _isDraggingItem = true;
            _itemDropTargetIndex = -1;
            
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(ItemDragAndDropId, itemIndex);
            DragAndDrop.StartDrag($"Item_{tabIndex}_{itemIndex}");
            
            OnDragStateChanged?.Invoke(true);
        }
        
        /// <summary>
        /// 结束工具项拖拽操作
        /// </summary>
        private void EndItemDrag()
        {
            _currentTabIndex = -1;
            _draggedItemIndex = -1;
            _itemDropTargetIndex = -1;
            _isDraggingItem = false;
            
            DragAndDrop.AcceptDrag();
            
            if (!_isDraggingTab)
            {
                OnDragStateChanged?.Invoke(false);
            }
        }
        
        #endregion
    }
}