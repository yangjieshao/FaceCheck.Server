namespace FaceCheck.Server.Helper.Configs
{
    public class CutPhotoConfig
    {
        public const string Section = "CutPic";

        /// <summary>
        /// 是否要截图
        /// </summary>
        public bool Need { get; set; } = true;
        /// <summary>
        /// 照片最大宽度 大于这个宽度会自动缩小
        /// </summary>
        public int MaxWidth { get; set; } = 240;

        /// <summary>
        /// 照片最小宽度 小于这个宽度 大于最小可用宽度 宽度会自动缩放
        /// </summary>
        public int MinWidth { get; set; } = 240;

        /// <summary>
        /// 最小可用宽度 (建议为最小瞳距的3~4倍) 小于这个宽度会返回错误信息 默认 50
        /// <para />
        /// 照片最小可进行缩放长宽（小于这个尺寸就放弃照片）
        /// </summary>
        public int MinSize { get; set; } = 50;

        /// <summary>
        /// 外扩比例(分母) 整数
        /// </summary>
        public int OutwardScale { get; set; } = 2;

        /// <summary>
        /// 最小外扩像素
        /// </summary>
        public int MinOutwardPix { get; set; } = 15;

        /// <summary>
        /// 裁剪照片 高度比
        /// </summary>
        public double ScaleHeight { get; set; } = 1.0;

        /// <summary>
        /// 裁剪照片 宽度比
        /// </summary>
        public double ScaleWidth { get; set; } = 1.0;

        /// <summary>
        /// </summary>
        public BackgroundColor NewBg { set; get; } = new BackgroundColor();

        public class BackgroundColor
        {
            /// <summary>
            /// 是否需要将图片填充至1:1
            /// </summary>
            public bool Need { set; get; } = false;

            /// <summary>
            /// </summary>
            public byte R { set; get; } = 255;

            /// <summary>
            /// </summary>
            public byte G { set; get; } = 255;

            /// <summary>
            /// </summary>
            public byte B { set; get; } = 255;

            /// <summary>
            /// </summary>
            public byte A { set; get; } = 255;
        }
    }
}