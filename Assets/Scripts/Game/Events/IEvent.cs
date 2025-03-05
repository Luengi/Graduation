using System;

public interface IEvent 
{
	public event Action OnEventDone;
    public EventState State {get; set;}
	public void StopEvent();
}
