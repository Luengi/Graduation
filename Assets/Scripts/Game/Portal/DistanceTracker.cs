using System;
using UnityEngine;

public class DistanceTracker : MonoBehaviour
{
	[SerializeField] private Transform _object1;
	[SerializeField] private Transform _object2;
	[SerializeField] private float _maxDistance;
	
	public event Action OnMaxDistanceReached;

    void Update()
    {
		CheckDistance();
    }

	private void HandleMaxDistanceReached() 
	{
		OnMaxDistanceReached?.Invoke();
	}

	void CheckDistance()
	{
		if ((!IsPlotActive() && !BeeMovingTowardsPlot()) || !HaveObjectsToTrack()) return;

		if (Vector3.Distance(_object1.position, _object2.position) > _maxDistance)
		{
			HandleMaxDistanceReached();
		}
	}

	private static bool BeeMovingTowardsPlot() => Bee.Instance.State == BeeState.EnteringPlot;

	private bool HaveObjectsToTrack() => _object1 != null && _object2 != null;

	private bool IsPlotActive() => PlotsManager.Instance.CurrentPlot != Plot.None;

	public void UpdateObjects (Transform object1, Transform object2)
	{
		_object1 = object1;
		_object2 = object2;
	}

	public bool OverMaxDistance (Transform object1, Transform object2) => Vector3.Distance(object1.position, object2.position) > _maxDistance;

	public void UpdateMaxDistance (float maxDistance)
	{
		_maxDistance = maxDistance;
	}
}
