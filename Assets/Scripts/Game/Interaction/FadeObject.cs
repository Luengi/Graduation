using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer)), ]
public class FadeObject : MonoBehaviour
{
    private Renderer _renderer;
    private Material _material;
    private Color _originalColor;
    
    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _material = _renderer.material;
        _originalColor = _material.color;
    }

    public IEnumerator Fade (float fadeSpeed)
    {
        Color initialColor = _material.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(initialColor.a, 0f, elapsedTime / fadeSpeed);
            
            _material.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

            yield return null; 
        }
        
        _material.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
    }
    
    public void ResetColor()
    {
        _material.color = _originalColor;
    }
}
