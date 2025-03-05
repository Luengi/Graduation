using System;
using UnityEngine;

public class PlotsManager : Singleton<PlotsManager>
{
	const ushort AMOUNT_OF_PLOTS = 2;

	[SerializeField] internal Plot _currentPlot;
	[SerializeField] internal PlotCollection[] _plotObjects;

	private Plot _nextPlot;
	public Plot CurrentPlot => _currentPlot;

	public static event Action<Plot> OnPlotDeactivated;
	public static event Action<Plot> OnPlotActivated;

    internal protected override void Awake() 
	{
		base.Awake();
		if (_plotObjects.Length != AMOUNT_OF_PLOTS)
			Debug.LogWarning($"The amount of plot objects in the PlotsManager is not equal to the amount of plots in the game.\nThere should be {AMOUNT_OF_PLOTS} plot objects in the PlotsManager, but there are only {_plotObjects.Length} plot objects.");
	}

	private void Start()
	{
		BeeMovement.OnBeeEnteredPlot += HandleBeeEnteredPlot;
		ImageTrackingPlotActivatedResponse.OnPlotActivated += HandlePlotActivated;
		ImageTrackingPlotUpdatedResponse.OnPlotNeedsActivation += HandlePlotActivated;
		ImageTrackingPlotUpdatedResponse.OnPlotNeedsDeactivation += HandleMaxDistanceReached;
	}

	private void HandleMaxDistanceReached (Plot plot)
	{
		_currentPlot = Plot.None;
		_nextPlot = Plot.None;
		DeactivateCurrentPlotObjects(plot);
		OnPlotDeactivated?.Invoke(plot);
	}

	private void HandleBeeEnteredPlot()
	{
		_currentPlot = _nextPlot;
		_nextPlot = Plot.None;
	}

	private void HandlePlotActivated (Plot plot)
	{
		_nextPlot = plot;
		ActivateCurrentPlotObjects(plot);
		OnPlotActivated?.Invoke(plot);
	}

	private void DeactivateCurrentPlotObjects (Plot plotToDeactivate)
	{
		foreach (var plotObject in _plotObjects)
		{
			if (plotObject.Plot != plotToDeactivate) continue;

			foreach (var plotObj in plotObject.PlotObjects)
			{
				plotObj.SetActive(false);
			}
			break;
		}
	}

	private void ActivateCurrentPlotObjects (Plot plotToActivate)
    {
        foreach (var plotObject in _plotObjects)
        {
            if (plotObject.Plot != plotToActivate) continue;

			// if (plotObject.PlotObject == null) break;
            
			foreach (var plotObj in plotObject.PlotObjects)
			{
				plotObj.SetActive(true);
			}
			//
			// plotObject.PlotObject.SetActive(true);
            break;
        }
    }
	
	private void OnDestroy()
	{
		BeeMovement.OnBeeEnteredPlot -= HandleBeeEnteredPlot;
		ImageTrackingPlotActivatedResponse.OnPlotActivated -= HandlePlotActivated;
		ImageTrackingPlotUpdatedResponse.OnPlotNeedsActivation -= HandlePlotActivated;
	}
}
