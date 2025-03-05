using UnityEngine;
using UnityEngine.Serialization;

public class PortalCameraMovementBehaviour : MonoBehaviour
{
	[SerializeField] internal GameObject _portal;
	[SerializeField] internal GameObject _worldAnchor;

	private Camera _mainCamera;
	
	private void Awake()
	{
		_mainCamera = Camera.main;
		if (_mainCamera != null) transform.rotation = _mainCamera.transform.rotation;
	}

    void Update()
    {
	    if (_mainCamera == null) return;
	    
		UpdatePortalTransform(_worldAnchor, _portal, _mainCamera.gameObject);
    }

    internal void UpdatePortalTransform (GameObject anchor, GameObject portal, GameObject mainCameraGameObject)
    {
	    var adjustedPositionAndRotationMatrix = anchor.transform.localToWorldMatrix * portal.transform.worldToLocalMatrix * mainCameraGameObject.transform.localToWorldMatrix;
	    transform.SetPositionAndRotation(adjustedPositionAndRotationMatrix.GetColumn(3), adjustedPositionAndRotationMatrix.rotation);
    }
}
