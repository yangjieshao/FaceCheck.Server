using FaceCheck.Server.Configs;
using FaceCheck.Server.Helper;
using FaceCheck.Server.Helper.Configs;
using FaceCheck.Server.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FaceCheck.Server.Util
{
    /// <summary>
    /// 实际处理照片
    /// </summary>
    public class PhotoCheck
    {
        private ILogger<PhotoCheck> Logger { get; }
        private ArcFaceUtil ArcFaceUtil { get; }
        private SystemConfig SystemConfig { get; } = new SystemConfig();
        private CheckPicConfig CheckPicConfig { get; } = new CheckPicConfig();
        private ArcSoftConfig ArcSoftConfig { get; } = new ArcSoftConfig();
        private CutPhotoConfig CutPhotoConfig { get; } = new CutPhotoConfig();

        /// <summary>
        /// </summary>
        public PhotoCheck(ILogger<PhotoCheck> logger, ArcFaceUtil arcFaceUtil, IConfiguration configuration)
        {
            Logger = logger;
            ArcFaceUtil = arcFaceUtil;
            configuration.Bind(SystemConfig.Section, SystemConfig);
            configuration.Bind(ArcSoftConfig.Section, ArcSoftConfig);
            configuration.Bind(CutPhotoConfig.Section, CutPhotoConfig);
            CheckPicConfig = ArcSoftConfig.CheckPic ?? new();
        }

        public HttpResult<byte[]> CheckFace(byte[] picBuffer)
        {
            if (!picBuffer.IsFacePhoto())
            {
                throw new Exception("只支持png或jpg或bmp或gif图片");
            }
            var result = new HttpResult<byte[]>();

            if (!SystemConfig.IsReal)
            {
                Logger.LogInformation($"不校验 直接返回原数据");
                result.Message = string.Empty;
                result.Data = picBuffer;
                result.Success = true;
                return result;
            }

            var faceInfo = GetFaceInfo(picBuffer);

            if (faceInfo.Rectangle.Width < CutPhotoConfig.MinSize
                || faceInfo.Rectangle.Height < CutPhotoConfig.MinSize)
            {
                throw new Exception($"小于最小可用宽度 {faceInfo.Rectangle.Width}*{faceInfo.Rectangle.Height} PhotoMinSize:{CutPhotoConfig.MinSize}");
            }

            var image = SKBitmap.Decode(picBuffer)
                ?? throw new Exception("图片解析失败");
            GetMaxFaceImage(ref image, faceInfo);

            result.Data = ImageToBytes(image);
            result.Success = true;
            image.Dispose();

            if (SystemConfig.IsSaveImg)
            {
                Helper.Util.SaveTempPic(result.Data);
            }
            return result;
        }

        public HttpResult<HeadLocationInfoBase> CheckIdFace(byte[] picBuffer)
        {
            if (!picBuffer.IsFacePhoto())
            {
                throw new Exception("只支持png或jpg或bmp图片");
            }

            var result = new HttpResult<HeadLocationInfoBase>();

            if (!SystemConfig.IsReal)
            {
                Logger.LogInformation($"不校验 直接返回原数据");
                var headLocationInfo_ = new HeadLocationInfoBase
                {
                    FaceInfo = new FaceInfo
                    {
                        IsLeftEyeClosed = false,
                        IsRightEyeClosed = false,
                        RgbLive = 1,
                        FaceShelter = 0,
                        Face3DAngle = new Face3DAngle
                        {
                        }
                    },
                    SnapBuffer = picBuffer
                };
                result.Data = headLocationInfo_;
                result.Success = true;
                return result;
            }

            var faceInfo = GetFaceInfo(picBuffer, only4Id: true);

            byte[] snapBuffer = null;
            if (CutPhotoConfig.Need)
            {
                var image = SKBitmap.Decode(picBuffer)
                    ?? throw new Exception("图片解析失败");
                GetMaxFaceImage(ref image, faceInfo, only4Id: true);
                snapBuffer = ImageToBytes(image);
                image.Dispose();
            }

            var headLocationInfo = new HeadLocationInfoBase
            {
                Height = faceInfo.Rectangle.Height,
                Width = faceInfo.Rectangle.Width,
                X = faceInfo.Rectangle.X,
                Y = faceInfo.Rectangle.Y,
                ImageQuality = faceInfo.ImageQuality,
                FaceInfo = faceInfo,
                SnapBuffer = snapBuffer
            };
            result.Data = headLocationInfo;
            result.Success = true;
            if (SystemConfig.IsSaveImg)
            {
                Helper.Util.SaveTempPic(result.Data.SnapBuffer);
            }
            Logger.LogInformation($"身份证照片扣脸校验完毕");
            return result;
        }

        public HttpResult<HeadLocationInfoBase> CheckFace2(byte[] picBuffer)
        {
            if (!picBuffer.IsFacePhoto())
            {
                throw new Exception("只支持png或jpg或bmp图片");
            }

            var result = new HttpResult<HeadLocationInfoBase>();

            if (!SystemConfig.IsReal)
            {
                Logger.LogInformation($"不校验 直接返回原数据");
                var headLocationInfo_ = new HeadLocationInfoBase
                {
                    FaceInfo = new FaceInfo
                    {
                        IsLeftEyeClosed = false,
                        IsRightEyeClosed = false,
                        RgbLive = 1,
                        FaceShelter = 0,
                        Face3DAngle = new Face3DAngle
                        {
                        }
                    },
                    SnapBuffer = picBuffer
                };
                result.Data = headLocationInfo_;
                result.Success = true;
                return result;
            }

            var faceInfo = GetFaceInfo(picBuffer);

            if (faceInfo.Rectangle.Width < CutPhotoConfig.MinSize
                || faceInfo.Rectangle.Height < CutPhotoConfig.MinSize)
            {
                throw new Exception($"小于最小可用宽度 {faceInfo.Rectangle.Width}*{faceInfo.Rectangle.Height} PhotoMinSize:{CutPhotoConfig.MinSize}");
            }

            byte[] snapBuffer = null;
            if (CutPhotoConfig.Need)
            {
                var image = SKBitmap.Decode(picBuffer)
                    ?? throw new Exception("图片解析失败");
                GetMaxFaceImage(ref image, faceInfo);
                snapBuffer = ImageToBytes(image);
                image.Dispose();
            }

            var headLocationInfo = new HeadLocationInfoBase
            {
                Height = faceInfo.Rectangle.Height,
                Width = faceInfo.Rectangle.Width,
                X = faceInfo.Rectangle.X,
                Y = faceInfo.Rectangle.Y,
                ImageQuality = faceInfo.ImageQuality,
                FaceInfo = faceInfo,
                SnapBuffer = snapBuffer
            };
            result.Data = headLocationInfo;
            result.Success = true;
            if (SystemConfig.IsSaveImg)
            {
                Helper.Util.SaveTempPic(result.Data.SnapBuffer);
            }
            Logger.LogInformation($"登记照扣脸校验完毕");
            return result;
        }

        public HttpResult<double> PicCompare(byte[] picBuffer1, byte[] picBuffer2)
        {
            if (!picBuffer1.IsFacePhoto()
                || !picBuffer2.IsFacePhoto())
            {
                throw new Exception("只支持png或jpg或bmp图片");
            }
            var result = new HttpResult<double>();
            if (!SystemConfig.IsReal)
            {
                Logger.LogInformation($"不校验 直接返回成功数据");
                result.Message = string.Empty;
                result.Data = 0.9;
                result.Success = true;
                return result;
            }

            if (picBuffer1 == null
                || picBuffer1.Length == 0
                || picBuffer2 == null
                || picBuffer2.Length == 0)
            {
                Logger.LogInformation($"未接到照片流");
                result.Data = -1;
                result.Success = false;
                return result;
            }
            var engine1 = ArcFaceUtil.GetFreeEngine();
            if (engine1 == IntPtr.Zero)
            {
                Logger.LogInformation($"系统忙，稍后重试");
                return new HttpResult<double> { Success = false, Message = "系统忙，稍后重试" };
            }
            var engine2 = engine1;
            if (ArcSoftConfig.EngineNum > 1)
            {
                engine2 = ArcFaceUtil.GetFreeEngine();
                if (engine2 == IntPtr.Zero)
                {
                    engine2 = engine1;
                }
            }
            FaceInfo faceInfo1 = null;
            FaceInfo faceInfo2 = null;
            if (engine1 != engine2)
            {
                var task1 = Task.Run(() =>
                {
                    Logger.LogInformation($"准备获取第一张照片的人脸数据");
                    faceInfo1 = ArcFaceUtil.GetFaceInfo(engine1, picBuffer1, isRegister: true, needFaceInfo: false, needFeatures: true);
                    Logger.LogInformation($"获取第一张照片的人脸数据结束");
                });
                var task2 = Task.Run(() =>
                {
                    Logger.LogInformation($"准备获取第二张照片的人脸数据");
                    faceInfo2 = ArcFaceUtil.GetFaceInfo(engine2, picBuffer2, isRegister: true, needFaceInfo: false, needFeatures: true);
                    Logger.LogInformation($"获取第二张照片的人脸数据结束");
                });
                Task.WaitAll(task1, task2);
            }
            else
            {
                Logger.LogInformation($"准备获取第一张照片的人脸数据");
                faceInfo1 = ArcFaceUtil.GetFaceInfo(engine1, picBuffer1, isRegister: true, needFaceInfo: false, needFeatures: true);
                Logger.LogInformation($"获取第一张照片的人脸数据结束");

                Logger.LogInformation($"准备获取第二张照片的人脸数据");
                faceInfo2 = ArcFaceUtil.GetFaceInfo(engine2, picBuffer2, isRegister: true, needFaceInfo: false, needFeatures: true);
                Logger.LogInformation($"获取第二张照片的人脸数据结束");
            }

            if (faceInfo1 == null)
            {
                if (engine1 != engine2)
                {
                    ArcFaceUtil.ReturnEngine(engine2);
                }
                ArcFaceUtil.ReturnEngine(engine1);
                throw new Exception("第一张照片没有人脸");
            }
            if (faceInfo2 == null)
            {
                if (engine1 != engine2)
                {
                    ArcFaceUtil.ReturnEngine(engine2);
                }
                ArcFaceUtil.ReturnEngine(engine1);
                throw new Exception("第二张照片没有人脸");
            }

            var feature1 = ArcFaceUtil.Feature2IntPtr(faceInfo1.Feature);
            var feature2 = ArcFaceUtil.Feature2IntPtr(faceInfo2.Feature);
            result.Data = ArcFaceUtil.FaceFeatureCompare(engine1, feature1, feature2, isIdcardCompare: ArcSoftConfig.IsIdcardCompare);

            if (engine1 != engine2)
            {
                ArcFaceUtil.ReturnEngine(engine2);
            }
            ArcFaceUtil.ReturnEngine(engine1);
            result.Success = result.Data >= ArcSoftConfig.Similarity;

            Logger.LogInformation("两张照片相似度为:{Similarity} 对比算法通过:<{Success}>", result.Data, result.Success);
            return result;
        }

        public void ReadyUrlBuffer(ref SetPicInfo comparePicInfo)
        {
            var info4Log = new SetPicInfo4Log
            {
                Pic1 = comparePicInfo.Pic1 is { Length: > 0 },
                Pic2 = comparePicInfo.Pic2 is { Length: > 0 },
            };
            if (SystemConfig.IsSaveImg)
            {
                // 演示时用 正式使用 关闭保存图片配置
                info4Log.Pic1Path = Helper.Util.SavePic(comparePicInfo.Pic1, false);
                info4Log.Pic2Path = Helper.Util.SavePic(comparePicInfo.Pic2, false);
            }
            Logger.LogInformation($"接到数据（转换后） Pic1:<{info4Log.Pic1}> Pic2:<{info4Log.Pic2}> " +
                                                 $"Pic1Path:<{info4Log.Pic1Path}> Pic2Path:<{info4Log.Pic2Path}>");
        }
        /// <summary>
        /// </summary>
        /// <param name="picBuffer"></param>
        /// <param name="only4Id"> 只是用于提取证件照(证件照不校验大小、活体、质量等人脸参数) </param>
        private Model.FaceInfo GetFaceInfo(byte[] picBuffer,bool only4Id=false)
        {
            var engine = ArcFaceUtil.GetFreeEngine();
            if (engine == IntPtr.Zero)
            {
                throw new Exception("系统忙，稍后重试");
            }

            var faceInfo = ArcFaceUtil.GetFaceInfo(engine, picBuffer, isRegister: false, needFaceInfo: true, needFeatures: false);
            if (faceInfo == null)
            {
                ArcFaceUtil.ReturnEngine(engine);
                throw new Exception("照片提取人脸失败");
            }
            if (CheckPicConfig.NeedRgbLive
                && ArcFaceUtil.IsPro
                && faceInfo.RgbLive != 1
                && !only4Id)
            {
                ArcFaceUtil.ReturnEngine(engine);
                var msg = $"活体检测未通过" + faceInfo.RgbLive switch
                {
                    -1 => " 不确定是否真人",
                    -2 => " 画面内人脸数大于1",
                    -3 => " 人脸过小",
                    -4 => " 角度过大",
                    -5 => " 人脸超出边界",
                    -6 => " 深度图错误",
                    -7 => " 红外图太亮了",
                    _ => throw new NotImplementedException()
                }; ;
                Logger.LogWarning(msg);
                throw new Exception(msg);
            }
            if (CheckPicConfig.ImageQuality > 0
                && ArcFaceUtil.IsPro
                && faceInfo.ImageQuality < CheckPicConfig.ImageQuality
                && !only4Id)
            {
                ArcFaceUtil.ReturnEngine(engine);
                throw new Exception("照片质量不合格");
            }

            ArcFaceUtil.ReturnEngine(engine);
            return faceInfo;
        }

        /// <summary>
        /// 获取外扩大小
        /// </summary>
        private void GetOutSize(int left, int right, int top, int bottom, ref int headOutWidth, ref int headOutHeight, bool needSwapWidthHeight = false)
        {
            // 外扩宽度
            headOutWidth = (right - left) / CutPhotoConfig.OutwardScale;
            // 外扩高度
            headOutHeight = (bottom - top) / CutPhotoConfig.OutwardScale;
            if (headOutWidth < CutPhotoConfig.MinOutwardPix)
            {
                headOutWidth = CutPhotoConfig.MinOutwardPix;
            }
            if (headOutHeight < CutPhotoConfig.MinOutwardPix)
            {
                headOutHeight = CutPhotoConfig.MinOutwardPix;
            }
            if (Math.Abs(CutPhotoConfig.ScaleWidth - CutPhotoConfig.ScaleHeight) > double.Epsilon)
            {
                var oldWidth = right - left;
                var oldHeight = bottom - top;
                var newWidth = oldWidth + headOutWidth * 2;
                var newHeight = oldHeight + headOutHeight * 2;
                if (CutPhotoConfig.ScaleWidth < CutPhotoConfig.ScaleHeight)
                {
                    headOutHeight = Convert.ToInt32((newWidth * CutPhotoConfig.ScaleHeight / CutPhotoConfig.ScaleWidth - oldHeight) / 2);
                }
                else
                {
                    headOutWidth = Convert.ToInt32((newHeight * CutPhotoConfig.ScaleWidth / CutPhotoConfig.ScaleHeight - oldWidth) / 2);
                }

                if (needSwapWidthHeight)
                {
                    (headOutWidth, headOutHeight) = (headOutHeight, headOutWidth);
                }
            }
        }
        
        private static SKBitmap CutImage(SKBitmap image, Rectangle rect)
        {
            var rotatedBitmap = new SKBitmap(rect.Width, rect.Height);
            
            using (var surface = new SKCanvas(rotatedBitmap))
            {
                surface.DrawBitmap(image, new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom), new SKRect(0, 0, rect.Width, rect.Height));
            }
            return rotatedBitmap;
        }

        private SKBitmap Rotate(SKBitmap bitmap, double angle, int orient)
        {
            var radians = Math.PI * angle / 180d;
            var sine = (float)Math.Abs(Math.Sin(radians));
            var cosine = (float)Math.Abs(Math.Cos(radians));
            var originalWidth = bitmap.Width;
            var originalHeight = bitmap.Height;
            var rotatedWidth = (int)(cosine * originalWidth + sine * originalHeight);
            var rotatedHeight = (int)(cosine * originalHeight + sine * originalWidth);

            var rotatedBitmap = new SKBitmap(rotatedWidth, rotatedHeight);

            using (var surface = new SKCanvas(rotatedBitmap))
            {
                surface.Translate(rotatedWidth / 2, rotatedHeight / 2);
                surface.RotateDegrees((float)angle);
                surface.Translate(-originalWidth / 2, -originalHeight / 2);
                surface.DrawBitmap(bitmap, new SKPoint());
            }

            Logger.LogInformation($"人脸角度{orient} 旋转{angle}°");
            return rotatedBitmap;
        }

        /// <summary>
        /// </summary>
        /// <param name="image"></param>
        /// <param name="maxFaceInfo"></param>
        /// <param name="only4Id">只是用于提取证件照(证件照不校验大小、活体、质量等人脸参数)</param>
        private void GetMaxFaceImage(ref SKBitmap image, FaceInfo maxFaceInfo, bool only4Id = false)
        {
            if (!CheckPicConfig.NeedRgbLive
                || maxFaceInfo.RgbLive == 1
                || !ArcFaceUtil.IsPro
                || only4Id)
            {
                if ((CheckPicConfig.FaceMaxAngle > float.Epsilon
                    && maxFaceInfo.Face3DAngle != null
                    && maxFaceInfo.Face3DAngle.Status == 0
                    && Math.Abs(maxFaceInfo.Face3DAngle.Roll) <= CheckPicConfig.FaceMaxAngle
                    && Math.Abs(maxFaceInfo.Face3DAngle.Pitch) <= CheckPicConfig.FaceMaxAngle
                    && Math.Abs(maxFaceInfo.Face3DAngle.Yaw) <= CheckPicConfig.FaceMaxAngle)
                    || CheckPicConfig.FaceMaxAngle <= float.Epsilon
                    || only4Id)
                {
                    int outWidth = 0;
                    int outHeight = 0;
                    GetOutSize(maxFaceInfo.Rectangle.Left, maxFaceInfo.Rectangle.Right, maxFaceInfo.Rectangle.Top, maxFaceInfo.Rectangle.Bottom
                            , ref outWidth, ref outHeight, IsNeedRotate(maxFaceInfo.FaceOrient));

                    var rect = GetJHeadRect(image, maxFaceInfo.Rectangle.Left, maxFaceInfo.Rectangle.Right, maxFaceInfo.Rectangle.Top, maxFaceInfo.Rectangle.Bottom
                        , outWidth, outHeight);
                    var headImg = CutImage(image, rect);

                    image.Dispose();
                    image = headImg;
                    if (maxFaceInfo.FaceOrient != 0)
                    {
                        // 旋转人脸
                        var tempImage = maxFaceInfo.FaceOrient switch
                        {
                            60 or 90 or 120 => Rotate(image, 90, maxFaceInfo.FaceOrient),
                            150 or 180 or 210 => Rotate(image, 180, maxFaceInfo.FaceOrient),
                            240 or 270 or 300 => Rotate(image, -90, maxFaceInfo.FaceOrient),
                            _ => throw new NotImplementedException()
                        };
                        image.Dispose();
                        image = tempImage;
                    }
                    SetBackgroundColor(ref image);

                    // 照片是否过大 true 太大 要缩小 false 太小 要放大 null 不用缩放
                    bool? isToobig = null;

                    if(!only4Id)
                    {
                        if (image.Width > CutPhotoConfig.MaxWidth
                            || image.Height > CutPhotoConfig.MaxWidth)
                        {
                            isToobig = true;
                        }
                        else if (image.Width < CutPhotoConfig.MinWidth
                            && image.Height < CutPhotoConfig.MinWidth)
                        {
                            isToobig = false;
                        }
                    }
                    // 缩放尺寸
                    if (isToobig.HasValue)
                    {
                        var scale1 = Convert.ToSingle(image.Width) / Convert.ToSingle(isToobig.Value ? CutPhotoConfig.MaxWidth : CutPhotoConfig.MinWidth);
                        var scale2 = Convert.ToSingle(image.Height) / Convert.ToSingle(isToobig.Value ? CutPhotoConfig.MaxWidth : CutPhotoConfig.MinWidth);
                        var scale = scale1 < scale2 ? scale1 : scale2;
                        if (Math.Abs(scale - 1f) > float.Epsilon)
                        {
                            var newWidth = (int)Math.Floor(image.Width / scale);
                            var newHeight = (int)Math.Floor(image.Height / scale);
                            {
                                Logger.LogInformation($"照片尺寸不合适: {image.Width}*{image.Height} 进行自动缩放 {newWidth}*{newHeight}");
                                var newImage = ScaleImage(image, newWidth, newHeight);
                                image.Dispose();
                                image = newImage;
                            }
                        }
                    }
                }
                else
                {
                    Logger.LogWarning("人脸角度过大");
                    throw new Exception("人脸角度过大");
                }
            }
            else
            {
                var msg = $"活体检测未通过" + maxFaceInfo.RgbLive switch
                {
                    -1 => " 不确定是否真人",
                    -2 => " 画面内人脸数大于1",
                    -3 => " 人脸过小",
                    -4 => " 角度过大",
                    -5 => " 人脸超出边界",
                    -6 => " 深度图错误",
                    -7 => " 红外图太亮了",
                    _ => throw new NotImplementedException()
                }; ;
                Logger.LogWarning(msg);
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// 按指定宽高缩放图片
        /// </summary>
        /// <param name="image">原图片</param>
        /// <param name="dstWidth">目标图片宽</param>
        /// <param name="dstHeight">目标图片高</param>
        /// <returns></returns>
        private static SKBitmap ScaleImage(SKBitmap image, int dstWidth, int dstHeight)
        {
            try
            {
                ////按比例缩放
                //float scaleRate = GetWidthAndHeight(image.Width, image.Height, dstWidth, dstHeight);
                //int width = (int)(image.Width * scaleRate);
                //int height = (int)(image.Height * scaleRate);

                var width = dstWidth;
                var height = dstHeight;

                //将宽度调整为4的整数倍
                if (width % 4 != 0)
                {
                    width -= width % 4;
                }

                return image.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
            }
            catch (Exception)
            {
                // no use
            }

            return null;
        }

        /// <summary>
        /// 按长宽1:1填充背景色
        /// </summary>
        /// <param name="image"> </param>
        private void SetBackgroundColor(ref SKBitmap image)
        {
            if (image != null
                && image.Width != image.Height
                && CutPhotoConfig.NewBg.Need)
            {
                Logger.LogInformation("填充图片背景色");
                var isWidthLonger = image.Width > image.Height;
                //构造最终的图片白板
                var rotatedBitmap = new SKBitmap(isWidthLonger ? image.Width : image.Height, isWidthLonger ? image.Width : image.Height);

                using (var surface = new SKCanvas(rotatedBitmap))
                {
                    surface.DrawColor(new SKColor(CutPhotoConfig.NewBg.R, CutPhotoConfig.NewBg.G, CutPhotoConfig.NewBg.B, CutPhotoConfig.NewBg.A));
                    surface.DrawBitmap(image, new SKPoint((rotatedBitmap.Width - image.Width) / 2, (rotatedBitmap.Height - image.Height) / 2));
                }
                image.Dispose();
                image = rotatedBitmap;
            }
        }

        /// <summary>
        /// 矫正剪裁图片范围
        /// </summary>
        /// <param name="image"> 原图片 </param>
        /// <param name="rect"> </param>
        /// <param name="needSwapWidthHeight"> 需要交换长宽 </param>
        /// <returns> 剪裁后的图片 </returns>
        public static Rectangle GetJHeadRect(SKBitmap image, int rectLeft, int rectRight, int rectTop, int rectBottom, int headOutWidth, int headOutHeight)
        {
            var leftChange = rectLeft > headOutWidth ? rectLeft - headOutWidth : 0;
            var topChange = rectTop > headOutHeight ? rectTop - headOutHeight : 0;
            var rightChange = rectRight + headOutWidth;
            var bottomChange = rectBottom + headOutHeight;

            if (rightChange > image.Width)
            {
                rightChange = image.Width;
            }
            if (bottomChange > image.Height)
            {
                bottomChange = image.Height;
            }
            return new Rectangle { X = leftChange, Y = topChange, Width = rightChange - leftChange, Height = bottomChange - topChange };
        }

        private static byte[] ImageToBytes(SKBitmap image)
        {
            using var memStream = new MemoryStream();
            using var wstream = new SKManagedWStream(memStream);
            image.Encode(wstream, SKEncodedImageFormat.Jpeg, 80);
            return memStream.ToArray();
        }

        public static bool IsNeedRotate(int faceOrient)
        {
            return faceOrient switch
            {
                60 or 90 or 120 or 240 or 270 or 300 => true,
                _ => false
            };
        }
    }
}