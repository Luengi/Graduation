using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Linq;

[RequireComponent(typeof(ImageTrackingTrackRespondedObjects)), ]
public class ImageTrackingResponseManager : MonoBehaviour
{
   	[SerializeField] private ARTrackedImageManager _arTrackedImageManager;
	[SerializeField] private List<ImageObjectReference> _imageObjectReferences = new List<ImageObjectReference>();
	[SerializeField] private GameObject _imageTrackingResponsesContainer;

	private ImageTrackingTrackRespondedObjects _imageTrackingTrackRespondedObjects;
	internal List<IImageTrackingResponse> _imageTrackingResponses = new List<IImageTrackingResponse>();
    
	private void Awake() 
	{
		if (_arTrackedImageManager == null) Debug.LogWarning("ImageTrackingSpawnResponse: ARTrackedImageManager is not set");//throw new System.NullReferenceException("ImageTrackingSpawnResponse: ARTrackedImageManager is not set");
		
		if (_imageObjectReferences.Count < 1) Debug.LogWarning("ImageTrackingSpawnResponse: No ImageObjectReferences were added"); //throw new System.ArgumentException("ImageTrackingSpawnResponse: No ImageObjectReferences were added");
		
		if (_imageTrackingResponsesContainer == null) Debug.LogWarning("ImageTrackingSpawnResponse: An empty game object container that holds the IImageTrackingResponses needs to be set for this script to work as expected"); //throw new System.NullReferenceException("ImageTrackingSpawnResponse: An empty game object container to hold the IImageTrackingResponses needs to be set for this script to work as expected");
		else _imageTrackingResponses = _imageTrackingResponsesContainer.GetComponents<IImageTrackingResponse>().ToList();

		_imageTrackingTrackRespondedObjects = GetComponent<ImageTrackingTrackRespondedObjects>();
	}

	private void OnEnable() 
	{
		if (_arTrackedImageManager == null) return;

		_arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
	}

	private void OnDisable() 
	{
		if (_arTrackedImageManager == null) return;

		_arTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
	}

	private void OnTrackedImagesChanged (ARTrackedImagesChangedEventArgs eventArgs) 
	{
		HandleAddedImages(eventArgs.added);
		HandleUpdatedImages(eventArgs.updated);
	}

	private void HandleUpdatedImages (List<ARTrackedImage> updatedImages)
	{
		foreach (var trackedImage in updatedImages)
		{
			// Handle added tracked images
			ImageObjectReference imageObjectReference = _imageObjectReferences.FirstOrDefault(i => i.ImageName == trackedImage.referenceImage.name);
            Debug.Log(trackedImage.referenceImage.name); //Check which plot reference image is used. (Matthijs Added) (Space works)

            // If no ImageObjectReference is found, skip to the next tracked image
            if (!HasImageObjectReferences(imageObjectReference)) continue;
			
			GameObject trackedObject = _imageTrackingTrackRespondedObjects.GetTrackedObject(trackedImage.referenceImage.name);

			if (trackedObject == null) 
			{
				Debug.LogWarning("ImageTrackingSpawnResponse: No object to track was found for the tracked image");
				continue;
			}
			
			HandleTrackedImageUpdatedResponse(trackedImage, imageObjectReference);
		}
	}

	private void HandleTrackedImageUpdatedResponse (ARTrackedImage trackedImage, ImageObjectReference imageObjectReference)
	{
		foreach (var response in _imageTrackingResponses)
		{
			if (response.ResponseType == imageObjectReference.UpdatedResponse)
				response.Respond(_imageTrackingTrackRespondedObjects.GetTrackedObject(imageObjectReference.ImageName), trackedImage);
		}
	}

	private void HandleAddedImages (List<ARTrackedImage> addedImages) 
	{
		foreach (var trackedImage in addedImages)
		{
			// Handle added tracked images
			ImageObjectReference imageObjectReference = _imageObjectReferences.FirstOrDefault(i => i.ImageName == trackedImage.referenceImage.name);
			// If no ImageObjectReference is found, skip to the next tracked image
			if (!HasImageObjectReferences(imageObjectReference)) continue;

			HandleTrackedImageAddedResponse(trackedImage, imageObjectReference);
		}
	}

	internal void HandleTrackedImageAddedResponse (ARTrackedImage trackedImage, ImageObjectReference imageObjectReference)
	{
		foreach (var response in _imageTrackingResponses)
		{
			if (response.ResponseType == imageObjectReference.AddedResponse) 
			{
				GameObject manipulatedObject = response.Respond(imageObjectReference.ObjectReference, trackedImage);
				_imageTrackingTrackRespondedObjects.TrackObject(imageObjectReference.ImageName, manipulatedObject);
			}
		}
	}

	private bool HasImageObjectReferences (ImageObjectReference imageObjectReference) 
	{
		if (imageObjectReference == null) 
		{
			Debug.LogWarning("ImageTrackingSpawnResponse: No ImageObjectReference found for the tracked image");
			return false;
		} 

		return true;
	}
}
