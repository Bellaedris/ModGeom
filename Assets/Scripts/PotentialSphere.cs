using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PotentialSphere : MonoBehaviour
{
    public float speed = 10f;
    public float potentialModifyer = 20f;
    private SphereCollider sphereCollider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetKey(KeyCode.Q))
            direction.y = -1f;
        if (Input.GetKey(KeyCode.E))
            direction.y = 1f;
        
        //scale the sphere up when pressing R, down when pressing F
        if (Input.GetKey(KeyCode.R))
            transform.localScale += Vector3.one * 0.1f;
        if (Input.GetKey(KeyCode.F))
            transform.localScale -= Vector3.one * 0.1f;
        
        transform.Translate(speed * Time.deltaTime * direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Voxel"))
        {
            var v = other.GetComponent<Voxel>();
            v.AddPotential(-potentialModifyer);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Voxel"))
        {
            var v = other.GetComponent<Voxel>();
            v.AddPotential(potentialModifyer);
        }
    }
}