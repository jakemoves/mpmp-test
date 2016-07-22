/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System.Collections;
using System.Linq;
using System;

namespace monoflow {
    public class MPMP_DelayQuit : MonoBehaviour {

       [Tooltip("Little delay (seconds) when closing the app or kill the process to prevent some cleanup problems. On some graphics cards this could be an issue.")]
        public float threadSleep = 1.0F;
        private bool allowQuitting = false;
        private bool isQuiting = false;

  

        void OnApplicationQuit()
        {
       
            //Debug.Log("OnApplicationQuit");
          
            if (!isQuiting)
            {
              
                isQuiting = true;
                MPMP[] mpmpInstances = FindObjectsOfType<MPMP>();

                mpmpInstances.ToList().ForEach(m => {                
                    //Destroy(m);
                    DestroyImmediate(m);
                });
            }
           
        
            if (!allowQuitting)
            {
                //cancel first quit attemp and wait a short time:
                //Debug.Log("OnApplicationQuit.CANCEL");
                Application.CancelQuit();
                //Keep the media framework some time for cleanup
                System.Threading.Thread.Sleep((int)Math.Floor(threadSleep *1000));

                allowQuitting = true;
                Application.Quit();
            }

       
        }



    }



}
