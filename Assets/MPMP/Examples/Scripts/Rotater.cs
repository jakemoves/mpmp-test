using UnityEngine;
using System.Collections;

namespace monoflow
{
    public class Rotater : MonoBehaviour
    {

        public enum Axis { X, Y, Z }
        public Axis axisMode;

        public float speed;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            switch (axisMode)
            {
                case Axis.X:
                    transform.Rotate(Vector3.left, speed * Time.deltaTime);
                    break;
                case Axis.Y:
                    transform.Rotate(Vector3.up, speed * Time.deltaTime);
                    break;
                case Axis.Z:
                    transform.Rotate(Vector3.forward, speed * Time.deltaTime);
                    break;
            }
                   
        }
    }
}
