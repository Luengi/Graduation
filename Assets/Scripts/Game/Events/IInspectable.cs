using System;
using UnityEngine;

public interface IInspectable
{
	public GameObject InspectablePoint {get;}
    public void Inspect();
	public void StopInspecting();
	public event Action OnInspected;
}
