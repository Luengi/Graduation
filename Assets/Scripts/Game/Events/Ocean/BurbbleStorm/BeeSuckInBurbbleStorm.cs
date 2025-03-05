using System;
using UnityEngine;

[RequireComponent(typeof(CaughtObject)), RequireComponent(typeof(PlayAnimation)), ]
public class BeeSuckInBurbbleStorm : MonoBehaviour
{
    private const string SUCK_IN_STORM_ANIMATION_PARAMETER = "IsSuckingIn";
    private CaughtObject _caughtObject;
    private Rigidbody _rigidbody;
    private PlayAnimation _playAnimation;
    public static event Action SuckInStorm;

    private void Awake()
    {
        _playAnimation = GetComponent<PlayAnimation>();
    }

    void Start()
    {
        SetUp();
        SubscribeToEvents();
    }

    private void SetUp()
    {
        _caughtObject = GetComponent<CaughtObject>();
        
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
    }

    private void HandleStormStart (UpdatePassiveEventCollection eventMetadata) 
    {
        if (eventMetadata.CurrentEvent != PassiveEvent.BurbbleStorm) return;
        _rigidbody.isKinematic = false;
        _playAnimation.SetBoolParameter(SUCK_IN_STORM_ANIMATION_PARAMETER,true);
        SuckInStorm?.Invoke();
    }

    private void HandleStormEnd (UpdatePassiveEventCollection eventMetadata)
    {
        if (eventMetadata.PreviousEvent != PassiveEvent.BurbbleStorm && PlotsManager.Instance.CurrentPlot != Plot.None) return;
        _playAnimation.SetBoolParameter(SUCK_IN_STORM_ANIMATION_PARAMETER,false);
        _rigidbody.isKinematic = true;
    }
    
    private void SubscribeToEvents() 
    {
        PlotEvent.OnPassiveEventStart += HandleStormStart;
        PlotEvent.OnPasiveEventEnd += HandleStormEnd;
    }

    private void UnsubscribeFromEvents() 
    {
        PlotEvent.OnPassiveEventStart -= HandleStormStart;
        PlotEvent.OnPasiveEventEnd -= HandleStormEnd;
    }

    private void OnDestroy() => UnsubscribeFromEvents();
}
