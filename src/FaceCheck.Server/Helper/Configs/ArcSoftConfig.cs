using System.Collections.Generic;

namespace FaceCheck.Server.Helper.Configs
{
    /// <summary>
    /// </summary>
    public class ArcSoftConfig
    {
        public const string Section = "ArcSoft";

        /// <summary>
        /// </summary>
        public string APPID { set; get; }

        /// <summary>
        /// </summary>
        public string KEY64 { set; get; }

        /// <summary>
        /// </summary>
        public string KEY32 { set; get; }

        /// <summary>
        /// </summary>
        public string KEYSo64 { set; get; }

        /// <summary>
        /// </summary>
        public string ProActiveKey32 { set; get; }

        /// <summary>
        /// </summary>
        public string ProActiveKey64 { set; get; }

        /// <summary>
        /// </summary>
        public string ProActiveKeySo64 { set; get; }

        /// <summary>
        /// 授权文件绝对路径  仅4.0有效 优先授权文件激活
        /// </summary>
        public List<string> ActiveFiles { set; get; }

        /// <summary>
        /// 引擎个数
        /// </summary>
        public int EngineNum { set; get; }

        /// <summary>
        /// 相似度 (0.0~1.0) 默认0.8
        /// </summary>
        public double Similarity { get; set; } = 0.8;

        /// <summary>
        /// 获取最相似的人脸
        /// </summary>
        public bool GetMostSimilar { set; get; } = true;

        /// <summary>
        /// 是否是人证对比模式(登记照是证件照)
        /// </summary>
        public bool IsIdcardCompare { set; get; }

        /// <summary>
        /// 输入照片是否肯定是正脸
        /// </summary>
        public bool IsAngleZeroOnly { set; get; }

        /// <summary>
        /// </summary>
        public CheckPicConfig CheckPic { set; get; }
    }
}