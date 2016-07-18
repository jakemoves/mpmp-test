using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class webViewOnButtonClick : MonoBehaviour {

	public void Click(){
		Debug.Log ("fired");

		SceneManager.LoadScene("webView");
	}

}


