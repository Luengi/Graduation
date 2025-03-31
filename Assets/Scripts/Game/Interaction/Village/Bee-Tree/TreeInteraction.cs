using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PlayAnimation)), RequireComponent(typeof(SoundComponent)), RequireComponent(typeof(BoxCollider)), ]
public class TreeInteraction : MonoBehaviour, IInteractable, IEvent, IInterruptible
{
	private const string LOOK_DOWN_ANIMATION_PARAMETER = "IsLookingDown";
	
	[SerializeField] private ObjectMovement _beeMovement;
	
	[SerializeField] private string _animationTriggerVariable;
	[SerializeField] private string _animationState;
	[SerializeField] private float _resetTime;

	[SerializeField] private Collider _randomRangeCollider;
	[SerializeField] private float _minDistanceFromCenter;
	
	[SerializeField] private Transform _aCornHolder;

	[SerializeField] private Transform _inspectPosition;
	[SerializeField] private Sound _onceTapTreeSFX;
	[SerializeField] private Sound _onceTreeRotateSFX;
	[SerializeField] private Sound _onceAcornFallingSFX;
	[SerializeField] private Sound _onceAcornHitGroundSFX;
	[SerializeField] private PlayAnimation _beeAnimation;
	
	private int _lastSelectedConeIndex;
	private List<ACorn> _aCorns = new List<ACorn>();
	private List<ACorn> _aCornsOnGround = new List<ACorn>();
	public bool CanInterrupt {get; set;}
	public bool MultipleInteractions {get; set;}
	
	public event Action OnEventDone;
	public event Action<IInterruptible> OnInterruptedDone;
	public EventState State {get; set;}
	
	private SoundComponent _soundComponent;
	
	internal PlayAnimation _playAnimation;
	internal bool _ready;
	

	private void Awake()
	{
		_playAnimation = GetComponent<PlayAnimation>();
		_soundComponent = GetComponent<SoundComponent>();
	}
	
	private void Start()
	{
		SetUp();
	}
	
	private void SetUp()
	{
		_ready = true;
		CanInterrupt = true;
		MultipleInteractions = false;
		GetCone();
	}

	private void GetCone()
	{
		foreach (Transform cone in _aCornHolder)
		{
			if (cone.GetComponent<ACorn>())
			{
				_aCorns.Add(cone.GetComponent<ACorn>());
			}
		}
	}
	
	public void Interact()
	{
		if (_ready)
		{
			_soundComponent.PlaySound(_onceTapTreeSFX);
			StartCoroutine(ShakeTree(GetRandomCone(_aCorns)));
			
			BeeMoveToInspectPosition();
		}
		
	}

	private IEnumerator ShakeTree (ACorn aCorn)
	{
		_ready = false;
		
		_soundComponent.PlaySound(_onceTreeRotateSFX);
		yield return RepareConeAndPlayAnimation(aCorn);
		_soundComponent.PlaySound(_onceAcornFallingSFX);

		yield return DropTheCone(aCorn);
		_soundComponent.PlaySound(_onceAcornHitGroundSFX);

		OnEventDone?.Invoke();
		_ready = true;
		
		yield return FadeAndResetCone(aCorn);

		if (_aCornsOnGround.Count < 1)
		{
			if (Bee.Instance.State == BeeState.InspectTree) ReleaseBee();
		}
	}

	private IEnumerator FadeAndResetCone (ACorn aCorn)
	{
		yield return aCorn.FadeCone(_resetTime / 2);
		aCorn.ResetCone(GetRandomPointInBox());
		_aCornsOnGround.Remove(aCorn);
	}

	private IEnumerator DropTheCone (ACorn aCorn)
	{
		aCorn.DropCone();

		yield return new WaitForSeconds(_resetTime / 2);
	}

	private IEnumerator RepareConeAndPlayAnimation (ACorn aCorn)
	{
		_playAnimation.SetTrigger(_animationTriggerVariable);

		_aCornsOnGround.Add(aCorn);
		aCorn.ResetCone(GetRandomPointInBox());

		yield return WaitForAnimationStateToPlay(_animationState);
	}

	private void BeeMoveToInspectPosition()
	{
		Bee.Instance.UpdateState(BeeState.InspectTree);
		StartCoroutine(MoveBee());
	}

	private IEnumerator MoveBee()
	{
		var targetPosition = _inspectPosition.position;
		while (!_beeMovement.IsInPlace(targetPosition))
		{
			_beeMovement.MoveTo(targetPosition, 3);
			_beeMovement.SnapRotationTowards(targetPosition);
			yield return null;
		}
		_beeMovement.SnapRotationTowards(transform.position);
		_beeAnimation.SetTrigger(LOOK_DOWN_ANIMATION_PARAMETER);
	}

	private void ReleaseBee()
	{
		Bee.Instance.UpdateState(BeeState.Idle);
	}
	
	private IEnumerator WaitForAnimationStateToPlay (string state)
	{
		yield return StartCoroutine(_playAnimation.WaitForAnimationToStart(state));
		yield return StartCoroutine(_playAnimation.WaitForAnimationToEnd());
	}

	public Vector3 GetRandomPointInBox()
	{
		var colliderBound = _randomRangeCollider.bounds;
		Vector3 center = colliderBound.center;
		Bounds bounds = colliderBound;

		Vector3 randomPosition;

		do
		{
			// Generate random positions within the bounds
			float randomX = Random.Range(bounds.min.x, bounds.max.x);
			float randomY = Random.Range(bounds.min.y, bounds.max.y);
			float randomZ = Random.Range(bounds.min.z, bounds.max.z);

			randomPosition = new Vector3(randomX, randomY, randomZ);
		} 
		while (Vector3.Distance(center, randomPosition) < _minDistanceFromCenter);

		return randomPosition;
	}

	private ACorn GetRandomCone (List<ACorn> list)
	{
		if (list == null || list.Count == 0)
		{
			return null;
		}
		
		ACorn chosen;
		int randomIndex;
		
		do
		{
			randomIndex = Random.Range(0, list.Count);

			chosen = list[randomIndex];
		} 
		while (_aCornsOnGround.Contains(chosen));
		
		return chosen;
	}
	
	public void InterruptEvent()
	{
		StopCoroutine(MoveBee());
		ReleaseBee();
		
		OnInterruptedDone?.Invoke(this);
	}
	
	public void StopEvent()
	{
		StopAllCoroutines();
	}

	private void OnDisable()
	{
		StopEvent();
	}
}
