using System;

public interface IToggleComponent
{
	public event Action OnToggleDone;
    public ToggleState CurrentToggleState{get; set;}
    public ToggleState NextToggleState{get; set;}
    
    public void ToggleOn();

    public void ToggleOff();

    public void Toggle();
}
