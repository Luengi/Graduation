using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagePlotTutorial : MonoBehaviour
{
	[SerializeField] private RaycastManager _raycastManager;
	[SerializeField] private GameObject _tapIconsContainer;
	[SerializeField] private float _secondsToForceEndTutorial;
	
	private List<GameObject> _tapIconsList;
	
	private void OnEnable()
	{
		_raycastManager.OnRaycastHit += OnInteract;
		
		BeginTutorial();
	}
	
	private void OnDisable()
	{
		_raycastManager.OnRaycastHit -= OnInteract;
	}
	
	private void BeginTutorial()
	{
		SetupTapIcons();
		StartCoroutine(ForceEndTutorialTimer(_secondsToForceEndTutorial));
	}
	
	private IEnumerator ForceEndTutorialTimer (float secondsToWait)
	{
		yield return new WaitForSeconds(secondsToWait);
		EndTutorial();
	}
	
	private void EndTutorial()
	{
		StopAllCoroutines();
		DisableTapIcons();
		gameObject.SetActive(false);
	}
	
	private void OnInteract (Collider collider)
	{
		EndTutorial();
	}
	
	private void SetupTapIcons()
	{
		if (_tapIconsContainer == null)
		{
			Debug.LogError("Tap Icons container was not passed to PlotTutorial");
			return;
		}

		_tapIconsList = new List<GameObject>();
		foreach (Transform child in _tapIconsContainer.transform)
		{
			_tapIconsList.Add(child.gameObject);
		}
		
		EnableTapIcons();
	}
	
	private void EnableTapIcons()
	{
		SetObjectsStateInList(_tapIconsList, true);
	}
	
	private void DisableTapIcons()
	{
		SetObjectsStateInList(_tapIconsList, false);
	}
	
	private void SetObjectsStateInList (List<GameObject> list, bool toggleState)
	{
		if (list.Count == 0) return;

		foreach (GameObject icon in list)
		{
			icon.SetActive(toggleState);
		}
	}
}
