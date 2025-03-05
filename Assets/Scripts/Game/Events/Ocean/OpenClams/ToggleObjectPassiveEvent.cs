using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ToggleObjectPassiveEvent
{
	[Tooltip("Container that holds all toggleable objects")]
   	public GameObject _toggleableObjectContainer;

	private Dictionary<GameObject, IToggleComponent> _toggleableObjects = new Dictionary<GameObject, IToggleComponent>();
    
	public bool TryGetRandomObjectWithStateOn (out IToggleComponent toggleComponent, out GameObject toggleableObject) 
	{
		toggleComponent = TryGetRandomObjectWithState(ToggleState.On, out toggleableObject);
		return toggleComponent != null && toggleableObject != null;
	}

	public bool TryGetRandomObjectWithStateOff (out IToggleComponent toggleComponent, out GameObject toggleableObject) 
	{
		toggleComponent = TryGetRandomObjectWithState(ToggleState.Off, out toggleableObject);
		return toggleComponent != null && toggleableObject != null;
	}

	private IToggleComponent TryGetRandomObjectWithState (ToggleState state, out GameObject toggleableObject) 
	{
		foreach (KeyValuePair<GameObject, IToggleComponent> toggleableObj in _toggleableObjects) 
		{
			if (toggleableObj.Value.CurrentToggleState != state) continue;

			toggleableObject = toggleableObj.Key;
			return toggleableObj.Value;
		}
		toggleableObject = null;
		return null;
	}

	public void RetrieveToggleableObjects() 
	{
		GameObject[] children = new GameObject[_toggleableObjectContainer.transform.childCount];

		for (int i = 0; i < _toggleableObjectContainer.transform.childCount; i++) 
		{
			children[i] = _toggleableObjectContainer.transform.GetChild(i).gameObject;
		}

		foreach(GameObject toggleableGameObject in children) 
		{
			IToggleComponent toggleComponent = toggleableGameObject.GetComponent<IToggleComponent>();

			if (toggleComponent == null) 
			{
				Debug.LogError("Toggle component not found on object: " + toggleableGameObject.name);
				continue;
			}

			_toggleableObjects.Add(toggleableGameObject, toggleComponent);
		}
	}
}
