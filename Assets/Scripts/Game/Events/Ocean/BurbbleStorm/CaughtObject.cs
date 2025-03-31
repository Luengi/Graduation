using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class CaughtObject : MonoBehaviour
{
    private Tornado _tornadoReference;
    private SpringJoint _spring;
    
    public Rigidbody rigid {get; set;}
    public bool allowUpdate{get; set;}

    private void Awake()
    {
        enabled = false;
        SetUp();
    }

    private void SetUp()
    {
        rigid = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        SetSpringStrength();
        Rotate();
        
        if (!allowUpdate) return;
        
        if (_tornadoReference.allowConstrain) LockZ();
        
        //LiftObject();
        RotateObjectAroundTornado(_tornadoReference);
    }

    private void LockZ()
    {
        rigid.constraints = RigidbodyConstraints.FreezePositionZ;
    }

    private void Rotate()
    {
        transform.Rotate(_tornadoReference.rotationAxis,_tornadoReference.tornadoStrength*10 *Time.deltaTime,Space.Self);
    }
    
    private void LiftObject()
    {
        if (_spring == null || _tornadoReference == null) return;
        
        Vector3 newPosition = _spring.connectedAnchor;
        newPosition.y = transform.position.y;
        _spring.connectedAnchor = newPosition;
    }

    private void SetSpringStrength()
    {
        _spring.spring = _tornadoReference.tornadoStrength;
    }

    private void RotateObjectAroundTornado (Tornado tornado)
    {
        if (tornado == null) return;
        
        // Calculate the vector pointing from the tornado's center to the object's current position
        Vector3 direction = transform.position - tornado.transform.position;
    
        // Project this direction vector onto the plane defined by the tornado's rotation axis
        // This keeps the movement within the tornado's swirl plane
        Vector3 projection = Vector3.ProjectOnPlane(direction, tornado.rotationAxis);
    
        // Normalize the projection vector to make it a unit vector (direction only, no magnitude)
        projection.Normalize();
    
        // Rotate the normalized vector around the rotation axis by the specified rotation angle
        // This gives the swirl direction as if the object is moving around the tornado center
        Vector3 normal = Quaternion.AngleAxis(tornado.rotationAngle, tornado.rotationAxis) * projection;
    
        // Apply a secondary rotation to simulate "lift" in the direction of the swirl,
        // creating an upward (or downward) draft effect around the tornado
        //normal = Quaternion.AngleAxis(tornado.lift, projection) * normal;
    
        // Add force in the direction of the calculated vector `normal`, scaled by the tornado's strength
        // This results in the object following a swirling, lifting motion
        rigid.AddForce(normal * tornado.rotationStrength, ForceMode.Force);
    }
    
    public void Init (Tornado tornadoRef, Rigidbody tornadoRigidbody, float springForce)
    {
        enabled = true;
        allowUpdate = true;
        
        _tornadoReference = tornadoRef;

        InitSpring(tornadoRigidbody, springForce);
    }

    private void InitSpring (Rigidbody tornadoRigidbody, float springForce)
    {
        _spring = gameObject.AddComponent<SpringJoint>();
        _spring.spring = springForce;
        _spring.connectedBody = tornadoRigidbody;
        _spring.autoConfigureConnectedAnchor = false;
        
        Vector3 initialPosition = Vector3.zero;
        // initialPosition.y = transform.position.y;
        
        _spring.connectedAnchor = initialPosition;
    }

    public void Release()
    {
        if (this==null) return;

        rigid.constraints = RigidbodyConstraints.None;
        enabled = false;
        allowUpdate = false;
        Destroy(_spring);
    }
}
