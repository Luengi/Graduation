using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ScrollingTexture : MonoBehaviour
{
    [SerializeField] Vector2 _scrollSpeedVec;
    MeshRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        ScrollTexture(_scrollSpeedVec);
    }

    private void ScrollTexture (Vector2 scrollSpeed)
    {
        _renderer.material.mainTextureOffset = scrollSpeed * Time.realtimeSinceStartup;
    }
}
