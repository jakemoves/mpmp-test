/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;

namespace monoflow
{
    /// <summary>
    /// Method that flip the localScale.y on all platforms that have a flipped video texture
    /// </summary>
    public class MPMPScaleFlip : MonoBehaviour {

        public enum Axis { X,Y,Z}
        public Axis axisMode;
        Transform _t;
	   
	    void Start () {

            DoFlip();

        }

        public void DoFlip()
        {

            _t = gameObject.transform;

#if !UNITY_ANDROID || UNITY_EDITOR

            switch (axisMode)
            {
                case Axis.X:
                    _t.localScale = new Vector3(_t.localScale.x * -1f, _t.localScale.y , _t.localScale.z);
                    break;
                case Axis.Y:
                    _t.localScale = new Vector3(_t.localScale.x, _t.localScale.y * -1f, _t.localScale.z);
                    break;
                case Axis.Z:
                    _t.localScale = new Vector3(_t.localScale.x, _t.localScale.y , _t.localScale.z * -1f);
                    break;
            }
            //  _t.localScale = new Vector3(_t.localScale.x, _t.localScale.y * -1f, _t.localScale.z);
#else
            //

              switch (axisMode)
            {
                case Axis.X:
                    _t.localScale = new Vector3(_t.localScale.x * -1f, _t.localScale.y , _t.localScale.z);
                    break;
            /*
                case Axis.Y:
                    _t.localScale = new Vector3(_t.localScale.x, _t.localScale.y * -1f, _t.localScale.z);
                    break;
            */
                case Axis.Z:
                    _t.localScale = new Vector3(_t.localScale.x, _t.localScale.y , _t.localScale.z * -1f);
                    break;
            }

#endif
        }


    }

}

