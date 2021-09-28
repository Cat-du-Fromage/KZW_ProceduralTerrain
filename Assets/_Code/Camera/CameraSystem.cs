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
using float3 = Unity.Mathematics.float3;
using float2 = Unity.Mathematics.float2;
namespace KaizerWaldCode.Camera
{
    public class CameraSystem : MonoBehaviour
    {
        [Min(1)]
        [SerializeField] private int rotationSpeed, moveSpeed, zoomSpeed;

        private bool canRotate;
        private int sprint;
        private float zoom;
        private float2 startPosition, endPosition;
        private float2 horizontal;
        Transform cameraTransform;

        private void Awake()
        {
            cameraTransform = GetComponent<Transform>();
            rotationSpeed = MinMax(1, rotationSpeed);
            moveSpeed = MinMax(1, moveSpeed);
            zoomSpeed = MinMax(1, zoomSpeed);
            sprint = max(1, sprint);
            canRotate = false;
        }

        private void Update()
        {
            if (canRotate)
                SetCameraRotation();

            if (!horizontal.Equals(float2.zero))
            {
                //real forward of the camera (aware of the rotation)
                float3 currentCameraForward = new float3(cameraTransform.forward.x, 0, cameraTransform.forward.z);
                float3 z = float3.zero;
                float3 x = float3.zero;

                if (horizontal.x != 0)
                    x = select(cameraTransform.right, -cameraTransform.right, horizontal.x > 0);
                if (horizontal.y != 0)
                    z = select(-currentCameraForward, currentCameraForward, horizontal.y > 0);
                cameraTransform.position += (Vector3)(x + z) * max(1f,cameraTransform.position.y) * moveSpeed * sprint * Time.deltaTime;
            }

            if (zoom != 0)
                cameraTransform.position = mad(up(), zoom, transform.position);
        }

        public void SetCameraRotation()
        {
            endPosition = Mouse.current.position.ReadValue();
            if (!endPosition.Equals(startPosition))
            {
                float distanceX = (endPosition - startPosition).x * rotationSpeed;
                float distanceY = (endPosition - startPosition).y * rotationSpeed;
                cameraTransform.Rotate(0f, distanceX * Time.deltaTime, 0f, Space.World);
                cameraTransform.Rotate(-distanceY * Time.deltaTime, 0f, 0f, Space.Self);

                startPosition = endPosition;
            }
        }

        #region EVENTS CALLBACK
        public void RotateCamera(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                startPosition = Mouse.current.position.ReadValue();
                canRotate = true;
            }
            else if (context.canceled)
            {
                canRotate = false;
            }
        }

        public void MoveCamera(InputAction.CallbackContext context)
        {
            horizontal = context.ReadValue<Vector2>();
        }

        public void ZoomCamera(InputAction.CallbackContext context)
        {
            zoom = context.ReadValue<float>();
        }

        public void SprintCamera(InputAction.CallbackContext context)
        {
            sprint = select(3, 1, context.canceled);
        }
        #endregion EVENTS CALLBACK
    }
}
