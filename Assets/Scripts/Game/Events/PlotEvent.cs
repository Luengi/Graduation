using System;
using UnityEngine;

public abstract class PlotEvent : MonoBehaviour, IEvent
{
    [SerializeField] internal protected PlotEventConfig _config;
	internal protected EventState _state;
	public EventState State {get => _state; set => _state = value;}
	public static event Action<UpdatePassiveEventCollection> OnPassiveEventStart;
	public static event Action<UpdatePassiveEventCollection> OnPasiveEventEnd;
	public event Action OnEventDone;

	public virtual void StartEvent() 
	{
		_state = _state == EventState.InitialReady ? EventState.InitialActive : EventState.Active;
	}
	
	internal protected virtual void UpdateEventStatus()
	{
		switch (_state)
		{
			case EventState.InitialWaiting:
				_state = EventState.InitialReady;
				break;
			case EventState.InitialActive:
				HandleWaitingStatus();
				break;
			case EventState.Waiting:
				_state = EventState.Ready;
				break;
			case EventState.Active:
				HandleWaitingStatus();
				break;
		}
	}
	
	internal protected virtual void HandleWaitingStatus() 
	{		
		_state = EventState.Waiting;
	}

	internal protected virtual void HandleDoneStatus() 
	{
		UpdatePassiveEventCollection eventMetadata = SetupEndEventMetadata();
		FireEndEvent(eventMetadata);
	}

	public abstract bool CanPlay();
	
	protected abstract void HandlePlotActivated();

	protected virtual void SubscribeToEvents() 
	{
		BeeMovement.OnBeeEnteredPlot += HandlePlotActivated;
	}

	protected virtual void UnsubscribeFromEvents() 
	{
		BeeMovement.OnBeeEnteredPlot -= HandlePlotActivated;
	}

	protected virtual UpdatePassiveEventCollection SetupEndEventMetadata() 
	{
		return new UpdatePassiveEventCollection{PreviousEvent = PassiveEventManager.Instance.CurrentEventPlaying, };
	}

	protected virtual UpdatePassiveEventCollection SetupForceEndEventMetadata() 
	{
		return new UpdatePassiveEventCollection{PreviousEvent = PassiveEvent.None, CurrentEvent = PassiveEvent.None, State = BeeState.FollowingCamera, };
	}

	internal protected void FireStartEvent (UpdatePassiveEventCollection eventMetadata) 
	{
		OnPassiveEventStart?.Invoke(eventMetadata);
	}

	internal protected void FireEndEvent (UpdatePassiveEventCollection eventMetadata) 
	{
		OnPasiveEventEnd?.Invoke(eventMetadata);
		OnEventDone?.Invoke();
	}

	public virtual void StopEvent()
	{
		StopAllCoroutines();
	}
}
