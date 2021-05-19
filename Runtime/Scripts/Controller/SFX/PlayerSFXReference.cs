using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Controller.SFX {
    public class PlayerSFXReference : MonoBehaviour
    {
        private PlayerSFX playerSFX;

        private void Awake()
        {
            playerSFX = GameObject.FindObjectOfType<PlayerSFX>();
        }

        public void PlayGroundSFX ()
        {
            if (playerSFX != null)
            playerSFX.PlayFootstepSound();
        }

    }

}
