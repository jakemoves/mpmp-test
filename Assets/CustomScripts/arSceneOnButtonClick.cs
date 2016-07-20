using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class arSceneOnButtonClick : MonoBehaviour {

	public void Click(){
		Debug.Log ("fired");

		SceneManager.LoadScene("arscene");
	}

}
