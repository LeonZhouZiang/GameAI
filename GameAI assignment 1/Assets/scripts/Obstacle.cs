using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float t;
    public Vector2 speed;
    public float x = 0;
    public float z = 0;
    void Start()
    {   
        speed.x = x / t; 
        speed.y = z / t;
        x += transform.position.x;
        z += transform.position.z;

        transform.DOMoveX(x, t).SetLoops(-1).SetEase(Ease.Linear);
        transform.DOMoveZ(z, t).SetLoops(-1).SetEase(Ease.Linear);
    }

}
