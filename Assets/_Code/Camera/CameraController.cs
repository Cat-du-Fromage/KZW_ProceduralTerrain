using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using UnityEngine.InputSystem.Interactions;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using float3 =  Unity.Mathematics.float3;
using float2 = Unity.Mathematics.float2;
namespace KaizerWaldCode
{
    public class CameraController : MonoBehaviour
    {
        [Min(1)]
        [SerializeField] private byte _rotationSpeed;
        [Min(1)]
        [SerializeField] private byte _moveSpeed;
        [Min(1)]
        [SerializeField] private byte _zoomSpeed;

        private int _sprint;
        private float _zoom;
        private bool _canRotate;
        private float2 _startPosition;
        private float2 _endPosition;
        private float2 _horizontal;

        private void Awake()
        {
            _rotationSpeed = MinMaxByte(1, _rotationSpeed);
            _moveSpeed = MinMaxByte(1, _moveSpeed);
            _zoomSpeed = MinMaxByte(1, _zoomSpeed);
            _sprint = max(1, _sprint);
            _canRotate = false;
        }
        
        private void Update()
        {
            if(_canRotate)
                SetCameraRotation();

            if (!_horizontal.Equals(float2.zero))
            {
                //real forward of the camera (aware of the rotation)
                float3 currentCameraForward = new float3(transform.forward.x, 0, transform.forward.z);
                float3 z = float3.zero;
                float3 x = float3.zero;

                if (_horizontal.x != 0) 
                    x = select(transform.right, -transform.right, _horizontal.x > 0);
                if (_horizontal.y != 0) 
                    z = select(-currentCameraForward, currentCameraForward, _horizontal.y > 0);
                transform.position += (Vector3)(x + z) * _moveSpeed * _sprint * Time.deltaTime;
            }

            if (_zoom != 0)
                transform.position = mad(up(), _zoom, transform.position);
        }
        
        public void SetCameraRotation()
        {
            _endPosition = Mouse.current.position.ReadValue();
            if (!_endPosition.Equals(_startPosition))
            {
                float distanceX = (_endPosition - _startPosition).x * _rotationSpeed * Time.deltaTime;
                float distanceY = (_endPosition - _startPosition).y * _rotationSpeed * Time.deltaTime;
                transform.Rotate(0f, distanceX, 0f, Space.World);
                transform.Rotate(-distanceY, 0f, 0f, Space.Self);
                
                _startPosition = _endPosition;
            }
        }

        #region EVENTS CALLBACK
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
            _sprint = select(3, 1, context.canceled);
        }
        #endregion EVENTS CALLBACK
    }
}
