using UnityEngine;

public class FollowObject : MonoBehaviour
{
	[SerializeField] protected internal FollowConfiguration _followConfiguration;

	private void Awake() 
	{
		if (_followConfiguration == null) Debug.LogWarning("FollowObject: FollowConfiguration is not set");
		if (_followConfiguration != null && _followConfiguration.Target == null) Debug.LogWarning("FollowObject: Target is not set");
	}

	internal void LateUpdate()
	{
		if (_followConfiguration == null || _followConfiguration.Target == null) return;
		// Calling this in LateUpdate to make sure the target has moved before we move the object that follows
		Follow();
	}

	protected internal virtual void Follow() 
	{
		Vector3 targetPosition = _followConfiguration.Target.position + _followConfiguration.Offset;
		float scaledSpeed = _followConfiguration.Speed * Time.deltaTime;
		transform.position = Vector3.Lerp(transform.position, targetPosition, scaledSpeed);
	}
}
