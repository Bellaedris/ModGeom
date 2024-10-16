using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Voxel : MonoBehaviour
{

    public float _potential;
    private MeshRenderer _meshRenderer;

    private bool _isGameRunning = false;
    
    // Start is called before the first frame update
    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _isGameRunning = true;
    }

    public void AddPotential(float delta)
    {
        _potential += delta;
        if (_isGameRunning)
            _meshRenderer.enabled = !(_potential < 0f);
    }
}
