using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayAnimation))]
public class Sheep : MonoBehaviour
{	
	[SerializeField] private Sound _onceSheepJumpSFX;
	
	private SoundComponent _soundComponent;
	
	private void Awake()
	{
		_soundComponent = GetComponent<SoundComponent>();
	}
	
	public void Jump (float jumpForce,float jumpDuration)
	{
		_soundComponent.PlaySound(_onceSheepJumpSFX);
		StartCoroutine(JumpEnumerator(jumpForce, jumpDuration));
	}

	private IEnumerator JumpEnumerator (float jumpForce, float jumpDuration)
	{
		Vector3 startPosition = transform.position;
		Vector3 peakPosition = startPosition + Vector3.up * jumpForce;

		float elapsedTime = 0f;
		
		while (elapsedTime < jumpDuration / 2f)
		{
			transform.position = Vector3.Lerp(startPosition, peakPosition, (elapsedTime / (jumpDuration / 2f)));
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		
		transform.position = peakPosition;

		elapsedTime = 0f;
		
		while (elapsedTime < jumpDuration / 2f)
		{
			transform.position = Vector3.Lerp(peakPosition, startPosition, (elapsedTime / (jumpDuration / 2f)));
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		
		transform.position = startPosition;
	}
}
