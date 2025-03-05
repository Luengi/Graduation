using System;
using UnityEngine;

[RequireComponent(typeof(RaycastManager)), ]
public class InteractionManager : MonoBehaviour
{
	private RaycastManager _raycastManager;

	public event Action<EventInterruption> OnInteractionReadyToStart;
	
	void Awake() 
	{
		_raycastManager = GetComponent<RaycastManager>();
	}

    void Start() 
	{
		_raycastManager.OnRaycastHit += OnRaycastHit;	
    }

	private void OnRaycastHit (Collider collider) 
	{
		IInteractable interactable = collider.GetComponent<IInteractable>();
		
		if (interactable == null) return;
		
		if (interactable.CanInterrupt) 
		{
			EventInterruption eventInterruption = new EventInterruption(collider.gameObject, EventType.Active);
			OnInteractionReadyToStart?.Invoke(eventInterruption);
			return;
		} 
		
		interactable.Interact();
	}

	void OnDestroy() 
	{
		_raycastManager.OnRaycastHit -= OnRaycastHit;
	}
}
