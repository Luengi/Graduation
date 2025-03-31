using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ImageTrackingSyncWithImageResponse : MonoBehaviour, IImageTrackingResponse
{
	public ImageTrackingResponses ResponseType => ImageTrackingResponses.SyncObjectWithImage;

	public GameObject Respond (GameObject objectToSync, ARTrackedImage trackedImage)
	{
		// if (objectToSync.transform.parent != trackedImage.transform) objectToSync.transform.SetParent(trackedImage.transform);
		
		// objectToSync.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		objectToSync.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);
		// objectToSync.transform.rotation = trackedImage.transform.rotation;
		return objectToSync;
	}
}
