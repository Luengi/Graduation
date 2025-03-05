using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayAnimation)), RequireComponent(typeof(PlayParticle)), RequireComponent(typeof(SoundComponent)), ]
public class ChestInspection : MonoBehaviour, IInteractable, IEvent, IInterruptible
{
	private const string STARTED_ANIMATION_PARAMETER_NAME = "HasStarted";
	private const string RATTLE_ANIMATION_PARAMETER_NAME = "IsRattling";
	private const string OPEN_STATE_NAME = "chest_open_animation";
	private const string CLOSE_STATE_NAME = "chest_close_animation";

	[SerializeField] private string _boolAnimatorParameterName;
	[SerializeField] private ObjectMovement _beeMovement;
	[SerializeField] private Transform _infrontOfChestPosition;
	[SerializeField] private Transform _inChestPosition;
	[SerializeField] private Movement _beeMovementConfig;
	[SerializeField] private float _cooldownTimeToForceReleaseBee = 2f;
	[SerializeField] private Sound _tapSFX;
	[SerializeField] private Sound _onceChestOpenSFX;
	[SerializeField] private Sound _onceChestCloseSFX;
	[SerializeField] private Sound _chestLockSFX;
	[SerializeField] private Sound _onceBeeGoToChestSFX;
	[SerializeField] private Sound _onceBeeLockedInChestSFX;
	[SerializeField] private ParticleSystem _sparkleParticle;
	
	private Cooldown _cooldown;
	private PlayAnimation _playAnimation;
	private SoundComponent _soundComponent;
	private PlayParticle _playParticle;
	private bool _hasAnimationStarted = false;
	private ChestEventState _chestEventState = ChestEventState.None;
	private bool _isEventInterrupted = false; 

	public event Action OnEventDone;
	public event Action<IInterruptible> OnInterruptedDone;

	public bool CanInterrupt {get; set;}
	public EventState State {get; set;}
	public bool MultipleInteractions {get; set;}

	private void Awake() 
	{
		_playAnimation = GetComponent<PlayAnimation>();
		_playParticle = GetComponent<PlayParticle>();
		_soundComponent = GetComponent<SoundComponent>();
		_cooldown = new Cooldown();
	}

	private void Start() 
	{
		CanInterrupt = true;
		MultipleInteractions = true;

		SubscribeToEvents();
	}

	private void Update() 
	{
		_cooldown.DecreaseCooldown(Time.deltaTime);
	}

	public void Interact()
	{
		if (!IsChestInspectionRunning()) 
		{
			_soundComponent.PlaySound(_tapSFX);
			StartCoroutine(InitialChestAnimation());
			return;
		}

		switch (_chestEventState)
		{
			case ChestEventState.InforntOfChest:
				if (_cooldown.IsOnCooldown) _cooldown.StopCooldown();
				UpdateChestEventState(ChestEventState.GoingInsideChest);				
				break;
			case ChestEventState.InsideChest:
				if (_cooldown.IsOnCooldown) _cooldown.StopCooldown();
				UpdateChestEventState(ChestEventState.LeavingChest);				
				break;
		}
	}

	public bool IsChestInspectionRunning() => _chestEventState != ChestEventState.None && Bee.Instance.State == BeeState.ChestInspection;

	private IEnumerator InitialChestAnimation() 
	{
		if (!_hasAnimationStarted) 
		{
			_hasAnimationStarted = true;
			EnableChestAnimation();
		}

		UpdateChestEventState(ChestEventState.OpeningChest);
		yield return StartCoroutine(OpenAnimation());
		// change the bee state after the chest opens
		Bee.Instance.UpdateState(BeeState.ChestInspection);
		UpdateChestEventState(ChestEventState.GoingInFrontChest);
	}

	private IEnumerator MoveBeeOutOfChest()
	{		
		_playAnimation.SetBoolParameter(RATTLE_ANIMATION_PARAMETER_NAME, false);
		yield return StartCoroutine(OpenAnimation());
		yield return StartCoroutine(MoveBeeToPosition(_infrontOfChestPosition.position));
		EventDoneSetup();
		yield return StartCoroutine(CloseAnimation());
	}

	private void EventDoneSetup()
	{
		ReleaseBeeFromEvent();
		UpdateChestEventState(ChestEventState.None);
		OnEventDone?.Invoke();
	}

	private void ReleaseBeeFromEvent() 
	{
		Bee.Instance.UpdateState(BeeState.Idle);
	}

	private IEnumerator FinishUpdatingAnimationState (string stateName) 
	{
		while (!_playAnimation.CurrentAnimationState(stateName)) 
		{
			yield return null;
		}
	}

	private void EnableChestAnimation() 
	{
		_playAnimation.SetBoolParameter(STARTED_ANIMATION_PARAMETER_NAME, true);
	}

	private void SetOpenChestAnimation() 
	{
		_playAnimation.SetBoolParameter(_boolAnimatorParameterName, true);
	}

	private void SetCloseChestAnimation() 
	{
		_playAnimation.SetBoolParameter(_boolAnimatorParameterName, false);
	}

	private IEnumerator FinishPlayingAnimation() 
	{
		while (!_playAnimation.IsAnimationOver()) 
		{
			yield return null;
		}
	}

	private void UpdateChestEventState (ChestEventState state) 
	{
		if (_isEventInterrupted) return;

		_chestEventState = state;
		HandleState(state);
	}

	private void HandleState (ChestEventState stateToHandle) 
	{
		switch (stateToHandle)
		{
			case ChestEventState.GoingInFrontChest:
				StartCoroutine(MoveBeeInfrontOfChest());
				break;

			case ChestEventState.GoingInsideChest:
				StartCoroutine(MoveBeeInChest());
				break;

			case ChestEventState.LeavingChest:
				StartCoroutine(MoveBeeOutOfChest());
				break;
		}
	}

	private IEnumerator MoveBeeToPosition (Vector3 position) 
	{
		while (!_beeMovement.IsInPlace(position)) 
		{
			_beeMovement.MoveTo(position, _beeMovementConfig.MovementSpeed);
			yield return null;
		}
	}

	private IEnumerator MoveBeeInfrontOfChest() 
	{
		_soundComponent.PlaySound(_onceBeeGoToChestSFX);
		yield return StartCoroutine(MoveBeeToPosition(_infrontOfChestPosition.position));

		_cooldown.StartCooldown(_cooldownTimeToForceReleaseBee);
		UpdateChestEventState(ChestEventState.InforntOfChest);
	}

	private IEnumerator MoveBeeInChest()
	{
		yield return StartCoroutine(MoveBeeToPosition(_inChestPosition.position));
		UpdateChestEventState(ChestEventState.ClosingChest);
		_soundComponent.PlaySound(_onceBeeLockedInChestSFX);
		yield return CloseAnimation();
		_soundComponent.PlaySound(_chestLockSFX);
		_playAnimation.SetBoolParameter(RATTLE_ANIMATION_PARAMETER_NAME, true);
		_cooldown.StartCooldown(_cooldownTimeToForceReleaseBee);
		UpdateChestEventState(ChestEventState.InsideChest);
	}

	private void HandleCooldownDone() 
	{
		if (_chestEventState == ChestEventState.InforntOfChest) 
		{
			UpdateChestEventState(ChestEventState.GoingInsideChest);
			return;
		}

		// bee is inside the chest and bee should leave
		UpdateChestEventState(ChestEventState.LeavingChest);
	}

	public void InterruptionSetup() 
	{
		_isEventInterrupted = true;
		if (_cooldown.IsOnCooldown) _cooldown.StopCooldown();
	}

	public void InterruptEvent()
	{
		StopAllCoroutines();
		InterruptionSetup();
		StartCoroutine(InterruptionCleanup());
	}

	private IEnumerator CloseAnimation() 
	{
		_soundComponent.PlaySound(_onceChestCloseSFX);
		_playParticle.ToggleOff();
		_sparkleParticle.Stop();
		SetCloseChestAnimation();
		yield return StartCoroutine(FinishAnimation(CLOSE_STATE_NAME));
	}

	private IEnumerator OpenAnimation() 
	{
		_soundComponent.PlaySound(_onceChestOpenSFX);
		SetOpenChestAnimation();
		yield return StartCoroutine(FinishAnimation(OPEN_STATE_NAME));
		_playParticle.ToggleOn();
		_sparkleParticle.Play();
	}

	private IEnumerator FinishAnimation (string animationStateName) 
	{
		yield return StartCoroutine(FinishUpdatingAnimationState(animationStateName));
		yield return StartCoroutine(FinishPlayingAnimation());
	}

	public IEnumerator InterruptionCleanup() 
	{
		switch (_chestEventState)
		{
			case ChestEventState.OpeningChest:
				ReleaseBeeFromEvent();
				yield return StartCoroutine(CloseAnimation());
				break;
			case ChestEventState.GoingInFrontChest:
			case ChestEventState.InforntOfChest:
			case ChestEventState.GoingInsideChest:
				ReleaseBeeFromEvent();
				yield return StartCoroutine(CloseAnimation());
				break;
			case ChestEventState.ClosingChest:
			case ChestEventState.InsideChest:
				yield return StartCoroutine(OpenAnimation());
				ReleaseBeeFromEvent();
				yield return StartCoroutine(CloseAnimation());
				break;
		}

		OnInterruptedDone?.Invoke(this);
		_isEventInterrupted = false;
	}

	private void SubscribeToEvents() 
	{
		_cooldown.OnCooldownOver += HandleCooldownDone;
	}

	private void UnsubscribeToEvents() 
	{
		_cooldown.OnCooldownOver -= HandleCooldownDone;
	}

	private void OnDestroy() 
	{
		UnsubscribeToEvents();
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
