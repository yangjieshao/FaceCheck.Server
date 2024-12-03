using FaceCheck.Server.Configs;
using FaceCheck.Server.Helper.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASFFunctions_3_X = Yj.ArcSoftSDK.ASFFunctions;
using ASFFunctions_4_0 = Yj.ArcSoftSDK._4_0.ASFFunctions;
using FaceInfo_3_X = Yj.ArcSoftSDK.Models.FaceInfo;
using FaceInfo_4_0 = Yj.ArcSoftSDK._4_0.Models.FaceInfo;

namespace FaceCheck.Server.Util
{
    /// <summary>
    /// </summary>
    public class ArcFaceUtil : IDisposable
    {
        private ILogger<ArcFaceUtil> Logger { get; }

        private ArcSoftConfig ArcSoftConfig { get; } = new();
         
        private SystemConfig SystemConfig { get; } = new();

        private CheckPicConfig CheckPicConfig { get; }

        /// <summary>
        /// 是否为商用授权版本
        /// </summary>
        public bool IsPro { set; get; }

        /// <summary>
        /// 是否已激活 未激活可能是没填授权码 是正常情况
        /// </summary>
        public bool HadActivation { private set; get; }

        private readonly ConcurrentBag<IntPtr> Engines = [];

        /// <summary>
        /// 获取空闲引擎
        /// </summary>
        /// <returns></returns>
        public IntPtr GetFreeEngine()
        {
            var result = IntPtr.Zero;
            int i = 0;
            while (result == IntPtr.Zero
                && i < 100)
            {
                if (!Engines.IsEmpty
                    && Engines.TryTake(out IntPtr val))
                {
                    result = val;
                }
                Task.Delay(2).Wait();
                i++;
            }

            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="engine"></param>
        public void ReturnEngine(IntPtr engine)
        {
            if (engine != IntPtr.Zero)
            {
                Engines.Add(engine);
            }
        }

        /// <summary>
        /// </summary>
        public ArcFaceUtil(ILogger<ArcFaceUtil> logger, IConfiguration configuration)
        {
            Logger = logger;
            configuration.Bind(ArcSoftConfig.Section, ArcSoftConfig);
            configuration.Bind(SystemConfig.Section, SystemConfig);
            CheckPicConfig = ArcSoftConfig.CheckPic ?? new();
        }

        /// <summary>
        /// 激活虹软SDK
        /// </summary>
        /// <returns> </returns>
        public bool Activation()
        {
            if(!SystemConfig.IsReal)
            {
                Logger.LogInformation("不真实使用，跳过虹软");
                return true;
            }

            int activationRet;
            if (ArcSoftConfig.ActiveFiles is { Count:>0})
            {
                Logger.LogInformation("准备激活虹软SDK");

                var deviceInfo = ASFFunctions_4_0.GetActiveDeviceInfo();
                Logger.LogInformation("设备信息: {deviceInfo}", deviceInfo);

                IsPro = true;
                foreach (var activeFile in ArcSoftConfig.ActiveFiles)
                {
                    activationRet = ASFFunctions_4_0.OfflineActivation(activeFile);
                    if (activationRet == 0)
                    {
                        var initEngine = IntPtr.Zero;
                        var initEngineRet = ASFFunctions_4_0.InitEngine(pEngine: ref initEngine, isImgMode: true, faceMaxNum: 0,
                            isAngleZeroOnly: false, needFaceInfo: false, needRgbLive: false, needIrLive: false,
                            needFaceFeature: false, needImageQuality: false);
                        if(initEngineRet == 0 )
                        {
                            HadActivation = true;
                            Logger.LogInformation("初始化虹软 使用授权文件 激活SDK成功 永久版:true  授权文件地址:{activeFile}", activeFile);
                            ASFFunctions_4_0.UninitEngine(ref initEngine);
                            break;
                        }
                        else if(initEngineRet == 90118)
                        {
                            Logger.LogWarning("初始化虹软 使用授权文件 激活SDK失败 设备不匹配 授权文件地址:{activeFile}", activeFile);
                        }
                        else
                        {
                            Logger.LogWarning("初始化虹软 使用授权文件 激活SDK失败 授权文件地址:{activeFile}  {initEngineRet}", activeFile, initEngineRet);
                        }
                    }
                    else
                    {
                        Logger.LogWarning("初始化虹软 设置授权文件失败 授权文件地址:{activeFile}  {initEngineRet}", activeFile, activationRet);
                        activationRet = -1;
                    }
                }
            }

            if(!HadActivation)
            {
                if (string.IsNullOrWhiteSpace(ArcSoftConfig.APPID))
                {
                    Logger.LogInformation("未配置虹软授权码 不进行虹软SDK激活");
                    return false;
                }
                Logger.LogInformation("准备激活虹软SDK");
                if (string.IsNullOrWhiteSpace(ArcSoftConfig.ProActiveKey32)
                   && string.IsNullOrWhiteSpace(ArcSoftConfig.ProActiveKey64)
                   && string.IsNullOrWhiteSpace(ArcSoftConfig.ProActiveKeySo64))
                {
                    activationRet = ASFFunctions_3_X.Activation(ArcSoftConfig.APPID, ArcSoftConfig.KEY32, ArcSoftConfig.KEY64, ArcSoftConfig.KEYSo64, string.Empty, string.Empty, string.Empty);
                }
                else
                {
                    var deviceInfo = ASFFunctions_4_0.GetActiveDeviceInfo();
                    Logger.LogInformation("设备信息: {deviceInfo}", deviceInfo);
                    IsPro = true;
                    activationRet = ASFFunctions_4_0.Activation(ArcSoftConfig.APPID, ArcSoftConfig.KEY32, ArcSoftConfig.KEY64, ArcSoftConfig.KEYSo64
                                                              , ArcSoftConfig.ProActiveKey32, ArcSoftConfig.ProActiveKey64, ArcSoftConfig.ProActiveKeySo64);
                }
                if (activationRet == 0)
                {
                    HadActivation = true;
                    Logger.LogInformation("初始化虹软 激活SDK成功 永久版:{IsPro}", IsPro);
                }
                else
                {
                    Logger.LogWarning("初始化虹软 激活SDK失败 Error :{ret}", activationRet);
                }
            }

            if (HadActivation)
            {
                Logger.LogInformation("开始初始化引擎");
                for (int i = 0; i < ArcSoftConfig.EngineNum; i++)
                {
                    var pEngine = CreateDetectFacesEngine(faceMaxNum: 1, needRgbLive: CheckPicConfig.NeedRgbLive, needImageQuality: CheckPicConfig.ImageQuality > 0
                    , needFaceInfo: true, needFaceFeature: true, isAngleZeroOnly: ArcSoftConfig.IsAngleZeroOnly);
                    if (pEngine != IntPtr.Zero)
                    {
                        Logger.LogInformation("初始化第{num}个引擎<{Engine}> 成功", i + 1, pEngine);
                        Engines.Add(pEngine);
                    }
                }
                Logger.LogInformation("共初始化了{num}个引擎", Engines.Count);
            }
            return HadActivation;
        }

        /// <summary>
        /// 创建引擎
        /// </summary>
        /// <returns> </returns>
        private IntPtr CreateDetectFacesEngine(int faceMaxNum, bool needRgbLive, bool needImageQuality, bool needFaceInfo, bool needFaceFeature, bool isAngleZeroOnly)
        {
            if (!HadActivation)
            {
                return IntPtr.Zero;
            }
            var result = IntPtr.Zero;
            int ret;
            if (IsPro)
            {
                ret = ASFFunctions_4_0.InitEngine(pEngine: ref result, isImgMode: true, faceMaxNum: faceMaxNum,
                    isAngleZeroOnly: isAngleZeroOnly, needFaceInfo: needFaceInfo, needRgbLive: needRgbLive, needIrLive: false,
                    needFaceFeature: needFaceFeature, needImageQuality: needImageQuality);
            }
            else
            {
                ret = ASFFunctions_3_X.InitEngine(pEngine: ref result, isImgMode: true, faceMaxNum: faceMaxNum,
                    isAngleZeroOnly: isAngleZeroOnly, needFaceInfo: needFaceInfo, needRgbLive: needRgbLive, needIrLive: false,
                    needFaceFeature: needFaceFeature, needImageQuality: needImageQuality);
            }
            if (ret != 0)
            {
                Logger.LogError($"InitEngine Error :{ret}");
            }
            return result;
        }

        /// <summary>
        /// 获取特征值
        /// </summary>
        public Model.FaceInfo GetFaceInfo(IntPtr pEngine, byte[] imageBuffer, bool isRegister = true, bool needFaceInfo = true, bool needFeatures = true)
        {
            if (!HadActivation)
            {
                return null;
            }

            List<FaceInfo_3_X> faceFeatures_3_x = null;
            List<FaceInfo_4_0> faceFeatures_4_0 = null;

            if (IsPro)
            {
                faceFeatures_4_0 = ASFFunctions_4_0.DetectFacesEx(pEngine, imageBuffer,
                               faceMinWith: 0, needCheckImage: true, needFaceInfo: needFaceInfo,
                               needRgbLive: CheckPicConfig.NeedRgbLive, needIrLive: false, needFeatures: needFeatures,
                               isRegister: isRegister, needImageQuality: CheckPicConfig.ImageQuality > 0);
            }
            else
            {
                faceFeatures_3_x = ASFFunctions_3_X.DetectFacesEx(pEngine, imageBuffer,
                               faceMinWith: 0, needCheckImage: true, needFaceInfo: needFaceInfo,
                               needRgbLive: CheckPicConfig.NeedRgbLive, needIrLive: false, needFeatures: needFeatures);
            }

            Model.FaceInfo result = null;

            if (faceFeatures_4_0 != null
                && faceFeatures_4_0.Count > 0)
            {
                var faceInfo = GetMaximumFaceInfo(faceFeatures_4_0);
                if (faceInfo != null)
                {
                    result = new Model.FaceInfo
                    {
                        Feature = faceInfo.Feature,
                        ImageQuality = faceInfo.ImageQuality,
                        RgbLive = faceInfo.RgbLive,
                        Age = faceInfo.Age,
                        IsLeftEyeClosed = faceInfo.IsLeftEyeClosed,
                        IsRightEyeClosed = faceInfo.IsRightEyeClosed,
                        FaceShelter = faceInfo.FaceShelter,
                        Mask = faceInfo.Mask,
                        FaceOrient = faceInfo.FaceOrient,
                        Sex = faceInfo.Gender,
                        WearGlasses = faceInfo.WearGlasses,
                        Rectangle = new Model.Rectangle
                        {
                            Y = faceInfo.Rectangle.Y,
                            X = faceInfo.Rectangle.X,
                            Width = faceInfo.Rectangle.Width,
                            Height = faceInfo.Rectangle.Height
                        },
                        FaceLandPoint = new Model.PointF
                        {
                            X = faceInfo.FaceLandPoint.X,
                            Y = faceInfo.FaceLandPoint.Y
                        },
                        Face3DAngle = new Model.Face3DAngle
                        {
                            Pitch = faceInfo.Face3DAngle.Pitch,
                            Roll = faceInfo.Face3DAngle.Roll,
                            Yaw = faceInfo.Face3DAngle.Yaw,
                            Status = faceInfo.Face3DAngle.Status
                        }
                    };
                }
            }
            if (faceFeatures_3_x != null
                && faceFeatures_3_x.Count > 0)
            {
                var faceInfo = GetMaximumFaceInfo(faceFeatures_3_x);
                if (faceInfo != null)
                {
                    result = new Model.FaceInfo
                    {
                        Feature = faceInfo.Feature,
                        ImageQuality = faceInfo.ImageQuality,
                        RgbLive = faceInfo.RgbLive,
                        Age = faceInfo.Age,
                        FaceOrient = faceInfo.FaceOrient,
                        Sex = faceInfo.Gender,
                        Rectangle = new Model.Rectangle
                        {
                            Y = faceInfo.Rectangle.Y,
                            X = faceInfo.Rectangle.X,
                            Width = faceInfo.Rectangle.Width,
                            Height = faceInfo.Rectangle.Height
                        },
                        Face3DAngle = new Model.Face3DAngle
                        {
                            Pitch = faceInfo.Face3DAngle.Pitch,
                            Roll = faceInfo.Face3DAngle.Roll,
                            Yaw = faceInfo.Face3DAngle.Yaw,
                            Status = faceInfo.Face3DAngle.Status
                        }
                    };
                }
            }

            if (result == null)
            {
                // 照片没有人脸
                return result;
            }

            if (!CheckPicConfig.NeedRgbLive)
            {
                result.RgbLive = 1;
            }
            if (!IsPro || CheckPicConfig.ImageQuality <= 0)
            {
                result.ImageQuality = -1;
            }
            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="pEngine"> </param>
        /// <param name="pFeature1"> </param>
        /// <param name="pFeature2"> </param>
        /// <param name="isIdcardCompare"> </param>
        /// <returns> 相似度 0.0~1.0 </returns>
        public double FaceFeatureCompare(IntPtr pEngine, IntPtr pFeature1, IntPtr pFeature2, bool isIdcardCompare = false)
        {
            if (!HadActivation)
            {
                return 0.0d;
            }
            if (pFeature1 == IntPtr.Zero
                || pFeature2 == IntPtr.Zero)
            {
                return 0.0d;
            }
            double retult;
            if (IsPro)
            {
                retult = ASFFunctions_4_0.FaceFeatureCompare(pEngine, pFeature1, pFeature2, isIdcardCompare);
            }
            else
            {
                retult = ASFFunctions_3_X.FaceFeatureCompare(pEngine, pFeature1, pFeature2, isIdcardCompare);
            }

            return retult;
        }

        /// <summary>
        /// 获取最大人脸范围
        /// </summary>
        /// <returns> </returns>
        private static FaceInfo_3_X GetMaximumFaceInfo(List<FaceInfo_3_X> faceInfos)
        {
            FaceInfo_3_X result = null;
            int maxWidth = 0;
            if (faceInfos != null
                && faceInfos.Count > 0)
            {
                foreach (var faceInfo in faceInfos)
                {
                    var rect = faceInfo.Rectangle;
                    int width = rect.Right - rect.Left;
                    if (width > maxWidth)
                    {
                        maxWidth = width;
                        result = faceInfo;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取最大人脸范围
        /// </summary>
        /// <returns> </returns>
        private static FaceInfo_4_0 GetMaximumFaceInfo(List<FaceInfo_4_0> faceInfos, float imageQuality = -1)
        {
            FaceInfo_4_0 result = null;
            int maxWidth = 0;
            if (faceInfos != null
                && faceInfos.Count > 0)
            {
                foreach (var faceInfo in faceInfos)
                {
                    if (result == null
                        || imageQuality <= float.Epsilon
                        || imageQuality <= faceInfo.ImageQuality)
                    {
                        var rect = faceInfo.Rectangle;
                        int width = rect.Right - rect.Left;
                        if (width > maxWidth)
                        {
                            maxWidth = width;
                            result = faceInfo;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 销毁引擎
        /// </summary>
        /// <param name="pEngine"> </param>
        private void UninitEngine(ref IntPtr pEngine)
        {
            if (!HadActivation)
            {
                return;
            }
            if (pEngine != IntPtr.Zero)
            {
                if (IsPro)
                {
                    ASFFunctions_4_0.UninitEngine(ref pEngine);
                }
                else
                {
                    ASFFunctions_3_X.UninitEngine(ref pEngine);
                }
                pEngine = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 释放特征值指针 create by <see cref="Feature2IntPtr(byte[])" />
        /// </summary>
        /// <param name="featureIntPtr"> </param>
        public void FreeFeatureIntPtr(IntPtr featureIntPtr)
        {
            if (featureIntPtr == IntPtr.Zero)
            {
                return;
            }
            if (IsPro)
            {
                ASFFunctions_4_0.FreeFeatureIntPtr(featureIntPtr);
            }
            else
            {
                ASFFunctions_3_X.FreeFeatureIntPtr(featureIntPtr);
            }
        }

        /// <summary>
        /// 获取特征值指针 free by <see cref="FreeFeatureIntPtr(IntPtr)" />
        /// </summary>
        /// <param name="feature"> </param>
        /// <returns> </returns>
        public IntPtr Feature2IntPtr(byte[] feature)
        {
            if (IsPro)
            {
                return ASFFunctions_4_0.Feature2IntPtr(feature);
            }
            else
            {
                return ASFFunctions_3_X.Feature2IntPtr(feature);
            }
        }

        #region IDisposable Support

        private bool disposedValue = false;

        /// <summary>
        /// </summary>
        /// <param name="disposing"> </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (!Engines.IsEmpty)
                    {
                        IntPtr[] engines = new IntPtr[Engines.Count];

                        Engines.CopyTo(engines, 0);
                        Engines.Clear();
                        for (int i = 0; i < engines.Length; i++)
                        {
                            UninitEngine(ref engines[i]);
                        }
                    }
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// </summary>
        ~ArcFaceUtil()
        {
            Dispose(false);
        }

        /// <summary>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}