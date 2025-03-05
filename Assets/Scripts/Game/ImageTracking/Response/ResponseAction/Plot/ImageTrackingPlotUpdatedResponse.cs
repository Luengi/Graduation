using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingPlotUpdatedResponse : MonoBehaviour, IImageTrackingResponse
{
	[SerializeField] private PlotTrackedImageCollection[] _plotTrackedImageCollections;
	[SerializeField] private PlotGameObjectCollection[] _plotGameObjectCollections;
	[SerializeField] private DistanceTracker _distanceTracker;

	private Dictionary<Plot, bool> _plotActiveState = new Dictionary<Plot, bool>();
	private Plot _currentPlotActive = Plot.None;
	private List<Plot> _initialHandledPlots = new List<Plot>();

	public ImageTrackingResponses ResponseType => ImageTrackingResponses.UpdatePlot;
	public static event Action<Plot> OnPlotNeedsActivation;
	public static event Action<Plot> OnPlotNeedsDeactivation;

	private void Start() 
	{
		_distanceTracker.OnMaxDistanceReached += HandleMaxDistanceReached;
		ImageTrackingPlotActivatedResponse.OnPlotActivated += HandlePlotInitialActivation;
	}

	public GameObject Respond (GameObject objectToManipulate, ARTrackedImage trackedImage)
	{
		Plot currentPlotUpdating = GetPlot(trackedImage.referenceImage.name);

		if (!_initialHandledPlots.Contains(currentPlotUpdating)) return objectToManipulate;

		GameObject plotGameObject = GetGameObject(currentPlotUpdating);

		if (_distanceTracker.OverMaxDistance(plotGameObject.transform, Camera.main.transform)) return objectToManipulate;
		
		if (_currentPlotActive != currentPlotUpdating && _distanceTracker != null) 
		{
			_currentPlotActive = currentPlotUpdating;
			_distanceTracker.UpdateObjects(plotGameObject.transform, Camera.main.transform);
		}

		if (trackedImage.trackingState == TrackingState.Tracking) 
			HandleTracking();

		return objectToManipulate;
	}

	private void HandlePlotInitialActivation(Plot plotActivated) 
	{
		_initialHandledPlots.Add(plotActivated);
	}

	private void HandleMaxDistanceReached()
	{
		DeactivateCurrentPlot();

		_distanceTracker.UpdateObjects(null, null);
	}

	private void DeactivateCurrentPlot()
	{
		// GameObject plotGameObject = GetGameObject(_currentPlotActive);
		_plotActiveState[_currentPlotActive] = false;
		// plotGameObject.SetActive(false);
		OnPlotNeedsDeactivation?.Invoke(_currentPlotActive);
		_currentPlotActive = Plot.None;
	}

	private void HandleTracking()
	{
		if (_plotActiveState.ContainsKey(_currentPlotActive) && _plotActiveState[_currentPlotActive]) return;

		ActivatePlot();
	}

	private void ActivatePlot()
	{
		// if (!plotGameObject.activeInHierarchy) plotGameObject.SetActive(true);
		_plotActiveState[_currentPlotActive] = true;
		OnPlotNeedsActivation?.Invoke(_currentPlotActive);
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

	private GameObject GetGameObject (Plot plot)
	{
		foreach (var plotGameObjectCollection in _plotGameObjectCollections)
		{
			if (plotGameObjectCollection.Plot != plot) continue;
			
			return plotGameObjectCollection.GameObject;
		}

		return null;
	}

	private void OnDestroy() 
	{
		_distanceTracker.OnMaxDistanceReached -= HandleMaxDistanceReached;
		ImageTrackingPlotActivatedResponse.OnPlotActivated -= HandlePlotInitialActivation;
	}
}

[Serializable]
public struct PlotGameObjectCollection
{
	public Plot Plot;
	public GameObject GameObject;
} 
