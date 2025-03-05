using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayAnimation)), RequireComponent(typeof(AvoidObjectSwimmingBehavior)), RequireComponent(typeof(ObjectMovement)), RequireComponent(typeof(SoundComponent)), ]
public class BeeMovement : MonoBehaviour
{
	[Header("Setting")]
	[SerializeField] private float _maxRotationAngle = 200f;
	[SerializeField] internal Movement _beeMovementStat;
	
	[Header("Ocean Reference")]
	[SerializeField] private Transform _oceanAnchor;
	[SerializeField] private Transform _oceanPortal;

	[FormerlySerializedAs("_villageStartPosition")]
	[Header("Village Reference")] 
	[SerializeField] private Transform _villageIdleRotatePoint;
	[SerializeField] private Vector3 _idleRotateAxis = new Vector3(0,1,0);
	[SerializeField] private float _distanceToPivot = 1;

	[Header("Animation")]
	[Tooltip("The name of the animation parameter that triggers the bee swimming animation")]
	[SerializeField] private string _beeSwimmingAnimationParameterName;
	[Tooltip("The name of the animation state that contains the bee swimming animation")]
	[SerializeField] private string _beeSwimmingAnimationStateName;
	
	[Header("Sound")]
	[SerializeField] Sound _goToPlotSoundSFX;
	[SerializeField] Sound _swimSFX;
	[SerializeField] Sound _flySFX;

	private PlayAnimation _playAnimation;

	private bool _overPortal;
	private Coroutine _movementCoroutine;
	
	private AvoidObjectSwimmingBehavior _avoidObjectSwimmingBehavior;

	private ObjectMovement _objectMovement;
	private SoundComponent _soundComponent;

	public static event Action OnBeeEnteredPlot;

	private void Awake()
	{
		_soundComponent = GetComponent<SoundComponent>();
		_objectMovement = GetComponent<ObjectMovement>();
		_playAnimation = GetComponent<PlayAnimation>();
		_avoidObjectSwimmingBehavior = GetComponent<AvoidObjectSwimmingBehavior>();
	}

	private void Start()
	{
		_soundComponent.PlaySound(_flySFX);
		SubscribeToEvents();
	}

	private void Update()
	{
		Move();
	}

	// TODO: Adjust implementation when handling movement in village plot - see how it goes because the same movement could be used for all 3 plots in my opinion (Orestis). Up to discussion
	private void Move()
	{
		if (Bee.Instance.State != BeeState.Idle) return;
		
		if (PlotsManager.Instance._currentPlot == Plot.Ocean)
		{
			// Playing bee swimming animation if it is not already playing
			if (!_playAnimation.CurrentAnimationState(_beeSwimmingAnimationStateName)) 
			{
				_playAnimation.SetBoolParameter(_beeSwimmingAnimationParameterName, true);
			};
				
			_avoidObjectSwimmingBehavior.Move(_beeMovementStat.MovementSpeed);
		}
		else if (PlotsManager.Instance._currentPlot == Plot.Village)
		{
			_objectMovement.MoveAroundPivot(_villageIdleRotatePoint.position,_idleRotateAxis,_distanceToPivot, _beeMovementStat.RotationSpeed,_beeMovementStat.MovementSpeed);
		}
	}

	private void HandlePlotChange()
	{
		_soundComponent.PlaySound(_goToPlotSoundSFX);
		// This is for now until the movement for the village plot is implemented. 
		// Currently the movement is just made for the ocean plot. We should check the Plot and based on that decided the animation, etc.
		if (PlotsManager.Instance._currentPlot != Plot.Ocean) 
		{
			// If the bee is not in the ocean plot, we should stop the swimming animation
			if (_playAnimation.CurrentAnimationState(_beeSwimmingAnimationStateName)) 
			{
				_playAnimation.SetBoolParameter(_beeSwimmingAnimationParameterName, false);
			}
		}
	}

	private IEnumerator MoveThroughPortal (Transform achor, Transform portal)
	{
		bool isAtPortal = false;
		
		while (!isAtPortal && !_overPortal)
		{
			isAtPortal = MathHelper.AreVectorApproximatelyEqual(transform.position, portal.position, 0.1f);
			MoveToPortalPosition(portal.position);
			yield return null;
		}

		EnterPortal(achor.position, portal.position);
		
		OnBeeEnteredPlot?.Invoke();
		Bee.Instance.UpdateState(BeeState.Idle);
		_overPortal = false;
	}

	private void MoveToPortalPosition (Vector3 portalPosition)
	{
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(portalPosition - transform.position), _maxRotationAngle * Time.deltaTime);
		transform.position = transform.forward * _beeMovementStat.MovementSpeed * Time.deltaTime + transform.position;
	}
	
	private void EnterPortal (Vector3 targetCameraPosition, Vector3 portalPosition)
	{
		transform.position = targetCameraPosition + (portalPosition - Camera.main.transform.position);
		_overPortal = true;
	}

	private IEnumerator UpdateVillage()
	{
		OnBeeEnteredPlot?.Invoke();
		Bee.Instance.UpdateState(BeeState.Idle);
		yield return null;
	}

	private IEnumerator MoveToplot (Transform plotPosition)
	{
		Bee.Instance.UpdateState(BeeState.EnteringPlot);

		do
		{
			_objectMovement.MoveTo(plotPosition.position, _beeMovementStat.MovementSpeed);
			yield return null;
		} while (!_objectMovement.IsInPlace(plotPosition.position));
		
		OnBeeEnteredPlot?.Invoke();
		Bee.Instance.UpdateState(BeeState.Idle);
	}

	private void HandleGoingToPlot (Plot plot) 
	{
		if (Bee.Instance.State != BeeState.FollowingCamera) return;

		Bee.Instance.UpdateState(BeeState.EnteringPlot);
		
		switch (plot)
		{
			case Plot.Ocean:
				_movementCoroutine = StartCoroutine(MoveThroughPortal(_oceanAnchor,_oceanPortal));
				break;
			case Plot.Village:
				_movementCoroutine = StartCoroutine(MoveToplot(_villageIdleRotatePoint));
				break;
			default:
				// Debug.LogError(this + ": Can not find plot for bee to handle");
				break;
		}
		
		HandlePlotChange();
	}

	private void SubscribeToEvents()
	{
		PlotsManager.OnPlotActivated += HandleGoingToPlot;
		PlotsManager.OnPlotDeactivated += HandlePlotDeactivated;
		Bee.OnBeeStateChanged += HandleBeeStateChange;
	}
	
	// TODO: Possibly handle swimming animation here in a switch statement
	private void HandleBeeStateChange (BeeState state)
	{
		if (state == BeeState.Idle && PlotsManager.Instance.CurrentPlot == Plot.Ocean)
		{
			_soundComponent.PlaySound(_swimSFX);
		}
		else if (state == BeeState.Idle && PlotsManager.Instance.CurrentPlot == Plot.Village)
		{
			_soundComponent.PlaySound(_flySFX);
		}
		else
		{
			_soundComponent.StopSound();
		}
	}

	private void HandlePlotDeactivated (Plot plotDeactivated)
	{
		if (_movementCoroutine != null)
		{
			StopCoroutine(_movementCoroutine);
			_movementCoroutine = null;
		}


		PlaceBeeBehindCamera();

		if (plotDeactivated == Plot.Ocean) 
		{
			_playAnimation.SetBoolParameter(_beeSwimmingAnimationParameterName, false);
		}
		
		if (Bee.Instance.State == BeeState.FollowingCamera) return;
		
		Bee.Instance.UpdateState(BeeState.FollowingCamera);
	}

	private void PlaceBeeBehindCamera()
	{
		transform.position = Camera.main.transform.position + Camera.main.transform.forward * -2f;
	}

	private void UnSubscribeToEvents()
	{
		PlotsManager.OnPlotActivated -= HandleGoingToPlot;
		PlotsManager.OnPlotDeactivated -= HandlePlotDeactivated;
		Bee.OnBeeStateChanged -= HandleBeeStateChange;
	}

	private void OnDestroy()
	{
		UnSubscribeToEvents();
	}
}
