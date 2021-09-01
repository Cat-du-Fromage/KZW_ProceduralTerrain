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
    public class CameraRotation : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _zoomSpeed;

        private float _zoom;
        private bool _canRotate;
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private Vector3 _horizontal;
        
        private void Awake()
        {
            _rotationSpeed = max(10f, _rotationSpeed);
            _moveSpeed = max(1f, _moveSpeed);
            _zoomSpeed = max(1f, _zoomSpeed);
            _canRotate = false;
        }
        
        private void Update()
        {
            if(_canRotate)
            {
                SetCameraRotation();
            }

            if (_horizontal != Vector3.zero)
            {
                Vector3 _moveZPositiv = new Vector3(transform.forward.x * _horizontal.x * _moveSpeed, 0, transform.forward.z * _horizontal.z * _moveSpeed);
                transform.position += _moveZPositiv;
            }

            if (_zoom != 0)
            {
                transform.position += Vector3.up * _zoom;
            }
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
                transform.GetChild(0).transform.rotation *= EulerZXY(new float3(- distanceY, 0, 0));
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
            
            _horizontal.x = context.ReadValue<Vector2>().x;
            _horizontal.z = context.ReadValue<Vector2>().y;
        }

        public void ZoomCamera(InputAction.CallbackContext context)
        {
            _zoom = context.ReadValue<float>();
        }
    }
}
