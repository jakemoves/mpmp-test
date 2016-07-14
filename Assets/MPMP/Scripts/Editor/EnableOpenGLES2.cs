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
using System.Linq;
using System.Collections.Generic;


namespace monoflow {

    public class EnableOpenGLES2 : MonoBehaviour
    {

#if (UNITY_IOS || UNITY_ANDROID) && UNITY_EDITOR
        [PostProcessSceneAttribute(1)]
        public static void OnPostprocessScene()
        {

            // Debug.Log("OnPostprocessScene");
            if (Application.isPlaying) { return; }

            BuildTarget BT = EditorUserBuildSettings.activeBuildTarget;
          

            if (BT != BuildTarget.iOS) return;

            List<GraphicsDeviceType> gdtl = PlayerSettings.GetGraphicsAPIs(BT).Where(api => api != GraphicsDeviceType.Metal).ToList();
			//gdtl.ForEach(x=> {Debug.Log(x);});

            if (PlayerSettings.GetUseDefaultGraphicsAPIs(BT))
            {           
                Debug.LogWarning("<color='red'>MPMP</color>: Auto Graphics API is selected in your publish settings! Removing Metal as build option.");                           
				PlayerSettings.SetUseDefaultGraphicsAPIs(BT,false);
			}
            else if (PlayerSettings.GetGraphicsAPIs(BT).Contains(GraphicsDeviceType.Metal))
            {
                Debug.LogWarning("<color='red'>MPMP</color>: Removing Metal as build option.");
            }
			
			//gdtl.ForEach(x=> {Debug.Log(x);});
            PlayerSettings.SetGraphicsAPIs(BT, gdtl.ToArray());

        }
#endif


        //Legacy
        public static void SwitchToOpenGLES2(BuildTarget BT)
        {
           // return;//DEBUG!!!!!
            PlayerSettings.SetUseDefaultGraphicsAPIs(BT, false);

            GraphicsDeviceType[] apis = { GraphicsDeviceType.OpenGLES2 };

            PlayerSettings.SetGraphicsAPIs(BT, apis);
        }


       
    }

}


