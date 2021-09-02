using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using UnityEngine.InputSystem.Interactions;

using static Unity.Mathematics.math;
using float3 =  Unity.Mathematics.float3;
using static Unity.Mathematics.quaternion;
namespace KaizerWaldCode
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _zoomSpeed;

        private int _sprint;
        private float _zoom;
        private bool _canRotate;
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private Vector2 _horizontal;
        private Transform child;
        
        private void Awake()
        {
            child = transform.GetChild(0);
            _rotationSpeed = max(10f, _rotationSpeed);
            _moveSpeed = max(1f, _moveSpeed);
            _zoomSpeed = max(1f, _zoomSpeed);
            _canRotate = false;
            _sprint = max(1, _sprint);
        }
        
        private void Update()
        {
            if(_canRotate)
                SetCameraRotation();

            if (_horizontal != Vector2.zero)
            {
                //real forward of the camera (aware of the rotation)
                //Vector3 currentCameraForward = new Vector3(transform.forward.x, 0, transform.forward.z);
                Vector3 z = Vector3.zero;
                Vector3 x = Vector3.zero;

                if (_horizontal.x != 0) 
                    x = _horizontal.x > 0 ? -transform.right : transform.right;
                if (_horizontal.y != 0) 
                    z = _horizontal.y > 0 ? transform.forward : -transform.forward;

                Vector3 dir = (x + z) * _moveSpeed * Time.deltaTime * _sprint;

                transform.position += dir;
            }

            if (_zoom != 0)
                transform.position += Vector3.up * _zoom;
        }
        
        public void SetCameraRotation()
        {
            float distanceX, distanceY;
            _endPosition = Mouse.current.position.ReadValue();
            if (_endPosition != _startPosition)
            {
                distanceX = radians((_endPosition - _startPosition).x * _rotationSpeed * Time.deltaTime);
                distanceY = radians((_endPosition - _startPosition).y * _rotationSpeed * Time.deltaTime);

                transform.rotation *= EulerZXY(new float3(0, distanceX, 0));
                child.transform.rotation *= EulerZXY(new float3(-distanceY, 0, 0));
                //transform.rotation *= Quaternion.Euler(new Vector3(0, distanceX, 0));
                //transform.GetChild(0).transform.rotation *= Quaternion.Euler(new Vector3(-distanceY, 0, 0));
                _startPosition = _endPosition;
            }
        }

        public void RotateCamera(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _startPosition = Mouse.current.position.ReadValue();
                _canRotate = true;
            }
            else if (context.canceled)
            {
                _canRotate = false;
            }
        }

        public void MoveCamera(InputAction.CallbackContext context)
        {
            _horizontal = context.ReadValue<Vector2>();
        }

        public void ZoomCamera(InputAction.CallbackContext context)
        {
            _zoom = context.ReadValue<float>();
        }

        public void SprintCamera(InputAction.CallbackContext context)
        {
            _sprint = context.canceled ? 1 : 3;
        }
    }
}
