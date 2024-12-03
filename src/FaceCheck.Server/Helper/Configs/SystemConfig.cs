using System.Collections.Generic;

namespace FaceCheck.Server.Configs
{
    /// <summary>
    /// </summary>
    public class SystemConfig
    {
        public const string Section = "System";

        /// <summary>
        /// 是否对照片进行校验
        /// </summary>
        public bool IsReal { set; get; }

        /// <summary>
        /// </summary>
        public bool IsBase64Log { set; get; }

        /// <summary>
        /// 测试时使用 是否缓存照片文件
        /// </summary>
        public bool IsSaveImg { get; set; }

        /// <summary>
        /// 是否开发模式(返回可视化的错误页面) 启动开发者模式 返回的错误数据格式会不正确
        /// </summary>
        public bool IsDev { set; get; }

        /// <summary>
        /// json 是否漂亮打印
        /// </summary>
        public bool PrettyPrintingJson { set; get; }

        /// <summary>
        /// </summary>
        public bool Hide { set; get; }

        /// <summary>
        /// 自定义Title
        /// </summary>
        public string Title { set; get; }

        /// <summary>
        /// 启用目录浏览
        /// </summary>
        public bool UseDirectoryBrowser { set; get; }
    }
}