/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;


namespace monoflow {

    [ScriptOrder(-100)]
    [AddComponentMenu("MPMP")]
    public partial class MPMP : MonoBehaviour
    {
      
        private bool _loadLock;          
        private bool _hasHadPixelBufferError;

        private static Dictionary<int,MPMP> _instances;

        private List<Action<MPMP>> _currentActions = new List<Action<MPMP>>();

        /// <summary>
        /// If you have trouble with linear color space you can force MPMP to use always RGB textures on the native site (DX11)
        /// <para/>This could be the case in dual graphic cards setups.
        ///  <para/>Keep in mind that your video texture will be displayed in a wrong way using linear colorspace in Unity
        /// </summary>
        public bool forceGamma = false;

        /// <summary>
        /// The path to a file or stream
        /// Yo can use as sheme file:// or http[s]://
        /// </summary>
        public string videoPath;

        /// <summary>
        /// Name of the texture property within video materials shader, default is _MainTex for maximal shader support.
        /// <para>Only change this if you use custom shaders</para>
        /// </summary>
        public string texturePropertyName = DEFAULT_TEXTURE_NAME;

        [SerializeField, HideInInspector]
        private Material _videoMaterial = null;

        private bool _hasTextureProperty;

        private Texture2D _videoTexture = null;
        [SerializeField]//, HideInInspector]
        private Texture _videoDefaultTexture = null;
        [SerializeField]//, HideInInspector]
        private Texture _videoDefaultTextureInit = null;
        [SerializeField]
        private bool _preventFlicker = false;


        [SerializeField, HideInInspector]
        private FilterModeMPMP _filtermode = FilterModeMPMP.Bilinear;
        private int _anisoLevel = 15;
        public static bool _isOnWindows= false;
        public static bool _isOnWindows7 = false;
        public static bool _hasNativeAccess = true;

        public static bool isNVIDIA {
            get;
            private set;
        } 

        [SerializeField, HideInInspector]
        private bool _autoPlay;

        [SerializeField, HideInInspector]
        private bool _looping;

        [SerializeField, HideInInspector]
        private float _seek;      

       [SerializeField, HideInInspector]
        private float _balance;//-1 - 1 

        [SerializeField, HideInInspector]
        private float _volume;//0 - 1   

        [SerializeField, HideInInspector]
        private float _rate;

       [SerializeField, HideInInspector]
        private bool _redrawFlag;//only to force the CustomInspector to redraw

        [SerializeField, HideInInspector]
        private bool _foldoutPreview;//     

        private int _id;
        private IntPtr nativeTexturePtr;
        private IntPtr RenderEventFunc;

        private bool frameLock;
        private float frameDuration;
        [SerializeField, HideInInspector]
        private float _updateFrequency = 60f;
      
        private float _updateFrequencyInverse = 1.0f / 60f;//If you set the _updateFrequency by hand you have to adjust this value!

        private bool _applicationPause_pause;
        private bool _applicationPause_playing;

#pragma warning disable 414

        [SerializeField, HideInInspector]
        private bool _seeking;//only for Android 

        private Vector2 _currentNativeVideoSize = Vector2.zero;
        private Vector2 _oldNativeVideoSize = Vector2.zero;


        #region callback_Windows 
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NativeCallbackDebugDelegate(string str);

        private GCHandle _handleNativeCallbackDebug;
        private IntPtr _intptr_NativeCallbackDebug_delegate;


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NativeCallbackDelegate(int evt);
      
        private GCHandle _handleNativeCallback;
        private IntPtr _intptr_NativeCallback_delegate;

        #endregion

        #region callback_AOT

        /*!
         * @brief	attribute that allows static functions to have callbacks (from C) generated AOT
         */
         
        public class MonoPInvokeCallbackAttribute : System.Attribute
        {
//#pragma warning disable 414
            private Type type;
            public MonoPInvokeCallbackAttribute(Type t) { type = t; }
//#pragma warning restore 414
        }
        /*
        public delegate void webGLCallback(int id, int evt);

        [MonoPInvokeCallback(typeof(webGLCallback))]
        public  void WebGLCallbackFunc(int msgId)
        {
            Debug.Log("Unity:WebGLCallbackFunc:" + msgId);
            this.CallBackNative(msgId);
        }
        */
        public delegate void nativeCallback(string message);

        [MonoPInvokeCallback(typeof(nativeCallback))]
        public static void NativeCallbackFunc(string message){
			Debug.Log("Unity:NativeCallbackFunc:"+message);
		}

        public delegate void NativeCallbackDelegateAOT(int id,int evt);

        #endregion
#pragma warning restore 414

		public static void Init(){
            if (_instances == null)
            {
                _instances = new Dictionary<int, MPMP>();


                _isOnWindows = Application.platform.ToString().StartsWith("Windows");
                if (_isOnWindows)
                {
                    _isOnWindows7 = SystemInfo.operatingSystem.Split(' ')[1].StartsWith("7");
                    if (_isOnWindows7)
                    {
                       // Debug.LogWarning("MPMP runs only on Windows 8+ !");
#if !VLC_BACKEND
                        Debug.LogWarning("On Windows 7 MPMP can only run with the VLC backend!");
                        _hasNativeAccess = false;
                        return;
#else
                        _isOnWindows7 = false;
#endif                      
                    }
                }


              

                if (Application.isEditor)
                {
                     #if UNITY_EDITOR
                    //we need the vendor of our graphics card so turn off emulation
                    UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Graphics Emulation/No Emulation");
                    #endif
                }

                isNVIDIA = SystemInfo.graphicsDeviceVendor == "NVIDIA";

                if (isNVIDIA)
                {               
                   // Debug.LogWarning("");
                }

                //It's more safe to use the destroy while loading technique also on ATI so we override the flag. It's also more consistent on windows
                //(There where some editor crashes so i made this decision)               
                if (_isOnWindows) isNVIDIA = true;

                // 0 for no sync, 1 for panel refresh rate, 2 for 1/2 panel rate
                //QualitySettings.vSyncCount = 1;

            }   
                  
		}

#if UNITY_EDITOR
        void OnPlayModeChanged(PlayModeState currentMode, PlayModeState changedMode)
        {
           
           // Debug.Log(currentMode.ToString() + " => " + changedMode.ToString());
            if(currentMode == PlayModeState.Stopped && changedMode == PlayModeState.Playing){ }

            if (currentMode == PlayModeState.Playing && changedMode == PlayModeState.Paused)
            {
                //Pause in editor
                 _applicationPause_pause = IsPaused();
                 _applicationPause_playing = IsPlaying();
                Pause();
            }

            if (currentMode == PlayModeState.Paused && changedMode == PlayModeState.Playing)
            {
                //Resume in editor
             
                if (_applicationPause_pause) {
                   // Pause();
                }else if (_applicationPause_playing)
                {
                    Play();
                }
               
            }


        }
#endif

        static  MPMP()
        {
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && VLC_BACKEND
        
            //Debug.Log("STATIC");
            //If we want the VLC dlls in the Data folder we need to make some limbo so VLC can resolve internal references.
            //In the editor this isn't needed.          
            //http://stackoverflow.com/questions/10003028/dllnotfoundexception-in-unity3d-plugin-for-c-dll/33124250#33124250
            var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            var process = Process.GetCurrentProcess();
            string fullPath = process.MainModule.FileName;
           // Debug.Log("fullPath:" + fullPath);
            string exePath = Path.GetDirectoryName(fullPath);
            string exeName = Path.GetFileNameWithoutExtension(fullPath);
            string destPath = Path.Combine(exePath, exeName + "_Data/Plugins/");

            var dllPath = destPath;
            dllPath = dllPath.Replace("/", @"\");
            //Debug.Log("dllPath:" + dllPath);
            if (currentPath != null && !currentPath.Contains(dllPath))
            {
                Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator + dllPath, EnvironmentVariableTarget.Process);
            }
#endif
        }

        void Awake()
	    {

           
            // Debug.Log("::::::::::::::::::::::::::::::::::::::");

#if UNITY_EDITOR
            EditorPlayMode.PlayModeChanged += OnPlayModeChanged;
#endif

            Init();

			if(Application.platform.ToString().StartsWith("OSX")){
				OnPixelBufferError+=(mpmp) =>{Load();};
			}
            /*
            OnTextureChanged += (mpmp) => {
               // _UpdateTextureFromNative();
               // _UpdateNativeVideoSize();
              //  Vector2 size = mpmp.GetNativeVideoSize();             
              //  Debug.Log("OnTextureChanged:"+ size);
               
            };

            OnLoaded += (mpmp) =>
            {
                Debug.Log("OnLoaded");
            };
            */

            SetUpdateFrequency(_updateFrequency);
          


            if (_isOnWindows && _isOnWindows7) return;// yield break;

            // RenderEventFunc = GetRenderEventFunc();

#if !(UNITY_WEBGL && !UNITY_EDITOR)
            RenderEventFunc = GetRenderEventFunc();
#endif



#if ((UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX) || UNITY_EDITOR_WIN)

            //Setup the debug callback system for Windows---

#if MPMP_DEBUG
            //http://www.gamedev.net/page/resources/_/technical/game-programming/c-plugin-debug-log-with-unity-r3349

            NativeCallbackDebugDelegate callbackDebug_delegate = new NativeCallbackDebugDelegate(CallbackNativeDebug);
            _handleNativeCallbackDebug = GCHandle.Alloc(callbackDebug_delegate, GCHandleType.Pinned);
            // Convert callback_delegate into a function pointer that can be used in unmanaged code.
            _intptr_NativeCallbackDebug_delegate = Marshal.GetFunctionPointerForDelegate(callbackDebug_delegate);

            // Call the API passing along the function pointer.
            SetDebugFunction(_intptr_NativeCallbackDebug_delegate);
          
			
            try { 
       
                if (!IsMEPlayerInitialized())
                {
                    Debug.Log("InitDebugConsole");
                    InitDebugConsole();
                  
                }
          
            }
            catch (Exception e)
            {
                Debug.Log("EXCEPTION:" + e.Message);
            }
     		
#endif


            UnityPluginInit();//Set the Unity graphics device in the native plugin


#endif //Windows




#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass unity_jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
           AndroidJavaObject activity_jo = unity_jc.GetStatic<AndroidJavaObject>("currentActivity");


           AndroidJavaObject mpmp_jo = new AndroidJavaObject("org.monoflow.media.MPMP", activity_jo);
          //AndroidJavaClass mpmp_jc = new AndroidJavaClass("org.monoflow.media.MPMP");
       
            _id = mpmp_jo.Call<int>("GetId");
#else
            //Windows,Editor,iOS

            ColorSpace cs = QualitySettings.activeColorSpace;

            bool isLinear = (cs == ColorSpace.Linear);

            //There are some issues with dual graphic card setups and linear colorspace so we can force to use always RGB textures on the native side. But this will cause not accurate texture colors!
            if (forceGamma) isLinear = false;
             _id = NewMPMP(isLinear);//Create the native player 

#endif
            // Debug.Log("ID:" + _id);


            if (!_instances.ContainsKey(_id)){
				_instances.Add(_id,this);
			}



            //Setup the callback system-----------------------

            // Call the API passing along the function pointer.
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IPHONE
             SetCallbackFunction(_id,CallBackNativeAOT);
#else

            //-------
#if UNITY_WEBGL && !UNITY_EDITOR
           // SetCallbackFunction(WebGLCallbackFunc);
             SetCallbackFunction(_id,CallBackNativeAOT);
#else

            NativeCallbackDelegate _callback_delegate = new NativeCallbackDelegate(CallBackNative);
			_handleNativeCallback = GCHandle.Alloc(_callback_delegate, GCHandleType.Pinned);
			// Convert callback_delegate into a function pointer that can be used in unmanaged code.
			_intptr_NativeCallback_delegate = Marshal.GetFunctionPointerForDelegate(_callback_delegate);

            SetCallbackFunction(_id, _intptr_NativeCallback_delegate);
#endif
//---------

#endif


            //--------------------------------------

            SetAutoPlay(_id,autoPlay);
            SetLooping(_id, looping);
			SetVolume(_id, _volume);

#if (UNITY_ANDROID && !UNITY_EDITOR)
            if (Application.dataPath.Contains(".obb"))
            {  
                SetOBBPath(_id,Application.dataPath);
            }
            
             GL.IssuePluginEvent(RenderEventFunc, _id);//Trigger the OpenGL Initializing
#endif



#if (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IPHONE) && MPMP_DEBUG
            _ext2_SetLogCallback(_id,NativeCallbackFunc);
#endif

            _UpdateStatus();

            //Unity 5.3 bug with multiple instances of this script .Normal Update interval is used!
            //yield return StartCoroutine(_Update());
            /*
            yield return new WaitForEndOfFrame();//One frame delay to be sure that all stuff is initialized on the native site
            if (OnInit != null) OnInit(this);

            yield break;
            */
        }

        IEnumerator Start()
        {
            // Debug.Log("MPMP.Start");
            if (String.IsNullOrEmpty(texturePropertyName)) { texturePropertyName = DEFAULT_TEXTURE_NAME; }
            SetVideoMaterial(_videoMaterial,true);
           
            yield return new WaitForEndOfFrame();//One frame delay to be sure that all stuff is initialized on the native site
            if (OnInit != null) OnInit(this);

            yield break;
        }


        
        //callback Method is called from native plugin if we debug
        static void CallbackNativeDebug(string str)
        {        
            Debug.Log(String.Format("NATIVE::{0}", str));
        }
   
       
        [MonoPInvokeCallback(typeof(NativeCallbackDelegateAOT))]
        public static void CallBackNativeAOT(int id,int evt)
        {
           // Debug.Log(String.Format("CallBackNativeAOT id:{0}, evt:{1}", id,evt));         
			MPMP instance;
			if(_instances.TryGetValue(id,out instance)){
				instance.CallBackNative(evt);
			}
            
        }

        /// <summary>
        /// Callback method is called from the native plugin and triggers the events
        /// To be thread safe we store the events in a list. At the next Update() this list is executed.
        /// </summary>
        /// <param name="evt">Event id</param>
        public void CallBackNative(int evt)
        {
            // Debug.Log(String.Format("CallBackNative::{0}, {1}", (Events)evt, _id));
            if (this == null) return;
            //Debug.Log(String.Format("CallBackNative.B"));
            lock (_currentActions)
            {                     
            
                switch ((Events)evt)
                {
                    case Events.LOAD:
                        // _loadLock = true;
                        _currentActions.Add(OnLoad);
                        break;
                    case Events.LOADED:
                       // Debug.Log("LOADED");
                        _currentActions.Add((me) =>
                        {
                           // Debug.Log("OnLoaded");
                            _loadLock = false;
                            _UpdateDuration();                     
                            _UpdateTextureFromNative();                          
                            _UpdateIsLoading();
                            _UpdateStatus();
                        });
                        _currentActions.Add(OnLoaded);
                        break;
                    case Events.PAUSE:
                        _currentActions.Add((me) => { _UpdateStatus(); });
                        _currentActions.Add(OnPause);                                         
                        break;
                    case Events.PLAY:
                        _currentActions.Add((me) => { _UpdateStatus(); });
                        _currentActions.Add(OnPlay);
                        break;
                    case Events.STOP:
                         _currentActions.Add((me) => { _UpdateStatus(); });
                       // _UpdateStopped();
                        _currentActions.Add(OnStop);
                        break;
                    case Events.DESTROY:
                        _currentActions.Add(OnDestroyed);
                        break;
                    case Events.ERROR:
                        _currentActions.Add(OnError);
                        break;
                    case Events.PLAYBACKCOMPLETED:
                        _currentActions.Add((me) => { _UpdateStatus(); });
                        _currentActions.Add(OnPlaybackCompleted);
                        break;
                    case Events.AVF_PIXELBUFFER_ERROR:
                        _currentActions.Add(OnPixelBufferError);
                        _hasHadPixelBufferError = true;
                        break;
                    case Events.TEXTURE_CHANGED:
                       /*
                        _currentActions.Add((me) => {                           
                              //Vector2 size = me.GetNativeVideoSize();             
                              //Debug.Log("OnTextureChanged:"+ size);
                           // _UpdateTextureFromNative();
                         });
                         */
                         _currentActions.Add(OnTextureChanged);
                        break;
                    case Events.BUFFERING:
                        // Debug.Log("BUFFERING");
                        _currentActions.Add(OnBuffering);
                        break;
                    

                }//switch

            }//lock

        }
        
        void FixedUpdate()
        {
            // Debug.Log("FixedUpdate");
            // _UpdateInternal()
        }
         
        void Update()
        {
            // Debug.Log("Update");
            _UpdateInternal();
        }

        
        void LateUpdate()
        {
            //_UpdateInternal()         
            _hasHadPixelBufferError = false;
        }
    

        IEnumerator _Update()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                _UpdateInternal();
            }          
       
        }

        void _UpdateInternal()
        {
            if (_isOnWindows7) return;
            _Refresh();
            _redrawFlag = !_redrawFlag;
        }

        private void _Refresh()
        {
            //Debug.Log("_Refresh:"+ Time.deltaTime);
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            if (RenderEventFunc == IntPtr.Zero)  return ;
#endif
            if(Time.deltaTime > 0) { frameDuration += Time.deltaTime; } else { frameDuration += Time.unscaledDeltaTime; }
            //frameDuration += Time.deltaTime;
            frameLock = (frameDuration < _updateFrequencyInverse);
            //frameLock = false;//DEBUG!!!!

            if (!frameLock)
            {
               // Debug.Log("!frameLock");
#if (UNITY_ANDROID && !UNITY_EDITOR)
                  if (!_loadLock) GL.IssuePluginEvent(RenderEventFunc, _id);
#else
                // GL.IssuePluginEvent(RenderEventFunc, _id);


#if UNITY_WEBGL && !UNITY_EDITOR
          // UpdateNativeTexture(_id);
#else
                GL.IssuePluginEvent(RenderEventFunc, _id);
#endif




#endif

                frameDuration = 0;

                if (!_loadLock)
                {
                    if (_videoTexture == null)
                    {
                        _UpdateTextureFromNative();
                    }
                    else
                    {
                        //
#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
                        //on iOS devices we have to update the texture on every update as it comes from a texture pool
                        //on Android it is also more safe 
                        _UpdateTextureFromNative();
#endif
                    }

                    _UpdateCurrentPosition();
                    _UpdateIsPlaying();
                    _UpdateIsPaused();
                    _UpdateIsLoading();
                    _UpdateVolume();
                    _UpdatePlaybackRate();
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && VLC_BACKEND
                    _UpdateDuration();//for VLC
#endif
                }

              


#if ((UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX) || UNITY_EDITOR_WIN)
                //On Windows our callbacks must be called on the main thread otherwise the Editor will hang on quit!
                UpdateCallbacks(_id);          
#endif
            }

 

            lock (_currentActions)
            {
                for (int i = 0; i < _currentActions.Count; i++)
                {
                    if (_currentActions[i] != null)
                    {
                       // Debug.Log("Action.execute");
                        _currentActions[i](this);                    
                    }                    
                }
                _currentActions.Clear();
            }



        }

        private void _UpdateTextureFromNative()
        {
            // Debug.Log("_UpdateTextureFromNative");

            lock (this)
            {



#if UNITY_WEBGL && !UNITY_EDITOR

               // _UpdateStatus();   //hack todo onLoaded            
               //  _UpdateDuration(); //hack todo onLoaded

                   _UpdateNativeVideoSize();

                  if (_videoTexture == null)
                {
                 Debug.Log("NEW.Texture");
                 Debug.Log(String.Format("NativeVideoSize({0}:{1})", _currentNativeVideoSize.x, _currentNativeVideoSize.y));  
                     _videoTexture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
                    //_videoTexture = new Texture2D(0, 0, TextureFormat.RGBA32, false);
                   // _videoTexture.wrapMode = TextureWrapMode.Repeat;
                    _videoTexture.wrapMode = TextureWrapMode.Clamp;
                    _videoTexture.Apply();
                    SetNativeTextureID(_id, _videoTexture.GetNativeTexturePtr().ToInt32());
                 
                }
                else
                {
                    if (_oldNativeVideoSize.x != _currentNativeVideoSize.x || _oldNativeVideoSize.y != _currentNativeVideoSize.y)
                    {    
                        Debug.Log("CHANGE.Texture");
                         Debug.Log(String.Format("NativeVideoSize({0}:{1})", _currentNativeVideoSize.x, _currentNativeVideoSize.y));                 
                        _videoTexture.Resize((int)_currentNativeVideoSize.x, (int)_currentNativeVideoSize.y, TextureFormat.ARGB32, false); 
                         _videoTexture.Apply();                  
                    }
                }


           UpdateNativeTexture(_id, _videoTexture.GetNativeTexturePtr().ToInt32());//Rendering


                 if (_videoMaterial != null)
                {
                    _videoMaterial.SetTexture(texturePropertyName, _videoTexture);
                }
#else

                nativeTexturePtr = GetNativeTexture(_id);

                if (nativeTexturePtr == IntPtr.Zero)
                {
					//Debug.Log("_UpdateTextureFromNative.NULLTexture");
                    return;
                }






                //GetNativeVideoSize(_id, ref _currentNativeVideoSize);
                _UpdateNativeVideoSize();


                if (_videoTexture == null) {
                   // Debug.Log("UpdateTextureFromNative.NEW");
                   // Debug.Log(String.Format("NativeVideoSize({0}:{1})", _currentNativeVideoSize.x, _currentNativeVideoSize.y));
                    _videoTexture = Texture2D.CreateExternalTexture((int)_currentNativeVideoSize.x, (int)_currentNativeVideoSize.y, TextureFormat.RGBA32, true, true, nativeTexturePtr);
                    _videoTexture.filterMode = FilterMode.Bilinear;
                    _videoTexture.wrapMode = TextureWrapMode.Repeat;
                } else {         
                   // Debug.Log("UpdateTextureFromNative.RESIZE");
                   // Debug.Log(String.Format("NativeVideoSize({0}:{1})", _currentNativeVideoSize.x, _currentNativeVideoSize.y));
                   if(_oldNativeVideoSize.x != _currentNativeVideoSize.x || _oldNativeVideoSize.y != _currentNativeVideoSize.y)
                    {
                        //Debug.Log("UpdateTextureFromNative.RESIZE");
                        _videoTexture.Resize((int)_currentNativeVideoSize.x, (int)_currentNativeVideoSize.y, TextureFormat.RGBA32, true);
                        //_videoTexture.Apply();????
                        //Resources.UnloadUnusedAssets();
                        //GC.Collect;
                    }
                            
                   
                }

              

                _videoTexture.UpdateExternalTexture(nativeTexturePtr);

                if (_isOnWindows)
                {
                    _videoTexture.filterMode = (FilterMode)_filtermode;
                    _videoTexture.wrapMode = TextureWrapMode.Repeat;//.Clamp;
                    if (_filtermode != FilterModeMPMP.Point) _videoTexture.anisoLevel = _anisoLevel + 1;// 16;       
                }

                if (_videoMaterial != null)
                {
                    _videoMaterial.SetTexture(texturePropertyName, _videoTexture);

                    if (_isOnWindows)
                    {
                        if (_filtermode != FilterModeMPMP.Point) _videoMaterial.GetTexture(texturePropertyName).anisoLevel = _anisoLevel;//15
                    }

                }




#endif
                

            }//lock

            _oldNativeVideoSize = _currentNativeVideoSize;
        }

			

        public void OnDisable()
        {
            //Debug.Log("OnDisable");    
           
        }


        void OnApplicationPause(bool pauseStatus)
        {
           // Debug.Log("OnApplicationPause : " + pauseStatus);
          
           
            if (pauseStatus == true)
            {
                _applicationPause_pause = IsPaused();
                _applicationPause_playing = IsPlaying();
                Pause();             
            }
            else
            {
                if (_applicationPause_pause)
                {
                    Pause();                
                }
                else if (_applicationPause_playing)
                {
                    Play();                
                }

            }

        }


        public void OnDestroy()
        {
            //Debug.Log("OnDestroy");
            // destroyed = true;
            //Resources.UnloadUnusedAssets();
            //GC.Collect();
            //UnityPluginUnload();

            _handleNativeCallback.Free();
            _intptr_NativeCallback_delegate = IntPtr.Zero;
            if (_hasNativeAccess)
            {
                Destroy(_id);//Native cleanup 
            }
           // Destroy(_id);//Native cleanup


#if MPMP_DEBUG

#if ((UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX) || UNITY_EDITOR_WIN)
            CloseDebugConsole();
          
#endif

#endif


            _RestoreMaterialState(true);

            //#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN        
            if (nativeTexturePtr != IntPtr.Zero) nativeTexturePtr = IntPtr.Zero;
                if (_videoMaterial != null)
                {
                    if (_videoTexture != null)
                    {
                        // Destroy(_videoTexture);
                        DestroyImmediate(_videoTexture);
                        _videoTexture = null;
                    }
                }     

            _handleNativeCallbackDebug.Free();
            _intptr_NativeCallbackDebug_delegate = IntPtr.Zero;

            //#endif


            lock (_currentActions)
            {              
                _currentActions.Clear();
            }

            if (OnDestroyed != null) OnDestroyed(this);

            //event cleanup
            OnInit = null;
            OnLoad = null;
            OnLoaded = null;
            OnPause = null;
            OnPlay = null;
            OnDestroyed = null;
            OnError = null;          
            OnPlaybackCompleted = null;
            OnPixelBufferError = null;
            OnTextureChanged = null;

           // Debug.Log("OnDestroy.END");
        }


    }

}
