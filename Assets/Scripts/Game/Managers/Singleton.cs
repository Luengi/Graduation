using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance {get; internal set;}

	internal protected virtual void Awake() 
	{
		if (Instance == null) 
		{
			Instance = this as T;
		} 
		else 
		{
			Destroy(gameObject);
		}
	}
}
