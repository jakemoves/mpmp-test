/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/

using UnityEngine;
using System.Collections;


namespace monoflow
{
    public class MPMP_ugui_Host : MonoBehaviour
    {
        //ToDO: mpmpplayer als Property mit get set und propertychenged -> UpdatePlayer
        public MPMP mpmpPlayer;
        private MPMP_ugui_Element[] _elements;
        void Awake()
        {
            if (mpmpPlayer == null) return;
            _elements = GetComponentsInChildren<MPMP_ugui_Element>();
            UpdatePlayer();
        }

        /// <summary>
        /// Call this method when the mpmpPlayer instance has changed
        /// </summary>
        public void UpdatePlayer()
        {
           
            if (_elements != null)
            {
                foreach (var e in _elements)
                {
                    e.player = mpmpPlayer;
                }
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
