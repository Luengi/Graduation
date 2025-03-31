using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ImageTrackingSpawnResponse : MonoBehaviour, IImageTrackingResponse
{
	public ImageTrackingResponses ResponseType => ImageTrackingResponses.SpawnObject;

	public GameObject Respond (GameObject objectToSpawn, ARTrackedImage trackedImage)
	{
		GameObject instantitatedObject = Instantiate(objectToSpawn, trackedImage.transform.position, trackedImage.transform.rotation);
		
		objectToSpawn.transform.position = trackedImage.transform.position;

		return instantitatedObject;
	}
}
