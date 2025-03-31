using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PassiveEventManager : Singleton<PassiveEventManager>
{
	[SerializeField] private List<PlotEventsCollection> _plotEventsCollections;
	[SerializeField] private float _delayedEventStartDuration = 1f;

	internal PassiveEvent _currentEventPlaying;
	private PassiveEvent _previousEventPlayed;
	private Coroutine _findPassiveEvent;
	private List<PlotEvent> _playedEvents = new List<PlotEvent>();

	public PassiveEvent CurrentEventPlaying => _currentEventPlaying;
	public PassiveEvent PreviousEventPlayed => _previousEventPlayed;

	public event Action<EventInterruption> OnPassiveEventReadyToStart;

	private void Start() 
	{
		Setup();
		SubscribeToEvents();
	}

// Update this to when the bee reaches into the plot
	void Update()
    {
		if (PlotsManager.Instance.CurrentPlot == Plot.None) return;

		if (EventManager.Instance.PlayingEvent) 
		{
			StopLookingForEvent();
			return;
		} 
		
		if (_findPassiveEvent == null)
		{
			_findPassiveEvent = StartCoroutine(FindPassiveEvent());
		} 
    }

	private IEnumerator FindPassiveEvent() 
	{
		yield return new WaitForSeconds(_delayedEventStartDuration);

		PlotEvent plotEvent = GetRandomPlotEventToPlay();

		if (plotEvent == null) 
		{
			StopLookingForEvent();
			yield break;
		}

		InformEventReadyToPlay(plotEvent);
		_findPassiveEvent = null;
	}

	private PlotEvent GetRandomPlotEventToPlay() 
	{
		List<PlotEvent> playableEvents = GetPlayablePlotEvents(GetPlotEventsFromCurrentPlot());

		if (playableEvents.Count < 1) return null;

		PlotEvent eventToPlay = playableEvents[UnityEngine.Random.Range(0, playableEvents.Count)];
		_playedEvents.Add(eventToPlay);
	
		return eventToPlay;
	}

	private List<PlotEvent> GetPlayablePlotEvents (List<PlotEvent> plotEvents) 
	{
		CheckIfAllEventsPlayed(plotEvents);

		List<PlotEvent> playableEvents = plotEvents.FindAll(plotEvent => plotEvent.CanPlay());
		List<PlotEvent> playableEventsNotPlayed = playableEvents.FindAll(plotEvent => !_playedEvents.Contains(plotEvent)); 

		return playableEventsNotPlayed.Count > 0 ? playableEventsNotPlayed : playableEvents;
	}

	private void CheckIfAllEventsPlayed(List<PlotEvent> plotEvents) 
	{
		if (_playedEvents.Count == plotEvents.Count) _playedEvents.Clear();
	}

	private List<PlotEvent> GetPlotEventsFromCurrentPlot() 
	{
		foreach (PlotEventsCollection plotEventsCollection in _plotEventsCollections)
		{
			if (!IsEventOfCurrentPlot(PlotsManager.Instance.CurrentPlot, plotEventsCollection.Plot)) continue;
			return plotEventsCollection.PlotEvents;
		}

		return null;
	}
	
	internal bool IsEventOfCurrentPlot (Plot currentEnvironmentPlot, Plot eventsPlot) => currentEnvironmentPlot == eventsPlot;

	private void InformEventReadyToPlay (PlotEvent plotEvent) 
	{
		EventInterruption eventInterruption = new EventInterruption(plotEvent.gameObject, EventType.Passive);
		OnPassiveEventReadyToStart?.Invoke(eventInterruption);
	}

	private void Setup() 
	{
		_currentEventPlaying = PassiveEvent.None;
	}

	private void SubscribeToEvents() 
	{
		PlotEvent.OnPassiveEventStart += HandlePassiveEventStateChanged;
		PlotEvent.OnPasiveEventEnd += HandlePassiveEventStateChanged;
		ImageTrackingPlotUpdatedResponse.OnPlotNeedsDeactivation += HandlePlotDeactivated;
	}

	private void HandlePlotDeactivated (Plot plot)
	{
		_currentEventPlaying = PassiveEvent.None;
		StopLookingForEvent();
	}

	private void StopLookingForEvent()
	{
		if (_findPassiveEvent != null)
		{
			StopCoroutine(_findPassiveEvent);
			_findPassiveEvent = null;
		}
	}

	private void HandlePassiveEventStateChanged (UpdatePassiveEventCollection eventChangeArgs)
	{
		HandleEventChange(eventChangeArgs.CurrentEvent);
	}

	internal void HandleEventChange (PassiveEvent newEvent) 
	{
		_previousEventPlayed = _currentEventPlaying;
		_currentEventPlaying = newEvent;
	}

	private void UnsubscribeFromEvents() 
	{
		PlotEvent.OnPassiveEventStart -= HandlePassiveEventStateChanged;
		PlotEvent.OnPasiveEventEnd -= HandlePassiveEventStateChanged;
		ImageTrackingPlotUpdatedResponse.OnPlotNeedsDeactivation -= HandlePlotDeactivated;
	}

	private void OnDestroy() 
	{
		UnsubscribeFromEvents();
	}
}
