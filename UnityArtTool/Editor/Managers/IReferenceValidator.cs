/**
 * 文件名: IReferenceValidator.cs
 * 作用: 为需要验证资产引用的类提供一个通用接口
 * 作者: yuxuhai
 * 日期: 2024
 */

namespace ArtTools
{
    /// <summary>
    /// 提供一个接口，用于检查对象内部的资产引用是否有效。
    /// 实现此接口的类（特别是 ArtToolItem 的子类）可以被系统自动调用，
    /// 以报告或处理丢失的引用（例如，在Unity中资源被删除或移动后）。
    /// </summary>
    public interface IReferenceValidator
    {
        /// <summary>
        /// 验证此对象持有的所有关键资产引用是否仍然有效。
        /// </summary>
        /// <returns>如果所有引用都有效，则返回true；否则返回false。</returns>
        bool ValidateReferences();

        /// <summary>
        /// 当检测到无效引用时，获取一条描述性的错误或警告消息。
        /// </summary>
        /// <returns>描述引用问题的消息字符串。</returns>
        string GetValidationMessage();
    }
}