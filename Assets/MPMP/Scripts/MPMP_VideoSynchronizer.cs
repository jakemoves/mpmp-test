/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace monoflow { 
    public class MPMP_VideoSynchronizer : MonoBehaviour {

        public  MPMP mpmpMaster;
        public  MPMP[] mpmpClients;           

        [Range(0f,1f)]
        [Tooltip("If you use a too small value the sync can stall the player!")]
        public double threshold = 0.005f;

        private double pos1;
        private double pos2;
        private  double _delay;      
        private List<MPMP> _mpmpClients;

        private bool frameLock;
        private float frameDuration;
        [SerializeField, HideInInspector]
        private float _updateFrequencyInverse = 1.0f / 10f;


        IEnumerator Start() {

            //on mobile devices here should be a delay otherwise there could be some initialization/loading problem  
            yield return new WaitForEndOfFrame();
            //yield return new WaitForSeconds(1f);

            if (mpmpMaster == null) yield break;

            _mpmpClients = mpmpClients.ToList();


            mpmpMaster.OnPlaybackCompleted += _OnPlaybackCompleted;

            mpmpMaster.Load();
            mpmpClients.ToList().ForEach(c => { if (c != null) c.Load(); });
           

            while (mpmpMaster.IsLoading() && _mpmpClients.Where(c => c!= null && c.IsLoading() ).Count() > 0  )
            {
                yield return new WaitForEndOfFrame();
            }
          
            mpmpMaster.Play();
            _mpmpClients.ForEach(c => { if (c != null) c.Play(); });

        }
	
        void _OnLoaded(monoflow.MPMP instance)
        {
            instance.Play();
        }
        void _OnPlaybackCompleted(MPMP instance)
        {
            mpmpMaster.SeekTo(0f);
           _mpmpClients.ForEach(c => { if (c != null) c.SeekTo(0f); });

            mpmpMaster.Play();        
           _mpmpClients.ForEach(c => { if (c != null) c.Play(); });
        }

 
        void LateUpdate () {
           // return;
            if (mpmpMaster == null) return;
            if (_mpmpClients == null) return;

            frameDuration += Time.deltaTime;
            frameLock = (frameDuration < _updateFrequencyInverse);

            if (frameLock) return;
            frameDuration = 0; 

              pos1 = mpmpMaster.GetCurrentPosition();

            bool stop = false;
            _mpmpClients.ForEach(c =>
            {
                if (stop)
                {                 
                    return;
                }
                    if (c != null)
                {

                    pos2 = c.GetCurrentPosition();
                    _delay = Math.Abs(pos1 - pos2);

                    if (_delay > threshold)
                    {
                        // Debug.Log(String.Format("Delay.Seek:{0}, {1}" , _delay,c.name));                    
                        c.SeekTo((float)pos1);
                        stop = true;
                    }
                };

            });//Foreach

          

           
        }


    }
}
