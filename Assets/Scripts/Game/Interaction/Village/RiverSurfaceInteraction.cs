using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverSurfaceFishInteraction : MonoBehaviour, IEvent, IInteractable
{
	[SerializeField] GameObject _fishPrefab;
	[SerializeField] RaycastManager _raycastManager;
	[SerializeField] float _spawnFishRandomAngleAmount = 15f;
	[SerializeField] int _maxSequencePlaysAmount = 3;
	[SerializeField] float _sphereCastRadius = 5f;
	[SerializeField] float _extendWaypointForwardDistance = 3f;
	[SerializeField] float _hitpointYOffset = 1f;
	
	public bool CanInterrupt {get;  set;} = true;
	public bool MultipleInteractions {get; set;} = false;
	public EventState State {get; set;} = EventState.None;
	public event Action OnEventDone;

	// The location in the river where the last action took place (Either spawned fish OR raycast hitpoint)
	private Vector3 _lastRiverActionLocation;
	private int _sequencePlayedAmount = 0;
	private RiverFish _spawnedRiverFish;

	public void Interact()
	{
		if (State == EventState.Active) return;
		
		State = EventState.Active;
		
		_lastRiverActionLocation = _raycastManager.HitPoint;
		_lastRiverActionLocation.y -= _hitpointYOffset;
		DoSpawnFishSequence();
	}
	
	private void DoSpawnFishSequence()
	{
		List<RiverWaypoint> nearbyWaypoints = GetNearbyWaypoints(_lastRiverActionLocation);
		if (nearbyWaypoints.Count == 0)
		{
			HandleEventDone();
			return;
		}
		
		RiverWaypoint targetWaypoint = GetFurthestDownstreamWaypoint(nearbyWaypoints);
		SpawnFish(targetWaypoint);
	}

	private void SpawnFish (RiverWaypoint targetWaypoint)
	{	
		// First fish gets rotates towards the target waypoint in order to stay within the river
		Quaternion rotation = _sequencePlayedAmount == 0 ? GetRotationToWaypoint(targetWaypoint) : GetRandomRotationFromWaypoint(targetWaypoint);

		GameObject spawnedObject = Instantiate(_fishPrefab, _lastRiverActionLocation, rotation);
		_spawnedRiverFish = spawnedObject.GetComponent<RiverFish>();
		SubscribeToSpawnedFish();

		_sequencePlayedAmount++;
	}
	
	
	private void HandleFishAnimationDone (RiverFish riverFish)
	{	
		if (_sequencePlayedAmount >= _maxSequencePlaysAmount) // Interaction done - reset
		{		
			HandleEventDone();
			return;
		}
		
		_lastRiverActionLocation = riverFish.transform.position;
		Destroy(_spawnedRiverFish.gameObject);
		DoSpawnFishSequence();
	}

	private List<RiverWaypoint> GetNearbyWaypoints (Vector3 point)
	{
		RaycastHit[] hitArray = GetSphereCastCollisions(point);
		List<RiverWaypoint> waypoints = new List<RiverWaypoint>();
		
		for (int i = 0; i < hitArray.Length; i++)
		{
			if (hitArray[i].collider.gameObject.TryGetComponent<RiverWaypoint>(out RiverWaypoint waypoint))
			{
				waypoints.Add(waypoint);
			}
		}

		return waypoints;
	}
	
	private RiverWaypoint GetFurthestDownstreamWaypoint (List<RiverWaypoint> waypoints)
	{
		float longestDistance = -1;
		RiverWaypoint targetWaypoint = null;

		for (int i = 0; i < waypoints.Count; i++)
		{
			float currentDistance = waypoints[i].GetExtendedForwardDistanceToPoint(_lastRiverActionLocation, _extendWaypointForwardDistance);
			if (currentDistance >= longestDistance)
			{
				longestDistance = currentDistance;
				targetWaypoint = waypoints[i];
			}
		}

		return targetWaypoint;
	}
		
	private RaycastHit[] GetSphereCastCollisions (Vector3 point)
	{
		return Physics.SphereCastAll(point, _sphereCastRadius, Vector3.forward, 
		_sphereCastRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);
	}
	
	private Quaternion GetRotationToWaypoint (RiverWaypoint waypoint)
	{
		Vector3 direction = (waypoint.transform.position - _lastRiverActionLocation).normalized;
		direction.y = _lastRiverActionLocation.y;
		return Quaternion.LookRotation(direction);
	}
	
	private Quaternion GetRandomRotationFromWaypoint (RiverWaypoint waypoint)
	{
		Vector3 randomRotationVector = new Vector3(0, GetNewRandomHorizontalDirectionDegrees(_spawnFishRandomAngleAmount), 0);
		return Quaternion.Euler(waypoint.transform.eulerAngles + randomRotationVector);
	}
	
	private float GetNewRandomHorizontalDirectionDegrees (float newAngleAmount)
	{
		return UnityEngine.Random.Range(-newAngleAmount, newAngleAmount);
	}
	
	private void HandleEventDone()
	{
		if (_spawnedRiverFish != null)
		{
			UnsubscriveFromSpawnedFish();
			Destroy(_spawnedRiverFish.gameObject);
		}
		_sequencePlayedAmount = 0;
		State = EventState.None;
		OnEventDone?.Invoke();
	}
	
	private void SubscribeToSpawnedFish()
	{
		_spawnedRiverFish.OnAnimationFinished += HandleFishAnimationDone;
	}
	
	private void UnsubscriveFromSpawnedFish()
	{
		_spawnedRiverFish.OnAnimationFinished -= HandleFishAnimationDone;
	}

	public void StopEvent()
	{
		OnEventDone?.Invoke();
	}	
}
