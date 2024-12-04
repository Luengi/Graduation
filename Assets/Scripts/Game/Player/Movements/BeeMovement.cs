using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[
	RequireComponent(typeof(SwimmingBehaviour)),
	RequireComponent(typeof(PlayAnimation))
]
public class BeeMovement : MonoBehaviour
{
    [SerializeField] internal Movement _beeMovementStat;
    [SerializeField] internal LayerMask _portalLayerMask;
	[SerializeField] private Transform _otherWorldAnchor;
    [SerializeField] private Transform _portal;
    [SerializeField] private Transform _target;

	[Header("Animation")]
	[Tooltip("The name of the animation parameter that triggers the bee swimming animation")]
	[SerializeField] private string _beeSwimmingAnimationParameterName;
	[Tooltip("The name of the animation state that contains the bee swimming animation")]
	[SerializeField] private string _beeSwimmingAnimationStateName;

    private bool _overPortal;
    private bool _castRay;
    private RaycastHit _targetRaycastHit;
    private GameObject _hitPointObject;
	private Coroutine _portalMovementCoroutine;

	private PlayAnimation _playAnimation;
    private SwimmingBehaviour _swimmingBehaviour;

    public static event Action OnBeeEnteredPlot;

    private void Awake()
    {
        _swimmingBehaviour = GetComponent<SwimmingBehaviour>();
		_playAnimation = GetComponent<PlayAnimation>();
    }

    private void Start()
    {
        SetUp();
        SubscribeToEvents();
    }

    private void Update()
    {
        Move();
    }

    //TODO: Adjust implementation when handling movement in village plot - see how it goes because
    // the same movement could be used for all 3 plots in my opinion (Orestis). Up to discussion
    private void Move()
    {
		// This is for now until the movement for the village plot is implemented. 
		// Currently the movement is just made for the ocean plot. We should check the Plot and based on that decided the animation, etc.
		if(PlotsManager.Instance._currentPlot != Plot.Ocean) {
			// If the bee is not in the ocean plot, we should stop the swimming animation
			if(_playAnimation.CurrentAnimationState(_beeSwimmingAnimationStateName)) {
				_playAnimation.SetBoolParameter(_beeSwimmingAnimationParameterName, false);
			}

			return;
		}
		if(Bee.Instance.State != BeeState.Idle) return;

		// Playing bee swimming animation if it is not already playing
		if(!_playAnimation.CurrentAnimationState(_beeSwimmingAnimationStateName)) {
			_playAnimation.SetBoolParameter(_beeSwimmingAnimationParameterName, true);
		};

        _swimmingBehaviour.Move(_beeMovementStat.MovementSpeed);
    }

    private void SetUp()
    {
        _targetRaycastHit = default;
        _hitPointObject = new GameObject("RaycastHitforPortal");
    }
    
    /// <summary>
    /// Moves the object towards a target position with an offset, checking if the target is reached,
    /// and triggers the movement end event when the target is reached.
    /// </summary>
    /// <param name="targetPosition">The position the object is moving towards.</param>
    /// <param name="targetOffset">The offset added to the target position to modify the movement target.</param>
    public void MoveTowardPosition(Vector3 targetPosition, Vector3 targetOffset)
    {
        bool reachTarget = MathHelper.AreVectorApproximatelyEqual(transform.position, targetPosition,0.1f);

        if (!reachTarget)
        {
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 100f * Time.deltaTime);
			transform.position = transform.forward * _beeMovementStat.MovementSpeed * Time.deltaTime + transform.position;
        }
        else
			HandleDone();
    }
    
    /// <summary>
    /// Moves the object towards the target through a portal, adjusting its position based on a raycast from the portal.
    /// If the raycast hits an object, the movement is adjusted to the hit point; otherwise, it moves directly from the portal to the target.
    /// </summary>
    /// <param name="portal">The current portal's transform (starting point for the raycast).</param>
    /// <param name="anchor">The target portal's transform (destination for the movement).</param>
    /// <param name="target">The target transform to move towards.</param>
    /// <param name="targetOffset">The offset applied to the target position during movement.</param>
    public void MoveTowardPositionThroughPortal(Transform portal, Transform anchor, Transform target, Vector3 targetOffset)
    {
        if (!_castRay)
        {
            _castRay = true;
            CastingRaycastThroughPortal(portal, anchor, target, out _targetRaycastHit);
            
            if(_targetRaycastHit.collider != null)
                _hitPointObject.transform.position = _targetRaycastHit.point;
        }
        
        MoveThroughPortal(portal.position, 
                anchor.position, 
                target.position,
                targetOffset);
    }

	
    private IEnumerator MoveThroughPortal(Vector3 portalPosition, Vector3 targetPortalPos, Vector3 targetPosition, Vector3 targetOffset)
    {        
        // bool reachTarget = false;
        bool isAtPortal = false;
        
		while(!isAtPortal && !_overPortal)
		{
			isAtPortal = MathHelper.AreVectorApproximatelyEqual(transform.position, _portal.position, 0.1f);
			MoveToPortalPosition(_portal.position);
			yield return null;
		}

		EnterPortal(_otherWorldAnchor.position);

		// while(!reachTarget) {
        // 	reachTarget = MathHelper.AreVectorApproximatelyEqual(transform.position, _target.position, 0.1f);
		// 	MoveTowardPosition(_target.position, targetOffset);
		// 	yield return null;
		// }

		HandleDone();
		OnBeeEnteredPlot?.Invoke();
		Bee.Instance.UpdateState(BeeState.Idle);
		_overPortal = false;
    }

    private void MoveToPortalPosition(Vector3 portalPosition)
    {
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(portalPosition - transform.position), 100f * Time.deltaTime);
		transform.position = transform.forward * _beeMovementStat.MovementSpeed * Time.deltaTime + transform.position;
    }
    
    private void EnterPortal(Vector3 targetCameraPosition)
    {
        transform.position = targetCameraPosition + (_portal.position - Camera.main.transform.position);
        _overPortal = true;
    }

    private void CastingRaycastThroughPortal(Transform portal, Transform targetPortal, Transform target,
        out RaycastHit hit)
    {
        Vector3 simulateObjectPosition = AdjustPosition(portal, targetPortal,transform);

        int layerMask = _portalLayerMask;

        Vector3 targetPosition = target.position;
        Vector3 direction = (simulateObjectPosition - targetPosition).normalized;
        float distance = Vector3.Distance(targetPosition, simulateObjectPosition);

        Physics.Raycast(targetPosition, direction, out hit, distance, layerMask);
    }

    private Vector3 AdjustPosition(Transform portal, Transform target, Transform toAdjust)
    {
        var adjustedPositionAndRotationMatrix = target.transform.localToWorldMatrix
                                                * portal.transform.worldToLocalMatrix
                                                * toAdjust.localToWorldMatrix;

        Vector3 stimulatePosition = adjustedPositionAndRotationMatrix.GetColumn(3);
        return stimulatePosition;
    }
    
    private void HandleDone()
    {
        _targetRaycastHit = default; 
        _castRay = false;
        _overPortal = false;
    }

	private void HandleGoingToPlot(Plot plot) {
		if(Bee.Instance.State != BeeState.FollowingCamera) return;

		Bee.Instance.UpdateState(BeeState.EnteringPlot);

		if(plot == Plot.Ocean || plot == Plot.Space)
			_portalMovementCoroutine = StartCoroutine(MoveThroughPortal(_portal.position, _otherWorldAnchor.position, _target.position, Vector3.zero));
	}


    private void HandleBeeStateChange(BeeState state)
    {
        if (PlotsManager.Instance.CurrentPlot != Plot.Ocean) return;

        if (state == BeeState.Idle)
        {
			      _swimmingBehaviour.CheckBounds = true;
            _swimmingBehaviour.RestartSwimmingSequence();
            return;
        }
         _swimmingBehaviour.CheckBounds = false;
        _swimmingBehaviour.StopSwimmingSequence();
    }

    private void SubscribeToEvents()
    {
		ImageTrackingPlotActivatedResponse.OnPlotActivated += HandleGoingToPlot;
		ImageTrackingPlotUpdatedResponse.OnPlotActivated += HandleGoingToPlot;
        Bee.OnBeeStateChanged += HandleBeeStateChange;
		ImageTrackingPlotUpdatedResponse.OnPlotDeactivated += HandlePlotDeactivated;
    }

	private void HandlePlotDeactivated(Plot plot)
	{
		if(Bee.Instance.State == BeeState.FollowingCamera) return;

		if(_portalMovementCoroutine != null)
			StopCoroutine(_portalMovementCoroutine);

        _swimmingBehaviour.StopSwimmingSequence();
		
		Bee.Instance.UpdateState(BeeState.FollowingCamera);
		transform.position = Camera.main.transform.position + Camera.main.transform.forward * -2f;
	}

	private void UnSubscribeToEvents()
    {
        ImageTrackingPlotActivatedResponse.OnPlotActivated -= HandleGoingToPlot;
		ImageTrackingPlotUpdatedResponse.OnPlotActivated -= HandleGoingToPlot;
        Bee.OnBeeStateChanged -= HandleBeeStateChange;
		ImageTrackingPlotUpdatedResponse.OnPlotDeactivated -= HandlePlotDeactivated;
    }

    private void OnDestroy()
    {
        UnSubscribeToEvents();
    }
}
