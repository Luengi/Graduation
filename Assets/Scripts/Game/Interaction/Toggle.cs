using UnityEngine;

[RequireComponent(typeof(IToggleComponent))]
public class Toggle : MonoBehaviour, IInteractable
{
    private IToggleComponent[] _toggleComponent;

	public bool CanInterrupt {get; set;}
	public bool MultipleInteractions {get; set;}

	public void Start()
    {
        _toggleComponent = GetComponents<IToggleComponent>();

        if (_toggleComponent == null || _toggleComponent.Length <= 0)
        {
            throw new System.Exception("No toggle component found. Please assign it in the Inspector.");
        }
    }
    
    public void Interact()
    {
        OnToggle();
    }
    
    internal void OnToggle()
    {
        foreach (var component in _toggleComponent)
        {
            component.Toggle();
        }
    }
}
