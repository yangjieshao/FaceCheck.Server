using System.Text.Json.Serialization;

namespace FaceCheck.Server.Model
{
    /// <summary>
    /// 人脸信息
    /// </summary>
    public class FaceInfo
    {
        /// <summary>
        /// 头像角度
        /// <para> 60,90,120 旋转90° </para>
        /// 150,180,210 旋转180°
        /// <para> 240,270,300 旋转-90° </para>
        /// </summary>
        public int FaceOrient { set; get; }

        /// <summary>
        /// 0：男；1：女；
        /// </summary>
        public int Sex { set; get; }

        /// <summary>
        /// </summary>
        public int Age { set; get; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("_3DAngle")]
        public Face3DAngle Face3DAngle { set; get; }

        /// <summary>
        /// RGB 活体 0：非真人；1：真人；-1：不确定；-2:传入人脸数&gt;1；-3: 人脸过小 -4: 角度过大 -5: 人脸超出边界 -6: 深度图错误 -7: 红外图太亮了
        /// </summary>
        public int RgbLive { set; get; }

        /// <summary>
        /// 口罩 "0" 代表没有带口罩，"1"代表带口罩 ,"-1"表不确定
        /// </summary>
        public int Mask { set; get; } = -1;

        /// <summary>
        /// 戴眼镜置信度[0-1],推荐阈值0.5
        /// </summary>
        public float WearGlasses { set; get; }

        /// <summary>
        /// 左眼状态
        /// </summary>
        public bool? IsLeftEyeClosed { set; get; }

        /// <summary>
        /// 右眼状态
        /// </summary>
        public bool? IsRightEyeClosed { set; get; }

        /// <summary>
        /// "1" 表示 遮挡, "0" 表示 未遮挡, "-1" 表示不确定
        /// </summary>
        public int FaceShelter { set; get; } = -1;

        /// <summary>
        /// 额头坐标 Empty 表示无效
        /// </summary>
        public PointF FaceLandPoint { set; get; } = PointF.Empty;

        /// <summary>
        /// 特征
        /// </summary>
        public byte[] Feature { get; set; }

        /// <summary>
        /// 图像质量 -1无效
        /// </summary>
        public float ImageQuality { get; set; } = -1f;

        /// <summary>
        /// </summary>
        [JsonIgnore]
        public Rectangle Rectangle { get; set; } = Rectangle.Empty;
    }
}