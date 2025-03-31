using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayAnimation)), RequireComponent(typeof(PlayParticle)), RequireComponent(typeof(SoundComponent)), ]
public class ClambInteraction : MonoBehaviour, IInteractable, IEvent, IToggleComponent
{
	private const string INITIAL_ANIMATION_PARAMETER_NAME = "HasStarted";
	private const string OPEN_ANIMATION_STATE = "clam_open_Animation";
	private const string CLOSE_ANIMATION_STATE = "clam_close_Animation";

	[SerializeField] private string _clamAnimationToggleParameterName;
	[SerializeField] private float _scaleIncreaseRate = 1.5f;
	[SerializeField] private Sound _tapSFX;
	[SerializeField] private Sound _onceClamOpenSFX;
	[SerializeField] private Sound _onceClamCloseSFX;
	[SerializeField] private Sound _oncePearlSparkOnOpenSFX;

	public bool CanInterrupt {get; set;}
	public bool MultipleInteractions {get; set;}
	public EventState State {get; set;}
	public ToggleState CurrentToggleState {get; set;}
	public ToggleState NextToggleState {get; set;}

	internal PlayAnimation _playAnimation;
	internal PlayParticle _playParticle;
	internal bool _hasStartedAnimation;

	private SoundComponent _soundComponent;
	
#pragma warning disable CS0067
	public event Action OnEventDone;
#pragma warning restore CS0067
	public event Action OnToggleDone;

	private BoxCollider _collider;
	
	private void Awake()
	{
		_collider = GetComponent<BoxCollider>();
		_playAnimation = GetComponent<PlayAnimation>();
		_playParticle = GetComponent<PlayParticle>();
		_soundComponent = GetComponent<SoundComponent>();
	}

	private void Start()
	{
		Setup();
	}

	private void Setup()
	{
		CanInterrupt = false;
		MultipleInteractions = false;
		CurrentToggleState = ToggleState.Off;
	}

	public void Interact()
	{
		if (!_hasStartedAnimation) 
		{
			_hasStartedAnimation = true;
			_playAnimation.SetBoolParameter(INITIAL_ANIMATION_PARAMETER_NAME, true);
		}
		
		if (CurrentToggleState == ToggleState.Switching) return;

		Toggle();
	}

	public void ToggleOn()
	{
		StartCoroutine(OpenClam());
	}

	public void ToggleOff()
	{
		StartCoroutine(CloseClam());
	}

	private IEnumerator OpenClam() 
	{
		UpdateColliderScale(_scaleIncreaseRate);
		_soundComponent.PlaySound(_tapSFX);
		_soundComponent.PlaySound(_onceClamOpenSFX);
		SetOpenAnimationState();
		yield return StartCoroutine(WaitForAnimationStateToPlay(OPEN_ANIMATION_STATE));
		_playParticle.ToggleOn();
		UpdateState(ToggleState.On);
		
		_soundComponent.PlaySound(_oncePearlSparkOnOpenSFX);
		OnToggleDone?.Invoke();
	}

	private void SetOpenAnimationState()
	{
		_playAnimation.SetBoolParameter(_clamAnimationToggleParameterName, true);
	}

	private void SetCloseAnimationState()
	{
		_playAnimation.SetBoolParameter(_clamAnimationToggleParameterName, false);
	}

	private IEnumerator CloseClam()
	{
		UpdateColliderScale(-_scaleIncreaseRate);
		_soundComponent.PlaySound(_onceClamCloseSFX);
		_playParticle.ToggleOff();
		SetCloseAnimationState();
		yield return StartCoroutine(WaitForAnimationStateToPlay(CLOSE_ANIMATION_STATE));
		UpdateState(ToggleState.Off);
		OnToggleDone?.Invoke();
	}

	private IEnumerator WaitForAnimationStateToPlay (string state)
	{
		yield return StartCoroutine(_playAnimation.WaitForAnimationToStart(state));
		yield return StartCoroutine(_playAnimation.WaitForAnimationToEnd());
	}

	private void UpdateState (ToggleState state)
	{
		CurrentToggleState = state;
	}

	public void Toggle()
	{
		switch (CurrentToggleState)
		{
			case ToggleState.Off:
				UpdateState(ToggleState.Switching);
				ToggleOn();
				break;
			case ToggleState.On:
				UpdateState(ToggleState.Switching);
				ToggleOff();
				break;
		}
	}

	public void StopEvent()
	{
		StopAllCoroutines();
	}

	private void UpdateColliderScale (float increaseRate)
	{
		if (_collider == null) return;
		_collider.center += new Vector3(0, increaseRate/2, 0);
		_collider.size += new Vector3(0, increaseRate, 0);
	}

	private void OnDisable()
	{
		StopEvent();
	}
}
