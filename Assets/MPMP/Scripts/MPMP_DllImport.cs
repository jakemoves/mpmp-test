/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/

#define MPMP_FULL
//#define MPMP_MOBILE
//#define MPMP_WINDOWS
using UnityEngine;
using System.Collections;

using System;
using System.Runtime.InteropServices;

namespace monoflow
{
    public partial class MPMP : MonoBehaviour
    {

        private const string NOIMPL = "Sorry! <color='yellow'>{0}</color> is not implemented in this version";

#if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
     private const string DLL_PATH = "__Internal";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_ANDROID || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

		#if ((UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX)|| UNITY_EDITOR_WIN) && VLC_BACKEND
        private const string DLL_PATH = @"MPMP_VLC";
#else
        private const string DLL_PATH = @"MPMP";
#endif
        //private const string DLL_PATH = @"MPMP";

#endif


#if MPMP_FULL || (MPMP_MOBILE && ((UNITY_IOS && !UNITY_EDITOR_WIN) || (UNITY_ANDROID && !UNITY_EDITOR))) || (MPMP_WINDOWS && (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN))

#if (UNITY_ANDROID && !UNITY_EDITOR)
        [DllImport(DLL_PATH)]
        private static extern void SetOBBPath(int id, [MarshalAs(UnmanagedType.LPStr)] string path);
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
          [DllImport(DLL_PATH)]
		private static extern void SetNativeTextureID(int id,int texId);
          [DllImport(DLL_PATH)]
		private static extern void UpdateNativeTexture(int _id, int texId);
#else

#endif
        [DllImport(DLL_PATH)]
		private static extern void _ext2_SetLogCallback(int id, nativeCallback cb);

        [DllImport(DLL_PATH)]
        private static extern void UnityPluginInit();

#if !(UNITY_WEBGL && !UNITY_EDITOR)
        [DllImport(DLL_PATH)]
        private static extern IntPtr GetRenderEventFunc();
#endif

        [DllImport(DLL_PATH)]
        private static extern int NewMPMP(bool isLinear);


        [DllImport(DLL_PATH)]
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IPHONE
         public static extern void SetCallbackFunction(int id, NativeCallbackDelegateAOT fp);
#else
        public static extern void SetCallbackFunction(int id, IntPtr fp);
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport(DLL_PATH)]
        public static extern void SetCallbackFunction(int id, NativeCallbackDelegateAOT fp);
       // public static extern void SetCallbackFunction(webGLCallback fp);
#endif


#if ((UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX) || UNITY_EDITOR_WIN)
        [DllImport(DLL_PATH)]
        public static extern void UpdateCallbacks(int id);
#endif
        ///*
#if ((UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX) || UNITY_EDITOR_WIN) && MPMP_DEBUG
        [DllImport(DLL_PATH)]
        private static extern void InitDebugConsole();

        [DllImport(DLL_PATH)]
        private static extern void CloseDebugConsole();   
        
        [DllImport(DLL_PATH)]
        private static extern bool IsMEPlayerInitialized();

        [DllImport(DLL_PATH)]
        public static extern void SetDebugFunction(IntPtr fp); 
#endif
        // */
#if !(UNITY_WEBGL && !UNITY_EDITOR)
        [DllImport(DLL_PATH, EntryPoint = "Load")]
        private static extern void Load(int id, [MarshalAs(UnmanagedType.LPStr)] string path);

#else
         [DllImport(DLL_PATH)]
         private static extern void Load(int id, string path);
#endif

        // [DllImport(DLL_PATH, EntryPoint = "Load")]
        // private static extern void MPMP_Load(int id, [MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport(DLL_PATH)]
        private static extern void Play(int id);

        [DllImport(DLL_PATH)]
        private static extern void Pause(int id);

        [DllImport(DLL_PATH)]
        private static extern void Stop(int id);
        
        [DllImport(DLL_PATH)]
        private static extern void SetAutoPlay(int id, bool status);

        [DllImport(DLL_PATH)]
        private static extern void SeekTo(int id, float t, bool normalized);
    
        

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IPHONE
		[DllImport(DLL_PATH)]
		private static extern void SeekToWithTolerance(int _id, float seek,float tolerance,bool normalized);
#endif

        [DllImport(DLL_PATH)]
        private static extern void SetVolume(int id, float t);

        [DllImport(DLL_PATH)]
        private static extern void SetBalance(int id, double fBal);

        [DllImport(DLL_PATH)]
        private static extern void SetLooping(int id, bool status);

        [DllImport(DLL_PATH)]
        private static extern void SetPlaybackRate(int id, float rate);

        [DllImport(DLL_PATH)]
        private static extern void SetAudioTrack(int id, int track);

        [DllImport(DLL_PATH)]
        private static extern bool HasAudioTrack(int id, int track);

        [DllImport(DLL_PATH)]
        private static extern void Destroy(int id);    

        [DllImport(DLL_PATH)]
        private static extern IntPtr GetNativeTexture(int id);

        [DllImport(DLL_PATH)]
        private static extern void GetNativeVideoSize(int id, ref Vector2 videoSize);
#if !(UNITY_WEBGL && !UNITY_EDITOR)

#else
         [DllImport(DLL_PATH)]
         private static extern int GetNativeVideoSizeW(int id);
         [DllImport(DLL_PATH)]
         private static extern int GetNativeVideoSizeH(int id);
#endif

#if !(UNITY_WEBGL && !UNITY_EDITOR)
        [DllImport(DLL_PATH)]
        private static extern double GetCurrentPosition(int id);

        [DllImport(DLL_PATH)]
        private static extern double GetDuration(int id);
#else
         [DllImport(DLL_PATH)]
        private static extern float GetCurrentPosition(int id);

        [DllImport(DLL_PATH)]
        private static extern float GetDuration(int id);
#endif

        [DllImport(DLL_PATH)]
        private static extern float GetCurrentVolume(int id);

        [DllImport(DLL_PATH)]
        private static extern float GetBufferLevel(int id);

       [DllImport(DLL_PATH)]
        private static extern bool IsPlaying(int id);

        [DllImport(DLL_PATH)]
        private static extern bool IsPaused(int id);

        [DllImport(DLL_PATH)]
        private static extern bool IsStopped(int id);

        [DllImport(DLL_PATH)]
        private static extern bool IsLoading(int id);

        [DllImport(DLL_PATH)]
        private static extern float GetPlaybackRate(int _id);


#else

        private static  void SetOBBPath(int id,string path){
            Debug.Log(String.Format(NOIMPL, "SetOBBPath"));
            return ;
        }

         private static int NewMpMediaPlayer( )
        {
            Debug.Log(String.Format(NOIMPL, "NewMpMediaPlayer"));
            return 0;
        }

          private static  IntPtr GetNativeTexture(int id)
        {
            Debug.Log(String.Format(NOIMPL, "GetNativeTexture"));
            return IntPtr.Zero;
        }

       private static void GetNativeVideoSize(int id, ref Vector2 videoSize)
        {
         Debug.Log(String.Format(NOIMPL, "GetNativeVideoSize"));
        }

        private static IntPtr GetRenderEventFunc()
        {
            //Debug.Log(String.Format(NOIMPL, "GetRenderEventFunc"));
            return IntPtr.Zero;
        }
        private static void Load(int id, [MarshalAs(UnmanagedType.LPStr)] string path)
        {
            Debug.Log(String.Format(NOIMPL, "Load"));
        }

        private static void Play(int id)
        {
            Debug.Log(String.Format(NOIMPL, "Play"));
        }

        private static  void Pause(int id)
        {
            Debug.Log(String.Format(NOIMPL, "Pause"));
        }

        private static void Destroy(int id)
        {
            Debug.Log(String.Format(NOIMPL, "Destroy"));
        }

        private static  void SetAutoPlay(int id, bool status)
        {
            Debug.Log(String.Format(NOIMPL, "SetAutoPlay"));
        }

        private static  void SeekTo(int id, float t)
         {
            Debug.Log(String.Format(NOIMPL, "SeekTo"));
        }

         private static  void SetVolume(int id, float t)
         {
            Debug.Log(String.Format(NOIMPL, "SetVolume"));
        }

         private static  void SetBalance(int id, double fBal)
         {
            Debug.Log(String.Format(NOIMPL, "SetBalance"));
        }

         private static  void SetLooping(int id, bool status)
        {
            Debug.Log(String.Format(NOIMPL, "SetLooping"));
        }


        private static  double GetCurrentPosition(int id)
         {
            Debug.Log(String.Format(NOIMPL, "GetCurrentPosition"));
            return 0;
        }

         private  static double GetDuration(int id)
         {
            Debug.Log(String.Format(NOIMPL, "GetDuration"));
            return 0;
        }

         private  static float GetCurrentVolume(int id)
         {
            Debug.Log(String.Format(NOIMPL, "GetCurrentVolume"));
            return 0;
        }

        
         private static bool IsPlaying(int id)
        {
            Debug.Log(String.Format(NOIMPL, "IsPlaying"));
            return false;
        }


        private static bool IsPaused(int id)
        {
            Debug.Log(String.Format(NOIMPL, "IsPaused"));
            return false;
        }


        private static bool IsLoading(int id)
        {
            Debug.Log(String.Format(NOIMPL, "IsLoading"));
            return false;
        }

#endif


        /*
       #elif MPMP_MOBILE

               //#if (UNITY_IOS && ! UNITY_EDITOR_WIN ) || (UNITY_ANDROID && !UNITY_EDITOR) 

       #elif MPMP_WINDOWS

               //#if (UNITY_STANDALONE_WIN ||  UNITY_EDITOR_WIN )  

       #endif
       */


    }



}
