using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ButterflyColors : MonoBehaviour
{
    public MeshRenderer[] renderers;

    // Start is called before the first frame update
    void Start()
    {
        Color newColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
        ApplyMaterial(newColor, 0);
    }

    void ApplyMaterial(Color color, int targetMaterialIndex)
    {
        Material generatedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        generatedMaterial.SetColor("_BaseColor", color);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = generatedMaterial;
        }
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
