using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverWaypoint : MonoBehaviour
{
	public float GetExtendedForwardDistanceToPoint (Vector3 point, float distanceToExtend)
	{
		Vector3 extendedPoint = (transform.position + transform.forward).normalized * distanceToExtend;
		return (point - extendedPoint).magnitude;
	}
}
