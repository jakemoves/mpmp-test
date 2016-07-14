/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace monoflow
{

    [CustomEditor(typeof(MPMP_VR_Setup))]
    [CanEditMultipleObjects]
    public class MPMP_VR_SetupEditor : Editor
    {

        SerializedProperty cameraProp;
        //SerializedProperty vrModeProp;

        private MPMP_VR_Setup _target;

        void OnEnable()
        {

            if (target != _target) _target = target as MPMP_VR_Setup;

            serializedObject.Update();

            cameraProp = serializedObject.FindProperty("vrCamera");
            //vrModeProp = serializedObject.FindProperty("vrMode");

        }



        override public void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
           // DrawDefaultInspector();

            serializedObject.Update();


            GUILayout.BeginVertical("box");

            _target.vrMode = (VRVideoMode)EditorGUILayout.EnumPopup(new GUIContent("VR Mode", "VR mode"), _target.vrMode);

            GUILayout.Space(10);           

            EditorGUILayout.PropertyField(cameraProp, new GUIContent("VR Camera", "The camera that should render only the mesh of this gameobject."));
           
            GUILayout.EndVertical();

            GUILayout.Space(10);

            if (_target.gameObject.layer <= 0)
            {
                string msg = "Please specify a layer for this gameobject that is unique. The layer will be set as culling mask to your VR camera. With a unique layer you ensure that the VR camera only renders this gameobject.";
                EditorGUILayout.HelpBox(msg, MessageType.Warning, true);
            }


            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_target);
            }

        }



        }
}
