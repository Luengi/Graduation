using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AnchorManager : MonoBehaviour
{
	private const string ANCHOR_GAMEOBJECT_NAME = "AnchorObject";

	[SerializeField] private ARAnchorManager _anchorManager;
	[SerializeField] private ImageTrackingResponseManager _arTrackedImageManager;

	private List<ImageAnchorCollection> _imageAnchorCollections = new List<ImageAnchorCollection>();

	public event Action<ImageAnchorCollection> OnAnchorTracked;
    
	// Start is called before the first frame update
    void Start()
	{
		if (_anchorManager == null) Debug.LogWarning("AnchorManager: ARAnchorManager is not set");
		if (_arTrackedImageManager == null) Debug.LogWarning("AnchorManager: ARTrackedImageManager is not set");
		
		SubscribeToEvents();
	}

	private void HandleAnchorsChanged (ARAnchorsChangedEventArgs args)
	{
		foreach (var anchor in args.added)
		{
			ImageAnchorCollection? imageAnchorCollection = GetImageAnchorCollection(anchor.gameObject);

			if (imageAnchorCollection == null) continue;

			OnAnchorTracked?.Invoke(imageAnchorCollection.Value);
		}
	}

	public void AttachToAnchor (GameObject objectToAnchor, ARTrackedImage trackedImage) 
	{
		GameObject anchorObject = CreateAnchorObject(trackedImage.transform.position, trackedImage.transform.rotation);

		objectToAnchor.transform.SetParent(anchorObject.transform);
		objectToAnchor.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		objectToAnchor.SetActive(true);

		_imageAnchorCollections.Add(new ImageAnchorCollection { Image = trackedImage, PlotObject = objectToAnchor, AnchorObject = anchorObject });
	}

	private GameObject CreateAnchorObject (Vector3 position, Quaternion rotation) 
	{
		GameObject anchorObject = new GameObject(ANCHOR_GAMEOBJECT_NAME);
		anchorObject.transform.SetPositionAndRotation(position, rotation);
		anchorObject.AddComponent<ARAnchor>();

		return anchorObject;
	}

	private ImageAnchorCollection? GetImageAnchorCollection (GameObject anchorGameObject) 
	{
		foreach (var imageAnchor in _imageAnchorCollections) 
		{
			if (imageAnchor.AnchorObject == anchorGameObject) return imageAnchor;
		}

		return null;
	}

	private void SubscribeToEvents()
	{
		_anchorManager.anchorsChanged += HandleAnchorsChanged;
	}

	private void UnsubscribeFromEvents()
	{
		_anchorManager.anchorsChanged -= HandleAnchorsChanged;
	}

	private void OnDestroy()
	{
		UnsubscribeFromEvents();
	}
}
