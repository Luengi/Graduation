using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ImageTrackingPlotActivatedResponse : MonoBehaviour, IImageTrackingResponse
{
	[SerializeField] private PlotTrackedImageCollection[] _plotTrackedImageCollections;
	[SerializeField] private AnchorManager _anchorManager;

	public ImageTrackingResponses ResponseType => ImageTrackingResponses.ActivatePlot;
	public static event Action<Plot> OnPlotActivated;

	private void Start() 
	{
		_anchorManager.OnAnchorTracked += HandleAnchorActivated;
	}
	
	public GameObject Respond (GameObject portal, ARTrackedImage trackedImage)
	{
		StartCoroutine(CheckTracking(portal, trackedImage));

		return portal;
	}

	// We want to make sure that the tracking state is consistent before attaching the portal to the anchor
	private IEnumerator CheckTracking (GameObject portal, ARTrackedImage trackedImage) 
	{
		for (int i = 0; i < 30; i++) 
		{
			if (trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Limited) i = 0;
			yield return null;
		}

		_anchorManager.AttachToAnchor(portal, trackedImage);
	}

	private void HandleAnchorActivated (ImageAnchorCollection anchorCollection) 
	{
		Plot plotActivated = GetPlot(anchorCollection.Image.referenceImage.name);

		OnPlotActivated?.Invoke(plotActivated);
	}

	private Plot GetPlot (string trackedImageName)
	{
		foreach (var plotTrackedImageCollection in _plotTrackedImageCollections)
		{
			if (plotTrackedImageCollection.TrackedImageName != trackedImageName) continue;
			
			return plotTrackedImageCollection.Plot;
		}

		return Plot.None;
	}

	private void OnDestroy() 
	{
		_anchorManager.OnAnchorTracked -= HandleAnchorActivated;
	}
}

[Serializable]
public struct PlotTrackedImageCollection
{
	public string TrackedImageName;
	public Plot	Plot;
}
