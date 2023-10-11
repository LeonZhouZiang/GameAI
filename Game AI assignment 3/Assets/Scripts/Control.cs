using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control : MonoBehaviour
{
    private void Update()
    {
        Vector3 move = new(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        transform.Translate(move * 10 * Time.deltaTime, Space.World);
        transform.LookAt(transform.position + move);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Leader.Instance.Remove(other.gameObject);
            Destroy(other.gameObject);
        }
    }
}
