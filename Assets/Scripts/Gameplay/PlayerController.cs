using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float speed = 10f;
    public float rotationSmoothingTime = .1f;
    public float jumpForce = 5f;
    public Camera mainCamera;

    private float _currentRotation;
    
    private Rigidbody _rb;
    private CharacterController _cc;
    private Animator _anim;
    
    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        if (direction.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg +
                                mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _currentRotation,
                rotationSmoothingTime);

            Vector3 targetDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            _cc.Move(speed * Time.deltaTime * targetDirection);
        }
    }
}
