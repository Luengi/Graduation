using System;
using UnityEngine;
using System.Diagnostics;

public class Bee : Singleton<Bee>
{
	private BeeState _state;

	public BeeState State => _state;

	public static event Action<BeeState> OnBeeStateChanged;
	
	private void Start()
	{
		_state = BeeState.FollowingCamera;

		SubscribeToEvents();
	}

	private void SubscribeToEvents()
	{
		PlotEvent.OnPassiveEventStart += UpdateState;
		PlotEvent.OnPasiveEventEnd += UpdateState;
	}

	private void UnsubscribeFromEvents()
	{
		PlotEvent.OnPassiveEventStart -= UpdateState;
		PlotEvent.OnPasiveEventEnd -= UpdateState;
	}

	private void UpdateState (UpdatePassiveEventCollection newState) 
	{
		// StackTrace stackTrace = new StackTrace();
		// UnityEngine.Debug.Log($"Bee.UpdateState() to {newState.State} called from {stackTrace.GetFrame(1).GetMethod().Name}");
		_state = newState.State;
		OnBeeStateChanged?.Invoke(_state);
	}

	public void UpdateState (BeeState newState) 
	{
		// StackTrace stackTrace = new StackTrace();
		// UnityEngine.Debug.Log($"Bee.UpdateState() to {newState} called from {stackTrace.GetFrame(1).GetMethod().Name}");
		_state = newState;
		OnBeeStateChanged?.Invoke(_state);
	}

	private void OnDestroy()
	{
		UnsubscribeFromEvents();
	}
}
