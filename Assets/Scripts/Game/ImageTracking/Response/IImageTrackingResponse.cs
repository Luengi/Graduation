using UnityEngine;
using UnityEngine.XR.ARFoundation;

public interface IImageTrackingResponse 
{
	ImageTrackingResponses ResponseType {get;}
    public GameObject Respond (GameObject objectToManipulate, ARTrackedImage trackedImage);
}
