// using System.Collections;
// using System.Collections.Generic;
// using NUnit.Framework;
// using UnityEngine;

// public class PlotEventTest
// {
// 	HideAndSea hideAndSea;

// 	[SetUp]
// 	public void Setup() {
// 		hideAndSea = new GameObject().AddComponent<HideAndSea>();
// 		hideAndSea._hideSpots = new List<Transform>{
// 			new GameObject().transform,
// 			new GameObject().transform,
// 			new GameObject().transform
// 		};
// 		PlotEventConfig config = ScriptableObject.CreateInstance<PlotEventConfig>();
// 		config.Timing.StartDelay = 5f;
// 		config.Timing.Frequency = 2;
// 		hideAndSea._config = config;
		
// 		hideAndSea.SetUpPassiveEvent();

// 		PassiveEventManager.Instance = new GameObject().AddComponent<PassiveEventManager>();
// 	}

// 	[Test]
// 	public void PlotEvent_StartEvent_ShouldSetCorrectState()
// 	{
// 		hideAndSea._state = EventState.InitialReady;
// 		hideAndSea.StartEvent();

// 		Assert.AreEqual(EventState.InitialActive, hideAndSea._state);

// 		hideAndSea._state = EventState.Ready;
// 		hideAndSea.StartEvent();

// 		Assert.AreEqual(EventState.Active, hideAndSea._state);		
// 	}

// 	[Test]
// 	public void PlotEvent_StartEvent_ShouldDecreaseFrequency()
// 	{
// 		hideAndSea._state = EventState.InitialReady;
// 		uint currentFrequencyAmount = hideAndSea._frequency.FrequencyAmount;
// 		hideAndSea.StartEvent();

// 		Assert.AreEqual(currentFrequencyAmount - 1, hideAndSea._frequency.FrequencyAmount);
// 	}

// 	[Test]
// 	public void PlotEvent_UpdateEventStatus_ShouldSwitchStateToInitialReady() {
// 		hideAndSea._state = EventState.InitialWaiting;
// 		hideAndSea.UpdateEventStatus();

// 		Assert.AreEqual(EventState.InitialReady, hideAndSea._state);
// 	}

// 	[Test]
// 	public void PlotEvent_UpdateEventStatus_ShouldSwitchStateToReady() {
// 		hideAndSea._state = EventState.Waiting;
// 		hideAndSea.UpdateEventStatus();

// 		Assert.AreEqual(EventState.Ready, hideAndSea._state);
// 	}

// 	[Test]
// 	public void PlotEvent_UpdateEventStatus_ShouldSwitchStateToWaiting() {
// 		hideAndSea._state = EventState.InitialActive;
// 		hideAndSea.UpdateEventStatus();

// 		Assert.AreEqual(EventState.Waiting, hideAndSea._state);
// 	}

// 	[Test]
// 	public void PlotEvent_UpdateEventStatus_ShouldSwitchStateToDone() {
// 		hideAndSea._state = EventState.Active;
// 		hideAndSea._frequency.FrequencyAmount = 0;
// 		hideAndSea.UpdateEventStatus();

// 		Assert.AreEqual(EventState.Done, hideAndSea._state);
// 	}

// 	[Test]
// 	public void PlotEvent_HandleWaitingStatus_ShouldStartCooldown() {
// 		hideAndSea._state = EventState.InitialActive;
// 		hideAndSea.HandleWaitingStatus();

// 		Assert.IsTrue(hideAndSea._cooldown.IsOnCooldown);
// 	}

// 	[Test]
// 	public void PlotEvent_HandleDoneStatus_ShouldFireEndEvent() {
// 		bool isEndEventFired = false;
// 		PlotEvent.OnPasiveEventEnd += (metadata) => {
// 			isEndEventFired = true;
// 		};

// 		hideAndSea.HandleDoneStatus();

// 		Assert.IsTrue(isEndEventFired);
// 	}

// 	[Test]
// 	public void PlotEvent_HandleDoneStatus_ShouldFireEndEventWithCorrectMetadata() {
// 		PassiveEventManager.Instance._currentEventPlaying = PassiveEvent.HideAndSea;
// 		UpdatePassiveEventCollection metadata = null;
// 		PlotEvent.OnPasiveEventEnd += (pMetadata) => {
// 			metadata = pMetadata;
// 		};

// 		hideAndSea.HandleDoneStatus();

// 		Assert.AreEqual(BeeState.Idle, metadata.State);
// 		Assert.AreEqual(null, metadata.Metadata);
// 		Assert.AreEqual(PassiveEvent.None, metadata.CurrentEvent);
// 		Assert.AreEqual(PassiveEvent.HideAndSea, metadata.PreviousEvent);
// 	}

// 	[Test]
// 	public void PlotEvent_FireStartEvent_ShouldFireStartEvent() {
// 		bool isStartEventFired = false;
// 		PlotEvent.OnPassiveEventStart += (metadata) => {
// 			isStartEventFired = true;
// 		};

// 		UpdatePassiveEventCollection metadata = new UpdatePassiveEventCollection();
// 		hideAndSea.FireStartEvent(metadata);

// 		Assert.IsTrue(isStartEventFired);
// 	}

// 	[Test]
// 	public void PlotEvent_FireEndEvent_ShouldFireEndEvent() {
// 		bool isEndEventFired = false;
// 		PlotEvent.OnPasiveEventEnd += (metadata) => {
// 			isEndEventFired = true;
// 		};

// 		UpdatePassiveEventCollection metadata = new UpdatePassiveEventCollection();
// 		hideAndSea.FireEndEvent(metadata);

// 		Assert.IsTrue(isEndEventFired);
// 	}
// }
