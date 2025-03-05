using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider)), RequireComponent(typeof(SoundComponent)), ]
public class WindowInteraction : MonoBehaviour, IInteractable, IInterruptible, IEvent
{
	private const string WAVE_ANIMATION_PARAMETER = "IsWaving";
	private const string WAVE_ANIMATION_NAME = "Wave";
	private const string WINDOW_OPEN_ANIMATION_PARAMETER = "OpeningWindow";
	private const string WINDOW_CLOSE_ANIMATION_PARAMETER = "ClosingWindow";
	private const string SHEEP_POPS_OUT_ANIMATION_PARAMETER = "Pop";
	private const string SHEEP_POPS_OUT_ANIMATION_STATE_NAME = "PopOut";
	
	[SerializeField] Transform _windowTransform;
	[SerializeField] Transform _windowFrontPosition;
	[SerializeField] ObjectMovement _beeObjectMovement;
	[SerializeField] BeeMovement _beeMovement;
	[SerializeField] PlayAnimation _beePlayAnimation;
	[SerializeField] PlayAnimation _housePlayAnimation;
	[SerializeField] Sound _onceWindowOpenSFX;
	[SerializeField] Sound _onceWindowCloseSFX;
	[SerializeField] PlayAnimation _sheepPlayAnimation;
	
	// Temporary until animations are implemented
	[SerializeField] float _secondsToWaitForAnimations;
		
	public bool CanInterrupt {get; set;} = true;
	public bool MultipleInteractions {get; set;} = false;
	public EventState State {get; set;}
	public event Action<IInterruptible> OnInterruptedDone;
	public event Action OnEventDone;
	
	private WindowInteractionState _interactionState;
	private SoundComponent _soundComponent;
	
	private void Awake()
	{
		_soundComponent = GetComponent<SoundComponent>();
	}

	public void Interact()
	{
		Bee.Instance.UpdateState(BeeState.WindowInteraction);
		UpdateState(WindowInteractionState.ApproachingWindow);
	}
	
	private void UpdateState (WindowInteractionState _stateToSet)
	{
		_interactionState = _stateToSet;
		HandleState(_interactionState);
	}
	
	private void HandleState (WindowInteractionState _state)
	{
		switch (_state)
		{
			case WindowInteractionState.ApproachingWindow:
				StartCoroutine(ApproachWindow());
				break;
			case WindowInteractionState.BeeWaving:
				StartCoroutine(Wave());
				break;
			case WindowInteractionState.OpeningWindow:
				StartCoroutine(OpenWindow());
				break;
			case WindowInteractionState.SheepResponds:
				StartCoroutine(SheepResponse());
				break;
			case WindowInteractionState.ClosingWindow:
				StartCoroutine(CloseWindow(false));
				break;
			case WindowInteractionState.LeavingWindow:
				HandleEventDone();
				break;
		}
	}
	
	// Temporary until animations are implemented
	private IEnumerator DelayCoroutine (float secondsToWait)
	{
		yield return new WaitForSeconds(secondsToWait);
	}
	
	private IEnumerator ApproachWindow()
	{
		yield return StartCoroutine(MoveBeeToPosition(_windowFrontPosition.position));
		yield return StartCoroutine(_beeObjectMovement.RotateUntilLookAt(_windowTransform.position, 0.25f));
		UpdateState(WindowInteractionState.BeeWaving);
	}
	
	private IEnumerator Wave()
	{
		_beePlayAnimation.SetBoolParameter(WAVE_ANIMATION_PARAMETER, true);
		yield return _beePlayAnimation.WaitForAnimationToStart(WAVE_ANIMATION_NAME);
		yield return _beePlayAnimation.WaitForAnimationToEnd();
		_beePlayAnimation.SetBoolParameter(WAVE_ANIMATION_PARAMETER, false);
		
		UpdateState(WindowInteractionState.OpeningWindow);
	}
	
	private IEnumerator OpenWindow()
	{
		_housePlayAnimation.SetBoolParameter(WINDOW_OPEN_ANIMATION_PARAMETER, true);
		_soundComponent.PlaySound(_onceWindowOpenSFX);
		// yield return _housePlayAnimation.WaitForAnimationToStart(WINDOW_OPEN_ANIMATION_NAME);
		yield return _housePlayAnimation.WaitForAnimationToEnd();
		_housePlayAnimation.SetBoolParameter(WINDOW_OPEN_ANIMATION_PARAMETER, false);
		// yield return StartCoroutine(DelayCoroutine(_secondsToWaitForAnimations));
		UpdateState(WindowInteractionState.SheepResponds);
	}
	
	private IEnumerator SheepResponse()
	{
		// Debug.Log("WindowInteraction: Play sheep response animation here.");
		_sheepPlayAnimation.SetBoolParameter(SHEEP_POPS_OUT_ANIMATION_PARAMETER,true);
		yield return StartCoroutine(_sheepPlayAnimation.WaitForAnimationToStart(SHEEP_POPS_OUT_ANIMATION_STATE_NAME));
		yield return StartCoroutine(_sheepPlayAnimation.WaitForAnimationToEnd());
		_sheepPlayAnimation.SetBoolParameter(SHEEP_POPS_OUT_ANIMATION_PARAMETER,false);
		UpdateState(WindowInteractionState.ClosingWindow);
	}
	
	private IEnumerator CloseWindow (bool toInterrupted)
	{
		_housePlayAnimation.SetBoolParameter(WINDOW_CLOSE_ANIMATION_PARAMETER, true);
		_housePlayAnimation.SetBoolParameter(WINDOW_OPEN_ANIMATION_PARAMETER, false);
		_soundComponent.PlaySound(_onceWindowCloseSFX);
		// yield return _housePlayAnimation.WaitForAnimationToStart(WINDOW_CLOSE_ANIMATION_NAME);
		yield return _housePlayAnimation.WaitForAnimationToEnd();
		_housePlayAnimation.SetBoolParameter(WINDOW_CLOSE_ANIMATION_PARAMETER, false);
		// yield return StartCoroutine(DelayCoroutine(_secondsToWaitForAnimations));
		if(!toInterrupted) UpdateState(WindowInteractionState.LeavingWindow);
	}
	
	
	private void HandleEventDone()
	{
		StopAllCoroutines();
		Bee.Instance.UpdateState(BeeState.Idle);
		OnEventDone?.Invoke();
	}
	
	private IEnumerator MoveBeeToPosition (Vector3 position)
	{
		// Move to the front of the window
		while (!_beeObjectMovement.IsInPlace(position))
		{
			_beeObjectMovement.MoveTo(position, _beeMovement._beeMovementStat.MovementSpeed);
			_beeObjectMovement.SnapRotationTowards(position);
			yield return null;
		}
		
		// Rotate towards the window
		Quaternion targetRotation = Quaternion.LookRotation((_windowTransform.position - _beeMovement.transform.position).normalized);
		yield return StartCoroutine(SmoothRotationCoroutine(targetRotation, 0.25f));
	}
	
	private	IEnumerator SmoothRotationCoroutine (Quaternion targetRotation, float duration)
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

	public void InterruptEvent()
	{
		_beePlayAnimation.SetBoolParameter(WAVE_ANIMATION_PARAMETER, false);
		StopAllCoroutines();
		_sheepPlayAnimation.SetBoolParameter(SHEEP_POPS_OUT_ANIMATION_PARAMETER,false);
		// close the window
		StartCoroutine(CloseWindow(true));
		Bee.Instance.UpdateState(BeeState.Idle);
		OnInterruptedDone?.Invoke(this);
	}

	public void StopEvent()
	{
		HandleEventDone();
		_beePlayAnimation.SetBoolParameter(WAVE_ANIMATION_PARAMETER, false);
	}

	private void OnDisable()
	{
		StopEvent();
	}
}
