namespace FaceCheck.Server.Helper.Configs
{
    /// <summary>
    /// </summary>
    public class CheckPicConfig
    {
        public const string Section = "CheckPic";

        /// <summary>
        /// 是否需要RGB活体检测
        /// </summary>
        public bool NeedRgbLive { get; set; }

        /// <summary>
        /// 是否需要带口罩
        /// </summary>
        public bool NeedMask { get; set; }

        /// <summary>
        /// 照片质量 (0.0~1.0) (-1表示不检测) (仅永久授权有效) 默认-1
        /// </summary>
        public float ImageQuality { get; set; } = -1;

        /// <summary>
        /// 最大人脸角度 默认30° 大于该角度的不进行对比 取值范围 0.0~180.0 建议30 小于等于0表示不比较
        /// </summary>
        public double FaceMaxAngle { get; set; } = 30.0;
    }
}