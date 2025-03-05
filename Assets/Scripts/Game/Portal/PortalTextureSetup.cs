using UnityEngine;

public class PortalTextureSetup : MonoBehaviour
{
	[SerializeField] private Camera _secondaryCamera;
	[SerializeField] private Material _portalMaterial;

    void Start()
    {
        if (_secondaryCamera.targetTexture != null) 
		{
			_secondaryCamera.targetTexture.Release();
		}
		_secondaryCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
		_portalMaterial.mainTexture = _secondaryCamera.targetTexture;
    }
}
