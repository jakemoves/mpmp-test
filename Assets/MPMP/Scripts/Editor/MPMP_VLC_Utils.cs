/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/

using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace monoflow
{
    public class MPMP_VLC_Utils : MonoBehaviour
    {



#if UNITY_EDITOR && UNITY_STANDALONE_WIN && VLC_BACKEND
        [PostProcessSceneAttribute(3)]
        public static void OnPostprocessSceneWIN_VLC() {

#if UNITY_5_2 || UNITY_5_1 || UNITY_5_0
             if (PlayerSettings.apiCompatibilityLevel != ApiCompatibilityLevel.NET_2_0)
            { 
             Debug.LogWarning("<color='red'>MPMP</color>:Changed ApiCompatibilityLevel to NET_2_0! Unity 5.2 makes some problem in the build. Better switch to Unity 5.3 or higher.");
            PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
            }
           
#endif

        }
#endif
        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            // Debug.Log("MPMP_VLC_Utils.OnPostprocessBuild");

            if (target != BuildTarget.StandaloneWindows && target != BuildTarget.StandaloneWindows64)
            {
                // Debug.LogWarning("<color='red'>MPMP</color>:VLC data is only copied when Buildtarget is Windows.");
                return;
            }


            // Debug.Log(pathToBuiltProject);


            string vlcPluginsSourcePath = "";
            string vlcLuaSourcePath = "";
            string dllSourcePath = "";


            string exePath = Path.GetDirectoryName(pathToBuiltProject);
            //Debug.Log("exePath:" + exePath);
            string exeName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
            //Debug.Log("exeName:" + exeName);

            string destPath = Path.Combine(exePath, exeName + "_Data/Plugins");
            string vlcPluginsDestPath = Path.Combine(exePath, exeName + "_Data/Plugins/plugins");
            string vlcLuaDestPath = Path.Combine(exePath, exeName + "_Data/Plugins/lua");

			#if !UNITY_EDITOR_OSX
            destPath = destPath.Replace("/", @"\");
            vlcPluginsDestPath = vlcPluginsDestPath.Replace("/", @"\");
            vlcLuaDestPath = vlcLuaDestPath.Replace("/", @"\");
			#endif


            if (target == BuildTarget.StandaloneWindows)
            {
                vlcPluginsSourcePath = Path.Combine(Application.dataPath, "MPMP/Plugins/x86/plugins");
                vlcLuaSourcePath = Path.Combine(Application.dataPath, "MPMP/Plugins/x86/lua");
                dllSourcePath = Path.Combine(Application.dataPath, "MPMP/Plugins/x86");
            }
            else if (target == BuildTarget.StandaloneWindows64)
            {
                vlcPluginsSourcePath = Path.Combine(Application.dataPath, "MPMP/Plugins/x86_64/plugins");
                vlcLuaSourcePath = Path.Combine(Application.dataPath, "MPMP/Plugins/x86_64/lua");
                dllSourcePath = Path.Combine(Application.dataPath, "MPMP/Plugins/x86_64");
			}

			#if !UNITY_EDITOR_OSX
            vlcPluginsSourcePath = vlcPluginsSourcePath.Replace("/", @"\");
            vlcPluginsDestPath = vlcPluginsDestPath.Replace("/", @"\");
            vlcLuaSourcePath = vlcLuaSourcePath.Replace("/", @"\");
            dllSourcePath = dllSourcePath.Replace("/", @"\");
			#endif
            //Debug.Log("vlcPluginsSourcePath:" + vlcPluginsSourcePath);
            //Debug.Log("vlcLuaSourcePath:" + vlcLuaSourcePath);

            // Take a snapshot of the file system.
            System.IO.DirectoryInfo dirVLCsource = new System.IO.DirectoryInfo(vlcPluginsSourcePath);

            if (dirVLCsource.Exists)
            {
                // This method assumes that the application has discovery permissions
                // for all folders under the specified path.
                IEnumerable<System.IO.FileInfo> fileListVLCsource = dirVLCsource.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
                var queryMatchingFiles =
                   from file in fileListVLCsource
                   where file.Extension == ".dll"
                   select file.Name;

                List<string> vlcPluginsSourceList = queryMatchingFiles.ToList();

                //queryMatchingFiles.ToList().ForEach((s) => { Debug.Log(s); });
                cleanDirectory(destPath, vlcPluginsSourceList);
#if VLC_BACKEND
                CopyDirectory(vlcPluginsSourcePath, vlcPluginsDestPath);
#endif
            }

            //Debug.Log("destPath:" + destPath);
            //Debug.Log("vlcPluginsDestPath:" + vlcPluginsDestPath);


#if VLC_BACKEND
            //delete MPMP.dll
            List<string> dllList = new List<string>() { "MPMP.dll" };//Path.Combine(dllSourcePath,"MPMP.dll")
            cleanDirectory(destPath, dllList);

#else
            //delete MPMP_VLC.dll
             List<string> dllList = new List<string>() {"MPMP_VLC.dll" };//Path.Combine(dllSourcePath,"MPMP_VLC.dll")
            if (target == BuildTarget.StandaloneWindows)
            {
                dllList.AddRange(new List<string>() { "libvlc.dll", "libvlccore.dll", });
            }
            else if (target == BuildTarget.StandaloneWindows64)
            {
                 dllList.AddRange(new List<string>() { "libvlc.dll", "libvlccore.dll","libgcc_s_seh-1.dll" });
            }
            cleanDirectory(destPath, dllList);
           
#endif


            //Luac files
            System.IO.DirectoryInfo dirLua = new System.IO.DirectoryInfo(vlcLuaSourcePath);
            if (dirVLCsource.Exists)
            {
                // Debug.LogWarning("<color='red'>MPMP</color>:VLC lua data is missing. No directory at "+ vlcLuaSourcePath);
                //return;
                IEnumerable<System.IO.FileInfo> fileListLuac = dirLua.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
                var queryMatchingFilesLuac =
                   from file in fileListLuac
                   where file.Extension == ".luac"
                   select file.Name;

                List<string> vlcPluginsListLuac = queryMatchingFilesLuac.ToList();


                cleanDirectory(destPath, vlcPluginsListLuac);
#if VLC_BACKEND
                CopyDirectory(vlcLuaSourcePath, vlcLuaDestPath);
#endif
            }




            Debug.Log("<color='red'>MPMP</color>:VLC data was copied successfully into build folder.");

        }


#if MPMP_DEBUG
        [MenuItem(MPMP.MENUITEM_MPMP_COPY_VLC_DATA, false, 0)]
#endif
        static void CopyVLCData()
        {
            // Debug.Log("MPMP_VLC_Utils.CopyVLCData");                   
            OnPostprocessBuild(EditorUserBuildSettings.activeBuildTarget, EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget));
        }

#if MPMP_DEBUG
        [MenuItem("File/UpdatePluginImporterVLCData", false, 0)]
        static void UpdatePluginImporterVLCData()
        {
            Debug.Log("MPMP_VLC_Utils.SetPlatformVLCData");
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;


            PluginImporter[] PIL = PluginImporter.GetImporters(target);

            string checkStr = "";
            if (target == BuildTarget.StandaloneWindows)
            {
                checkStr = "/MPMP/Plugins/x86/plugins";
            }
            else if (target == BuildTarget.StandaloneWindows64)
            {
                checkStr = "/MPMP/Plugins/x86_64/plugins";
            }


            PIL.ToList().ForEach((pi) => {
                if (pi.assetPath.Contains(checkStr))
                {
                    // Debug.Log(pi.assetPath);
                    pi.SetCompatibleWithAnyPlatform(false);
                    pi.SetCompatibleWithEditor(true);

                    if (target == BuildTarget.StandaloneWindows)
                    {
                        pi.SetEditorData("CPU", "x86");
                        pi.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);

                    }
                    else if (target == BuildTarget.StandaloneWindows64)
                    {
                        pi.SetEditorData("CPU", "x86_64");
                        pi.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);

                    }

                    pi.SetEditorData("OS", "Windows");

                    pi.SaveAndReimport();
                }

            });


        }
#endif


        static void cleanDirectory(string dirPath, List<string> list)
        {
			if (!Directory.Exists(dirPath)){
				Debug.Log(String.Format("Directory {0} doesn't exists",dirPath));
				return;
			}
            //https://msdn.microsoft.com/en-us/library/cc148994(v=vs.100).aspx
            foreach (string file in Directory.GetFiles(dirPath))
            {
                string fileName = Path.GetFileName(file);
                if (list.Contains(fileName))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    try
                    {
                        fi.Delete();
                    }
                    catch (System.IO.IOException e)
                    {
                        // Console.WriteLine(e.Message);
                        Debug.Log(e.Message);
                    }
                }
            }
        }

        //http://stackoverflow.com/questions/1066674/how-do-i-copy-a-folder-and-all-subfolders-and-files-in-net
        private static void CopyDirectory(string sourcePath, string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                //Debug.Log(String.Format(" destPath:{0}", destPath));
                Directory.CreateDirectory(destPath);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                if (file.Contains(".meta")) continue;
                string dest = Path.Combine(destPath, Path.GetFileName(file));
                // Debug.Log(String.Format("file:{0}, dest:{1}", file, dest));
                try
                {
                    File.Copy(file, dest, true);
                }
                catch (System.IO.IOException e)
                {
                    // Console.WriteLine(e.Message);
                    Debug.Log(e.Message);
                }
            }

            foreach (string folder in Directory.GetDirectories(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(folder));
                CopyDirectory(folder, dest);
            }
        }

        private static void CopyFile(string sourcePath, string destPath)
        {
            try
            {
                File.Copy(sourcePath, destPath, true);
            }
            catch (System.IO.IOException e)
            {
                // Console.WriteLine(e.Message);
                Debug.Log(e.Message);
            }
        }
    }

    /*
    [InitializeOnLoad]
    public class VLCSetup{
        static VLCSetup()
        {
            Debug.Log("VLCSetup:" + Application.dataPath);
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && VLC_BACKEND
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

            var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);

            string checkStr = "";
            if (target == BuildTarget.StandaloneWindows)
            {
                checkStr = "MPMP/Plugins/x86/";
            }
            else if (target == BuildTarget.StandaloneWindows64)
            {
                checkStr = "MPMP/Plugins/x86_64/";
            }

            string destPath = Path.Combine(Application.dataPath, checkStr);
            var dllPath = destPath;
            dllPath = dllPath.Replace("/", @"\");
            Debug.Log("dllPath:" + dllPath);

            
            if (currentPath != null && !currentPath.Contains(dllPath))
            {
                Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator + dllPath, EnvironmentVariableTarget.Process);
            }
            
#endif
        }
        
    }
    */
    [InitializeOnLoad]
    public class VLCInit
    {
        static string vlcKey = "MPMP_vlcBackend_key";
        static VLCInit()
        {
            //Debug.Log("VLCInit");
           
            //EditorPrefs.DeleteKey(vlcKey); return;
            if (!EditorPrefs.HasKey(vlcKey))
            {
                EditorPrefs.SetBool(vlcKey, true);
                MPMP_Editor_Utils.MountScriptingDefineSymbolToAllTargets("VLC_BACKEND");
            }
            else
            {
                if(EditorPrefs.GetBool(vlcKey, true)) MPMP_Editor_Utils.MountScriptingDefineSymbolToAllTargets("VLC_BACKEND");
            }
           
         
           
            
        }
    }
}
