using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FadeObject)), RequireComponent(typeof(DropObject)), ]
public class ACorn : MonoBehaviour
{
    private FadeObject _fadeObject;
    private DropObject _dropObject;

    private void Awake()
    {
        _fadeObject = GetComponent<FadeObject>();
        _dropObject = GetComponent<DropObject>();
    }

    public void DropCone()
    {
        _dropObject.Drop();
    }
    
    public IEnumerator FadeCone (float fadeSpeed)
    {
        yield return _fadeObject.Fade(fadeSpeed);
    }
    
    public void ResetCone (Vector3 resetPosition)
    {
        _dropObject.ResetObject(resetPosition);
        _fadeObject.ResetColor();
    }
}
