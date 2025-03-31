using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AvoidObjectSwimmingBehavior))]
public class Fish : MonoBehaviour
{
    public float MoveSpeed {get; set;} = 0.5f;
    
    private AvoidObjectSwimmingBehavior _avoidObjectSwimmingBehavior;

    private void Awake()
    {
        _avoidObjectSwimmingBehavior = GetComponent<AvoidObjectSwimmingBehavior>();
    }

    private void Update()
    {
        AvoidObjectMove();
    }

    private void AvoidObjectMove()
    {
        _avoidObjectSwimmingBehavior.Move(MoveSpeed);
    }
}
