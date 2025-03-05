using System;
using System.Collections;
using UnityEngine;

public class OpenClamsPassiveEvent : PlotEvent, IEvent, IInterruptible
{
	[SerializeField] internal ToggleObjectPassiveEvent _toggleObjectEvent;
	[SerializeField] internal ObjectMovement _beeMovement;
	[SerializeField] private Movement _beeMovementConfig;

	internal IToggleComponent _toggleComponent;
	internal GameObject _toggleableObject;

	public event Action<IInterruptible> OnInterruptedDone;

	private void Awake() 
	{
		_toggleObjectEvent.RetrieveToggleableObjects();
	}

	private void Start() 
	{
		SubscribeToEvents();
	}

   	// private void Update()
	// {
	// 	 _cooldown.DecreaseCooldown(Time.deltaTime);
	// }

	internal void SetUpPassiveEvent() 
	{
		_state = EventState.InitialWaiting;
		// _cooldown.StartCooldown(_config.Timing.StartDelay);
		// _frequency.FrequencyAmount = _config.Timing.Frequency;
	}

	private IEnumerator MoveBeeToClamp (Transform clam)
	{
		Vector3 target = clam.position + clam.forward;
		while (!_beeMovement.IsInPlace(target))
		{
			_beeMovement.MoveTo(target, _beeMovementConfig.MovementSpeed);
			yield return null;
		}

		_toggleComponent.Toggle();
		_toggleComponent.OnToggleDone += HandleCurrentClamToggle;
	}

	private void HandleCurrentClamToggle()
	{
		FireEndEvent(SetupEndEventMetadata());
		base.UpdateEventStatus();
		_toggleComponent.OnToggleDone -= HandleCurrentClamToggle;
	}

	public override void StartEvent()
	{
		base.StartEvent();
		UpdatePassiveEventCollection metadata = SetupStartEventMetadata(_toggleableObject.transform);

		FireStartEvent(metadata);
		StartCoroutine(MoveBeeToClamp(_toggleableObject.transform));
	}

	internal protected override void UpdateEventStatus()
	{
		if (!_toggleObjectEvent.TryGetRandomObjectWithStateOn(out IToggleComponent toggleComponent, out GameObject toggleableObject)) 
		{
			// _cooldown.StartCooldown(_state == EventState.InitialWaiting ? _config.Timing.StartDelay : _config.Timing.Cooldown);
			return;
		}
		
		_toggleComponent = toggleComponent;
		_toggleableObject = toggleableObject;
		base.UpdateEventStatus();
	}

	private bool TryGetOpenClam() 
	{
		if (!_toggleObjectEvent.TryGetRandomObjectWithStateOn(out IToggleComponent toggleComponent, out GameObject toggleableObject)) 
		{
			return false;
		}
		
		_toggleComponent = toggleComponent;
		_toggleableObject = toggleableObject;
		
		return true;
	}

	internal UpdatePassiveEventCollection SetupStartEventMetadata (Transform openClam)
	{
		return new UpdatePassiveEventCollection
		{
			PreviousEvent = PassiveEventManager.Instance.CurrentEventPlaying,
			CurrentEvent = PassiveEvent.OpenClams,
			State = BeeState.CloseClams,
			Metadata = new EventMetadata
			{
				Target = openClam
			}
		};
	}

	// TODO: see if this needs to be OnDestroy() !!!
	private void OnDestory() 
	{
		UnsubscribeFromEvents();
	}

	public void InterruptEvent()
	{
		_state = EventState.Waiting;
		if (_toggleComponent != null) _toggleComponent.OnToggleDone -= HandleCurrentClamToggle;

		HandleDoneStatus();
		OnInterruptedDone?.Invoke(this);
	}

	protected override void HandlePlotActivated()
	{
		if (PlotsManager.Instance.CurrentPlot != Plot.Ocean) return;

		SetUpPassiveEvent();
	}

	public override bool CanPlay() => TryGetOpenClam();
}
