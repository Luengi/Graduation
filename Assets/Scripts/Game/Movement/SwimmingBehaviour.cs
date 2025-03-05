using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimmingBehaviour : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 1f;
    [SerializeField] private Range _decisionDelayRange;
    [SerializeField] private EnvironmentBounds bounds;
    [SerializeField] private Transform _middlePoint;
    [SerializeField] private Vector2 _turnLimitRange;
    [Tooltip("Limit rotation on the X axis (pitch) to prevent unrealistic movement")]
    [SerializeField] private float _verticalRotationBound = 10;

    public bool CheckBounds {get; set;} = false;

    private Coroutine _activeChangeDirectionCoroutine;
    private Coroutine _activeSmoothRotationCoroutine;

    private bool _checkHorizontalDirection = true;
    private bool _checkVerticalDirection = true;

    private float _rotationDuration;

    private void Start()
    {
        EstablishRotationDuration();
        SetupBounds();
    }

    private void FixedUpdate()
    {
        CheckPositionInBounds();
    }

    public void Move (float speed)
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void CheckPositionInBounds()
    {
        if (!CheckBounds) return;

        CheckHorizontalPosition();
        CheckVerticalPosition();
    }

    private void CheckHorizontalPosition()
    {
        if (!_checkHorizontalDirection) return;

        if (bounds.ExceedsWidthBounds(transform.position.x) || bounds.ExceedsDepthBounds(transform.position.z))
        {
            // Temporarily stop horizontal position checking, until rotation finishes
            StartCoroutine(DisableDirectionCheckCoroutine(Direction.Horizontal, _rotationDuration + 1 / _rotationDuration));
            // Rotate bee
            SmoothRotateBeeQuaternion(GetLevelledRotationToMiddlePoint(_middlePoint.position), _rotationDuration);
        }
    }

    private void CheckVerticalPosition()
    {
        if (!_checkVerticalDirection) return;

        if (bounds.ExceedsHeightBounds(transform.position.y))
        {
            // Temporarily stop vertical position checking, until rotation finishes
            StartCoroutine(DisableDirectionCheckCoroutine(Direction.Vertical, _rotationDuration + 1 / _rotationDuration));
            // Rotate bee
            SmoothRotateBeeVector(new Vector3(-transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z), _rotationDuration);
        }
    }

    // Smoothly rotate bee TO newAngle vector
    // Called when reaching bounds
    private void SmoothRotateBeeVector (Vector2 newAngle, float rotationDuration)
    {
        Quaternion targetRotation = Quaternion.Euler(newAngle);
        SmoothRotateBeeQuaternion(targetRotation, rotationDuration);
    }

    // Smoothly rotate bee TO targetRotation quaternion
    // Called when reaching bounds
    private void SmoothRotateBeeQuaternion (Quaternion targetRotation, float rotationDuration)
    {
        // Stop previous rotation coroutine, if any
        StopSmoothRotationCoroutine();
        StopChangeDirectionCoroutine();
        _activeSmoothRotationCoroutine = StartCoroutine(SmoothRotationCoroutine(targetRotation, rotationDuration));
    }

    // Smoothly rotate bee BY newAngles amount
    // Called when randomly changing direction
    private void SmoothRotateBeeByAngle (Vector2 newAngles, float rotationDuration)
    {
        Vector3 newRotationVec = new Vector3(newAngles.x, newAngles.y, 0);
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles + newRotationVec);

        // Stop previous rotation coroutine, if any
        StopSmoothRotationCoroutine();

        _activeSmoothRotationCoroutine = StartCoroutine(SmoothRotationCoroutine(targetRotation, rotationDuration));
    }

    private void SmoothRotate (Quaternion startRotation, Quaternion targetRotation, float percentageCompleted)
    {
        transform.rotation = Quaternion.Slerp(startRotation, targetRotation, percentageCompleted);
    }

    private void ResetZAngle()
    {
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
    }

    IEnumerator ChangeDirectionCoroutine (float secondsToWait)
    {
        Vector2 newAngles = GetNewAngles();
        SmoothRotateBeeByAngle(newAngles, _rotationDuration);
        ResetZAngle(); // Set Z to 0 due to orientation messing up

        yield return new WaitForSeconds(secondsToWait);

        DoSwimmingSequence();
    }

    IEnumerator SmoothRotationCoroutine (Quaternion targetRotation, float duration)
    {
        Quaternion startRotation = transform.rotation;
        float timeElapsed = 0f;
        float percentageCompleted;

        while (timeElapsed < duration)
        {
            // Slerp from start rotation to the target rotation
            percentageCompleted = timeElapsed / duration;
            SmoothRotate(startRotation, targetRotation, percentageCompleted);
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        // Ensure the final rotation is exactly the target rotation
        transform.rotation = targetRotation;
    }

    IEnumerator DisableDirectionCheckCoroutine (Direction directionToDisableChecking, float disableDuration)
    {
        UpdateDirectionChecks(directionToDisableChecking, false);

        yield return new WaitForSeconds(disableDuration);

        UpdateDirectionChecks(directionToDisableChecking, true);
    }

    IEnumerator DisableGetDirectionCoroutine (float disableDuration)
    {
        StopCoroutine(_activeChangeDirectionCoroutine);

        yield return new WaitForSeconds(disableDuration);

        DoSwimmingSequence();
    }

    private void UpdateDirectionChecks (Direction directionToDisableChecking, bool updateValue)
    {
        switch (directionToDisableChecking)
        {
            case Direction.Horizontal:
                _checkHorizontalDirection = updateValue;
                break;
            case Direction.Vertical:
                _checkVerticalDirection = updateValue;
                break;
            default:
                throw new System.Exception($"{GetType()}: {directionToDisableChecking} direction not handled.");
        }
    }

    private void StopSmoothRotationCoroutine()
    {
        if (_activeSmoothRotationCoroutine != null)
            StopCoroutine(_activeSmoothRotationCoroutine);
    }

    private void StopChangeDirectionCoroutine()
    {
        if (_activeChangeDirectionCoroutine != null)
        {
            DisableGetDirectionCoroutine(_rotationDuration + _decisionDelayRange.GetRandomValueWithinRange());
        }
    }

    private Vector2 GetNewAngles() => new Vector2(GetNewRandomVerticalDirectionDegrees(_turnLimitRange.y), GetNewRandomHorizontalDirectionDegrees(_turnLimitRange.x));

    // Get rotation that points to the middle point, with the same Y as the bee
    private Quaternion GetLevelledRotationToMiddlePoint (Vector3 middlePoint)
    {
        // Set the middle point's Y the same as bee's Y
        Vector3 leveledMiddlePointPosition = middlePoint;
        leveledMiddlePointPosition.y = transform.position.y;

        // Get direction vector to levelled middle point
        Vector3 directionToMiddlePoint = (leveledMiddlePointPosition - transform.position).normalized;

        // Get quaternion facing the middle point
        Quaternion levelledRotation = Quaternion.LookRotation(directionToMiddlePoint);

        return levelledRotation;
    }

    // For realistic movement
    internal bool ExceedsVerticalAngleBound (float updatedAngle, float verticalRotationBound)
    {
        // Normalize the angle to be between -180 and 180 degrees
        float normalizedAngle = MathHelperAngles.NormalizeAngle(updatedAngle);

        // Check if the angle is outside the vertical rotation bounds (-10 to 10)
        return normalizedAngle < -verticalRotationBound || normalizedAngle > verticalRotationBound;
    }

    private float GetNewRandomVerticalDirectionDegrees (float verticalTurnLimit)
    {
        // Calculate new Vertical angle
        float currentAngle = transform.rotation.eulerAngles.x;
        float newAngle = Random.Range(-verticalTurnLimit, verticalTurnLimit);
        float updatedAngle = currentAngle + newAngle;

        // Flip angle if it exceeds the bounds to avoid unrealistic vertical movement
        updatedAngle = ExceedsVerticalAngleBound(updatedAngle, _verticalRotationBound) ? -updatedAngle : updatedAngle;

        return updatedAngle;
    }

    private float GetNewRandomHorizontalDirectionDegrees (float horizontalTurnLimit)
    {
        return Random.Range(-horizontalTurnLimit, horizontalTurnLimit);
    }

    private void EstablishRotationDuration()
    {
        _rotationDuration = 1 / _rotationSpeed;
    }

    private void SetupBounds()
    {
        bounds.SetCenter(_middlePoint.position);
    }

    public void DoSwimmingSequence()
    {
        _activeChangeDirectionCoroutine = StartCoroutine(ChangeDirectionCoroutine(_decisionDelayRange.GetRandomValueWithinRange()));
    }

    public void RestartSwimmingSequence()
    {
        StartCoroutine(RestartSwimmingSequenceCoroutine());
    }

    private IEnumerator RestartSwimmingSequenceCoroutine()
    {
        yield return StartCoroutine(SmoothRotationCoroutine(GetLevelledRotationToMiddlePoint(_middlePoint.position), _rotationDuration));
        DoSwimmingSequence();
    }

    public void StopSwimmingSequence()
    {
        StopAllCoroutines();
    }
}
