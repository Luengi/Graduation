using UnityEngine;

public class UpdateMaterial : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
	[SerializeField] private MaterialNameCollection[] _materials;

	private string _currentMaterialName;
	private string _previousMaterialName;

	private void Awake()
	{
		if (_renderer == null) Debug.LogWarning("Mesh Renderer is required by Update Material to work correctly");
	}

	public void UpdateMaterialByName (string name)
	{
		if (_renderer == null || _currentMaterialName == name) return;
		
		_previousMaterialName = _currentMaterialName;

		Material material = GetMaterialByName(name);
		
		if (material == null) return;
		
		_currentMaterialName = name;
		_renderer.material = material;
	}

	public void UpdateToPreviousMaterial() => UpdateMaterialByName(_previousMaterialName);

	private Material GetMaterialByName (string name)
	{
		foreach (MaterialNameCollection materialNameCollection in _materials)
		{
			if (materialNameCollection.Name != name) continue;

			return materialNameCollection.Material;
		}
		return null;
	} 
}
