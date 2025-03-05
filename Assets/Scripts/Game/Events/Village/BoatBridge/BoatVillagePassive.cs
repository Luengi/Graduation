using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SoundComponent))]
public class BoatVillagePassive : PlotEvent, IInterruptible
{
	[Header("Movement")]
	[Tooltip("The bee companion prefab which contains the ObjectMovement component.")]
	[SerializeField] private ObjectMovement _beeMovement;
	
	[Tooltip("The boat object to activate and move.")]
	[SerializeField] private ObjectMovement _boatObject;

	[Tooltip("The waypoints the boat will follow. Note: The first waypoint should be rotated facing the second waypoint.")]
	[SerializeField] private BoatWaypoint[] _waypoints;

	[Tooltip("The target position on the bridge for the bee to move to.")]
	[SerializeField] private Transform _bridgeTargetPosition;

	[Tooltip("The boat speed movement.")]
	[SerializeField] private float _boatSpeed = .5f;

	[Header("Animation")]

	[Tooltip("The bee companion prefab which contains the PlayAnimation component.")]
	[SerializeField] private PlayAnimation _beeAnimation;

	[Tooltip("The wave animation state name.")]
	[SerializeField] private string _waveAnimationStateName = "Wave";
	
	[Tooltip("The wave animation parameter name.")]
	[SerializeField] private string _waveAnimationParameterName = "IsWaving"; 

	[Tooltip("The amount of times the wave animation will play.")]
	[SerializeField] private byte _waveAnimationFrequency = 2;

	[Tooltip("The initial scale of the boat.")]
	[SerializeField] private float _boatInitialScale = 0.03f;

	[Tooltip("Speed to scale up the boat.")]
	[SerializeField] private float _scaleUpSpeed = 1f;

	[Tooltip("Speed to scale down the boat.")]
	[SerializeField] private float _scaleDownSpeed = 1f;
	
	[SerializeField] private Range _playHornSoundTimeRange;
	[SerializeField] private Sound _boatEngineSFX;
	[SerializeField] private Sound _onceBoatHornSFX;
	[SerializeField] private Sound _onceBeeGiggleSFX;
	
	// Different sound component in order to play looped and one shot sounds at the same time
	[SerializeField] private SoundComponent _hornSoundComponent;

	private SoundComponent _soundComponent;

	private Coroutine _boatPassiveEventCoroutine = null;
	private Coroutine _beeSequenceCoroutine = null;

	public event Action<IInterruptible> OnInterruptedDone;

	private void Awake()
	{
		_soundComponent = GetComponent<SoundComponent>();
	}
	
	public override bool CanPlay() => _boatPassiveEventCoroutine == null;
	
	public void InterruptEvent()
	{
		if (Bee.Instance.State == BeeState.MovingToBridge) 
		{
			Bee.Instance.UpdateState(BeeState.Idle);
		}
	
		OnInterruptedDone?.Invoke(this);
	}

	public override void StartEvent()
	{
		base.StartEvent();
		UpdatePassiveEventCollection metadata = SetupStartEventMetadata();

		FireStartEvent(metadata);

		_boatPassiveEventCoroutine = StartCoroutine(BoatPassiveEvent());
	}

	private IEnumerator BoatPassiveEvent()
	{
		if (_waypoints.Length == 0) yield break;

		SpawnAtFirstPosition(_waypoints[0].Waypoint.position, _waypoints[0].Waypoint.forward);
		StartCoroutine(PlayHornCoroutine(_playHornSoundTimeRange.GetRandomValueWithinRange()));

		for (int i = 0; i < _waypoints.Length; i++)
		{
			BoatWaypoint waypoint = _waypoints[i];

			if (waypoint.IsWaypointInfrontBridge)
				_beeSequenceCoroutine = StartCoroutine(BeeSequence());
			
			Vector3 waypointPosition = waypoint.Waypoint.position;

			if (i == 0) 
				StartCoroutine(ScaleUpBoat(waypointPosition, _waypoints[1].Waypoint.position));
			else if(i == _waypoints.Length - 1)
				StartCoroutine(ScaleDownBoat(waypointPosition, _waypoints[_waypoints.Length - 2].Waypoint.position));

			yield return StartCoroutine(_boatObject.RotateUntilLookAt(waypointPosition, .2f));
			yield return StartCoroutine(_boatObject.MoveUntilObjectReached(waypointPosition, _boatSpeed, 0.05f));
		}

		_boatObject.gameObject.SetActive(false);
		_soundComponent.StopSound();

		FireEndEvent(SetupEndEventMetadata());

		_boatPassiveEventCoroutine = null;
		StopAllCoroutines();
	}

	private IEnumerator ScaleUpBoat (Vector3 firstWaypointPosition, Vector3 secondWaypointPosition) 
	{
		while (_boatObject.transform.localScale.x < _boatInitialScale) 
		{
			float distanceBetweenWaypoints = Vector3.Distance(firstWaypointPosition, secondWaypointPosition);
			float distanceToSecondWaypoint = Vector3.Distance(firstWaypointPosition, _boatObject.transform.position);
			float scale = MathF.Min(distanceToSecondWaypoint / distanceBetweenWaypoints, 1);

			_boatObject.transform.localScale = scale * _boatInitialScale * _scaleUpSpeed * Vector3.one;
			yield return null;
		}
	}

	private IEnumerator ScaleDownBoat (Vector3 lastWaypointPosition, Vector3 oneBeforeLastWaypointPosition) 
	{
		while (_boatObject.transform.localScale.x > 0) 
		{
			float distanceBetweenWaypoints = Vector3.Distance(lastWaypointPosition, oneBeforeLastWaypointPosition);
			float distanceToSecondWaypoint = Vector3.Distance(lastWaypointPosition, _boatObject.transform.position);
			float scale = MathF.Min(distanceToSecondWaypoint / distanceBetweenWaypoints, 1f);

			_boatObject.transform.localScale = scale * _boatInitialScale * _scaleDownSpeed * Vector3.one ;
			yield return null;
		}
	}

	private IEnumerator BeeSequence()
	{
		yield return StartCoroutine(MoveBeeToBridge());
		yield return StartCoroutine(BeeLookAtBoat());
		yield return StartCoroutine(BeeWaveAnimation());

		Bee.Instance.UpdateState(BeeState.Idle);
	}

	private IEnumerator BeeWaveAnimation()
	{
		_beeAnimation.SetBoolParameter(_waveAnimationParameterName, true);
		yield return StartCoroutine(_beeAnimation.WaitForAnimationToStart(_waveAnimationStateName));
		_hornSoundComponent.PlaySound(_onceBeeGiggleSFX);
		yield return StartCoroutine(_beeAnimation.WaitForAnimationToEnd(_waveAnimationFrequency));
		_beeAnimation.SetBoolParameter(_waveAnimationParameterName, false);
		yield return StartCoroutine(_beeAnimation.WaitForAnimationToStart("Fly"));
	}

	private IEnumerator BeeLookAtBoat()
	{
		yield return StartCoroutine(_beeMovement.RotateUntilLookAt(_boatObject.transform.position, .1f));
	}

	private IEnumerator MoveBeeToBridge()
	{
		Bee.Instance.UpdateState(BeeState.MovingToBridge);
		Vector3 bridgePosition = _bridgeTargetPosition.position;
		StartCoroutine(_beeMovement.RotateUntilLookAt(bridgePosition, .75f));
		yield return StartCoroutine(_beeMovement.MoveUntilObjectReached(bridgePosition, .75f));
	}

	private void SpawnAtFirstPosition (Vector3 firstWaypointPosition, Vector3 firstWaypointForward)
	{
		_soundComponent.PlaySound(_boatEngineSFX);
		
		_boatObject.gameObject.SetActive(true);
		_boatObject.transform.localScale = Vector3.zero;
		_boatObject.transform.SetPositionAndRotation(firstWaypointPosition, Quaternion.LookRotation(firstWaypointForward));
	}
	
	private IEnumerator PlayHornCoroutine (float secondsToWait)
	{
		yield return new WaitForSeconds(secondsToWait);
		_hornSoundComponent.PlaySound(_onceBoatHornSFX);
		yield return new WaitForSeconds(_onceBoatHornSFX.clip.length);
		StartCoroutine(PlayHornCoroutine(_playHornSoundTimeRange.GetRandomValueWithinRange()));
	}

	protected override void HandlePlotActivated()
	{
		if (PlotsManager.Instance.CurrentPlot != Plot.Village) return;

		SetUpPassiveEvent();
	}

	internal void SetUpPassiveEvent() 
	{
		_state = EventState.InitialWaiting;
	}

	internal UpdatePassiveEventCollection SetupStartEventMetadata()
	{
		return new UpdatePassiveEventCollection
		{
			PreviousEvent = PassiveEventManager.Instance.CurrentEventPlaying,
			CurrentEvent = PassiveEvent.BoatTrip,
		};
	}

	public override void StopEvent()
	{
		base.StopEvent();
		_boatPassiveEventCoroutine = null;
	}
}
