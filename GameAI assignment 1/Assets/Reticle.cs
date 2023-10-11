using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    public GameObject target;
    void Start()
    {

    }


    void Update()
    {
        //Quaternion targetDirection = Quaternion.LookRotation(target.transform.position - body.transform.position, Vector3.up);
        
        transform.LookAt(target.transform);
    }
}