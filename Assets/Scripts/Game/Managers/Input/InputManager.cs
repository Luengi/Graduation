using UnityEngine;

public class InputManager : Singleton<InputManager>
{
	private InputState _inputState;
	public InputState InputState => _inputState;

	internal protected override void Awake() 
	{
		base.Awake();
		_inputState = InputState.None;
	}

    // Update is called once per frame
    void Update()
    {
		DefineInputState();
    }

	private void DefineInputState()
	{
		if (IsInteracting()) _inputState = InputState.Interact;
		else _inputState = InputState.None;
	}

	private bool IsInteracting() 
	{
		return Input.GetMouseButtonDown(0);
	}
}
