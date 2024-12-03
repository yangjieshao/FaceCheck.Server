namespace FaceCheck.Server.Model
{
    /// <summary>
    /// 3D人脸角度检测
    /// </summary>
    public class Face3DAngle
    {
        /// <summary>
        /// 是否检测成功，0成功，其他为失败
        /// </summary>
        public int Status { set; get; }

        /// <summary>
        /// </summary>
        public float Roll { set; get; }

        /// <summary>
        /// </summary>
        public float Yaw { set; get; }

        /// <summary>
        /// </summary>
        public float Pitch { set; get; }
    }
}