using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

public class WindZoneScript : MonoBehaviour
{
    [SerializeField] float _windForce;
    [SerializeField] bool windStorm = false;

    private void OnTriggerStay(Collider other)
    {
        //Set hitObj to other objects it collides with
        var hitObj = other.gameObject;
        if (hitObj != null && hitObj.tag != "Butterfly")
        {
            //Transform that objects Rigidbody upwards
            var rb = hitObj.GetComponent<Rigidbody>();
            var dir = transform.up;
            rb.AddForce(dir * _windForce);
            //Randomize the wind so objects don't stay in one place
            if (windStorm == true)
            {
                _windForce = Random.value * 5;
            } 
            else
            {
                _windForce = Random.value;
            }
            
        }
    }
}
