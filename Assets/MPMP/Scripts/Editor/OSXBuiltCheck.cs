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


namespace monoflow
{

    public class OSXBuiltCheck : MonoBehaviour
    {
        
#if UNITY_EDITOR && UNITY_STANDALONE_OSX
		[PostProcessSceneAttribute(2)]
		public static void OnPostprocessSceneOSX() {
			BuildTarget BT = EditorUserBuildSettings.activeBuildTarget;
			if(BT != BuildTarget.StandaloneOSXIntel64)
			{
				if(BT == BuildTarget.StandaloneOSXIntel || BT == BuildTarget.StandaloneOSXUniversal){
				Debug.LogWarning("<color='red'>MPMP</color>:MPMP runs only as x86_64 on OSX. Switching to BuildTarget.StandaloneOSXIntel64 explicit.<color='orange'><b>Please built again!</b></color>");
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneOSXIntel64);
				}
			}
		}
#endif
    }
}
