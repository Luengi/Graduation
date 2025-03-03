using UnityEngine;

public class WindZoneScript : MonoBehaviour
{
    [SerializeField] float _windForce = 4f;

    private void OnTriggerStay(Collider other)
    {
        var hitObj = other.gameObject;
        if (hitObj != null )
        {
            var rb = hitObj.GetComponent<Rigidbody>();
            var dir = transform.up;
            rb.AddForce(dir * _windForce);
        }
    }
}
