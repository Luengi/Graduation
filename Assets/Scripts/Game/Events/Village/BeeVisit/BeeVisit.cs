using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeVisit : PlotEvent, IEvent, IInterruptible
{
	private const string KNOCKING_ANIMATION_PARAMETER = "IsKnocking";
	
	[SerializeField] private GameObject[] _inspectables;
	[SerializeField] private ObjectMovement _beeMovement;

	private PlayAnimation _beePlayAnimation;
	private List<IInspectable> _inspectablesInterfaces = new List<IInspectable>();
	private bool _inspecting;
	private IInspectable _currentSubscribedInspectable = null;

	public event Action<IInterruptible> OnInterruptedDone;

	private void Awake()
	{
		_beePlayAnimation = _beeMovement.gameObject.GetComponent<PlayAnimation>();
		foreach (GameObject inspectable in _inspectables) 
		{
			if (inspectable == null) continue;

			_inspectablesInterfaces.Add(inspectable.GetComponent<IInspectable>());
		}
	}

	public override void StartEvent()
	{
		base.StartEvent();
		UpdatePassiveEventCollection metadata = SetupStartEventMetadata();

		FireStartEvent(metadata);
		StartCoroutine(VisitPlaces());
	}

	private IEnumerator VisitPlaces() 
	{
		foreach (IInspectable inspectable in _inspectablesInterfaces) 
		{
			yield return StartCoroutine(_beeMovement.MoveUntilObjectReached(inspectable.InspectablePoint.transform.position, 1f));
			InspectPlace(inspectable);

			yield return StartCoroutine(WaitForInspection());

			inspectable.OnInspected -= HandleInspectableInspected;
		}

		_currentSubscribedInspectable = null;

		FireEndEvent(SetupEndEventMetadata());
	}

	private IEnumerator WaitForInspection() 
	{
		while (_inspecting) 
		{
			yield return null;
		}
	}

	private void InspectPlace (IInspectable inspectable) 
	{
		_beePlayAnimation.SetTrigger(KNOCKING_ANIMATION_PARAMETER);
		inspectable.OnInspected += HandleInspectableInspected;
		inspectable.Inspect();
		_inspecting = true;
		_currentSubscribedInspectable = inspectable;
	}

	private void HandleInspectableInspected()
	{
		_inspecting = false;
	}

	internal UpdatePassiveEventCollection SetupStartEventMetadata()
	{
		return new UpdatePassiveEventCollection
		{
			PreviousEvent = PassiveEventManager.Instance.CurrentEventPlaying,
			CurrentEvent = PassiveEvent.BeeVisit,
			State = BeeState.Inspecting,
		};
	}

	public override bool CanPlay() => true;

	public void InterruptEvent()
	{
		StopAllCoroutines();

		if (_currentSubscribedInspectable != null) 
		{
			_currentSubscribedInspectable.StopInspecting();
			_currentSubscribedInspectable.OnInspected -= HandleInspectableInspected;
		}
		
		Bee.Instance.UpdateState(BeeState.Idle);
		OnInterruptedDone?.Invoke(this);
	}

	internal void SetUpPassiveEvent() 
	{
		_state = EventState.InitialWaiting;
	}

	protected override void HandlePlotActivated()
	{
		if (PlotsManager.Instance.CurrentPlot != Plot.Ocean) return;

		SetUpPassiveEvent();
	}
}
