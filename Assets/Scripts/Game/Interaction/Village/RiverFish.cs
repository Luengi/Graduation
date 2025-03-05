using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayAnimation))]
[RequireComponent(typeof(SoundComponent))]
public class RiverFish : MonoBehaviour
{
	[SerializeField] float _moveSpeed;
	[SerializeField] Sound _fishSplashOutSFX;
	[SerializeField] Sound _fishSplashInSFX;

	private PlayAnimation _playAnimation;
	private SoundComponent _soundComponent;
	

	private const string ANIMATION_JUMP_PARAMETER = "Jump";
	private const string ANIMATION_JUMP_STATE = "Jumping";
	private const string ANIMATION_IDLE_STATE = "InitialState";
	public event Action<RiverFish> OnAnimationFinished;
	
	private void Awake()
	{
		_playAnimation = GetComponent<PlayAnimation>();
		_soundComponent = GetComponent<SoundComponent>();
	}
	
	private void Start()
	{
		PlayAnimation();
	}

	private void Update()
	{
		Move();
	}

	private void Move()
	{
		transform.position += transform.forward * _moveSpeed * Time.deltaTime;
	}
	
	private void HandleAnimationDone()
	{
		OnAnimationFinished?.Invoke(this); 
	}

	private void PlayAnimation()
	{
		StartCoroutine(PlayAnimationCoroutine());
	}
	
	private IEnumerator PlayAnimationCoroutine()
	{
		_playAnimation.SetTrigger(ANIMATION_JUMP_PARAMETER);

		yield return _playAnimation.WaitForAnimationToEnd();

		HandleAnimationDone();
	}
	
	private void FishJumpAnimationEvent()
	{
		_soundComponent.PlaySound(_fishSplashOutSFX);
	}
	
	private void FishFallAnimationEvent()
	{
		_soundComponent.PlaySound(_fishSplashInSFX);
	}
}
