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

    [CustomEditor(typeof(MPMP_ugui_Element))]
    [CanEditMultipleObjects]
    public class MPMP_ugui_Element_Editor : Editor
    {


        private MPMP_ugui_Element _target;

        SerializedProperty timeTextProp;
    

        void OnEnable()
        {

            if (target != _target) _target = target as MPMP_ugui_Element;

            serializedObject.Update();
            if (_target.mode == MPMP_ugui_Element.Mode.TIME)
            {
                timeTextProp = serializedObject.FindProperty("timeText");
            }
          

            // vrModeProp = serializedObject.FindProperty("vrMode");

        }



        override public void OnInspectorGUI()
        {
           // EditorGUI.BeginChangeCheck();
             DrawDefaultInspector();

            serializedObject.Update();

            if (_target.mode == MPMP_ugui_Element.Mode.TEXTURE)
            {
               
            }

            if (_target.mode == MPMP_ugui_Element.Mode.TIME)
            {
                EditorGUILayout.PropertyField(timeTextProp, new GUIContent("Time text format", "Specify the format for your time text.\n{0} = minutes from position \n{1} = seconds from position \n{2} = minutes from duration \n{3} = seconds from duration"));
            }


            serializedObject.ApplyModifiedProperties();
        }



        }

}
