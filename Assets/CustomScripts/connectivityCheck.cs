using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class connectivityCheck : MonoBehaviour {

	//declare style as a new GUIstyle to be applied if there's no connection
	GUIStyle style = new GUIStyle();

	//connected? yes or no
	bool connected;
	//Buttons
	public Button webSiteLink;
	public Button webView;
	public Button beginAR;

	IEnumerator Start(){
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		//check if we hear anything from Google
		WWW www = new WWW ("http://google.com");
		yield return www;

		if (www.error != null) {
			Debug.Log ("not connected");
			connected = false;
			//styling for the text
			style.alignment = TextAnchor.MiddleCenter;
			//allow some html markup later
			style.richText = true;
			//remove Buttons
			webSiteLink.gameObject.SetActive(false);
			webView.gameObject.SetActive(false);
			beginAR.gameObject.SetActive(false);
		} else {
			Debug.Log ("connected");
			connected = true;

//			webSiteLink.gameObject.SetActive(true);
//			webView.gameObject.SetActive(true);
//			beginAR.gameObject.SetActive(true);
		}
	}//--end of Start code that runs once

	void OnGUI(){ //--all the GUI manipulation has to go in here
		if(connected == false){
			GUI.Box(new Rect(0, 0, Screen.width, Screen.height/2), "<size=25><color=white>I'm sorry, your device is not connected to a network</color></size>", style);
			GUI.Box(new Rect(0, 35, Screen.width, Screen.height/2), "<size=20><color=white>Please force quit this app, check your network settings, and try again</color></size>", style);
		}
	}

}
