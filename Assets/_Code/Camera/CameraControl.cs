using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace KaizerWaldCode
{
    public class CameraControl : MonoBehaviour
    {
        private float speed;
        private float zoomSpeed;
        private float rotationSpeed;

        private float maxHeight = 300f;
        private float minHeight = 3f;

        private Vector2 startPosition;
        private Vector2 EndPosition;

        // Start is called before the first frame update
        void Start()
        {
            rotationSpeed = 20f;
        }

        // Update is called once per frame
        void Update()
        {
            speed = Input.GetKey(KeyCode.LeftShift) ? 3.6f : 1.8f;
            zoomSpeed = Input.GetKey(KeyCode.LeftShift) ? 1080.0f : 540.0f;

            // "Mathf.Log(transform.position.y)" Adjust the speed the higher the camera is
            float t = Time.deltaTime;
            float HorizontalSpeed = t * (transform.position.y) * speed * Input.GetAxis("Horizontal");
            float VerticalSpeed = t * (transform.position.y) * speed * Input.GetAxis("Vertical");
            float ScrollSpeed = t * (-zoomSpeed * Mathf.Log(transform.position.y) * Input.GetAxis("Mouse ScrollWheel"));
            //========================\\
            //        ZOOM PART       \\
            //========================\\
            if ((transform.position.y >= maxHeight) && (ScrollSpeed > 0))
            {
                ScrollSpeed = 0;
            }
            else if ((transform.position.y <= minHeight) && (ScrollSpeed < 0))
            {
                ScrollSpeed = 0;
            }
            if ((transform.position.y + ScrollSpeed) > maxHeight)
            {
                ScrollSpeed = maxHeight - transform.position.y;
            }
            else if ((transform.position.y + ScrollSpeed) < minHeight)
            {
                ScrollSpeed = minHeight - transform.position.y;
            }

            Vector3 VerticalMove = new Vector3(0, ScrollSpeed, 0);
            Vector3 LateralMove = HorizontalSpeed * transform.right;
            //Movement forward by vector projection
            Vector3 ForwardMove = transform.forward;
            ForwardMove.y = 0; //remove vertical component
            ForwardMove.Normalize(); //normalize vector
            ForwardMove *= VerticalSpeed;

            Vector3 Move = VerticalMove + LateralMove + ForwardMove;

            transform.position += Move;

            getCameraRotation();
        }
        #region Camera ROTATION
        public void getCameraRotation()
        {

            if (Input.GetMouseButtonDown(2)) //check if the middle mouse button was pressed
            {
                startPosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(2)) //check if the middle mouse button is being held down
            {
                EndPosition = Input.mousePosition;

                float t = Time.deltaTime;
                float DistanceX = (EndPosition - startPosition).x * rotationSpeed * t;
                float DistanceY = (EndPosition - startPosition).y * rotationSpeed * t;

                transform.rotation *= Quaternion.Euler(new Vector3(0, DistanceX, 0));

                transform.GetChild(0).transform.rotation *= Quaternion.Euler(new Vector3(-DistanceY, 0, 0));
                startPosition = EndPosition;
            }

        }
        #endregion Camera ROTATION
    }
}

