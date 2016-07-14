/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System;
using System.Collections;
using System.IO;

namespace monoflow
{

    /// <summary>
    /// Enumeration for the VR Setup
    /// </summary>
    public enum VRVideoMode { LEFT, RIGHT, TOP, BOTTOM }

    public enum FacebookVideoMode { MONO, STEREO_LEFT, STEREO_RIGHT }

    public partial class MPMP : MonoBehaviour
    {

        public const string MENUITEM_NEW_MPMP = "GameObject/Create Other/MPMP/MPMP";
        public const string MENUITEM_NEW_MPMP_VIDEO_SYNCHRONIZER = "GameObject/Create Other/MPMP/VideoSynchronizer";
        public const string MENUITEM_NEW_MPMP_VR_SETUP = "GameObject/Create Other/MPMP/VR_Setup";
        public const string MENUITEM_MPMP_COPY_VLC_DATA = "File/Copy VLC data into build %#cv";
        //public const string VLC_BACKEND_DEFINE = "VLC_BACKEND";
        public const string DEFAULT_TEXTURE_NAME = "_MainTex";

        public const string LOGO64_NAME = "mpmp-logo.64x64";

        public static Color WARNING_COLOR = new Color(1f, 0.5f, 0f);

       


        public IEnumerator LoadData(Uri loadUri, Action<byte[]> callBackAction, Action errorCallbackAction, Action<float> progressAction)
        {
           // Debug.Log("LoadData from :" + loadUri.AbsoluteUri);
            //Safety first
            string loadUrlEsc = loadUri.AbsoluteUri.Replace(" ", "%20");//is this necessary? 

            WWW loader = new WWW(loadUrlEsc);

            //yield return loader;

            while (!loader.isDone)
            {
                //m_CurrentValue = loader.progress * 100;
                if (progressAction != null) progressAction(loader.progress * 100);
                yield return null;
            }

            if (loader.error == null)
            {
                if (progressAction != null) progressAction(100);
                byte[] data = loader.bytes;
                if (callBackAction != null) callBackAction(data);

            }
            else
            {
                //Error
                if (errorCallbackAction != null) errorCallbackAction();
            }

            loader.Dispose();
            loader = null;
            Resources.UnloadUnusedAssets();
            yield break;
        }



        public IEnumerator SaveData(Uri saveUri, byte[] data, Action<bool> errorCallbackAction)
        {
           // Debug.Log("SaveData To :" + saveUri.AbsoluteUri);
            string writeDirectory = Path.GetDirectoryName(saveUri.LocalPath);
            if (!Directory.Exists(writeDirectory))
            {
                Directory.CreateDirectory(writeDirectory);
               // Debug.Log("Directory created at:" + writeDirectory);
            }

            try
            {
                System.IO.File.WriteAllBytes(saveUri.LocalPath, data);
               // Debug.Log("data Saved at:" + saveUri.LocalPath);
                File.SetAttributes(saveUri.LocalPath, FileAttributes.Normal);//Android File Permissions
            }
            catch (Exception e)
            {              
                //Debug.Log("Error Saving data at :" + saveUri.LocalPath);
                Debug.LogWarning(e.ToString());
                if (errorCallbackAction != null) errorCallbackAction(false);
                yield break;
            }

            if (errorCallbackAction != null) errorCallbackAction(true);
            yield break;
        }


        /// <summary>
        ///Use this method when you want to download a media file from a remote uri and store it local .
        /// <para/> Yield as long as you download and save the data
        /// </summary>
        /// <param name="loadUri"></param>
        /// <param name="saveUri"></param>
        /// <param name="callbackAction"></param>
        /// <returns></returns>
        // yield return StartCoroutine(DownloadAndSaveData(myUri,(myString) => { newLocalPath = myString; }));
        public IEnumerator DownloadAndSaveData(Uri loadUri, Uri saveUri, Action<bool> callbackAction, Action<float> progressAction)
        {
            // strURL = strURL.Trim();
            byte[] data = null;
            bool loaded = false;
            bool saved = false;
           //  Debug.Log("DownloadStreamingVideo : " + loadUri.AbsoluteUri);
            yield return StartCoroutine(LoadData(loadUri, (bytes) => { data = bytes; loaded = true; }, null, progressAction));

            if (loaded)
            {
                yield return StartCoroutine(SaveData(saveUri, data, (flag) => { saved = flag; }));

            }

            if (callbackAction != null) callbackAction(saved);

        }

        /// <summary>
        /// Copies a file from the Application.streamingAssetsPath to the Application.persistentDataPath
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IEnumerator CopyStreamingAssetData(string path)
        {
            path = path.Trim();

            string write_path = Application.persistentDataPath + "/" + path;

            if (System.IO.File.Exists(write_path) == false)
            {
                // Debug.Log("CopyStreamingAssetVideo : " + strURL);

                WWW www = new WWW(Application.streamingAssetsPath + "/" + path);

                yield return www;

                if (string.IsNullOrEmpty(www.error))
                {
                    Debug.Log(write_path);
                    System.IO.File.WriteAllBytes(write_path, www.bytes);
                }
                else
                {
                    Debug.Log(www.error);
                }

                www.Dispose();
                www = null;
            }

        }




        /// <summary>
        /// Mirrors the uv.y of a mesh (1- uv.y)
        /// </summary>
        /// <param name="meshf"></param>
        public  static void MirrorUVY(MeshFilter meshf){
			//Debug.Log("MirrorUV.1");
			if(meshf == null)return;
			var mesh = meshf.mesh;//
			if(mesh == null)return;
			//Debug.Log("MirrorUV.2");
			var uvs = mesh.uv;
			for(int i = 0;i< uvs.Length;i++){
				var v =  uvs[i]; 
				v = new Vector2(v.x,1f-v.y);
				uvs[i] = v;
			}
			
			mesh.uv = uvs;
		}

     
        public static void Set_VR_UV(MeshFilter meshf,VRVideoMode vr_mode)
        {
            if (meshf == null) return;
            var mesh = meshf.mesh;//
            if (mesh == null) return;

            var uvs = mesh.uv;

            switch (vr_mode)
            {
                case VRVideoMode.LEFT:

                    for (int i = 0; i < uvs.Length; i++)
                    {
                        var v = uvs[i];
                        v = new Vector2((v.x*0.5f), v.y);
                        uvs[i] = v;
                    }

                    break;
                case VRVideoMode.RIGHT:

                    for (int i = 0; i < uvs.Length; i++)
                    {
                        var v = uvs[i];
                        v = new Vector2((v.x * 0.5f)+0.5f, v.y);
                        uvs[i] = v;
                    }

                    break;

                case VRVideoMode.TOP:

                    for (int i = 0; i < uvs.Length; i++)
                    {
                        var v = uvs[i];
                        v = new Vector2(v.x, (v.y*0.5f));
                        uvs[i] = v;
                    }

                    break;
                case VRVideoMode.BOTTOM:

                    for (int i = 0; i < uvs.Length; i++)
                    {
                        var v = uvs[i];
                        v = new Vector2(v.x, (v.y*0.5f)+0.5f);
                        uvs[i] = v;
                    }

                    break;
            }

            mesh.uv = uvs;
        }



    }

    public class ScriptOrder : Attribute
    {
        public int order;

        public ScriptOrder(int order)
        {
            this.order = order;
        }
    }

}
