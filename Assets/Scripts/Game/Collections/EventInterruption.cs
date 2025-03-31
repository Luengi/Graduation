using System;
using UnityEngine;

[Serializable]
public class EventInterruption
{
	public EventInterruption (GameObject eventObject, EventType eventType)
	{
		EventObject = eventObject;
		EventType = eventType;
	}
	
	public GameObject EventObject;
	public EventType EventType;
}
