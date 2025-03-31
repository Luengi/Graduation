using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class House : MonoBehaviour, IInspectable
{
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private GameObject _inspectablePoint;
	
	private Coroutine _inspectCoroutine;

	public GameObject InspectablePoint {get => _inspectablePoint;}

	public event Action OnInspected;

	public void Inspect()
	{
		_inspectCoroutine = StartCoroutine(InspectHouse());
	}

	private IEnumerator InspectHouse() 
	{
		_audioSource.Play();
		yield return new WaitForSeconds(_audioSource.clip.length);

		OnInspected?.Invoke();
	}

	public void StopInspecting()
	{
		if (_inspectCoroutine != null) 
		{
			StopCoroutine(_inspectCoroutine);
		}
	}
}
