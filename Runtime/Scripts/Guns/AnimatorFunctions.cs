using ClassicFPS.Audio;
using ClassicFPS.Controller.PlayerState;
using ClassicFPS.Controller.SFX;
using ClassicFPS.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*This script can be used on pretty much any gameObject. It provides several functions that can be called with 
animation events in the animation window.*/

public class AnimatorFunctions : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem particleSystem1;
    [SerializeField] private ParticleSystem particleSystem2;
    [SerializeField] private Animator setBoolInAnimator;
    [SerializeField] AudioClip[] randomSounds;
    private PlayerSFX playerSFX;


    private void Awake()
    {
        playerSFX = GameObject.FindObjectOfType<PlayerSFX>();
    }

    // If we don't specify what audio source to play sounds through, just use the one on player.
    void Start()
    {
        if (!audioSource) audioSource = playerSFX.movementAudioSource;
    }

    public void PlayEnemyFootstepSound()
    {
        if (playerSFX != null)
            playerSFX.PlayFootstepSound(audioSource, 1.8f, 2f);
    }

    //Player footstep sound
    public void PlayFootstepSound()
    {
        if (playerSFX != null)
            playerSFX.PlayFootstepSound(playerSFX.movementAudioSource);
    }

    public void CreateFootprint()
    {
        if (TerrainSurface.GetMainTexture(transform.position) == "Snow")
        {
            //Create a Ray
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, -transform.up, out hit, 10, GameManager.PlayerStatistics.footPrintLayerMask))
            {
                //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
               
                GameObject footPrintClone = Instantiate(GameManager.PlayerStatistics.footPrint, hit.point, Quaternion.LookRotation(hit.normal));
                Debug.Log(footPrintClone.transform.GetChild(0));
                footPrintClone.transform.GetChild(0).transform.rotation = Quaternion.Euler(footPrintClone.transform.GetChild(0).transform.eulerAngles.x, footPrintClone.transform.GetChild(0).transform.eulerAngles.y, GameManager.PlayerController.camera.transform.localRotation.y);
                footPrintClone.transform.parent = null;
            }

           
        }
    }

    public void ScreenshakeShoot()
    {
        StartCoroutine(GameManager.PlayerController.playerCameraController.ShakeScreen(1.4f, 5f, .1f));
    }

    public void ScreenshakeShootShotgun()
    {
        StartCoroutine(GameManager.PlayerController.playerCameraController.ShakeScreen(3f, 7f, .14f));
    }

    public void ScreenshakeShootRocket()
    {
        StartCoroutine(GameManager.PlayerController.playerCameraController.ShakeScreen(2f, 5f, 1f));
    }

    public void HurtPlayer(int damage = 1)
    {
        GameObject.FindObjectOfType<PlayerStatistics>().TakeDamage(damage);
    }

    //Hide and unhide the player
    public void HidePlayer(bool hide)
    {
        // NewPlayer.Instance.Hide(hide);
    }

    //Sometimes we want an animated object to force the player to jump, like a jump pad.
    public void JumpPlayer(float power = 1f)
    {
        //NewPlayer.Instance.Jump(power);
    }

    //Freeze and unfreeze the player movement
    void FreezePlayer(bool freeze)
    {
        // NewPlayer.Instance.Freeze(freeze);
    }

    //Play a sound through the specified audioSource
    void PlaySound(AudioClip whichSound)
    {
        audioSource.PlayOneShot(whichSound);
    }

    //Play a sound through the specified audioSource after stopping the source
    void PlaySoundWhileStoppingSource(AudioClip whichSound)
    {
        audioSource.Stop();
        audioSource.PlayOneShot(whichSound);
    }

    //Play a sound through the specified audioSource
    void PlayRandomSound()
    {
        audioSource.PlayOneShot(randomSounds[Random.Range(0, randomSounds.Length)]);
    }

    public void EmitParticles(int amount)
    {
        particleSystem1.Emit(amount);
    }

    public void EmitParticles2(int amount)
    {
        particleSystem2.Emit(amount);
    }

    public void ScreenShake(float power)
    {
        // NewPlayer.Instance.cameraEffects.Shake(power, 1f);
    }

    public void SetTimeScale(float time)
    {
        Time.timeScale = time;
    }

    public void SetAnimBoolToFalse(string boolName)
    {
        setBoolInAnimator.SetBool(boolName, false);
    }

    public void SetAnimBoolToTrue(string boolName)
    {
        setBoolInAnimator.SetBool(boolName, true);
    }

    public void FadeOutMusic()
    {
        // GameManager.Instance.gameMusic.GetComponent<AudioTrigger>().maxVolume = 0f;
    }

    public void LoadScene(string whichLevel)
    {
        SceneManager.LoadScene(whichLevel);
    }

    //Slow down or speed up the game's time scale!
    public void SetTimeScaleTo(float timeScale)
    {
        Time.timeScale = timeScale;
    }
}