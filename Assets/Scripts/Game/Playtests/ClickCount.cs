using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickCount : MonoBehaviour
{
    public int blue, cyan, magenta, orange, purple, yellow;
    public Text clickCount;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //public int 
        clickCount.text = "Blue: " + blue + " Cyan: " + cyan + " Magenta: " + magenta + " Orange: " + orange + " Purple: " + purple + " Yellow: " + yellow;
    }



}
