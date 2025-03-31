using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterflySpawner : MonoBehaviour
{

    [SerializeField] int ButterflyCount;
    [SerializeField] GameObject ButterflyPrefab;
    [SerializeField] Transform MiddlePoint;

    private AvoidObjectSwimmingBehavior SpawningScript;


    // Start is called before the first frame update
    void Start()
    {
        

        for (var i = 0; i < ButterflyCount; i++)
        {
            GameObject Butterfly = Instantiate(ButterflyPrefab, new Vector3(i * 2.0f, 0, 0), Quaternion.identity, transform);
            Butterfly.GetComponent<AvoidObjectSwimmingBehavior>().SetBoundCenter(MiddlePoint);
        }
    }


}
