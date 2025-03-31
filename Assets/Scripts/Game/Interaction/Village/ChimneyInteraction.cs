using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SoundComponent)), RequireComponent(typeof(PlayParticle)), RequireComponent(typeof(PlayAnimation)), RequireComponent(typeof(BoxCollider)), ]
public class ChimneyInteraction : MonoBehaviour, IInteractable
{
	private const string HOUSE_ANIMATION_NAME = "tapAnimationHouse";
	[SerializeField] private string _houseAnimationParameterName = "tapAnimationHouse";
	[SerializeField] private float _particleDuration = 3f;
	[SerializeField] private Sound _onceTapHouseSFX;
	[SerializeField] private Sound _onceChimneySmokeSFX;
	[SerializeField] private Sound _onceHouseWobbleSFX;

	private PlayParticle _playParticle;
	private PlayAnimation _playAnimation;
	private SoundComponent _soundComponent;
	private bool _isPlaying;

	public bool CanInterrupt {get; set;}
	public bool MultipleInteractions {get; set;}

	private void Awake()
	{
		_playParticle = GetComponent<PlayParticle>();
		_playAnimation = GetComponent<PlayAnimation>();
		_soundComponent = GetComponent<SoundComponent>();
	}
	private void Start()
	{
		CanInterrupt = false;
		MultipleInteractions = false;
	}

	public void Interact()
	{
		if (_isPlaying) return;
		
		_soundComponent.PlaySound(_onceTapHouseSFX);
		_soundComponent.PlaySound(_onceHouseWobbleSFX);
		StartCoroutine(PlayChimneyAnimation());
	}

	private IEnumerator HouseAnimation()
	{
		_playAnimation.SetBoolParameter(_houseAnimationParameterName, true);
		yield return StartCoroutine(_playAnimation.WaitForAnimationToStart(HOUSE_ANIMATION_NAME));
		yield return StartCoroutine(_playAnimation.WaitForAnimationToEnd());
	}

	private IEnumerator PlayChimneyAnimation()
	{
		_isPlaying = true;
		_playParticle.ToggleOn();
		_soundComponent.PlaySound(_onceChimneySmokeSFX);
		yield return StartCoroutine(HouseAnimation());
		// yield return StartCoroutine(ExpandChimney());

		_playAnimation.SetBoolParameter(_houseAnimationParameterName, false);
		yield return new WaitForSeconds(_particleDuration);
		_playParticle.ToggleOff();
		_isPlaying = false;
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		_playParticle.ToggleOff();
		_isPlaying = false;
	}
}
