using System;
using UnityEngine;
using UnityEngine.Serialization;

public class RaycastManager : MonoBehaviour
{
	[SerializeField] internal GameObject _secondaryCamera;
	[SerializeField] private LayerMask _portalMask;
	
	private GameObject _secondraycastPointer;
	private GameObject _mainRaycastPointer;

	private Camera _mainCamera;

	private Vector3 _hitPoint;

	public Vector3 HitPoint => _hitPoint;

	public event Action<Collider> OnRaycastHit;

	void Start()
	{
		_mainCamera = Camera.main;
		
		if (_mainCamera != null)
			_mainRaycastPointer = _mainCamera.transform.GetChild(0).gameObject;
		if (_secondaryCamera != null)
			_secondraycastPointer = _secondaryCamera.transform.GetChild(0).gameObject;
	}

	// Update is called once per frame
	void Update()
	{
		if (_mainCamera == null || InputManager.Instance.InputState != InputState.Interact) return;
		
		Raycast();
	}

	private void Raycast()
	{
		RaycastHit hit;
		if (DetectPerspectiveRayCast(out hit))
		{
			_hitPoint = hit.point;
			OnRaycastHit?.Invoke(hit.collider);
		}
	}
	
	internal Ray SecondCameraRay (Ray mainCameraRay, GameObject mainRaycastPointer, GameObject secondRaycastPointer)
	{	
		//cast the direction to local rotation for main camera using a child
		mainRaycastPointer.transform.forward = mainCameraRay.direction;
		//apply the local rotation of main camera chile to second camera child
		secondRaycastPointer.transform.localRotation = mainRaycastPointer.transform.localRotation;
		//cast a second ray using second camera child forward direction
		Ray secondaryCameraRay = new Ray(_secondaryCamera.transform.position, secondRaycastPointer.transform.forward);
		
		return secondaryCameraRay;
	}

	private bool DetectPerspectiveRayCast (out RaycastHit hit)
	{
		Ray mainCameraRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
		//check if first camera raycast hit anything
		if (!Physics.Raycast(mainCameraRay, out hit)) return false;
		//check if first camera raycast hit is in portal mask
		if (!LayerHelper.IsLayerInMask(hit.collider.gameObject.layer, _portalMask)) return true;
		//calculate second camera raycast
		Ray secondaryCameraRay = SecondCameraRay(mainCameraRay, _mainRaycastPointer,_secondraycastPointer);
		//check if second camera raycast hit anything
		if (!Physics.Raycast(secondaryCameraRay, out hit)) return false;
		//else
		return true;
	}
}
