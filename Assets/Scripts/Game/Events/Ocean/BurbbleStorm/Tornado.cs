using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Tornado : MonoBehaviour
{
    [Tooltip("Distance after which the rotation physics starts")]
    [SerializeField] private float _maxDistance = 1.5f;
    
    [Tooltip("The axis that the caught objects will rotate around")]
    [SerializeField] private Vector3 _rotationAxis = new Vector3(0, 1, 0);
    
    [Tooltip("The angle will be use to determine the direction and rotation angle of the caught oject")]
    public float _rotationAngle = 130f;
    
    [Tooltip("Angle that is added to the object's velocity (higher lift -> quicker on top)")]
    [Range(0, 90)]
    [SerializeField] private float _lift = 10;
    
    [Tooltip("The force that will drive the caught objects around the tornado's center")]
    [SerializeField] private float rotationRotationStrength = 10;

    [Tooltip("Tornado general pull force")]
    [SerializeField] private float _minTornadoStrength = 4;
    [SerializeField] private float _maxTornadoStrength = 100;

    private float _tornadoStrength;
    
    public float PullStrengthIncreaseDuration { get; set; }

    [SerializeField] private GameObject _caughtObjectsHolder;

    private GameObject _bee;

    private Rigidbody _rb;

    private List<CaughtObject> _caughtObjects = new List<CaughtObject>();

    public float tornadoStrength => _tornadoStrength;
    public float rotationStrength => rotationRotationStrength;
    
    public Vector3 rotationAxis => _rotationAxis;

    public float rotationAngle => _rotationAngle;

    public float lift => _lift;

    void Awake()
    {
        SetUp();
    }

    private void SetUp()
    {
        _tornadoStrength = _minTornadoStrength;
        
        _bee = Bee.Instance.gameObject;
        
        _rotationAxis.Normalize();

        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        
        AddCaughtObjects();
    }

    private void AddCaughtObjects()
    {
        AddBee();
        AddOtherObjects();
    }

    private void AddOtherObjects()
    {
        if (_caughtObjectsHolder != null)
            for (int i = 0; i < _caughtObjectsHolder.transform.childCount; i++)
            {
                Transform child = _caughtObjectsHolder.transform.GetChild(i);

                if (child.TryGetComponent(typeof(CaughtObject), out Component a))
                    _caughtObjects.Add(child.GetComponent<CaughtObject>());
            }
    }

    private void AddBee()
    {
        if (_bee.TryGetComponent(typeof(CaughtObject), out Component c))
            _caughtObjects.Add(_bee.GetComponent<CaughtObject>());
    }

    void FixedUpdate()
    {
        //try pull object in the center if it exceed maxdistance
        ApplyForce();
    }
    
    private void ApplyForce()
    {
        for (int i = 0; i < _caughtObjects.Count; i++)
        {
            if (_caughtObjects[i] != null)
            {
                Vector3 pull = transform.position - _caughtObjects[i].transform.position;

                if (pull.magnitude > _maxDistance)
                {
                    _caughtObjects[i].rigid.AddForce(pull.normalized * pull.magnitude, ForceMode.Force);
                    _caughtObjects[i].enabled = false;
                }
                else 
                    _caughtObjects[i].enabled = true;
            }
        }
    }

    private void OnEnable()
    {
        if(_caughtObjects == null || _caughtObjects.Count<=0) return;
        
        InitCaughtObjects();

        StartCoroutine(IncreaseTilThreshold(_minTornadoStrength, _maxTornadoStrength,
            PullStrengthIncreaseDuration, UpdateTornadoStrength));
    }

    private void InitCaughtObjects()
    {
        foreach (var caught in _caughtObjects)
        { 
            caught.Init(this, _rb, _tornadoStrength);
        }
    }

    private void OnDisable()
    {
        if(_caughtObjects == null || _caughtObjects.Count<=0) return;

        ReleaseCaughtObjects();

        _tornadoStrength = _minTornadoStrength;
    }

    private void ReleaseCaughtObjects()
    {
        foreach (var caught in _caughtObjects)
        {
            caught.Release();
        }
    }

    private IEnumerator IncreaseTilThreshold(float startValue, float threshold, float duration,
        Action<float>updateValue)
    {
        float elapseTime = 0f;
        float currentValue = startValue;

        while (currentValue < threshold)
        {
            elapseTime += Time.deltaTime;
            currentValue = Mathf.Lerp(startValue, threshold, elapseTime / duration);

            updateValue(currentValue);
            yield return null;
        }

        updateValue(currentValue);
    }

    private void UpdateTornadoStrength(float updateValue)
    {
        _tornadoStrength = updateValue;
    }
}
