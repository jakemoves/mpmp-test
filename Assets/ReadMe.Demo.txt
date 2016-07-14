This is a demo version of  MPMP (multi platform media player). (version  1.8.3)
It has the same capabilities as the official version from the Assetstore but draws a watermark on top of the videos.
(The VLC Watermark is a red image that flickers every 20 sec)
You can work with this version when you develop and can exchange the plugins folder with all the native components later.
(Experimental WebGL support comes only with the paid version)

For further information please read the manual.



If you have any questions, suggestions or found a bug you can reach me at info@monoflow.org

Thanks for your interest!

monoflow

MPMP © 2016 Stefan Schlupek


Version history:
============
Version 1.8.3 - 2016.07.04
• new MPMP.dll (Removed D3D11_CREATE_DEVICE_DEBUG flag)
• fixed issues with creating Windows builds on OSX
• fixed editor issues when target platform is Windows on OSX


Version 1.8.2 - 2016.06.29
• new MPMP_VLC.dll (Removed D3D11_CREATE_DEVICE_DEBUG flag flag that
fixes crash on Windows 7 with VLC backend when Windows 8.1 SDK is missing)


Version 1.8.1 - 2016.06.19
• Added WebGL support (experimental)
• Added gamma correction shaders


Version 1.8 - 2016.06.19
• OpenGL ES 3.0 support on mobile
• Added OnBuffering event
• Exception handling on Android improved when loading from path that could not be resolved
• Fixed problem playing videos when Time.deltaTime is null
• Added forceGamma option
• Documentation update


Version 1.7 - 2016.05.31
• VLC backend for Windows:
   • Windows 7 support
   • Better streaming support
   • Youtube player
   • Much more supported formats
• Added 'Prevent flicker' option
• Added Stop method + event (OnStop)
• Added STOP button to ugui elements
• Added SetAudioTrack(index) method. You can now use videos with multiple audio tracks and select one that should be used. Default is 0 for the first audio track
• Added HasAudioTrack(index) method to check if an audio track at index exists.
• Updated the MPMP_VR_Setup.cs script to work on VR devices without interfering Unitys camera handling
• Fixed issue with SeekToWithTolerance method missing normalized parameter
• Re-added seek property
• Fixed some errors of status properties not having the right values
• Documentation update


Version 1.6 - 2016.04.01
• API change: SeekTo(float time,bool normalized) and SeekTo(float timeInSeconds)
	Old seek code has to be changed from normalized values to seconds or you have to use the seekTo method with normalized = true!
• video pauses now when you pause the editor
• Fixed bug with IsPlaying,IsPaused & IsLoading are not updating their status
• Fixed bug when native texture size was not available at OnLoaded
• New seekTo example added to MPMP_APITest.cs
• SetVideoMaterial method update
• Fixed regression bug with OnApplicationPause
• Texture is now destroyed also on ATI cards (Windows) when loading to unify the loading behaviour and prevent rare editor crashes
• GL.IssuePluginEvent is called now on every frame (Android) to prevent update issues with streaming videos
• Added internal MediaPlayer.onVideoSizeChanged callback on Android. Triggers the TEXTURE_CHANGED event and updates the internal native size variables
• iOS/OSX native libs compiled with Xcode 7.3
• Documentation update


Version 1.5.1 - 2016.03.21
• fixed memory leak when Pixelbuffer error occurs(iOS/OSX)
• fixed memory leak with FBO in the demo version (iOS/OSX)


Version 1.5 - 2016.03.18
• On Windows with NVIDIA cards the videoMaterial now displays a texture while loading. 
(The texture that the material has attached when you are in editor mode)
• Improved error handling on Android when a video could not be loaded
• Changed the last direct native API calls from the uGUI to a cached version
• Removed some double update callings on OSX
• SetVideoMaterial method update
• Added a PreferenceItem for MPMP to change the scripting define symbol SEEK_TOLERANCE (seek mode on iOS/OSX)
• Fixed regression bug where events are not called (Android)
• Added a video synchronizer script
• Documentation update


Version 1.4 - 2016.03.12
• Added an option for adjusting the refresh interval of the native texture update 
• Linear color space on NVIDIA cards is now supported 
• Loading a new video with new dimensions don't cause a crash anymore on NVIDIA cards (Windows) 
• API calls from mediaPlayer uGUI don't call the plugin directly (crash on ATI cards fix) 
• Pause before load fix (Windows) 
• Critical Section now with higher spincount to improve stability (Windows) 
• SaveAndLoad method has now new action parameter for tracing the download progress 
• Changed the initialisation of MPMP to Awake. Also added a script execution order manager that forces the MPMP.cs to be executed earlier. 
• MPMP_DelayQuit.cs script for shutdown all MPMP instances before your application quits. Fixes a possible crash on quit (Windows) 
• New Events: 
	OnPixelbufferError (OSX/iOS) forces a reload for fixing some possible video refresh problem on OSX El Capitan.
	OnTextureChanged: called when the internal texture has changed the dimensions.(Windows,Android) 
• API addition: GetVideoMaterial, SetUpdateFrequency, GetUpdateFrequency 
• Documentation update


Version 1.3 - 2016.02.17
• video texture is now displayed correct in linear color space
• Texture filter mode(Point/Bilinear) option for Windows
• You can now specify the texture property name that should be used in the video texture.
So every custom shader should now work without tweaking the MPMP code by hand.(default is '_MainTex')
• Added a time ugui element for displaying the position and duration in seconds
• Added a path ugui element for managing the video path
• Added a 360° demo scene for watching stereoscopic panorama videos
• Fixed exception bug on OSX when Windows is the target platform and you run the scene in the editor
• Added a log warning and auto switch to x86_64 when publishing for OSX
• Documentation update


Version 1.2 - 2016.02.11
• Events are now called from the main thread
• SetPlaybackRate implemented
You can change the playback speed of the video.(negative values for reverse playback)<br>
On Android you need API level 23+ (Android 6+)
• OnInit event implemented
• Fixed issue with OnError events on Windows
• SeekToWithTolerance for iOS/OSX
This seek mode is more accurate but could cause some decoding delay
• Fixed issue with GetDuration on Android and iOS/OSX. The values are now available at OnLoaded in the right unit (seconds).
• ScaleFlipY.cs renamed to ScaleFlip.cs. The script has now the axis mode property for selecting which axis to flip
• Added a 360° demo scene for watching panorama videos
• Documentation update



Version 1.1 - 2016.02.04
• Events implemented : OnLoad, OnLoaded, OnPlay, OnPause, OnError, OnDestroy, OnPlaybackCompleted
• Fixed OnApplicationPaused issue. The native media player pauses now when this event is called
• API test is now in a seperate scene
• API test improved
• Fixed issue with debug dll reference on Windows x86_64
• Fixed issue with missing zip_file.jar on Android
• Fixed issue with seeking problem when using AwesomePlayer on Android
• Documentation update



Version 1.0 (2016.01.15)
Initial Release




