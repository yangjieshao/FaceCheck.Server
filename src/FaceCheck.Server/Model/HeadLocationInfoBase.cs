namespace FaceCheck.Server.Model
{
    public class HeadLocationInfoBase
    {
        public int X { set; get; }

        public int Y { set; get; }

        public int Width { set; get; }

        public int Height { set; get; }

        /// <summary>
        /// 图像质量 -1无效
        /// </summary>
        public float ImageQuality { set; get; } = -1;

        public byte[] SnapBuffer { set; get; }

        public FaceInfo FaceInfo { set; get; }
    }
}