using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SoundComponent))]
public class SheepAmbience : MonoBehaviour
{
	[SerializeField] private Range _playSheepSoundRangeTime;
	[SerializeField] private Sound _onceSheepBeehSFX;
	
	private SoundComponent _soundComponent;
	
	private void Awake()
	{
		_soundComponent = GetComponent<SoundComponent>();
		
		StartCoroutine(SheepBeehCoroutine(_playSheepSoundRangeTime.GetRandomValueWithinRange()));
	}
	
	private IEnumerator SheepBeehCoroutine (float secondsToWait)
	{
		yield return new WaitForSeconds(secondsToWait);
		_soundComponent.PlaySound(_onceSheepBeehSFX);
		StartCoroutine(SheepBeehCoroutine(_playSheepSoundRangeTime.GetRandomValueWithinRange()));
	}
}
