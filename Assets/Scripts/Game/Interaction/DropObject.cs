using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(SphereCollider)), ]
public class DropObject : MonoBehaviour
{
    private Rigidbody _objectRigidbody;
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _objectRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        SetUp();
    }

    private void SetUp()
    {
        _objectRigidbody.isKinematic = true;
        _objectRigidbody.useGravity = true;
    }

    public void Drop()
    {
        _collider.enabled = true;
        _objectRigidbody.isKinematic = false;
    }

    public void ResetObject (Vector3 resetPosition)
    {
        _objectRigidbody.isKinematic = true;
        _collider.enabled = false;
        transform.position = resetPosition;
    }
}
