/**
 * 文件名: TestToolItem.cs
 * 作用: 定义测试工具项的抽象基类
 * 作者: yuxuhai
 * 日期: 2024
 */

using System;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 所有具体工具项的抽象基类。
    /// 提供了工具项的基本接口，包括UI绘制和克隆功能。
    /// </summary>
    [Serializable]
    public abstract class TestToolItem
    {
        /// <summary>
        /// 绘制工具项的主要UI界面
        /// </summary>
        public abstract void DrawMainUI();
        
        /// <summary>
        /// 创建当前工具项的深拷贝副本
        /// </summary>
        /// <returns>工具项的克隆实例</returns>
        public abstract TestToolItem Clone();
    }
}