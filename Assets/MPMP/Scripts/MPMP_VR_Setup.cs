/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System.Collections;
using System;

namespace monoflow {
     
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]

    public class MPMP_VR_Setup : MonoBehaviour {

        public VRVideoMode vrMode;
        private MeshFilter _meshf;

        public Camera vrCamera;
       

        // Use this for initialization
        void Start () {
            // MPMP
            _meshf = GetComponent<MeshFilter>();
            SetupSphereUV();

              UpdateCamera();
            }

        public void SetupSphereUV()
        {
            MPMP.Set_VR_UV(_meshf, vrMode);
        }

        public void UpdateCamera()
        {
            if (vrCamera == null) return;
            if (UnityEngine.VR.VRSettings.enabled) {
                //let unity do the camera setup
               // Debug.Log("VR.Enabled");
            } else
            {
               // Debug.Log("VR.Not.Enabled");
                switch (vrMode)
                {
                    case VRVideoMode.LEFT:
                        vrCamera.rect = new Rect(0, 0, 0.5f, 1f);
                        break;
                    case VRVideoMode.RIGHT:
                        vrCamera.rect = new Rect(0.5f, 0, 0.5f, 1f);
                        break;
                    case VRVideoMode.TOP:
                        vrCamera.rect = new Rect(0f, 0.5f, 1f, 0.5f);
                        break;
                    case VRVideoMode.BOTTOM:
                        vrCamera.rect = new Rect(0f, 0f, 1f, 0.5f);
                        break;
                }
            }
           

            vrCamera.cullingMask = 1 << gameObject.layer;

        }



     }
}
