using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SoundComponent)), RequireComponent(typeof(PlayParticle))]
public class FishingRodPassive : PlotEvent, IInterruptible
{
	private const string SUPRISE_ANIMATION_PARAMETER = "IsSuprised";
	private const string CATCH_ANIMATION_PARAMETER = "Caught";
	private const string RELEASE_ANIMATION_PARAMETER = "FinishedCatching";
	private const string RELEASE_ANIMATION_NAME = "Release";
	
	[SerializeField] Transform _treeStumpHover;
	[Tooltip("Look at this position in front of the tree stump before looking down with animation")]
	[SerializeField] Transform _riverLook;
	[SerializeField] BeeMovement _beeMovement;
	[SerializeField] ObjectMovement _beeObjectMovement;
	[SerializeField] PlayAnimation _beePlayAnimation;
	[SerializeField] PlayAnimation _rodPlayAnimation;
	[SerializeField] float _catchFishDuration = 2f;
	[Tooltip("Wait this amount of seconds to end the sequence, after releasing the fish")]
	[SerializeField] float _delayToEndSequence = 2f;
	[SerializeField] Range _timeToTriggerCatch;
	[SerializeField] Sound _fishingRodIdleSFX;
	[SerializeField] Sound _onceFishingRodReleaseSFX;
	[SerializeField] Sound _onceFishingRodReleaseBeeReactionSFX;
	[SerializeField] Sound _onceBeeAngrySFX;
	[SerializeField] Sound _onceBeeCuriousSFX;
 
	public event Action<IInterruptible> OnInterruptedDone;
	
	private FishingRodPassiveState _fishingRodEventState;
	private SoundComponent _soundComponent;
	private PlayParticle _playParticle;
	
	private void Awake()
	{
		_playParticle = GetComponent<PlayParticle>();
		_soundComponent = GetComponent<SoundComponent>();
	}

	public override bool CanPlay() => _fishingRodEventState == FishingRodPassiveState.None;

	public void InterruptEvent()
	{
		LeaveStump();
		OnInterruptedDone?.Invoke(this);
	}
	
	public override void StartEvent()
	{
		base.StartEvent();
		UpdatePassiveEventCollection metadata = SetupStartEventMetadata();

		FireStartEvent(metadata);
		
		Bee.Instance.UpdateState(BeeState.Fishing);
		UpdateState(FishingRodPassiveState.ApproachingStump);
	}
	
	private void UpdateState (FishingRodPassiveState stateToUpdate)
	{
		_fishingRodEventState = stateToUpdate;
		HandleState(_fishingRodEventState);
	}
	
	private void HandleState (FishingRodPassiveState state)
	{
		switch (state)
		{
			case FishingRodPassiveState.ApproachingStump:
				StartCoroutine(ApproachStump());
				break;
			case FishingRodPassiveState.LookingAtRiver:
				StartCoroutine(LookAtRiver());
				break;
			case FishingRodPassiveState.CatchingFish:
				StartCoroutine(FishingRodCatch());
				break;
			case FishingRodPassiveState.ReleasingFish:
				StartCoroutine(FishingRodRelease());
				break;
			case FishingRodPassiveState.LeavingStump:
				LeaveStump();
				break;
		}
	}
	
	private IEnumerator ApproachStump()
	{
		yield return MoveBeeToPosition(_treeStumpHover.position);
		UpdateState(FishingRodPassiveState.LookingAtRiver);
	}
	
	private IEnumerator LookAtRiver()
	{
		_beePlayAnimation.SetTrigger(SUPRISE_ANIMATION_PARAMETER);
		_soundComponent.PlaySound(_onceBeeCuriousSFX);
		
		yield return new WaitForSeconds(_timeToTriggerCatch.GetRandomValueWithinRange());
			
		UpdateState(FishingRodPassiveState.CatchingFish);
	}
	
	private IEnumerator FishingRodCatch()
	{
		_playParticle.ToggleOn();
		_rodPlayAnimation.SetBoolParameter(CATCH_ANIMATION_PARAMETER, true);
		yield return new WaitForSeconds(_catchFishDuration);
		
		UpdateState(FishingRodPassiveState.ReleasingFish);
	}
	
	private IEnumerator FishingRodRelease()
	{
		_playParticle.ToggleOff();
		_soundComponent.PlaySound(_onceFishingRodReleaseSFX);
		_soundComponent.PlaySound(_onceFishingRodReleaseBeeReactionSFX);
		_soundComponent.PlaySound(_onceBeeAngrySFX);
		_rodPlayAnimation.SetBoolParameter(RELEASE_ANIMATION_PARAMETER, true);
		yield return _rodPlayAnimation.WaitForAnimationToEnd();
		
		_rodPlayAnimation.SetBoolParameter(RELEASE_ANIMATION_PARAMETER, false);
		_rodPlayAnimation.SetBoolParameter(CATCH_ANIMATION_PARAMETER, false);
		
		// Small delay before ending the sequence
		yield return new WaitForSeconds(_delayToEndSequence);

		UpdateState(FishingRodPassiveState.LeavingStump);
	}
	
	private void LeaveStump()
	{
		StopAnimations();
		Bee.Instance.UpdateState(BeeState.Idle);
		HandleEventDone();
	}
	
	private void HandleEventDone()
	{
		StopAnimations();
		_fishingRodEventState = FishingRodPassiveState.None;
		FireEndEvent(SetupEndEventMetadata());
	}
	
	private void StopAnimations()
	{
		StopAllCoroutines();
	}
	
	private IEnumerator MoveBeeToPosition (Vector3 position)
	{
		while (!_beeObjectMovement.IsInPlace(position))
		{
			_beeObjectMovement.MoveTo(position, _beeMovement._beeMovementStat.MovementSpeed);
			_beeObjectMovement.SnapRotationTowards(position);
			yield return null;
		}

		Quaternion targetRotation = Quaternion.LookRotation((_riverLook.position - _beeMovement.transform.position).normalized);
		yield return StartCoroutine(SmoothRotationCoroutine(targetRotation, 0.25f));
	}

	IEnumerator SmoothRotationCoroutine (Quaternion targetRotation, float duration)
	{
		Quaternion startRotation = _beeMovement.transform.rotation;
		float timeElapsed = 0f;
		float percentageCompleted;

		while (timeElapsed < duration)
		{
			// Slerp from start rotation to the target rotation
			percentageCompleted = timeElapsed / duration;
			_beeObjectMovement.SmoothRotate(startRotation, targetRotation, percentageCompleted);
			timeElapsed += Time.deltaTime;

			yield return null;
		}

		// Ensure the final rotation is exactly the target rotation
		_beeMovement.transform.rotation = targetRotation;
	}
	
	protected override void HandlePlotActivated()
	{	
		if (PlotsManager.Instance.CurrentPlot != Plot.Village) return;

		SetUpPassiveEvent();
	}
	
	private void SetUpPassiveEvent()
	{
		_state = EventState.InitialWaiting;
	}

	private UpdatePassiveEventCollection SetupStartEventMetadata()
	{
		return new UpdatePassiveEventCollection
		{
			PreviousEvent = PassiveEventManager.Instance.CurrentEventPlaying,
			CurrentEvent = PassiveEvent.FishingRod,
		};
	}
}
