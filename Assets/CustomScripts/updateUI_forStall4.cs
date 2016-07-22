using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Vuforia
{

	public class updateUI_forStall4 : MonoBehaviour,
	ITrackableEventHandler
	{

		private TrackableBehaviour mTrackableBehaviour;


		private bool showCornerImage = false;
		private bool previousScriptRemoved = false;
		private bool startTheTimer = false;

		//* - change
		public float videoLength = 25.0f;
		public Texture stall4;
		public Texture stall5;

		GUIContent content;

		private Texture newStallPic;
		private Texture currentStallPic;




		void Start () {
			//* - change
			currentStallPic = stall4;
			newStallPic = stall5;

			content = new GUIContent (currentStallPic);

			mTrackableBehaviour = GetComponent<TrackableBehaviour>();
			if (mTrackableBehaviour)
			{
				mTrackableBehaviour.RegisterTrackableEventHandler(this);
			}
		}

		void Update(){
			if (startTheTimer) {
				videoLength -= Time.deltaTime;
			}

			if (videoLength <= 0) {
				content = new GUIContent (newStallPic);
				showCornerImage = true;
			}
		}

		public void OnTrackableStateChanged(
			TrackableBehaviour.Status previousStatus,
			TrackableBehaviour.Status newStatus)

		{

			if (newStatus == TrackableBehaviour.Status.DETECTED ||
				newStatus == TrackableBehaviour.Status.TRACKED)
			{
				//removes previous script on the ImageTarget object
				//* - change
				GameObject.Find ("stall3").GetComponent<updateUI_forStall3>().enabled=false;
				previousScriptRemoved = true;
				startTheTimer = true;
				showCornerImage = false;

			}
			else
			{
				showCornerImage = true;
			}
		}

		void OnGUI() {
			if (showCornerImage && previousScriptRemoved) {
				GUI.Box(new Rect ((Screen.width - 200), 0, 200, 200), content);

			}
		}




	}

}
