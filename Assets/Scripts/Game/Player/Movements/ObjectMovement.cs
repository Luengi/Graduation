using System;
using System.Collections;
using UnityEngine;

public class ObjectMovement : MonoBehaviour
{
	[SerializeField] private bool _allowAvoidObject;
	
	private AvoidObstacle _avoidObstacle;
	private Vector3[] _directionsToCheck;
	private Vector3 _currentVelocity;
	private Vector3 _previousPosition;

	private readonly int _directionCount = 20;
	private readonly float _obstacleDistance = .5f;
	
	private const float SMOOTHNESS = 0.1f;
	private const float DISTANCE_TOLERANCE = 0.2f;

	public bool AllowAvoidObject
	{
		get => _allowAvoidObject;
		set => _allowAvoidObject = value;
	}
	
	private void Awake()
	{
		if (_allowAvoidObject) _avoidObstacle = gameObject.AddComponent<AvoidObstacle>();
	}

	private void Start()
	{
		if (_allowAvoidObject) _directionsToCheck = _avoidObstacle.CalculateDirections(_directionCount);
	}

	public void UpdatePosition (Vector3 position) 
	{
		transform.position = position;
	}

	public void MoveTo (Vector3 position, float speed, float tolerance = DISTANCE_TOLERANCE , float smoothness = SMOOTHNESS)
	{
		Vector3 moveVector = CalculateMovement(position, speed);
		SmootherAndApplyMovement(speed, moveVector, smoothness);
	}

	public void MoveAroundPivot (Vector3 position, Vector3 axis,  float distance, float rotateSpeed, float moveSpeed)
	{ 
		Vector3 direction = (transform.position - position).normalized;

		direction = Vector3.ProjectOnPlane(direction, axis);
		
		Vector3 target = position + direction * distance;

		if (!IsInPlace(target))
		{
			MoveTo(target, moveSpeed);
		}
		else
		{
			transform.RotateAround(position,axis,rotateSpeed * Time.deltaTime);
			
			direction = (transform.position - position).normalized;
			
			direction = Vector3.ProjectOnPlane(direction, axis);
			
			target = position + direction * distance;
			
			transform.position = target;
			FaceDirection(rotateSpeed/10);
		}
	}

	private void FaceDirection (float rotateSpeed)
	{
		Vector3 movementDirection = (transform.position - _previousPosition).normalized;
		
		if (movementDirection != Vector3.zero)
			transform.forward = Vector3.Lerp(transform.forward,movementDirection,rotateSpeed * Time.deltaTime);
		
		_previousPosition = transform.position;
	}
	
	private Vector3 CalculateMovement (Vector3 position, float speed)
	{
		Vector3 targetVector = position - transform.position;
		Vector3 avoidVector = Vector3.zero;

		if (_allowAvoidObject) 
			avoidVector = _avoidObstacle.CalculateObstacleAvoidance(speed, transform, _obstacleDistance, _directionsToCheck);

		Vector3 moveVector = targetVector + avoidVector;
		
		return moveVector;
	}

	private void SmootherAndApplyMovement (float speed, Vector3 moveVector , float smoothTime)
	{
		moveVector = Vector3.SmoothDamp(transform.forward, moveVector, ref _currentVelocity, smoothTime);
		moveVector = moveVector.normalized * speed;

		transform.forward = moveVector;

		transform.position += moveVector * Time.deltaTime;
	}

	public void SnapRotationTowards (Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;
    }

	public IEnumerator MoveUntilObjectReached (Vector3 targetPosition, float speed, float tolerance = DISTANCE_TOLERANCE) 
	{
		while (!IsInPlace(targetPosition, tolerance)) 
		{
			MoveTo(targetPosition, speed);
			yield return null;
		}
	}

	public IEnumerator RotateUntilLookAt (Vector3 targetPosition, float speed) 
	{
		while (!IsLookingAt(targetPosition)) 
		{
			SmoothRotate(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), speed);
			yield return null;
		}
	}

	private bool IsLookingAt (Vector3 targetPosition)
	{
		Vector3 direction = (targetPosition - transform.position).normalized;
		Quaternion targetRotation = Quaternion.LookRotation(direction);
		return Quaternion.Angle(transform.rotation, targetRotation) < 1f;
	}

	public void SmoothRotate (Quaternion startRotation, Quaternion targetRotation, float rotationSpeed)
    {
        transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationSpeed);
    }

    public bool IsInPlace (Vector3 position, float tolerance = DISTANCE_TOLERANCE) => ApproximatelyInPlace(position, tolerance);

	public bool ApproximatelyInPlace (Vector3 position, float tolerance) => Vector3.Distance(transform.position, position) <= tolerance;
}
