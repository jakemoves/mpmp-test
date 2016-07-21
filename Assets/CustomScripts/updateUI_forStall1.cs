using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Vuforia
{
	
	public class updateUI_forStall1 : MonoBehaviour,
								ITrackableEventHandler
	{

	private TrackableBehaviour mTrackableBehaviour;

		private bool targetHasBeenFound = false;
	private bool startTheTimer = false;
	private bool showCornerImage = true;
	string currentTargetTracked;
	public float videoLength = 25.0f;
	
	public Texture testImage;
	public Texture stall1;
	public Texture stall2;
	public Texture stall3;
	public Texture stall4;
	public Texture stall5;
	public Texture stall6;
	GUIContent content;
	
	void Start () {
			content = new GUIContent(stall1);

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
				
				startTheTimer = true;	
				showCornerImage = false;
		} else {
				showCornerImage = true;
		}
	}

	void OnGUI() {
			if (showCornerImage) {
				GUI.Box (new Rect ((Screen.width - 200), 0, 200, 200), content);
			
			}
		}


		void Update(){
			if (startTheTimer) {
				videoLength -= Time.deltaTime;
			}

			if (videoLength <= 0) {
				content = new GUIContent (stall2);
				showCornerImage = true;
				
			}

		}


	}

}



