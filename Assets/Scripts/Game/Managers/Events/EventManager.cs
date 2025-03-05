using System;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
	[SerializeField] internal PassiveEventManager _passiveEventManager;
	[SerializeField] internal InteractionManager _interactionManager;

    public GameObject _currentEventGameObject;
	private GameObject _nextEventToPlay;
	private IEvent _currentEvent;
	private bool _interruptionInProcess;
	private EventType _nextEventType = EventType.None;

	public bool PlayingEvent => _currentEventGameObject != null;

	internal protected override void Awake()
	{
		base.Awake();
		if (_passiveEventManager == null || _interactionManager == null) Debug.LogWarning("Interaction Manager or Passive Event Manager is not set in Event Manager");
	}

	internal void Start()
	{
		SubscribeToEvents();
	}

	private void SubscribeToEvents()
	{
		if (_passiveEventManager != null) 
			_passiveEventManager.OnPassiveEventReadyToStart += StartInterruptionSequence;
		if (_interactionManager != null) 
			_interactionManager.OnInteractionReadyToStart += StartInterruptionSequence;

		ImageTrackingPlotUpdatedResponse.OnPlotNeedsDeactivation += HandlePlotDeactivated;
	}

	private void HandlePlotDeactivated (Plot plot)
	{
		if (_currentEvent == null) return;

		_currentEvent.OnEventDone -= HandleCurrentEventDone;
		_currentEvent.StopEvent();
		_currentEventGameObject = null;
		_currentEvent = null;
		_nextEventToPlay = null;
		_nextEventType = EventType.None;
		_interruptionInProcess = false;
	}

	private void UnsubscribeFromEvents()
	{
		if (_passiveEventManager != null) 
			_passiveEventManager.OnPassiveEventReadyToStart -= StartInterruptionSequence;
		if (_interactionManager != null) 
			_interactionManager.OnInteractionReadyToStart -= StartInterruptionSequence;

		ImageTrackingPlotUpdatedResponse.OnPlotNeedsDeactivation -= HandlePlotDeactivated;
	}

	private void StartInterruptionSequence (EventInterruption eventData)
	{
		if (_interruptionInProcess) return;

		if (_currentEventGameObject == null)
		{
			UpdateCurrentEvent(eventData.EventObject);
			_currentEventGameObject = eventData.EventObject;
			if (eventData.EventType == EventType.Passive) 
			{
				_currentEventGameObject.GetComponent<PlotEvent>().StartEvent();
			}
			else 
			{
				var interactable = _currentEventGameObject.GetComponent<IInteractable>();
				interactable.Interact();
			}
			return;
		}
		
		if (_currentEventGameObject == eventData.EventObject) 
		{ 
			if (eventData.EventType == EventType.Active) 
			{
				var interactable = _currentEventGameObject.GetComponent<IInteractable>();
				if (interactable.MultipleInteractions) 
				{
					interactable.Interact();
				}
			}
			return; 
		}

		if (!_currentEventGameObject.TryGetComponent<IInterruptible>(out IInterruptible interruptibleEvent)) return;

		_nextEventType = eventData.EventType;
		_nextEventToPlay = eventData.EventObject;
		interruptibleEvent.OnInterruptedDone += HandleEventInterrupted;
		_interruptionInProcess = true;
		interruptibleEvent.InterruptEvent();
	}

	private void HandleEventInterrupted (IInterruptible interruptibleEvent)
	{
		_interruptionInProcess = false;
		interruptibleEvent.OnInterruptedDone -= HandleEventInterrupted;
		UpdateCurrentEvent(_nextEventToPlay);

		if (_nextEventType == EventType.Passive)
			_currentEventGameObject.GetComponent<PlotEvent>().StartEvent();
		else
		{
			var interactable = _currentEventGameObject.GetComponent<IInteractable>();
			interactable.Interact();
		}
	}

	private void UpdateCurrentEvent (GameObject nextEvent)
	{
		if (_currentEvent != null)
			_currentEvent.OnEventDone -= HandleCurrentEventDone;

		_currentEventGameObject = nextEvent;
		_currentEvent = _currentEventGameObject.GetComponent<IEvent>();
		_currentEvent.OnEventDone += HandleCurrentEventDone;
	}

	private void HandleCurrentEventDone() 
	{
		_currentEventGameObject = null;
		_currentEvent.OnEventDone -= HandleCurrentEventDone;
		_currentEvent = null;
	}

	private void OnDestroy()
	{
		UnsubscribeFromEvents();
	}
}
