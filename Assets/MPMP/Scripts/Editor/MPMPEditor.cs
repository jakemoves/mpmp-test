/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace monoflow
{

    [CustomEditor(typeof(MPMP))]
    [CanEditMultipleObjects]
    public class MPMPEditor : Editor
    {


        [MenuItem(MPMP.MENUITEM_NEW_MPMP)]
        static void _NewMPMP()
        {
            //Debug.Log("_NewMPMP");
            GameObject go = new GameObject("MPMP.instance");
            MPMP mpmp = go.AddComponent<MPMP>();
            mpmp.volume = 1.0f;
            mpmp.autoPlay = true;
            mpmp.looping = true;         
        }


        SerializedProperty filtermodeProp;

        SerializedProperty videoPathProp;
        SerializedProperty videoMaterialProp;
        SerializedProperty texturePropertyNameProp;

        SerializedProperty autoPlayProp;
        SerializedProperty loopingProp;
        SerializedProperty forceGammaProp;
        SerializedProperty volumeProp;
        SerializedProperty balanceProp;
        SerializedProperty foldoutPreviewProp;
        SerializedProperty updateFrequencyProp;
        SerializedProperty preventFlickerProp;

        SerializedProperty flipYProp;

        private MPMP _target;
        private Texture2D tex_logo;
        private Color _pauseColor;
        private Color _playColor;
        private Color _loadColor;
        private Color _stopColor;
        private Texture tex_preview;
        private bool _isOnWindows;

      

        void EditorApplicationUpdate()
        {
            // Debug.Log("EditorApplicationUpdate");
            EditorUtility.SetDirty(_target);
        }
        void OnDisable()
        {
           // Debug.Log("OnDisable");
            //EditorApplication.update -= EditorApplicationUpdate;
        }
        void OnEnable()
        {
           
            if (target != _target) _target = target as MPMP;

            serializedObject.Update();

            // EditorApplication.update -= EditorApplicationUpdate;
            // EditorApplication.update += EditorApplicationUpdate;
            filtermodeProp = serializedObject.FindProperty("_filtermode");

            videoPathProp = serializedObject.FindProperty("videoPath");         
            videoMaterialProp = serializedObject.FindProperty("_videoMaterial");
            texturePropertyNameProp = serializedObject.FindProperty("texturePropertyName");

            autoPlayProp = serializedObject.FindProperty("_autoPlay");
            loopingProp = serializedObject.FindProperty("_looping");
            forceGammaProp = serializedObject.FindProperty("forceGamma");
            volumeProp = serializedObject.FindProperty("_volume");
            balanceProp = serializedObject.FindProperty("_balance");

            foldoutPreviewProp = serializedObject.FindProperty("_foldoutPreview");
            updateFrequencyProp = serializedObject.FindProperty("_updateFrequency");

            preventFlickerProp = serializedObject.FindProperty("_preventFlicker");



            //flipYProp = serializedObject.FindProperty("_flipY");
            tex_logo = Resources.Load(MPMP.LOGO64_NAME, typeof(Texture2D)) as Texture2D;
            _pauseColor = new Color(1.0f, 0.5f, 0f);
            _playColor = Color.green;
            _loadColor = Color.red;
            _stopColor = Color.red;

            _isOnWindows = Application.platform.ToString().StartsWith("Windows");

        }

        override public void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            //DrawDefaultInspector();//debug

            serializedObject.Update();

          

            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();//1.

            GUILayout.BeginHorizontal();//2
            Rect rect = GUILayoutUtility.GetRect(tex_logo.width, tex_logo.height);
            GUI.DrawTexture(rect, tex_logo);
            //GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();//2

            GUILayout.Space(0);

            GUILayout.BeginVertical();//3

            //Toggles
            GUILayout.BeginHorizontal();
           // GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 60f;
            EditorGUILayout.PropertyField(autoPlayProp, new GUIContent("AutoPlay", "Video starts immediately after loading."));
            _target.autoPlay = autoPlayProp.boolValue;
            // GUILayout.FlexibleSpace();
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(loopingProp, new GUIContent("Looping", "Video loops endless."));
            _target.looping = loopingProp.boolValue;

            GUILayout.Space(10);
            EditorGUIUtility.labelWidth = 100f;
            EditorGUILayout.PropertyField(forceGammaProp, new GUIContent("Force Gamma", "NVIDIA has trouble on Windows 10 with linear colorspace. This option enforces that always a rgb instead of a srgb native texture is used. But you need a gamma correction shader for the right display of your videotexture."));
            //_target.forceGamma  = forceGammaProp.boolValue;

            

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            //Buttons
            GUILayout.BeginHorizontal();
           // GUILayout.FlexibleSpace();
            GUI.backgroundColor = _target.IsLoading() ? _loadColor : Color.white;
            if (GUILayout.Button("Load", GUILayout.Height(30f), GUILayout.Width(60f))) { _target.Load(); }

            GUI.backgroundColor = _target.IsPlaying() ? _playColor : Color.white;
            if (GUILayout.Button("Play", GUILayout.Height(30f), GUILayout.Width(60f))) { _target.Play(); }

            GUI.backgroundColor = _target.IsPaused() ? _pauseColor : Color.white;
            if (GUILayout.Button("Pause", GUILayout.Height(30f), GUILayout.Width(60f))) { _target.Pause(); }

            GUI.backgroundColor = _target.IsStopped() ? _stopColor : Color.white;
            if (GUILayout.Button("Stop", GUILayout.Height(30f), GUILayout.Width(60f))) { _target.Stop(); }

            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();//3

            GUILayout.EndHorizontal();//1.

            GUILayout.Space(5);

            //Sliders
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 50f;
            //can't work with serialized properties here, because we want to call into the native plugin
            EditorGUILayout.LabelField(new GUIContent("Seek", "Normalized values 0 -1 "), GUILayout.Width(46f));
            // _target.seek = EditorGUILayout.Slider(_target.seek, 0.0f, 1.0f, GUILayout.MinWidth(100f), GUILayout.MaxWidth(300f));
            _target.SeekTo( EditorGUILayout.Slider(_target.GetSeek(true), 0.0f, 1.0f, GUILayout.MinWidth(100f), GUILayout.MaxWidth(300f)),true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            //EditorGUIUtility.fieldWidth = 50f;
            EditorGUIUtility.labelWidth = 50f;
            EditorGUILayout.Slider(volumeProp, 0.0f, 1.0f, new GUIContent("Volume", "Normalized values 0 -1"), GUILayout.MinWidth(100f), GUILayout.MaxWidth(350f));
             _target.volume = volumeProp.floatValue;

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            //EditorGUIUtility.fieldWidth = 50f;
            EditorGUIUtility.labelWidth = 50f;
            EditorGUILayout.Slider(balanceProp, -1.0f, 1.0f, new GUIContent("Balance", "Adjust the stereo channel volume output. \n-1 = only left channel \n0 = both channels equal \n1 = only right channel"), GUILayout.MinWidth(100f), GUILayout.MaxWidth(350f));
            _target.balance = balanceProp.floatValue;

            GUILayout.EndHorizontal();          

           GUILayout.Space(10);

            EditorGUIUtility.labelWidth = 85f;
            EditorGUILayout.PropertyField(videoPathProp, new GUIContent("videoPath", "Path to a video file. Depending on the sheme the path is resolved in a different way:\n\nNo sheme: streamingAssets folder is the root.\nfile:// : Absolute path from a valid volume.\nhttp:// : the video is loaded from the url. Your video should be a stream or a progressiv streaming video file "));
           
            GUILayout.Space(10);
       
            GUI.enabled = (!Application.isPlaying);
         
            EditorGUILayout.PropertyField(videoMaterialProp, new GUIContent("videoMaterial", "The Material(shader with '_MainTex' property) that should receive the video texture."));


            GUILayout.Space(10);

            GUILayout.BeginVertical("box");

            EditorGUIUtility.labelWidth = 85f;
            bool isTexturePropertyNotDefault = texturePropertyNameProp.stringValue != MPMP.DEFAULT_TEXTURE_NAME;
            GUI.backgroundColor = isTexturePropertyNotDefault ? MPMP.WARNING_COLOR: Color.white;//GUI.contentColor
            EditorGUILayout.PropertyField(texturePropertyNameProp, new GUIContent("Texture Name", "Name of the texture property within the materials shader, default is _MainTex.\nChange this only if you use some custom shader."));
            GUI.backgroundColor = Color.white;
            if (String.IsNullOrEmpty(texturePropertyNameProp.stringValue))
            {
                string msg = String.Format("If you leave this property empty the default name '{0}' will be used!", MPMP.DEFAULT_TEXTURE_NAME);
                EditorGUILayout.HelpBox(msg, MessageType.Warning, true);
            }

           

            if (_isOnWindows) {
                GUILayout.Space(5);
                EditorGUILayout.PropertyField(filtermodeProp, new GUIContent("Filter mode", "Texture filterMode option is only supported under Windows. Use Bilinear for better quality."));
            }

            GUI.enabled = true;

            GUILayout.EndVertical();//box


            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_target);
            }
          
          
            GUILayout.Space(5);
            EditorGUIUtility.labelWidth = 150f;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Slider(updateFrequencyProp, 1.0f, 120.0f, new GUIContent("Update frequency", "Set the frequency how often per second the native plugin should update.\nKeep in mind that the real maximal frequency depends on the framerate of your app."), GUILayout.MinWidth(100f), GUILayout.MaxWidth(350f));

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                _target.SetUpdateFrequency(updateFrequencyProp.floatValue);
                EditorUtility.SetDirty(_target);
            }


            GUILayout.Space(5);
            EditorGUIUtility.labelWidth = 150f;

            EditorGUI.BeginChangeCheck();
            // EditorGUILayout.Slider(updateFrequencyProp, 1.0f, 120.0f, new GUIContent("Update frequency", "Set the frequency how often per second the native plugin should update.\nKeep in mind that the real maximal frequency depends on the framerate of your app."), GUILayout.MinWidth(100f), GUILayout.MaxWidth(350f));
            EditorGUILayout.PropertyField(preventFlickerProp, new GUIContent("Prevent flicker", "If this option is enabled a copy of the current video frame is made when start loading to prevent flicker. This operation could impact your performance and cause a short delay."));
            _target.preventFlicker = preventFlickerProp.boolValue;
           // serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                //_target.SetUpdateFrequency(updateFrequencyProp.floatValue);
                EditorUtility.SetDirty(_target);
            }


            


            GUILayout.EndVertical();





            EditorGUI.BeginChangeCheck();

            foldoutPreviewProp.boolValue = EditorGUILayout.Foldout(foldoutPreviewProp.boolValue, "Preview Video");
            if (foldoutPreviewProp.boolValue)
            {
                tex_preview = _target.GetVideoTexture();
                if (tex_preview != null)
                {
                    //Vector2 videoSize = _target.GetNativeVideoSize();
                    float w = EditorGUIUtility.currentViewWidth;
                    float wx = w / tex_preview.width;// videoSize.x;// tex_preview.width;
                    //Debug.Log(w);
                    //Debug.Log(tex_preview.width);
                                     
                    Rect rect2 = GUILayoutUtility.GetRect(w, Mathf.Floor(tex_preview.height * wx));                  
                    Rect rect3 = new Rect(rect2.x, rect2.y + rect2.height, rect2.width, -rect2.height);

                  //  Rect rect2 = GUILayoutUtility.GetRect(w, Mathf.Floor(videoSize.y * wx));
                  //  Rect rect3 = new Rect(rect2.x, rect2.y + rect2.height, rect2.width, -rect2.height);

                    GUI.DrawTexture(rect3, tex_preview);//,ScaleMode.ScaleToFit
                }

                EditorUtility.SetDirty(_target);//Force redraw

            }




                serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_target);
            }




        }



    }
}
