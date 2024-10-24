using System;
using UnityEngine;

public class Hide : MonoBehaviour
{
	[SerializeField] private Vector3 _hideOffset;
	[SerializeField] private Movement _movement;
	private Transform _hideSpot;
	private bool _isMovingToHidingSpot = false;
    
	public static event Action OnHidden;

    void Start()
    {
		SubscribeToEvents();
    }

	void FixedUpdate()
	{
		if (CannotHide()) return;

		MoveTowardsHidingSpot();
		if (IsHidden())
			HideGameObject();
	}

	private bool CannotHide() => _hideSpot == null || !_isMovingToHidingSpot;

	private void HandleHideStart(UpdatePassiveEventCollection eventMetadata) {
		if(eventMetadata.CurrentEvent != PassiveEvent.HideAndSea) return;

		_hideSpot = eventMetadata.Metadata.Target;
		_isMovingToHidingSpot = true;
	}

	private void HandleHideEnd(UpdatePassiveEventCollection eventMetadata) {
		if(eventMetadata.PreviousEvent != PassiveEvent.HideAndSea) return;

		_hideSpot = null;
	}

	private void MoveTowardsHidingSpot() {
		transform.position = Vector3.Lerp(transform.position, _hideSpot.position + _hideOffset, _movement.MovementSpeed * Time.deltaTime);
	}

	private bool IsHidden() => Vector3.Distance(transform.position, _hideSpot.position + _hideOffset) < 0.1f;

	private void HideGameObject() {
		_isMovingToHidingSpot = false;
		OnHidden?.Invoke();
	}

	private void SubscribeToEvents() {
		PlotEvent.OnPassiveEventStart += HandleHideStart;
		PlotEvent.OnPasiveEventEnd += HandleHideEnd;
	}

	private void UnsubscribeFromEvents() {
		PlotEvent.OnPassiveEventStart -= HandleHideStart;
		PlotEvent.OnPasiveEventEnd -= HandleHideEnd;
	}

	private void OnDestroy() {
		UnsubscribeFromEvents();
	}
}
