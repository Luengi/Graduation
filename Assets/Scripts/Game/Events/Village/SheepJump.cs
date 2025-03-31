using System;
using System.Collections.Generic;
using UnityEngine;

public class SheepJump : PlotEvent, IInterruptible
{
	[SerializeField] private Transform _sheepHolder;
	[SerializeField] private float minJumpForce = 0.1f;
	[SerializeField] private float maxJumpForce = 0.3f;
	[SerializeField] private float jumpDuration = 1f;
	
	private List<Sheep> _sheeps = new List<Sheep>();

	public event Action<IInterruptible> OnInterruptedDone;
	
	private void Awake()
	{
		if (_sheepHolder == null)
			Debug.LogError("Missing sheep holder game object reference");
		else
			LoadSheep();
	}
	
	private void Start() 
	{
		SubscribeToEvents();
	}

	private void LoadSheep()
	{
		foreach (Transform sheep in _sheepHolder)
		{
			if (sheep.GetComponent<Sheep>()) _sheeps.Add(sheep.GetComponent<Sheep>());
		}
	}
	
	public override void StartEvent()
	{
		base.StartEvent();
		Sheep sheep = GetRandomSheep(_sheeps, UnityEngine.Random.Range(0, _sheeps.Count));
		UpdatePassiveEventCollection metadata = SetupStartEventMetadata(sheep.transform);
		
		FireStartEvent(metadata);
		
		float randomJumpForce = UnityEngine.Random.Range(minJumpForce, maxJumpForce);

		sheep.Jump(randomJumpForce,jumpDuration);
		
		metadata = SetupEndEventMetadata();

		FireEndEvent(metadata);
		base.UpdateEventStatus();
	}
	
	internal Sheep GetRandomSheep (List<Sheep> sheep, int randomIndex) => sheep[randomIndex];
	
	internal UpdatePassiveEventCollection SetupStartEventMetadata(Transform sheep)
	{
		return new UpdatePassiveEventCollection
		{
			PreviousEvent = PassiveEventManager.Instance.CurrentEventPlaying,
			CurrentEvent = PassiveEvent.SheepJump,
			State = BeeState.Idle,
			Metadata = new EventMetadata
			{
				Target = sheep
			}
		};
	}

	private void OnDestroy()
	{
		UnsubscribeFromEvents();
	}
	
	public override bool CanPlay() => _sheeps.Count > 0;
	
	internal void SetUpPassiveEvent() 
	{
		_state = EventState.InitialWaiting;
	}
	
	protected override void HandlePlotActivated()
	{
		if (PlotsManager.Instance.CurrentPlot != Plot.Village) return;

		SetUpPassiveEvent();
	}

	public void InterruptEvent()
	{
		OnInterruptedDone?.Invoke(this);
	}
}
