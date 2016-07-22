/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/

using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace monoflow {
    public class MPMP_APITest : MonoBehaviour {

        public MPMP mediaPlayer;

        public Material newVideoMaterial;
        public Transform videoTransform;


        public string loadPathLocal = "trailer_1080p_1.mp4";
        public string loadPath = "http://download.blender.org/peach/trailer/trailer_iphone.m4v";// Application.streamingAssetsPath +"/"+ strURL
        private string savePath;// = Application.persistentDataPath + "/Data"; C:/MPMP_CACHE


        IEnumerator Start() {
            Debug.Log("MPMP_APITest.START");
            if(mediaPlayer == null)
            {
                Debug.Log("MPMP_APITest could not run. You have to specify the mediaPlayer!");
                yield break;
            }
            
            mediaPlayer.OnLoad += _OnLoad;
            mediaPlayer.OnLoaded += _OnLoaded;
            mediaPlayer.OnPause += _OnPause;
            mediaPlayer.OnPlay += _OnPlay;
            mediaPlayer.OnDestroyed += _OnDestroyed;
            mediaPlayer.OnPlaybackCompleted += _OnPlaybackCompleted;
            mediaPlayer.OnError += _OnError;
            mediaPlayer.OnBuffering += _OnBuffering;



            mediaPlayer.autoPlay = false;
            mediaPlayer.looping = false;
            mediaPlayer.volume = 0.1f;
            //On mobile platforms a little delay prevents loading issues!
            yield return new WaitForSeconds(2f);

            mediaPlayer.Load(loadPathLocal);

            while (mediaPlayer.IsLoading())
            {
                yield return new WaitForSeconds(0.5f);
            }

            Debug.Log("MPMP_APITest.Loading.READY");
           
            mediaPlayer.Play();               

            yield return new WaitForSeconds(5f);

            mediaPlayer.Pause();

            yield return new WaitForSeconds(2f);

            mediaPlayer.SeekTo(0f);

            yield return new WaitForSeconds(1f);

            mediaPlayer.SeekTo(10f);//seeking to sec

            yield return new WaitForSeconds(1f);

            mediaPlayer.SeekTo(0.5f,true);//normalized seeking

            yield return new WaitForSeconds(1f);

            mediaPlayer.Play();

           // yield break;

           yield return new WaitForSeconds(1f);

            if (String.IsNullOrEmpty(savePath)){
                savePath = Application.persistentDataPath + "/Data";
            }

            Debug.Log("savePath:" + savePath);

            Uri loadUri = new Uri(loadPath);
            Uri saveUri = new Uri(Path.Combine(savePath, Path.GetFileName(loadUri.AbsoluteUri)).Replace(@"\", "/"));
            bool saved = true;

            Debug.Log("saveUri: " + saveUri.AbsolutePath);

            if (!System.IO.File.Exists(saveUri.AbsolutePath))
            {
                saved = false;
                yield return StartCoroutine(mediaPlayer.DownloadAndSaveData(loadUri, saveUri, (flag)=> { saved = flag; },(progressAmount)=> { Debug.Log(String.Format("{0} percent downloaded", progressAmount)); }));
            }

            if (!saved)
            {
                Debug.LogWarning("Error loading or saving data");
                yield break;
            }

            Debug.Log("new video is available");

           // yield break;

            if(videoTransform.GetComponent<MeshRenderer>()!= null)
            {
                videoTransform.GetComponent<MeshRenderer>().material = newVideoMaterial;
            }


            mediaPlayer.SetVideoMaterial(newVideoMaterial);
          
            mediaPlayer.autoPlay= true;
            mediaPlayer.looping = false;         
           
            mediaPlayer.Load(saveUri.AbsoluteUri);                      


            Debug.Log("videoPath:" + mediaPlayer.videoPath);
            yield return new WaitForSeconds(5);
            if (Application.platform.ToString().StartsWith("Windows"))
            {
                mediaPlayer.filtermode = MPMP.FilterModeMPMP.Point;
                yield return new WaitForSeconds(5);
                mediaPlayer.filtermode = MPMP.FilterModeMPMP.Bilinear;
            }
            //Destroy(mediaPlayer.gameObject);
            yield break;
        }

        private void _OnLoad(MPMP mpmp)
        {
            Debug.Log("API Test::OnLoad:"+mpmp.name);
        }

        private void _OnLoaded(MPMP mpmp)
        {
            Debug.Log("API Test::OnLoaded:" + mpmp.name);
            Vector2 size = mpmp.GetNativeVideoSize();
            double duration = mpmp.GetDuration();
            Debug.Log(String.Format("NativeVideoSize: w:{0}, h:{1}", size.x, size.y));
            Debug.Log(String.Format("Duration: {0} in seconds", duration));
            if (videoTransform != null)
            {              
                videoTransform.localScale = new Vector3(size.x, size.y, 1f) * 0.002f;
                videoTransform.GetComponent<MPMPScaleFlip>().DoFlip();
            }
        }

        private void _OnPause(MPMP mpmp)
        {
            Debug.Log("API Test::OnPause:" + mpmp.name);
            if (videoTransform != null)
            {
                Vector2 size = mpmp.GetNativeVideoSize();
                videoTransform.localScale = new Vector3(size.x, size.y , 1f) * 0.002f;
                videoTransform.GetComponent<MPMPScaleFlip>().DoFlip();
            }
        }
        

        private void _OnPlay(MPMP mpmp)
        {
            Debug.Log("API Test::OnPlay:" + mpmp.name);
            if(videoTransform != null)
            {
                Vector2 size = mpmp.GetNativeVideoSize();
                videoTransform.localScale = new Vector3(size.x, size.y , 1f) * 0.0021f;
                videoTransform.GetComponent<MPMPScaleFlip>().DoFlip();
            }
        }

        private void _OnDestroyed(MPMP mpmp)
        {
            Debug.Log("API Test::OnDestroyed:" + mpmp.name);
        }

        private void _OnPlaybackCompleted(MPMP mpmp)
        {
            Debug.Log("API Test::_OnPlaybackCompleted:" + mpmp.name);
            if (videoTransform != null)
            {
                Vector2 size = mpmp.GetNativeVideoSize();
                videoTransform.localScale = new Vector3(size.x, size.y , 1f) * 0.0011f;
                videoTransform.GetComponent<MPMPScaleFlip>().DoFlip();
            }
        }

        private void _OnError(MPMP mpmp)
        {
            Debug.LogError("API Test::_OnError:" + mpmp.name);
            newVideoMaterial.color = new Color(1f, 0f, 0f);
        }

        private void _OnBuffering(monoflow.MPMP mpmp)
        {
            //Debug.Log("OnBuffering");
            Debug.Log("API Test::OnBuffering:" + mpmp.GetBufferLevel());

        }


    }
}
