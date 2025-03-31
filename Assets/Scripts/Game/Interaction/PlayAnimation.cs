using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(Animator))]
public class PlayAnimation : MonoBehaviour
{
	private Animator _animator;

	private void Awake() 
	{
		_animator = GetComponent<Animator>();
	}

	public void Play (string animationName) 
	{
		if (_animator != null) _animator.Play(animationName);
	}

	public void ToggleBoolParameter (string parameterName) 
	{
		if (_animator != null) _animator.SetBool(parameterName, !_animator.GetBool(parameterName));
	}

	public void SetBoolParameter (string parameterName, bool value) 
	{
		if (_animator != null) _animator.SetBool(parameterName, value);
	}

	public void SetTrigger (string triggerName)
	{
		if (_animator != null) _animator.SetTrigger(triggerName);
	}

	public bool IsPlaying() => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
	
	public bool IsAnimationOver (int animationLoops = 1) => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= animationLoops;

	public bool CurrentAnimationState (string animationStateName) => _animator.GetCurrentAnimatorStateInfo(0).IsName(animationStateName);
	
	public IEnumerator WaitForAnimationToStart (string animationName) 
	{
		while (!CurrentAnimationState(animationName)) 
		{
			yield return null;
		}
	}

	public IEnumerator WaitForAnimationToEnd (int animationLoops = 1) 
	{
		while (!IsAnimationOver(animationLoops)) 
		{
			yield return null;
		}
	}
	
	public void Stop() 
	{
		if (_animator != null) _animator.StopPlayback();
	}
}
