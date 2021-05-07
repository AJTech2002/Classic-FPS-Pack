using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;


namespace ClassicFPS.Controller.Movement
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("References")]
        public Transform pivotTransform;
        public CinemachineVirtualCamera virtualCamera;

        [Header("Mouse Properties")]
        public bool lockRotation;
        public CursorLockMode locked; //Whether or not the cursor is locked to the screen
        public bool visible; //Visibility of the mouse

        [Header("Clamping")]
        //Clamping Variables 
        public Vector2 pitchMinMax = new Vector2(-40, 85);

        [Header("Rotation Speeds")]
        //Sensitivity and Smoothing of the Mouse Movements
        public float mouseSensitivity = 10;
        public float rotationSmoothTime = 8f;


        [Header("Player Influenced Properties")]
        public bool cameraInfluencedByPlayerInput = true;
        public float fovZoomAmount = 10; //How much the Field of View increases when Sprinting 
        public float tiltAmount = 10; //How much the Camera tilts side to side when moving side to side

        public float fovZoomSpeed; //Speed of the FOV Effect
        public float tiltSpeed; //Speed of the Tilt Effect

        [HideInInspector]
        public Vector3 currentRotation;

        private float yaw;
        private float pitch;

        private float defaultFov;
        private float defaultTilt = 0f;

        private PlayerInputManager inputManager;
        private PlayerController playerController;

        //Initialise Cursor Variables
        private void Awake()
        {
            //Set the values of the cursor
            Cursor.lockState = locked;
            Cursor.visible = visible;

            inputManager = GetComponent<PlayerInputManager>();
            playerController = GetComponent<PlayerController>();
            defaultFov = virtualCamera.m_Lens.FieldOfView;
        }

        //Run the rotation on LateUpdate after movement for Player Controller has completed
        void LateUpdate()
        {
            if (!lockRotation && !overridingRotation)
            {
                //Rotate the eulerAngles of the current transform based on the pitch and yaw
                pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
                currentRotation = Vector3.Lerp(currentRotation, new Vector3(pitch, yaw), rotationSmoothTime * Time.deltaTime);
                pivotTransform.eulerAngles = currentRotation;


                if (cameraInfluencedByPlayerInput)
                {
                    //If the Player is Sprinting the Increase the FOV 
                    if (playerController.characterSpeed == playerController.sprintSpeed)
                    {
                        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, defaultFov + fovZoomAmount, fovZoomSpeed * Time.deltaTime);
                    }
                    else
                    {
                        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, defaultFov, fovZoomSpeed * Time.deltaTime);
                    }

                    if (inputManager.processedInputs.x != 0)
                    {
                        //Dutch is the 'tilting' motion observed, only do it when the horizontal input is active
                        virtualCamera.m_Lens.Dutch = Mathf.Lerp(virtualCamera.m_Lens.Dutch, -inputManager.processedInputs.x * tiltAmount, tiltSpeed * Time.deltaTime);
                    }
                }

            }

            //Rotate the camera on force
            if (overridingRotation)
            {
                pitch = forceAngles.x;
                yaw = forceAngles.y;
                currentRotation = new Vector3(pitch, yaw);
                overridingRotation = false;
            }
        }

        bool overridingRotation;
        Vector3 forceAngles;

        //Force rotation over 1 frame
        public void SetRotation(Vector3 eulerAngles)
        {
            overridingRotation = true;
            forceAngles = eulerAngles;
        }

        //GetMouseMove callback given by new Input System
        public void GetMouseMove(InputAction.CallbackContext callback)
        {
            Vector2 data = callback.ReadValue<Vector2>();

            //Based on 'data' which provides the deltas for mouse movement (how much the mouse moved per frame) add yaw and pitch
            yaw += data.x * mouseSensitivity;
            pitch -= data.y * mouseSensitivity;
        }

    }
}