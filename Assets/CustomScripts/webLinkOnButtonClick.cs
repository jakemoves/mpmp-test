

using UnityEngine;
using System.Collections;

public class webLinkOnButtonClick : MonoBehaviour {

	public void linkClick(){
		Debug.Log ("fired");

		Application.OpenURL("http://google.com/");
	}

}