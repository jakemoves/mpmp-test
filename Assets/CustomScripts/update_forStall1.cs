using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Vuforia
{
	
	public class update_forStall1 : MonoBehaviour,
								ITrackableEventHandler
	{

	private TrackableBehaviour mTrackableBehaviour;
	

	private bool startTheTimer = false;
	private bool showCornerImage = true;
	
	public float videoLength = 25.0f;
	
	public Texture testImage;
	public Texture stall1;
	public Texture stall2;
	
	public GameObject Luke;
	
	GUIContent content;
	//this has to be dragged into the inspector
	public monoflow.MPMP mpmpVideo;

	
	
	void Start () {
			
			//image to go in our corner box
			content = new GUIContent(stall1);

			//vuforia trackable stuff
			mTrackableBehaviour = GetComponent<TrackableBehaviour>();
				if (mTrackableBehaviour)
				{
					mTrackableBehaviour.RegisterTrackableEventHandler(this);
				}
		}





	public void OnTrackableStateChanged(
		TrackableBehaviour.Status previousStatus,
		TrackableBehaviour.Status newStatus)
		
	{

		if (newStatus == TrackableBehaviour.Status.DETECTED ||
			newStatus == TrackableBehaviour.Status.TRACKED)
		{
				//start mpmp when target is acquired
				mpmpVideo.Play ();
				mpmpVideo.looping = false;

				startTheTimer = true;	
				showCornerImage = false;
		} else {
				//show cornerImage when AR isn't playing
				showCornerImage = true;
		}
	}
		//all GUI updated are within the OnGUI()
	void OnGUI() {
			if (showCornerImage) {
				GUI.Box(new Rect ((Screen.width - 200), 0, 200, 200), content);
			
			}
		}


		void Update(){
			
			if (startTheTimer) {
				videoLength -= Time.deltaTime;
			}

			if (videoLength <= 0) {
				content = new GUIContent (stall2);
				showCornerImage = true;
				mpmpVideo.Stop ();

				Destroy(Luke);
			}

		}



	}

}



