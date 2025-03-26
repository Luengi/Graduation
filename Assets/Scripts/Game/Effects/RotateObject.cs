using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
	[SerializeField] private float speed;
	private bool _canRotate = false;
	
	public void EnableRotation()
	{
		_canRotate = true;
	}
	
	public void DisableRotation()
	{
		_canRotate = false;
	}
	
	private void Update()
	{
		if (!_canRotate) return;
		transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.Self);
	}
}
