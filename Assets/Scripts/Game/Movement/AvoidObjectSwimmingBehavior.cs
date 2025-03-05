using UnityEngine;

[RequireComponent(typeof(AvoidObstacle))]
public class AvoidObjectSwimmingBehavior : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Number of directions to check for obstacles.")]
    [SerializeField] private int _viewDirectionCount = 5;
    
    [Tooltip("Smooth damp time for movement vector adjustment.")]
    [SerializeField] private float _movementSmoothDamp = 0.8f;
    
    [Tooltip("Time interval (in seconds) before changing the movement direction.")]
    [SerializeField] private float _directionResetInterval = 1f;
    
    [Space]
    
    [Tooltip("Distance at which obstacles will be detected.")]
    [SerializeField] private float _obstacleDistance = 1f;
    
    [Tooltip("Weight applied to the obstacle avoidance vector.")]
    [SerializeField] private float _obstacleWeight = 40f;
    
    [Header("Bounds Settings")]
    [Tooltip("Center point of the bounding area.")]
    [SerializeField] private Transform _boundCenter;
    
    [Tooltip("Dimensions of the bounding area.")]
    [SerializeField] private Vector3 _bound = new Vector3(2,2,2);

    private Vector3[] _directionsToCheck;
    private Vector3 _currentVelocity;
    private Transform _myTransform;
    private Vector3 _moveVector;
    private float _changeDirectionTimer;

    private AvoidObstacle _avoidObstacle;

    private void Awake()
    {
        _avoidObstacle = GetComponent<AvoidObstacle>();
    }

    private void Start()
    {
        _myTransform = transform;
        _directionsToCheck = _avoidObstacle.CalculateDirections(_viewDirectionCount);
    }

    public void Move (float speed)
    {
        if (IsOutOfBounds(out Vector3 boundReturnVector))
        {
            _moveVector = AdjustToBounds(boundReturnVector, speed);
        }
        else
        {
            _moveVector = CalculateMovement(speed);
        }

        SmoothAndApplyMovement(speed);
    }

    public void SetBoundCenter(Transform boundTransform)
    {
        _boundCenter = boundTransform;
    }

    private bool IsOutOfBounds(out Vector3 boundReturnVector)
    {
        boundReturnVector = CalculateBoundReturnVector();
        return boundReturnVector != Vector3.zero;
    }

    private Vector3 AdjustToBounds (Vector3 boundReturnVector, float speed)
    {
        return boundReturnVector.normalized * speed;
    }

    private Vector3 CalculateMovement (float speed)
    {
        UpdateChangeDirectionTimer();

        Vector3 randomDirection = GetRandomDirectionOnInterval();
        Vector3 obstacleAvoidVector = _avoidObstacle.CalculateObstacleAvoidance(speed, _myTransform, _obstacleDistance, _directionsToCheck);

        return randomDirection + obstacleAvoidVector * _obstacleWeight;
    }

    private void UpdateChangeDirectionTimer()
    {
        _changeDirectionTimer -= Time.deltaTime;
    }

    private Vector3 GetRandomDirectionOnInterval()
    {
        return _changeDirectionTimer < 0 ? GetRandomDirection() : Vector3.zero;
    }

    private void SmoothAndApplyMovement (float speed)
    {
        _moveVector = Vector3.SmoothDamp(_myTransform.forward, _moveVector, ref _currentVelocity, _movementSmoothDamp);
        _moveVector = _moveVector.normalized * speed;

        _myTransform.forward = _moveVector;
        _myTransform.position += _moveVector * Time.deltaTime;
    }


    private Vector3 GetRandomDirection()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        _changeDirectionTimer = _directionResetInterval;
        return randomDirection;
    }

    private Vector3 CalculateBoundReturnVector()
    {
        Vector3 position = _myTransform.position;
        Vector3 boundCenterPosition = _boundCenter.position;
        
        Vector3 boundMin = boundCenterPosition - _bound / 2;
        Vector3 boundMax = boundCenterPosition + _bound / 2;

        bool isOutOfBounds = position.x < boundMin.x || position.x > boundMax.x ||
                             position.y < boundMin.y || position.y > boundMax.y ||
                             position.z < boundMin.z || position.z > boundMax.z;

        return isOutOfBounds ? _boundCenter.position - position : Vector3.zero;
    }
}
