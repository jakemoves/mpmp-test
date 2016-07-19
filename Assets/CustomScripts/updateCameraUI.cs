using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
namespace Vuforia
{
	
	public class updateCameraUI : MonoBehaviour,
								ITrackableEventHandler
	{

	private TrackableBehaviour mTrackableBehaviour;
	
	private bool showCornerImage = false;
	public Texture cornerImage;
	GUIContent content;
	
	void Start () {
		content = new GUIContent(cornerImage);
	
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
				showCornerImage = false;
		}
		else
		{
				showCornerImage = true;
		}
	}

	void OnGUI() {
			if (showCornerImage) {
				GUI.Box (new Rect (0, 0, 100, 100), content);
			
			}
		}

	}

}

