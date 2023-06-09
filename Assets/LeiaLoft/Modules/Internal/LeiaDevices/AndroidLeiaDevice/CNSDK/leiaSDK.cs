#if UNITY_ANDROID
using System;
using System.Runtime.InteropServices;

namespace Leia
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Config
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
        public float[] dotPitchInMM;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
        public Int32[] panelResolution;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
        public Int32[] numViews;
        public Int32 sharpeningKernelXSize;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 18)]
        public float[] sharpeningKernelX;
        public Int32 sharpeningKernelYSize;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 18)]
        public float[] sharpeningKernelY;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
        public Int32[] viewResolution;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
        public Int32[] displaySizeInMm;
        public float act_gamma;
        public float act_beta;
        public float act_singleTapCoef;
        public float systemDisparityPercent;
        public float systemDisparityPixels;
        public float cameraCenterX;
        public float cameraCenterY;
        public float cameraCenterZ;
        public float cameraThetaX;
        public float cameraThetaY;
        public float cameraThetaZ;
        public float centerViewNumber;
        public float convergence;
        public float n;
        public float theta;
        public float s;
        public float d_over_n;
        public float p_over_du;
        public float p_over_dv;
        public Int32 colorInversion;
        public Int32 colorSlant;
        public Int32 cameraWidth;
        public Int32 cameraHeight;
        public Int32 cameraFps;
        public float cameraBinningFactor;
        public float facePredictAlphaX;
        public float facePredictAlphaY;
        public float facePredictAlphaZ;
        public float facePredictBeta;
        public float facePredictLatencyMs;
        public float accelerationThreshold;
        public Int32 faceTrackingSingleFaceEnable;
        public float faceTrackingSingleFaceTooFarDistanceThreshold;
        public Int32 faceTrackingSingleFaceTooFarResetTimeoutMs;
        public Int32 faceTrackingMaxNumOfFaces;
        public float faceTrackingHeadPoseZLowPassAlpha;
        public Int32 overlay;
        public float phc;
        public float p1;
        public float p2;
        public float sl1;
        public float sl2;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] subpixCenterX;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] subpixCenterY;
    }
    public class SDK : IDisposable
    {
        public SDK(LogLevel logLevel)
        {
            _sdk = leiaSdkCreate(logLevel);
            if (_sdk == IntPtr.Zero)
            {
                throw new Exception("Failed to initialize CNSDK");
            }
        }
        public void Dispose()
        {
            if (_sdk != IntPtr.Zero)
            {
                leiaSdkShutdown(_sdk);
                _sdk = IntPtr.Zero;
            }
        }
        public Int32 GetConfig(out Config config)
        {
            return leiaSdkGetConfig(_sdk, out config);
        }
        public Int32 SetConfig(in Config config)
        {
            return leiaSdkSetConfig(_sdk, in config);
        }
        public Status SetBacklight(bool enable)
        {
            return (Status)leiaSdkSetBacklight(_sdk, enable ? 1 : 0);
        }
        public Status GetBacklight(out bool isEnabled)
        {
            Int32 isEnabledInt = 0;
            Status status = (Status)leiaSdkGetBacklight(_sdk, out isEnabledInt);
            isEnabled = isEnabledInt != 0;
            return status;
        }
        public enum Status
        {
            Success = 0,
            ErrorInvalidInstance,
            ErrorUnknown,
        }
        public Status SetFaceTrackingConfig(FaceDetectorConfig config)
        {
            return (Status)leiaSdkSetFaceTrackingConfig(_sdk, config);
        }
        public Status EnableFacetracking(bool enable)
        {
            return (Status)leiaSdkEnableFacetracking(_sdk, enable ? 1 : 0);
        }
        public Status StartFacetracking(bool start)
        {
            return (Status)leiaSdkStartFacetracking(_sdk, start ? 1 : 0);
        }
        public Status SetProfiling(bool enable)
        {
            return (Status)leiaSdkSetProfiling(_sdk, enable ? 1 : 0);
        }
        public Status GetFaceTrackingProfiling(out LeiaHeadTracking.FrameProfiling frameProfiling)
        {
            return (Status)leiaSdkGetFaceTrackingProfiling(_sdk, out frameProfiling);
        }
        public Status GetPrimaryFace(out Vector3 position)
        {
            return (Status)leiaSdkGetPrimaryFace(_sdk, out position);
        }
        public Status GetNonPredictedPrimaryFace(out Vector3 position)
        {
            return (Status)leiaSdkGetNonPredictedPrimaryFace(_sdk, out position);
        }
        public Status Resume()
        {
            return (Status)leiaSdkResume(_sdk);
        }
        public Status Pause()
        {
            return (Status)leiaSdkPause(_sdk);
        }

        private IntPtr _sdk;

        [DllImport("leiaSDK")]
        private static extern IntPtr leiaSdkCreate(LogLevel logLevel);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkSetFaceTrackingConfig(IntPtr sdk, FaceDetectorConfig config);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkEnableFacetracking(IntPtr sdk, Int32 enable);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkStartFacetracking(IntPtr sdk, Int32 start);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkSetProfiling(IntPtr sdk, Int32 enable);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkGetFaceTrackingProfiling(IntPtr sdk, out LeiaHeadTracking.FrameProfiling frameProfiling);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkGetPrimaryFace(IntPtr sdk, out Vector3 position);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkGetNonPredictedPrimaryFace(IntPtr sdk, out Vector3 position);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkSetBacklight(IntPtr sdk, Int32 enable);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkGetBacklight(IntPtr sdk, out Int32 isEnabled);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkGetConfig(IntPtr sdk, out Config config);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkSetConfig(IntPtr sdk, in Config config);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkResume(IntPtr sdk);
        [DllImport("leiaSDK")]
        private static extern Int32 leiaSdkPause(IntPtr sdk);
        [DllImport("leiaSDK")]
        private static extern void leiaSdkShutdown(IntPtr sdk);
    }
}
#endif