using System.Collections.Generic;
using UnityEngine;

public class ImageTrackingTrackRespondedObjects: MonoBehaviour
{
    private Dictionary<string, GameObject> _trackedObjects = new Dictionary<string, GameObject>();

	public void TrackObject (string imageName, GameObject objectToTrack)
	{
		if (_trackedObjects.ContainsKey(imageName)) return;

		_trackedObjects.Add(imageName, objectToTrack);
	}

	public GameObject GetTrackedObject (string imageName) => _trackedObjects.ContainsKey(imageName) ? _trackedObjects[imageName] : null;
}
