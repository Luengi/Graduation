using System;
using System.Collections;
using UnityEngine;

public class ToggleRotate : MonoBehaviour
{
    [SerializeField] internal float _openAngle = 90.0f;
    [SerializeField] internal float _rotateSpeed = 15f;
    [SerializeField] internal Axis _rotationAxis = Axis.Y;
	[SerializeField] internal Transform _objectToRotate;
    
    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    public IEnumerator RotateCoroutine (Quaternion targetRotation)
    {
        while (Quaternion.Angle(_objectToRotate.rotation, targetRotation) > 0.01f)
        {
            _objectToRotate.rotation = RotateToTarget(_objectToRotate.rotation, targetRotation, _rotateSpeed);
            yield return null; // Wait for the next frame
        }
        // make sure the rotation reach to the target rotation
        _objectToRotate.rotation = targetRotation;
    }
    
    public IEnumerator OpenCoroutine()
    {
        SetRotationToOpen();
        while (Quaternion.Angle(_objectToRotate.rotation, _openRotation) > 0.01f)
        {
            _objectToRotate.rotation = RotateToTarget(_objectToRotate.rotation, _openRotation, _rotateSpeed);
            yield return null; // Wait for the next frame
        }
        // make sure the rotation reach to the target rotation
        _objectToRotate.rotation = _openRotation;
    }
    
    public IEnumerator CloseCoroutine()
    {
        SetRotationToClose();
        while (Quaternion.Angle(_objectToRotate.rotation, _closedRotation) > 0.01f)
        {
            _objectToRotate.rotation = RotateToTarget(_objectToRotate.rotation, _closedRotation, _rotateSpeed);
            yield return null; // Wait for the next frame
        }
        // make sure the rotation reach to the target rotation
        _objectToRotate.rotation = _closedRotation;
    }
    
    internal Quaternion RotateToTarget (Quaternion origin, Quaternion target, float speed) => Quaternion.RotateTowards(origin, target, speed * Time.deltaTime);

    internal Quaternion GetOpenRotation (Axis axis)
    {
        Vector3 rotationVector = _closedRotation.eulerAngles;

        switch (axis)
        {
            case Axis.X:
                rotationVector.x += _openAngle;
                break;
            case Axis.Y:
                rotationVector.y += _openAngle;
                break;
            case Axis.Z:
                rotationVector.z += _openAngle;
                break;
        }

        return Quaternion.Euler(rotationVector);
    }
    internal Quaternion GetCloseRotation (Axis axis)
    {
        Vector3 rotationVector = _openRotation.eulerAngles;

        switch (axis)
        {
            case Axis.X:
                rotationVector.x -= _openAngle;
                break;
            case Axis.Y:
                rotationVector.y -= _openAngle;
                break;
            case Axis.Z:
                rotationVector.z -= _openAngle;
                break;
        }

        return Quaternion.Euler(rotationVector);
    }

    private void SetRotationToOpen()
    {
        _closedRotation = _objectToRotate.rotation;
        _openRotation = GetOpenRotation(_rotationAxis);
    }
    private void SetRotationToClose()
    {
        _openRotation = _objectToRotate.rotation;
        _closedRotation = GetCloseRotation(_rotationAxis);
    }
}
