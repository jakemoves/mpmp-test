/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/

using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.IO;

namespace monoflow
{
    public partial class MPMP : MonoBehaviour
    {

        /// <summary>
        /// Convenience method that resolves a path they way MPMP can handle it without bothering the user in certain circumstances.
        /// </summary>
        /// <param name="path"></param>
        /// <returns> A path that MPMP can use</returns>
        public static string GetFilePath(string path)
        {
            StringBuilder newValidPath = new StringBuilder();
            // Debug.Log("GetFilePath: " + path);
         
           // path = Uri.EscapeDataString(path);

            try
            {
                Uri tempUri;
                tempUri = new Uri(path);

                //Valid sheme
                newValidPath.Append(tempUri.AbsoluteUri);
                //Debug.Log("Valid path");
            }
            catch (UriFormatException)
            {
               //  Debug.Log("UriFormatException");
                if (String.IsNullOrEmpty(path)) return null;

                if (path.Contains("://"))
                {
                    Debug.Log("Wrong protocol");//is this possible?
                    return null;
                }
                else
                {
                    // Debug.Log("No protocol");

#if UNITY_ANDROID && !UNITY_EDITOR
                    //on Android the path resolution is different
                    if (Application.dataPath.Contains(".obb"))
                    {
                        newValidPath.Append(Path.Combine(Application.streamingAssetsPath, path).Replace(@"\", "/"));                   
                    }
                    else
                    {
                        newValidPath.Append(path);
                    }
                    return newValidPath.ToString();

#else


#if !(UNITY_WEBGL && !UNITY_EDITOR)
                    newValidPath.Append(@"file://");
                    newValidPath.Append(Path.Combine(Application.streamingAssetsPath, path).Replace(@"\", "/"));
#else
  newValidPath.Append(Path.Combine("StreamingAssets/", path).Replace(@"\", "/"));

#endif

                   // newValidPath.Append(@"file://");
                   // newValidPath.Append(Path.Combine(Application.streamingAssetsPath, path).Replace(@"\", "/"));

#endif

                }
            }
            catch (ArgumentNullException)
            {
                Debug.Log("Null Path!");
                return null;
            }
         


          // Debug.Log("New video path:" + newValidPath.ToString());



            try
            {
                Uri uri = new Uri(newValidPath.ToString());
                //  Debug.Log("AbsolutePath: " + uri.AbsolutePath);
                //  Debug.Log("AbsoluteUri: " + uri.AbsoluteUri);
                //  Debug.Log("Scheme: " + uri.Scheme);

                return uri.AbsoluteUri;
            }
            catch (UriFormatException)
            {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
                Debug.Log("UriFormatException");
#else

                return newValidPath.ToString();

#endif
            }
            catch (ArgumentNullException)
            {
                Debug.Log("ArgumentNullException");
            }




            return null;


        }


    }//class
}//namespace
