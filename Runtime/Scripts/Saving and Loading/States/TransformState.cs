using ClassicFPS.Controller.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Saving_and_Loading.States
{
    /* Sample State for the Transform Component */
    public class TransformState : State
    {

        //Creating a storage format for the transform component (must be marked Serializable)
        [System.Serializable]
        public struct TransformValues
        {
            //We need to store the position/rotation/scale in Serializable formats (XYZState provided)
            public Vector3State positionState;
            public Vector3State rotationState;
            public Vector3State scaleState;

            //Rigidbody information as well
            public Vector3State velocity;
            public Vector3State angularVelocity;
        }

        //The current Saved State
        [Header("Saved State")]
        public TransformValues savedState = new TransformValues();

        //Both the SaveState () and LoadState () will be called by the SaveManager
        public override string SaveState()
        {
            //Saving the values from Transform into a Serializable struct
            savedState.positionState = SaveUtils.Vector3ToXYZ(transform.position);

            //Check if the Transform is a PlayerController or not
            if (GetComponent<PlayerController>() == null)
                //If it isn't then just save rotation
                savedState.rotationState = SaveUtils.Vector3ToXYZ(transform.eulerAngles);
            else
                //If it is then store the rotation of the Camera
                savedState.rotationState = SaveUtils.Vector3ToXYZ(GetComponent<PlayerCameraController>().currentRotation);
        
            //Store the scale
            savedState.scaleState = SaveUtils.Vector3ToXYZ(transform.localScale);

            if (GetComponent<Rigidbody>() != null)
            {
                Rigidbody rBody = GetComponent<Rigidbody>();
            
                //Save the Velocity and Angular Velocity of Rigidbodies, so that when loaded then continue that trajectory
                savedState.velocity = SaveUtils.Vector3ToXYZ(rBody.velocity);
                savedState.angularVelocity = SaveUtils.Vector3ToXYZ(rBody.angularVelocity);
            }

            //Creating and returning JSON (SaveUtils provided)
            return SaveUtils.ReturnJSONFromObject<TransformValues>(savedState);
        }

        public override void LoadState(string loadedJSON)
        {
            //Getting back the struct through JSON
            savedState = SaveUtils.ReturnStateFromJSON<TransformValues>(loadedJSON);

            if (GetComponent<PlayerController>() != null)
            {
                //If it is a Player Controller, then TeleportPlayer to their last location
                GetComponent<PlayerController>().TeleportPlayer(SaveUtils.XYZToVector3(savedState.positionState));

                //Set the Rotation of Camera in CameraController
                GetComponent<PlayerCameraController>().SetRotation(SaveUtils.XYZToVector3(savedState.rotationState));
            }
            else
            {
                //Otherwise just set position/rotation
                transform.position = SaveUtils.XYZToVector3(savedState.positionState);
                transform.eulerAngles = SaveUtils.XYZToVector3(savedState.rotationState);
            }

            transform.localScale = SaveUtils.XYZToVector3(savedState.scaleState);

            //Apply rigidbody properties
            if (GetComponent<Rigidbody>() != null)
            {
                Rigidbody rBody = GetComponent<Rigidbody>();

                rBody.velocity = SaveUtils.XYZToVector3(savedState.velocity);
                rBody.angularVelocity = SaveUtils.XYZToVector3(savedState.angularVelocity);
            }

        }

        //Only save if the position has changed from the last time you saved (Saves Space)
        public override bool ShouldSave()
        {
            bool positionSame = transform.position == SaveUtils.XYZToVector3(savedState.positionState);
            bool rotationSame = transform.eulerAngles == SaveUtils.XYZToVector3(savedState.rotationState);

            if (positionSame && rotationSame && GetComponent<PlayerController>() == null)
                return false;

            return true;

        }


    }
}