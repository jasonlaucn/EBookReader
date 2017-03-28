using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBookReader.Model
{
    public enum AppSettingKey
    {
        /// <summary>
        /// 章节识别正则
        /// </summary>
        ChaptersRecogniseRegex,
        /// <summary>
        /// 书本扩展名
        /// </summary>
        BookExt,
    }

    public enum ConfigKey
    {
        /// <summary>
        /// 主窗口透明度
        /// </summary>
        WindowOpacity,
        /// <summary>
        /// 主窗口尺寸
        /// </summary>
        WindowSize,
        /// <summary>
        /// 内容字号
        /// </summary>
        FontSize,
        /// <summary>
        /// 内容文字亮度
        /// </summary>
        FontBrightness,
        /// <summary>
        /// 窗口背景色
        /// </summary>
        Background,
        /// <summary>
        /// 窗口最大化状态(是:1 否 0)
        /// </summary>
        WindowMaximized
    }
}
