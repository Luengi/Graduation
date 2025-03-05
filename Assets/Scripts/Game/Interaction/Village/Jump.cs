using UnityEngine;

public class Jump : MonoBehaviour, IInteractable
{
	public bool CanInterrupt {get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException();}
	public bool MultipleInteractions {get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException();}

	public void Interact()
	{
		transform.position += Vector3.up * 0.2f;
	}
}
