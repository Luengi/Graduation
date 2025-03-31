using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ToggleRotate))]
public class FrameInteraction : MonoBehaviour, IInteractable, IEvent, IToggleComponent
{    
    public ToggleState CurrentToggleState {get; set;}
    public ToggleState NextToggleState {get; set;}
    
    private bool _firstTimeOpen;
    public bool CanInterrupt {get; set;}
    public bool MultipleInteractions {get; set;}
    
#pragma warning disable CS0067
    public event Action OnToggleDone;
    public event Action OnEventDone;
#pragma warning restore CS0067
    
    public EventState State {get; set;}

    private ToggleRotate _toggleRotate;

    public static event Action OnFirstFrameOpen;

    private void Start()
    {
        SetUp();
    }

    private void SetUp()
    {
        CurrentToggleState = ToggleState.Off;
        _firstTimeOpen = false;
        _toggleRotate = GetComponent<ToggleRotate>();
    }

    public void Interact()
    {
        Toggle();
    }

    private void HandleOpen()
    {
        if (CurrentToggleState != ToggleState.On || _firstTimeOpen) return;
        _firstTimeOpen = true;
        OnFirstFrameOpen?.Invoke();
    }
    
    public void ToggleOn()
    {
        NextToggleState = ToggleState.On;
        StartCoroutine(ToggleOnEnumerator());
    }

    public void ToggleOff()
    {
        NextToggleState = ToggleState.Off;
        StartCoroutine(ToggleOffEnumerator());
    }

    public void Toggle()
    {
        if (CurrentToggleState == ToggleState.Switching) return;
        
        //interact
        if (CurrentToggleState == ToggleState.Off) ToggleOn(); 
        else ToggleOff();
    }
    
    private void UpdateState (ToggleState state)
    {
        CurrentToggleState = state;

        if (IsToggleDone(state))
            HandleOpen();
    }

    private bool IsToggleDone (ToggleState state) => state != ToggleState.Switching;

    private IEnumerator ToggleOnEnumerator()
    {
        UpdateState(ToggleState.Switching);
        yield return StartCoroutine(_toggleRotate.OpenCoroutine());
        
        UpdateState(NextToggleState);
    }
    
    private IEnumerator ToggleOffEnumerator()
    {
        UpdateState(ToggleState.Switching);
        yield return StartCoroutine(_toggleRotate.CloseCoroutine());
        
        UpdateState(NextToggleState);
    }

	public void StopEvent()
	{
		StopAllCoroutines();
	}

    private void OnDisable()
    {
        StopEvent();
    }
}
