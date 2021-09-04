using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace KaizerWaldCode.Camera
{
    public class CameraInputController : MonoBehaviour
    {
        private Controls controls;
        public MoveInputEvent moveInputEvent;

        void Awake()
        {
            controls = new Controls();
        }

        // Update is called once per frame
        void OnEnable()
        {
            controls.CameraControl.Enable();
            controls.CameraControl.Mouvement.performed += OnMouvement;
        }

        private void OnMouvement(InputAction.CallbackContext context)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();
            moveInputEvent.Invoke(moveInput.x, moveInput.y);
        }
    }

    [Serializable]
    public class MoveInputEvent : UnityEvent<float, float>{}
}
