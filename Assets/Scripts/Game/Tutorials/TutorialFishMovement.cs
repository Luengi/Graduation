using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFishMovement : MonoBehaviour
{
	[SerializeField] private float _speed;
	[SerializeField] private float _rotationAngle;
	
	private void Update()
	{
		// Swim in circles
		Move();
		Rotate();
	}
	
	private void Move()
	{
		transform.position += transform.forward * _speed * Time.deltaTime;
	}
	
	private void Rotate()
	{
		transform.Rotate(Vector3.up, _rotationAngle * Time.deltaTime);
	}
}
