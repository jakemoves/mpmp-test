/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System.Collections;
using System.Linq;

namespace monoflow
{
    /// <summary>
    /// Helper class for triggering a loading of a MPMP instance when Unity starts
    /// </summary>
    public class MPMP_LoadOnStart : MonoBehaviour
    {
        [Tooltip("If you don't specify any MPMP instance the script will add the MPMP instance that is on this GameObject")]
        public MPMP[] mpmpList;
    
        IEnumerator Start()
        {
            if(mpmpList.Length == 0)
            {               
                var mpmp = GetComponent<MPMP>();
                if (mpmp != null)
                {
                    mpmpList = new MPMP[] { mpmp };         
                }            
            }

            mpmpList.ToList().ForEach(m => { m.OnInit = (m2) => { m2.Load(); }; });

            yield break;
        }

      
    }
}
