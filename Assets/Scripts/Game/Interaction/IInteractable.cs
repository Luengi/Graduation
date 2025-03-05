public interface IInteractable 
{
	public bool CanInterrupt {get; set;}
	public bool  MultipleInteractions {get; set;}
    public void Interact();
}
