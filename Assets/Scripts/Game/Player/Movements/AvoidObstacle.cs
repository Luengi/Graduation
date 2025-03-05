using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidObstacle : MonoBehaviour
{
    private Vector3 _currentObstacleAvoidanceVelocity;
    
    public Vector3 CalculateObstacleAvoidance (float speed, Transform transform, float obstacleDistance, Vector3[] directionsToCheck)
    {
        Vector3 obstacleVector = Vector3.zero;

        if (Physics.Raycast(transform.position, transform.forward, out _, obstacleDistance))
        {
            obstacleVector = FindBestDirectionToAvoid(speed, transform, obstacleDistance, directionsToCheck);
        }
        else
        {
            _currentObstacleAvoidanceVelocity = Vector3.zero;
        }

        return obstacleVector;
    }

    private Vector3 FindBestDirectionToAvoid (float speed, Transform transform, float obstacleDistance, Vector3[] directionsToCheck)
    {
        // Check if the current avoidance velocity can still be used
        if (IsClearPathAhead(transform, obstacleDistance))
        {
            return _currentObstacleAvoidanceVelocity;
        }
        
        return DetermineBestAvoidanceDirection(speed, transform, obstacleDistance, directionsToCheck);
    }

    private bool IsClearPathAhead (Transform transform, float obstacleDistance)
    {
        if (_currentObstacleAvoidanceVelocity != Vector3.zero)
        {
            if (!Physics.Raycast(transform.position, transform.forward, out _, obstacleDistance))
            {
                return true;
            }
        }
        return false;
    }

    private Vector3 DetermineBestAvoidanceDirection (float speed, Transform transform, float obstacleDistance, Vector3[] directionsToCheck)
    {
        Vector3 bestDirection = Vector3.zero;
        float maxDistance = float.MinValue;

        foreach (var direction in directionsToCheck)
        {
            Vector3 worldDirection = transform.TransformDirection(direction.normalized);

            if (TryGetObstacleHit(transform, worldDirection, obstacleDistance, out float hitDistance))
            {
                if (hitDistance > maxDistance)
                {
                    maxDistance = hitDistance;
                    bestDirection = worldDirection;
                }
            }
            else
            {
                // No obstacles detected, use this direction immediately
                _currentObstacleAvoidanceVelocity = (worldDirection.normalized * speed) - transform.forward;
                return Vector3.ClampMagnitude(_currentObstacleAvoidanceVelocity, 4f);
            }
        }
        
        return Vector3.ClampMagnitude(bestDirection.normalized * speed - transform.forward, 4f);
    }

    private bool TryGetObstacleHit (Transform transform, Vector3 direction, float obstacleDistance, out float hitDistance)
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, obstacleDistance, LayerMask.GetMask("Obstacle")))
        {
            hitDistance = (hit.point - transform.position).sqrMagnitude;
            return true;
        }

        hitDistance = 0f;
        return false;
    }

    public Vector3[] CalculateDirections (int count)
    {
        Vector3[] directions = new Vector3[count];
        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);

            directions[i] = new Vector3(x, y, z);
        }

        return directions;
    }
}
