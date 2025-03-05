using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayAnimation)), RequireComponent(typeof(BoxCollider)), RequireComponent(typeof(PlayParticle)), RequireComponent(typeof(SoundComponent)), ]
public class MushroomInteraction : MonoBehaviour, IInteractable
{
	[Header("Animation")]
	[Tooltip("The name of the animation parameter that triggers the mushroom animation")]
	[SerializeField] private string _mushroomAnimationParameterName;

	[Tooltip("The name of the animation state that contains the mushroom animation")]
	[SerializeField] private string _mushroomAnimationStateName;

	[Header("Sound")]
	[Tooltip("The sound that will be played when the mushroom is interacted with")]
	[SerializeField] private Sound _sound;

	private bool _isPlaying;
	private PlayAnimation _playAnimation;
	private PlayParticle _playParticle;
	private SoundComponent _soundComponent;

	public bool CanInterrupt {get; set;}
	public bool MultipleInteractions {get; set;}

	void Awake() 
	{
		_playAnimation = GetComponent<PlayAnimation>();
		_playParticle = GetComponent<PlayParticle>();
		_soundComponent = GetComponent<SoundComponent>();
	}

	void Start()
    {
        CanInterrupt = false;
		MultipleInteractions = false;
    }

	public void Interact()
	{
		if (_isPlaying) return;

		_isPlaying = true;
		StartCoroutine(MushroomAnimation());
	}

	private IEnumerator MushroomAnimation()
	{
		yield return StartCoroutine(EnableMushroomAnimation());
		_soundComponent.PlaySound(_sound);
		_playParticle.ToggleOn();
		yield return StartCoroutine(_playAnimation.WaitForAnimationToEnd());
		_playAnimation.SetBoolParameter(_mushroomAnimationParameterName, false);

		_isPlaying = false;
	}

	private IEnumerator EnableMushroomAnimation() 
	{
		_playAnimation.SetBoolParameter(_mushroomAnimationParameterName, true);
		yield return StartCoroutine(_playAnimation.WaitForAnimationToStart(_mushroomAnimationStateName));
	}

	private void ResetInteraction() 
	{
		_isPlaying = false;
		_playAnimation.SetBoolParameter(_mushroomAnimationParameterName, false);
		_playParticle.ToggleOff();
		_soundComponent.StopSound();

		StopAllCoroutines();
	}

	private void OnDisable()
	{
		ResetInteraction();
	}
}
