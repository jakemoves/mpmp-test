//http://forum.unity3d.com/threads/script-execution-order-manipulation.130805/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace monoflow
{
    [InitializeOnLoad]
    public class ScriptOrderManager 
    {

        static ScriptOrderManager()
        {
            //Debug.Log("Changing the script Execution order for MPMP");
            foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
            {
                if (monoScript.GetClass() != null)
                {
                    foreach (var a in Attribute.GetCustomAttributes(monoScript.GetClass(), typeof(ScriptOrder)))
                    {
                        var currentOrder = MonoImporter.GetExecutionOrder(monoScript);
                        var newOrder = ((ScriptOrder)a).order;
                        if (currentOrder != newOrder)
                            MonoImporter.SetExecutionOrder(monoScript, newOrder);
                    }//foreach
                }//if
            }//foreach



            //EditorApplication.ExecuteMenuItem("Edit/Graphics Emulation/No Emulation");
        }



    }

}
